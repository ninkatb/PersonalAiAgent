# Faza 02 — Encrypted SQLite, vault, master lozinka i oporavak

## Cilj i princip

**Svi API ključevi i OAuth tokeni čuvaju se u SQLite bazi, ali isključivo enkriptovani.** Ne koristiti Windows Credential Manager, DPAPI ili drugi OS keychain kao obaveznu zavisnost. Radi zaštite razgovora, memorije i dokumenata od kopiranja datoteke, **cijela SQLite baza treba da bude šifrovana SQLCipher-om**. Tajne imaju dodatnu aplikativnu AES-256-GCM enkripciju (defense in depth). Ovo je kritična faza: nije dozvoljen tihi povratak na plaintext SQLite.

## Projektovana šema ključeva

```text
MASTER PASSPHRASE (nikad se ne čuva)
  + jedinstveni salt (16+ B) i parametri Argon2id
  -> 32-byte root key
  -> HKDF-SHA256, odvojeni domeni -> KEK
  -> dekripcija dva nezavisna, slučajno generisana ključa:
       1. DB_ENCRYPTION_KEY (32 B)  --> SQLCipher baza
       2. SECRET_ROOT_KEY (32 B)   --> AES-256-GCM šifrovanje tajni

vault.bootstrap.json (bez plaintext tajni):
  vaultId, verzije protokola, salt, Argon2id parametri,
  AES-GCM wrapped DB key i secret root key (nonce/tag/ciphertext),
  odvojeni optional recovery wrap podaci

agent.db (SQLCipher, šifrovano u mirovanju):
  ProviderCredentials / OAuthCredentials: ciphertext, nonce,
  authentication tag, recordId, providerId, ownerId, version, metadata
```

**Napomena:** Bootstrap datoteka sadrži samo šifrovane ključeve i javne parametre; **korisnički API ključevi ostaju u SQLite bazi**. Ne držati derivirani root/KEK, DB key ili secret root key na disku u plaintextu. Provesti dizajn kroz nezavisnu sigurnosnu reviziju prije prodaje.

## Kriptografski zahtjevi

- KDF: **Argon2id**, individualan kriptografski salt najmanje 16 bajtova; početni cilj `memory=64 MiB, iterations=3, parallelism=1`, uz benchmark na ciljnim Windows računarima; **nikad ispod aktuelnih OWASP minimuma** (danas 19 MiB, 2 iteracije, p=1). Verzija KDF parametara je dio metapodataka.
- AEAD: `AES-256-GCM` uz slučajni 96-bit nonce **bez ponavljanja za isti ključ**, 128-bit auth tag i AAD koji veže `vaultId`, `ownerId`, `providerId`, `recordId`, tip tajne i verziju formata. Tampering uvijek odbijen.
- Generisanje slučajnih vrijednosti isključivo sigurnim CSPRNG. Nema vlastitih kriptografskih algoritama. Provjeriti zavisnost i kompatibilnost/licencu SQLCipher builda za .NET 10/Windows x64.
- `Microsoft.Data.Sqlite` parametar `Password` **nije sam po sebi dokaz da je DB enkriptovana**; mora biti učitan kompatibilan native SQLCipher i to se provjerava testovima (pogrešan ključ i pristup običnim SQLite drajverom).
- Random DB i secret keys su nezavisni; master fraza služi samo za derivaciju ključa kojim su umotani (wrapped), tako da promjena master lozinke **ne zahtijeva re-enkripciju svih korisničkih podataka**.

## Predložene tabele

- `VaultState(Id, SchemaVersion, CryptoVersion, CreatedAtUtc, LastUnlockedAtUtc)` — bez tajni.
- `ProviderCredentials(Id, OwnerId, ProviderId, DisplayName, EncryptedPayload BLOB, Nonce BLOB, Tag BLOB, AlgorithmVersion, CreatedAtUtc, UpdatedAtUtc, IsActive)` — nikad javno ne vraća payload.
- `IntegrationCredentials(Id, OwnerId, ConnectorType, AccountRef, EncryptedPayload BLOB, Nonce BLOB, Tag BLOB, AlgorithmVersion, CreatedAtUtc, UpdatedAtUtc)` — OAuth tokeni i ostale tajne.
- `AuditEvents` — metadata bez ključeva, promptova, privatnog sadržaja, tokena ili sirovih HTTP zaglavlja.

Nazive i tipove potvrditi kroz EF Core migracije; `OwnerId` je eksplicitni identitet vlasnika čak i za single-user instalaciju, zbog budućeg modela pristupa.

## Koraci za Codex

1. Definiši threat model: kopiranje diska, krađa backup-a, drugi lokalni proces, krađa računara, kompromitovan renderer, malware dok je vault otključan; precizno navedi šta dizajn štiti, a šta ne može.
2. Implementiraj `IVaultService`: `Initialize`, `Unlock`, `Lock`, `ChangePassphrase`, `CreateRecoveryKey`, `Recover`, `GetStatus`, `RotateSecret`, `DeleteSecret`. Crypto servis i testovi odvojeni od UI-ja.
3. Pri prvom pokretanju traži master frazu; preporuči jaku frazu, ne skladišti je. Kreiraj DB i SECRET ključeve, bootstrap i **provjereno enkriptovanu** SQLCipher bazu.
4. Implementiraj `ISecretVault`: `Create/Replace/GetForUse/Remove/ListMetadata` uz AES-GCM, AAD i redakciju; frontend dobija samo maskirano ime, status i eventualno posljednja 4 znaka ključa **ako je bezbjedno i potrebno**, nikad kompletan ključ.
5. Recovery: 256-bit slučajni kod prikazan korisniku jednom i dostupan za ručno sigurnosno čuvanje; pomoću njega zasebno omotaj DB i SECRET ključ. Ne čuvaj recovery kod. Bez master fraze **i** recovery koda podaci nijesu povratljivi.
6. Promjena master fraze: uz verifikaciju postojeće ili recovery koda i uz backup/atomsku zamjenu bootstrap metadata; kod postojećeg aktivnog vaulta samo rewrap istih random master ključeva. Implementirati crash-recovery tako da tranzicija ne ostavi korisnika bez pristupa.
7. Sesija: stanje podrazumijevano `Locked`, tokeni/ključevi samo u procesu dok je otvoreno; auto-lock početno 15 min, lock na OS suspend/sleep, eksplicitno `Lock now`. Zatvori SQLCipher konekcije i poništi in-memory pristupe pri zaključavanju; zeroization best effort.
8. Enkriptovani backup/restore baze **zajedno sa bootstrap podacima**, bez plaintext izvoza. Restore se vrši u kontrolisanoj putanji nakon zatvaranja DB-a; zaštita od path traversal i overwrite-a bez potvrde.
9. Izmjene tajni i vaulta transakcione; za promjenu lozinke i recovery wrappera implementirati provjerenu atomsku zamjenu metadata/rezervnu kopiju, testirati prekid procesa usred promjene.
10. Primijeni `I18N_SPEC.md`: `ui.locale` podržava `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk` i `en-US`; `agent.responseLanguage` iste kodove i `follow-user`. Izbor iz Faze 01 uvezi u kanonska šifrovana podešavanja samo ako kanonska vrijednost ne postoji; stari sačuvani `sr-Latn-RS` migriraj u ijekavicu transakciono i idempotentno. Poslije uspješnog upisa atomarno sinhronizuj nesenzitivni `bootstrap.json` koji sadrži UI locale, odvojeno od kriptografskih metapodataka u `vault.bootstrap.json`. Ne unositi AI jezik ili privatne podatke u UI bootstrap.

## Teški rubni slučajevi

- Pogrešna lozinka i oštećena baza daju bezbjednu poruku bez otkrivanja osjetljivih detalja.
- Pokušaj izmjene šifrovanih bajtova, taga, nonce-a ili AAD konteksta mora pasti.
- Isključeno logovanje SQL parametara, EF sensitive logging, memorijski/crash dump sa tajnama. SQLite privremeni/WAL fajlovi i backupi ne smiju izlagati plaintext.
- Na zaključanom vaultu `GetForUse` odbija; kopiranje SQLite fajla na drugo računalo bez master/recovery ključa ne daje API tajne niti istoriju.
- Ako preporučeni SQLCipher native paket nije dostupan/kompatibilan ili pravilno licenciran, **zaustavi ovu fazu i prijavi blocker** umjesto fallback-a na obični SQLite.
- Ne tvrditi da se može zaštititi od malware-a sa privilegijama korisnika dok je vault otključan: to je preostali rizik.

## Kriterijumi prihvatanja

- [ ] Čisti instalacioni tok kreira šifrovanu SQLite DB i enkriptovane provider tajne.
- [ ] Otvaranje baze običnim SQLite drajverom je neuspješno; pogrešna master fraza ne otključava vault.
- [ ] Niti jedan plaintext ključ nije u DB, WAL, `bootstrap.json`, logovima ili backupu.
- [ ] Tajne su nedostupne kada je vault zaključan, uključujući API i zakazane poslove.
- [ ] Recovery code radi nakon gubitka lozinke; bez master/recovery ne postoji backdoor.
- [ ] Testovi pokrivaju tampering, zamjenu AAD, nonce unique, rotation, crash mid-change i backup/restore.
- [ ] UI omogućava korisniku da sam promijeni master frazu i upravlja recovery ključem (najmanje osnovni ekran u ovoj fazi).
- [ ] Vault UI i greške imaju sve tri lokalizacije; sačuvani UI jezik i srpska varijanta rade prije unlock-a i ostaju usklađeni sa kanonskim podešavanjima poslije unlock-a.
- [ ] Testovi prelaza bootstrap → SQLite, migracije starog `sr-Latn-RS` u ijekavicu, ponovljene migracije, neuspješnog upisa, sukoba bootstrap/SQLite vrijednosti i backup/restore potvrđuju očuvanje izbora, zasebno za UI i AI jezik.

**Ne implementirati:** automatsko trajno otključavanje bez lozinke, ključeve u config/env fajlovima, prečicu u vidu šifrovanja fiksnim ključem iz aplikacionog koda.
