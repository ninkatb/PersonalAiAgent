# Codex pravila — Personal AI Agent

## Jezik komunikacije i BMAD

- Sva komunikacija sa vlasnikom (pitanja, planovi, izvještaji i sažeci) vodi se na srpskom jeziku, latinicom i srpskom ijekavicom u upotrebi u Republici Srpskoj, osim kada vlasnik izričito zatraži drugi jezik ili varijantu.
- Vlasnik je iz Republike Srpske. Koristi oblike „nisam”, „nisi”, „nije”, „nismo”, „niste”, „nisu” i „sutra”; ne koristi „nijesam”, „nijesi”, „nijesmo”, „nijeste”, „nijesu” ili „sjutra”. Ijekavski oblici poput „vrijeme”, „mlijeko”, „sljedeći”, „prije” i „izvještaj” ostaju obavezni. Ovo važi i za dokumentaciju, BMAD dokumente, ijekavski UI katalog i uputstva za AI odgovore.
- BMAD vještine su dostupne kroz Codex; projektna konfiguracija je u `bmad/project.yaml`, a evidencija rada u `bmad/workflow-status.yaml` i `bmad/sprint-status.yaml`.
- Za BMAD komunikaciju i dokumente poštuj `language.communication_language` i `language.document_output_language`, uključujući odrednicu za Republiku Srpsku. Kod, identifikatori, API ugovori i mašinske vrijednosti ostaju na engleskom.
- Postojeći `docs/codex-plan/` je mjerodavan razvojni plan. BMAD faze organizuju rad i ne predstavljaju odobrenje za aktiviranje ili prelazak između implementacionih faza tog plana.

## Obavezno prije rada

1. Pročitaj `docs/codex-plan/00_START_HERE.md`, `DECISIONS.md`, **`I18N_SPEC.md`** i **samo aktivni** fajl faze. Ako putanja nije takva, potraži ih u trenutnom direktorijumu projekta.
2. Pregledaj repozitorijum, trenutno stanje i testove. Nikad ne briši korisničke podatke radi pojednostavljenja.
3. U kratkim crtama prikaži plan izmjena (komponente, migracije, rizici). Zatim implementiraj traženo bez nepotrebnog čekanja na odobrenje za rutinske promjene.
4. Ako je kritičan izbor za bezbjednost nejasan, koristi konzervativnu opciju ili **zaustavi rizični dio** i eksplicitno prijavi blokadu. Nikad ne implementiraj nesiguran fallback.

## Standardi implementacije

- Kod, identifikatori i API ugovori: **engleski**. UI od prvog builda podržava **tri lokalizacije: SRB (Ekavica) — `sr-Latn-RS-ekavsk`, SRB (Ijekavica) — `sr-Latn-RS-ijekavsk` i ENG — `en-US`**. Obje srpske varijante su latinicom, sa dijakritičkim znakovima (č, ć, š, đ, ž), u zasebnim katalozima; ne generisati jednu iz druge automatskom zamjenom znakova. Svi UI stringovi idu kroz kataloge; nema hardkodovanih tekstova. Dokumentacija za vlasnika i ijekavski katalog: srpski, latinica, ijekavica u upotrebi u Republici Srpskoj. Poštuj `I18N_SPEC.md`.
- Koristi .NET 10, TypeScript `strict`, provjerene biblioteke i zaključane verzije zavisnosti; provjeri aktuelnu podršku i licencu prije izbora paketa.
- Bez direktnog pristupa SQLite, ključevima ili provajder SDK-ovima iz React/Tauri renderera.
- Nikad ne stavljaj pravi API ključ, token, lozinku, sadržaj privatnog mejla ili dokumenta u test fixture, Git, konzolu, crash dump ili telemetry.
- Podrazumijevana vrijednost dozvola je odbijanje; izvođenje akcija isključivo iz registrovanih, validiranih alata. Vanjski tekst se smatra nepouzdanim.
- Spremi migracije bez gubitka podataka i testiraj putanju unaprijed/unazad gdje je predviđena; prije destruktivne migracije traži backup/potvrdu.
- Razdvoji interfejse za modele, konektore, skladište, izvršavanje alata, vault i autentifikaciju.
- **Korisnikov default UI jezik i izabrana srpska varijanta se pamte** između pokretanja; odvoji jezik interfejsa od jezika AI odgovora. Obje postavke podržavaju sve tri lokalizacije, a jezik AI odgovora dodatno `follow-user`. Locale na zaključanom ekranu se čita iz minimalne nesenzitivne bootstrap konfiguracije, a pri otključavanju se sinhronizuje sa kanonskim šifrovanim SQLite podešavanjima. Stari sačuvani `sr-Latn-RS` migrira se u `sr-Latn-RS-ijekavsk`, bez gubitka korisnikovog izbora.
- Pri svakom novom korisnički vidljivom tekstu dodaj **sva tri** prevoda, formatiranje datuma/brojeva i test pariteta sva tri kataloga (ključevi, placeholderi i pluralizacija). Za obje srpske varijante formatiranje koristi `sr-Latn-RS`, a izbor kataloga i sačuvana postavka zadržavaju puni kod varijante.
- Nema uvezivanja direktnog pristupa internetu za ASP.NET API: **samo 127.0.0.1 sa nasumičnim portom**, zaštita sesijskim tokenom i Origin/CORS politikom. Nema javnih endpointa u MVP-u.
- Sigurnosno kritičan kod pokrij unit i integracionim testovima sa negativnim scenarijima; ne preskači provjere zato što je „lokalna aplikacija”.

## Za svaku fazu dostavi

- Sažetak izmijenjenih fajlova i arhitektonskih odluka.
- Komande za build/test i stvarni rezultat, bez tvrdnji o testovima koji nisu pokrenuti.
- Dokaze ispunjenja svakog acceptance kriterijuma iz aktivne faze.
- Poznate rizike, dugove i precizan sljedeći korak.
- Ažuriran `README.md` i `docs/architecture/` kada se arhitektura promijeni.

**Ne prelazi na narednu fazu dok vlasnik to eksplicitno ne zatraži.**
