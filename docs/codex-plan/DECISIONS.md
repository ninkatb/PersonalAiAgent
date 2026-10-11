# Odluke i pretpostavke — v1.2

## Potvrđeno korisničkim zahtjevom

- Proizvod je **lični AI agent**, jedan vlasnik po privatnoj instanci; svaka osoba ima svog agenta.
- Za početak radi na vlasnikovom računaru, kasnije isti agent dostupan sa telefona i drugih računara.
- Namijenjen je budućoj prodaji kao usluga/proizvod: sigurnost i samostalno korisničko podešavanje su prioritet.
- Nema vezivanja samo za OpenAI. Svako sam bira AI provajdera i unosi **sopstveni API ključ**.
- **API ključevi treba da budu u SQLite**, ali enkriptovani. Ne oslanjati se obavezno na OS Credential Manager.
- Glavne funkcije: mejlovi, kalendar, dokumenti, obaveze, memorija, automatizacija.
- **Tri lokalizacije od starta: SRB (Ekavica), SRB (Ijekavica) i ENG**, korisnik bira i trajno čuva podrazumijevani jezik i srpsku varijantu, bez ponovnog izbora pri svakom pokretanju.

## Projektantske pretpostavke koje omogućavaju odmah početi

| Pitanje | Podrazumijevana odluka za plan | Posljedica |
|---|---|---|
| OS | Windows 11 x64 | Mac/Linux se projektuju za kasnije, ne blokiraju MVP |
| Jezik aplikacije | **`sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk` i `en-US`** kompletno podržani od Faze 01; obje srpske varijante latinicom | Korisnički default i varijanta sačuvani kroz restart i update; sve tri lokalizacije su MVP obaveza |
| Razdvajanje jezika | `ui.locale` i `agent.responseLanguage` su odvojena podešavanja; oba podržavaju tri lokalizacije, AI odgovori dodatno `follow-user` | Promjena UI jezika ili srpske varijante ne mijenja tiho jezik agentovih odgovora |
| Katalozi i formatiranje | Tri zasebna kataloga; srpske varijante koriste `sr-Latn-RS` za `Intl`/backend formatiranje | Varijanta bira prevod i AI preferenciju; ne zahtijeva zasebne regionalne formate |
| Prvi izbor prema OS-u | Eksplicitna podržana varijanta se zadržava; srpski bez varijante predlaže ekavicu; ostali nepodržani jezici `en-US` | Samo prijedlog pri novoj instalaciji; sačuvana postavka uvijek ima prednost |
| Kompatibilnost v1.1 | Ranije sačuvani `sr-Latn-RS` normalizuje se u `sr-Latn-RS-ijekavsk` u UI i AI postavkama | Očuvana prethodna ijekavica; migracija je idempotentna, uz test bootstrap/SQLite/restore putanje |
| Lokalni startup | Samo `ui.locale` u nesenzitivnom bootstrap fajlu prije unlock-a; poslije kanonski u enkriptovanoj SQLite | Zaključani ekran prikazuje izabrani jezik bez pristupa vaultu |
| Desktop stack | Tauri 2 + React/TypeScript + .NET 10 sidecar | Izolovati lokalni transport |
| DB | SQLite preko **SQLCipher** za cjelokupnu bazu + AES-GCM tajne u kolonama | Zahtijeva provjeren native SQLCipher build |
| Unlock | Korisnička master lozinka / fraza, Argon2id | Bez obavezne zavisnosti od OS keychaina |
| Recovery | Jednokratni recovery code korisnika | Bez recovery koda i master lozinke nema dekripcije |
| Session lock | Početno zaključano; automatsko zaključavanje nakon 15 min neaktivnosti i na sleep | Tajni pozivi i poslovi čekaju otključavanje |
| Provajderi | OpenAI, Anthropic, Gemini, Ollama; generički kompatibilni endpoint uz eksplicitno povjerenje | Primarni i rezervni model konfigurišu se u UI-ju |
| Pošta | Microsoft 365 prvi konektor | Gmail kao naredni adapter, bez promjene core-a |
| Microsoft OAuth | Public client + PKCE, delegirana prava | Korisnik daje saglasnost; ne unosi svoj Microsoft aplikacioni secret |
| Slanje/brisanje | Eksplicitno odobrenje korisnika | Nema samostalnih destruktivnih akcija |
| Automatizacija | Samo dok je lokalni backend aktivan **i vault otključan** | Nema neprimjetnog čuvanja dekripcionog ključa na disku |
| Cloud/telefon | Posebna faza, bez javnog lokalnog API-ja | Nema preuranjenog izlaganja podataka |

## Za korisničku odluku prije komercijalnog pilota (ne blokira Fazu 1)

1. Samo Windows ili instalacije i za macOS/Linux?
2. Da li je cilj i fizičkim licima sa privatnim Microsoft nalogom i poslovnim Microsoft 365 tenantima? Potrebno testirati oba i saglasnosti administratora gdje treba.
3. Da li je **master lozinka** prihvatljiv korak pri svakom otključavanju? Bez OS keychaina ili udaljenog čuvara ključa, to je uslov za sigurnu upotrebu API ključeva.
4. Da li želiš isključivo potpuno lokalnu obradu memorije ili korisnik smije izabrati da se relevantan sadržaj šalje cloud AI modelu (predviđeno uz vidljivu kontrolu)?
5. Licenciranje: jednokratna instalacija, pretplata ili uslužni hosting — odlučuje se nakon MVP-a.

Ako korisnik odgovori drugačije, ažurirati ovaj fajl i pogođene faze prije razvoja; **ne mijenjati uspostavljene sigurnosne invarijante bez revizije**.
