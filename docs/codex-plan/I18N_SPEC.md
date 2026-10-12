# I18N_SPEC — Internacionalizacija i korisnički podrazumijevani jezik (v1.2)

> **Obavezno od Faze 01.** Ovaj dokument važi za frontend, backend, sve integracije, obavještenja i budući udaljeni pristup. U slučaju neslaganja između planova, ova specifikacija ima prednost za jezik i lokalizaciju.

## Zahtjevi proizvoda

1. Aplikacija od **prve funkcionalne verzije** podržava **tri ravnopravne lokalizacije**. Podržani BCP 47 identifikatori:
   - `sr-Latn-RS-ekavsk` — **SRB (Ekavica)**, srpski, latinica, ekavica.
   - `sr-Latn-RS-ijekavsk` — **SRB (Ijekavica)**, srpski, latinica, ijekavica u upotrebi u Republici Srpskoj.
   - `en-US` — **ENG**, engleski (SAD).
   Obje srpske varijante koriste dijakritičke znakove (č, ć, š, đ, ž) i imaju zasebne, jezički provjerene kataloge. Ne pretvarati jednu u drugu automatskom zamjenom znakova. `ekavsk` i `ijekavsk` su registrovane varijante u [IANA registru](https://www.iana.org/assignments/lang-subtags-templates).
2. Korisnik u prvom pokretanju bira **podrazumijevani jezik aplikacije (UI locale)** i poslije ga može promijeniti u `Settings > Profile & Language`, bez reinstalacije i bez ponovnog biranja pri svakom pokretanju.
3. Promjena jezika odmah ažurira sve vidljive ekrane, menije, poruke, validacione greške, `aria-label` tekstove, dijaloge za odobravanje, obavještenja i tekstove praznog stanja — bez potrebe za restartom. Ne mijenja automatski sadržaj već postojećih mejlova, dokumenata ili razgovora.
4. Podesiti zasebnu opciju **`agentResponseLanguage`**: `follow-user` | `sr-Latn-RS-ekavsk` | `sr-Latn-RS-ijekavsk` | `en-US`. U početnom podešavanju predložiti izabrani UI jezik i varijantu, ali korisnik može odrediti drugi ili dozvoliti agentu da prati jezik korisničke poruke. Za srpske izbore uputstvo modelu eksplicitno navodi latinicu i ekavicu ili ijekavicu; sam kod bez objašnjenja nije dovoljan. Eksplicitna instrukcija u razgovoru („odgovori na engleskom”, „odgovori ekavicom”) ima prednost nad podrazumijevanim jezikom odgovora. Promjena UI jezika ili srpske varijante **ne smije tiho mijenjati već podešen** jezik AI odgovora.
5. Sačuvati izbor **po privatnoj instanci/vlasniku**; izlazak iz aplikacije, gašenje računara, restart, ažuriranje aplikacije i zaključavanje/otključavanje vaulta ne smiju poništiti postavku.
6. Prvi izbor pri **novoj instalaciji bez sačuvane postavke**: podržani puni kod zadrži; srpski OS sa eksplicitnom varijantom `ijekavsk` predlaže `sr-Latn-RS-ijekavsk`, sa `ekavsk` predlaže `sr-Latn-RS-ekavsk`. Srpski OS bez varijante (npr. `sr`, `sr-RS`, `sr-Latn-RS`, `sr-Cyrl-RS`) predlaže **ekavicu, latinicom**. Engleski OS predlaže `en-US`; drugi nepodržani jezici koriste `en-US`. Ne zaključivati izgovor iz države niti automatski mapirati hrvatski ili bosanski na srpski. Korisnik na onboarding ekranu može izmijeniti prijedlog. Nakon izbora prioritet je trajno sačuvana korisnička postavka, a ne jezik OS-a. Ranije **sačuvani** `sr-Latn-RS` ima drugo značenje i migrira se u ijekavicu, prema pravilima kompatibilnosti ispod.
7. Dodavanje novih jezika mora biti moguće uvođenjem kataloga i registracijom lokalizacije bez izmjena u Agent Core-u ili šemi poslovnih tabela. Srpska ćirilica je **moguća buduća zasebna lokalizacija**, nije obavezna za v1.

## Jezički stil srpske ijekavice

- Vlasnik je iz Republike Srpske. Komunikacija sa vlasnikom, projektna i BMAD dokumentacija, katalog `sr-Latn-RS-ijekavsk`, sistemske poruke i AI odgovori sa izabranom ijekavicom koriste srpsku ijekavicu u upotrebi u Republici Srpskoj.
- Koristiti „nisam”, „nisi”, „nije”, „nismo”, „niste”, „nisu” i „sutra”. Ne koristiti „nijesam”, „nijesi”, „nijesmo”, „nijeste”, „nijesu” ili „sjutra”. Zadržati ijekavske oblike „vrijeme”, „mlijeko”, „sljedeći”, „prije” i „izvještaj”; preciziranje govornog područja ne znači prelazak na ekavicu.
- Uputstvo modelu za `agent.responseLanguage = sr-Latn-RS-ijekavsk` mora eksplicitno navesti: „Odgovaraj na srpskom jeziku, latinicom, ijekavicom u upotrebi u Republici Srpskoj. Koristi nisu, nisam i sutra; zadrži ijekavske oblike poput vrijeme, sljedeći i izvještaj.” Eksplicitni korisnički zahtjev za drugim jezikom ili varijantom i dalje ima prednost.
- Kodovi lokalizacija, nazivi tri jezička izbora, regionalno formatiranje i pravila migracije ostaju isti. Ovo je jezičko preciziranje postojećeg ijekavskog kataloga, a ne dodatna lokalizacija.
- Izvorni korisnički dokumenti, mejlovi i citati zadržavaju originalni tekst; jezičko pravilo se primjenjuje na tekst koji aplikacija ili agent sastavlja.

## Tehnička implementacija

### Frontend / desktop

- Koristi `i18next` + `react-i18next` sa verzionisanim JSON resursima u **tri zasebna direktorijuma**: `src/locales/sr-Latn-RS-ekavsk/{common,settings,agent,errors}.json`, `src/locales/sr-Latn-RS-ijekavsk/...` i `src/locales/en-US/...`. Eksplicitno registruj sva tri puna koda kao podržana; učitavanje resursa ne smije svesti oba srpska kataloga na zajednički `sr` ili `sr-Latn-RS`.
- Svaki korisnički vidljiv string prolazi kroz translation key (`t('namespace:key')`). Zabraniti hardkodiranje UI teksta u komponentama, toasts, validacijama, e-mail draft UI-ju, naslovima tabova i accessibility atributima.
- `en-US` je **fallback za nedostajuće ključeve**. U development/CI režimu nedostajući ključ ili neslaganje kataloga predstavlja grešku. Nikad ne prikazivati sirovi ključ kao zadovoljavajući release rezultat.
- Koristi `Intl.DateTimeFormat`, `Intl.NumberFormat`, `Intl.RelativeTimeFormat` i CLDR-kompatibilnu pluralizaciju. Eksplicitna mapa za formatiranje i pluralna pravila je `sr-Latn-RS-ekavsk` → `sr-Latn-RS`, `sr-Latn-RS-ijekavsk` → `sr-Latn-RS`, `en-US` → `en-US`; isti princip primijeni na backend culture za formatiranje. **Mapa ne mijenja izbor kataloga, sačuvani `ui.locale` niti `agent.responseLanguage`.** Tekst relativnog vremena i nazive mjeseci/dana provjeri u obje srpske varijante; ako runtime vraća tekst druge varijante, koristi jezički provjerene kataloške oblike uz `Intl` numeričko formatiranje i pluralna pravila. Poštuj korisničku vremensku zonu kao odvojenu postavku. UTC/ISO vrijednosti čuvaj u bazi i API ugovorima, bez lokalizovanih stringova.
- Sve lokacije UI-ja (dashboard, chat, vault, e-mail, kalendar, dokumenti, zadaci, memorija, modeli, postavke, sistemske greške) moraju imati sva tri prevoda; ništa ne smije ostati „privremeno samo na jednom jeziku ili jednoj srpskoj varijanti”.
- Backend vraća **stabilne mašinske kodove grešaka** i parametre, npr. `VAULT_LOCKED` + `{}`; frontend prevodi opis. Ako backend generiše tekst za desktop obavještenje, prihvata eksplicitni `locale` i koristi iste semantičke ID-jeve poruka.
- Ne interpolirati ili renderovati korisnički/AI tekst kao HTML u prevodima; escape po defaultu. Placeholderi moraju biti imenovani i validirani u svim lokalizacijama.

### Trajno pamćenje i zaključan vault

- Poslije Faze 02 kanonski zapis u enkriptovanoj SQLite bazi, tabela npr. `UserSettings(OwnerId, Key, ValueJson, UpdatedAtUtc)`, sa `ui.locale = sr-Latn-RS-ekavsk | sr-Latn-RS-ijekavsk | en-US` i `agent.responseLanguage = follow-user | sr-Latn-RS-ekavsk | sr-Latn-RS-ijekavsk | en-US`.
- **Prije otključavanja SQLCipher vaulta**, ekran za otključavanje takođe treba prikazati na izabranom jeziku. Zato u lokalnom, neosjetljivom konfiguracionom fajlu (npr. `bootstrap.json`, izvan Git repozitorijuma i zaštićen aplikacionim direktorijumom) smije postojati **samo** kopija `ui.locale` i neosjetljivi podaci potrebni za pokretanje. Izričita dopuna od 12.10.2026. dozvoljava i `ui.theme = light | dark` prema [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md), radi potvrđenog zahtjeva za pamćenje svijetle/tamne teme. Allowlist sada sadrži te dvije UI postavke; ne dodaje proizvoljna polja. Ne upisivati API ključeve, tokene, lozinke, korisnički profil, istoriju ili `agent.responseLanguage` u taj fajl. Jezik i tema se čuvaju nezavisno i upis jednog ne smije izgubiti drugo.
- Prilikom promjene UI jezika dok je vault otključan: validiraj `locale`, **upis u SQLite mora uspjeti** prije potvrđivanja promjene, potom bezbjedno i atomarno osvježi bootstrap kopiju. Ako kopiranje u bootstrap ne uspije, prijavi i popravi/sinhronizuj pri sljedećem startu. Pri otključavanju kanonska SQLite vrijednost pobjeđuje i ispravlja bootstrap kopiju. Ne dozvoliti proizvoljne putanje ili JSON polja.
- U Fazi 01, prije implementacije vaulta, minimalna lokalna postavka `ui.locale` može se čuvati samo u tom bezbjednom bootstrap fajlu; Faza 02 je migrira u SQLite. Testirati migraciju, backup/restore, novi uređaj i povratak na staru konfiguraciju.
- Nikada ne brisati korisnikov izabrani jezik prilikom nadogradnje verzije ili promjene provajdera. Promjena UI jezika ne mijenja bezbjednosne dozvole.

### Kompatibilnost sa ranijom specifikacijom v1.1

- U v1.1 **sačuvani** `sr-Latn-RS` označavao je ijekavicu. Pri čitanju stare bootstrap konfiguracije, SQLite podešavanja ili backup-a normalizuj ga u `sr-Latn-RS-ijekavsk`. Ovo pravilo važi zasebno za `ui.locale` i `agent.responseLanguage`; `en-US` i `follow-user` ostaju neizmijenjeni. Novi upisi koriste isključivo podržane pune kodove.
- Prije unlock-a stari bootstrap kod prikazuje ijekavski katalog bez pristupa bazi. Poslije unlock-a migriraj stare kanonske vrijednosti transakciono i idempotentno, pa atomarno sinhronizuj bootstrap. Postojeća kanonska vrijednost uvijek ima prednost nad bootstrap kopijom. Ako migracija ili kopiranje ne uspije, sačuvaj prethodne podatke i prijavi lokalizovanu grešku; ne resetuj izbor na ekavicu ili ENG.
- Pri prelasku iz Faze 01 u Fazu 02 uvezi sačuvani izbor iz bootstrap konfiguracije samo ako kanonska postavka još ne postoji. Restore primjenjuje ista pravila i ne prepisuje važeći kanonski izbor sa OS prijedlogom.
- Starija verzija koja poznaje samo `sr-Latn-RS` ne može sačuvati ekavski izbor. Ne raditi tihu povratnu konverziju oba srpska koda u taj kod; pri planiranom povratku na takvu verziju sačuvati backup i eksplicitno prijaviti nepodržanu varijantu. Testirati bezbjedno odbijanje nekompatibilnog povratka gdje je predviđen.

## UX: izbor jezika

- U first-run wizardu, postavkama i na zaključanom ekranu: `Language / Jezik` sa opcijama **SRB (Ekavica)**, **SRB (Ijekavica)** i **ENG (English)**, koje su razumljive nezavisno od trenutno aktivnog jezika. Nazivi opcija takođe prolaze kroz kataloge; oba srpska izbora jasno označiti kao latinicu u pratećem lokalizovanom opisu.
- U `Settings > Profile & Language`: `Application language / Jezik aplikacije`, plus `AI response language / Jezik odgovora agenta`; promjena se odmah vidi, poslije restarta ostaje ista.
- Na zaključanom ekranu jezici su dostupni prije unosa lozinke; promjena na zaključanom ekranu može biti privremena do sljedećeg uspješnog otključavanja kada se potvrđuje kao kanonska postavka, tako da se ne može zaobići pravilo za upis u vault.
- Ne vezivati format datuma, vremena i brojeva za jezik agenta: UI `locale` određuje format, odvojena `timeZone` određuje kalendarsku zonu.

## Codex testovi (automatizovani + ručni)

1. Nova instalacija: engleski OS → `en-US`; srpski OS bez varijante → `sr-Latn-RS-ekavsk`; eksplicitni `ekavsk`/`ijekavsk` → odgovarajući puni srpski kod; drugi jezik → `en-US`. Sačuvani izbor ima prednost nad svim OS vrijednostima.
2. Za **svaku od tri lokalizacije**: izbor, zatvaranje aplikacije, cold start i unlock zadržavaju puni kod i odgovarajući katalog na onboarding-završenoj instanci, **uključujući ekran za otključavanje**.
3. Sve promjene između ENG, SRB (Ekavica) i SRB (Ijekavica) osvježavaju cijeli UI bez restarta i bez gubitka stanja razgovora/zadatka; prebacivanje između srpskih varijanti stvarno mijenja katalog.
4. Za sve kombinacije tri UI lokalizacije i četiri AI opcije: promjena UI jezika ne mijenja `agent.responseLanguage`. Sa UI-jem na engleskom, model dobija eksplicitnu preferenciju srpske latinice i izabrane ekavice/ijekavice; korisnički zahtjev za drugim jezikom ili varijantom ima prednost. Bez nepotrebnog prevoda teksta dokumenta/mejla.
5. Svaki translation key prisutan u **sva tri kataloga**; imenovani placeholderi imaju paritet, a pluralne varijante odgovaraju CLDR pravilima jezika (ne zahtijevati isti skup pluralnih sufiksa za engleski i srpski). Srpski katalozi provjereni i na latinicu, dijakritike i odgovarajuću varijantu; prijaviti hardkodovan UI tekst u review-u.
6. Test datuma, kalendara, brojeva, relativnog vremena i srpskih množina za sve tri lokalizacije; obje srpske varijante koriste regionalno formatiranje `sr-Latn-RS` uz tekst odgovarajuće varijante. Vremenska zona mijenja prikaz, ne sadržaj UTC zapisa.
7. Test grešaka vaulta, poruke neuspješnog API ključa, odobravanja slanja, podsjetnika, onboarding-a i accessibility natpisa u sve tri lokalizacije.
8. Oštećen/nevažeći bootstrap locale bezbjedno pada na `en-US`; ne ruši pokretanje i ne kreira pristup podacima.
9. Stari sačuvani `sr-Latn-RS` u bootstrap/SQLite/backup-u postaje `sr-Latn-RS-ijekavsk`, zasebno za UI i AI jezik. Testirati ponovljenu migraciju, prelaz bootstrap → SQLite, restore, novi uređaj, prekid/grešku upisa i sukob kanonske vrijednosti sa bootstrap kopijom, bez gubitka podataka. `en-US`, `follow-user` i novi puni kodovi ostaju sačuvani.
10. Nevažeći kodovi i proizvoljna bootstrap polja/putanje su odbijeni; puni srpski kodovi se ne skraćuju pri API validaciji, učitavanju kataloga ili upisu postavki. Za predviđeni povratak na staru konfiguraciju dokazati da se nepodržana ekavska varijanta ne konvertuje tiho u ijekavicu.
11. Jezička provjera ijekavskog kataloga i poruka koje aplikacija sastavlja potvrđuje oblike „nisu”, „nisam” i „sutra”, uz ispravne ijekavske oblike poput „vrijeme” i „izvještaj”. Automatizovana provjera uputstva modelu potvrđuje eksplicitnu odrednicu za Republiku Srpsku; ručna provjera AI odgovora potvrđuje isti stil. Oblici navedeni kao nepoželjni u odjeljku o jezičkom stilu ne smiju se pojavljivati u tekstu koji aplikacija sastavlja; izvorni citati i korisnički sadržaj izuzeti su od te provjere.

## Definition of Done

- [ ] SRB (Ekavica), SRB (Ijekavica) i ENG imaju potpunu pokrivenost svih MVP ekrana i sistemskih korisničkih poruka, u tri zasebna kataloga.
- [ ] Ijekavski tekstovi i uputstvo za AI odgovore prate srpsku ijekavicu u upotrebi u Republici Srpskoj, sa oblicima „nisu”, „nisam” i „sutra”.
- [ ] Izabrani default jezik i srpska varijanta se pamte između pokretanja, ažuriranja i zaključavanja.
- [ ] Ekran za otključavanje radi na sačuvanom jeziku bez otključavanja šifrovane baze.
- [ ] Jezik UI-ja i jezik AI odgovora su odvojene, jasno objašnjene postavke.
- [ ] Podaci i API ugovori ostaju jezički neutralni, sa normalizovanim `locale` kodovima.
- [ ] Stari sačuvani `sr-Latn-RS` migrira se u ijekavicu bez gubitka izbora; bootstrap, SQLite i restore ostaju usklađeni.
- [ ] Formatiranje i pluralizacija rade za sve tri lokalizacije, bez gubitka srpske varijante u postavkama ili tekstu.
- [ ] CI provjera sva tri lokalizacijska kataloga i E2E test svake lokalizacije prolaze.
