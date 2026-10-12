# Odluke i pretpostavke — v1.2

## Potvrđeno korisničkim zahtjevom

- Proizvod je **lični AI agent**, jedan vlasnik po privatnoj instanci; svaka osoba ima svog agenta.
- Za početak radi na vlasnikovom računaru, kasnije isti agent dostupan sa telefona i drugih računara.
- Namijenjen je budućoj prodaji kao usluga/proizvod: sigurnost i samostalno korisničko podešavanje su prioritet.
- Nema vezivanja samo za OpenAI. Svako sam bira AI provajdera i unosi **sopstveni API ključ**.
- **API ključevi treba da budu u SQLite**, ali enkriptovani. Ne oslanjati se obavezno na OS Credential Manager.
- Glavne funkcije: mejlovi, kalendar, dokumenti, obaveze, memorija, automatizacija.
- **Tri lokalizacije od starta: SRB (Ekavica), SRB (Ijekavica) i ENG**, korisnik bira i trajno čuva podrazumijevani jezik i srpsku varijantu, bez ponovnog izbora pri svakom pokretanju.
- **Govorno područje vlasnika: Republika Srpska.** Komunikacija, dokumentacija i lokalizacija SRB (Ijekavica) koriste srpsku latinicu i srpsku ijekavicu u upotrebi u Republici Srpskoj: „nisu”, „nisam”, „sutra”, uz „vrijeme”, „mlijeko”, „sljedeći” i „izvještaj”. Ne koristiti oblike „nijesu”, „nijesam” i „sjutra”. Postojeći kod `sr-Latn-RS-ijekavsk` ostaje isti; ovo je preciziranje jezičkog stila, ne nova lokalizacija niti promjena regionalnog formatiranja.

### Dopuna iz razgovora za BMAD opis proizvoda — 11.10.2026.

- Prvu verziju koristi samo vlasnik; komercijalni pilot i kupci nisu početni cilj.
- Glavni korisnički scenariji su selekcija njemu važnih mejlova, prijedlozi odgovora i podsjećanje na ranije nepročitane poruke i nezavršene zadatke. Vlasnik trenutno radi sve ručno.
- Željeni ishodi su ušteda vremena i isporuka obaveza na vrijeme. Početno mjerenje i brojčani pragovi još nisu potvrđeni.
- Vlasnik želi cijeli definisani MVP u prvoj verziji i isporuku što ranije; konkretan datum nije dogovoren. Faza 10 ostaje kasniji opseg prema mjerodavnom planu.
- Za sada nisu navedeni dodatni budžetski limiti, obavezne integracije ili rizici. Postojeće sigurnosne invarijante ostaju obavezne.
- Detaljan opis i označene pretpostavke: [BMAD opis proizvoda](../bmad/product-brief.md). Ova dopuna ne aktivira implementacionu fazu.

### Dopuna iz razgovora za BMAD PRD — 11.10.2026.

- Vlasnik je izabrao kombinaciju svojih pravila i oznaka sa dodatnom AI preporukom važnosti mejla. Pravila i oznake imaju prednost; preporuka ima razlog i ne skriva ostalu poštu.
- Potvrđen je podesiv dnevni objedinjeni pregled: nepročitani mejlovi stariji od 3 dana i nezavršeni zadaci koji kasne ili dospijevaju u naredna 24 sata. Vrijeme pregleda bira se pri podešavanju.
- Poslije zaključavanja prikazuje se jedan naknadni pregled aktuelno podobnih stavki. Poslovi i dalje rade samo uz aktivan backend i otključan vault.
- [PRD](../bmad/prd.md) razrađuje kriterijume, granice vremena, sljedivost i otvorene odluke. Brojčani ciljevi uspjeha i rok isporuke nisu potvrđeni; ova dopuna ne aktivira implementacionu fazu.

### Dopuna iz razgovora za BMAD UX — 12.10.2026.

- Vlasnik je potvrdio da korisnik bira svijetlu ili tamnu temu i da se izbor pamti za buduća prijavljivanja, bez ponovnog podešavanja.
- `ui.theme = light | dark` je nezavisna trajna postavka. Kanonsko čuvanje i nesenzitivna bootstrap kopija za zaključani ekran razrađeni su u [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md). Svijetla tema za novu instancu bez sačuvanog izbora ostaje UX prijedlog.
- [UX specifikacija](../bmad/ux-design.md) razrađuje ekrane, tokove i planirane provjere. Zahtjev za UX i izbor teme ne aktiviraju implementacionu fazu.

## Projektantske pretpostavke koje omogućavaju odmah početi

| Pitanje | Podrazumijevana odluka za plan | Posljedica |
|---|---|---|
| OS | Windows 11 x64 | Mac/Linux se projektuju za kasnije, ne blokiraju MVP |
| Jezik aplikacije | **`sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk` i `en-US`** kompletno podržani od Faze 01; obje srpske varijante latinicom | Korisnički default i varijanta sačuvani kroz restart i update; sve tri lokalizacije su MVP obaveza |
| Razdvajanje jezika | `ui.locale` i `agent.responseLanguage` su odvojena podešavanja; oba podržavaju tri lokalizacije, AI odgovori dodatno `follow-user` | Promjena UI jezika ili srpske varijante ne mijenja tiho jezik agentovih odgovora |
| Katalozi i formatiranje | Tri zasebna kataloga; srpske varijante koriste `sr-Latn-RS` za `Intl`/backend formatiranje | Varijanta bira prevod i AI preferenciju; ne zahtijeva zasebne regionalne formate |
| Prvi izbor prema OS-u | Eksplicitna podržana varijanta se zadržava; srpski bez varijante predlaže ekavicu; ostali nepodržani jezici `en-US` | Samo prijedlog pri novoj instalaciji; sačuvana postavka uvijek ima prednost |
| Kompatibilnost v1.1 | Ranije sačuvani `sr-Latn-RS` normalizuje se u `sr-Latn-RS-ijekavsk` u UI i AI postavkama | Očuvana prethodna ijekavica; migracija je idempotentna, uz test bootstrap/SQLite/restore putanje |
| Lokalni startup | Samo `ui.locale` i `ui.theme` u nesenzitivnom bootstrap fajlu prije unlock-a; poslije kanonski u enkriptovanoj SQLite | Zaključani ekran prikazuje sačuvani jezik i temu bez pristupa vaultu; ADR-005 proširuje allowlist samo za temu |
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
