# Zahtjevi proizvoda — Personal AI Agent

- **Datum:** 11.10.2026. (`2026-10-11`)
- **Verzija:** 1.1; dopuna UX zahtjeva za temu 12.10.2026.
- **Nivo projekta:** 3
- **Autor:** Codex, BMAD upravljanje proizvodom, na osnovu odgovora vlasnika i postojećeg plana.
- **Status:** Izrađen PRD; razvoj, testovi proizvoda i lični pilot nisu sprovedeni.
- **Opseg:** Cijeli MVP iz razvojnog plana 01–09; faza 10 je kasniji opseg.
- **Aktivna implementaciona faza:** Nije određena. Ovaj dokument ne aktivira nijednu fazu.

## 1. Sažetak <!-- Executive Summary -->

Personal AI Agent je privatna Windows desktop aplikacija za jednog vlasnika po instalaciji. Objedinjuje razgovore, Microsoft 365 poštu, kalendar, dokumente, obaveze, memoriju i automatizacije. Vlasnik sam povezuje naloge i bira AI provajdera sa sopstvenim ključem ili lokalni model.

Prvi korisnik je vlasnik projekta, koji trenutno ručno bira važne mejlove, priprema odgovore i prati nepročitanu poštu i nezavršene zadatke. Željeni ishodi su ušteda vremena i završavanje obaveza na vrijeme. Nema dogovorenog datuma isporuke, izmjerenog početnog stanja ili potvrđenih brojčanih ciljeva pilota.

Vlasnik je u ovom PRD razgovoru potvrdio dva operativna izbora: njegova pravila i oznake imaju prednost nad AI preporukom važnosti; početni podsjetnici su podesiv dnevni objedinjeni pregled nepročitanih poruka starijih od tri dana i nezavršenih zadataka koji kasne ili dospijevaju u naredna 24 sata. Vrijeme pregleda bira pri podešavanju, a nakon zaključavanja dobija jedan naknadni pregled.

Obavezni uslovi su šifrovano čuvanje, zaključan vault na pokretanju, kontrolisano slanje podataka modelu, autorizacija vanjskih promjena, tri potpune lokalizacije i provjerljiv backup/restore. Proizvod se ne smatra završenim samo zato što AI vraća odgovor.

## 2. Pregled projekta i izvori <!-- Project Overview -->

### 2.1. Hijerarhija i granice dokumenta

Mjerodavni razvojni plan je [00_START_HERE.md](../codex-plan/00_START_HERE.md), uz [DECISIONS.md](../codex-plan/DECISIONS.md) i [I18N_SPEC.md](../codex-plan/I18N_SPEC.md). Lokalizacijska specifikacija ima prednost za jezička pitanja. [AGENTS.md](../../AGENTS.md) uređuje rad, a [opis proizvoda](product-brief.md) prenosi prethodni razgovor sa vlasnikom.

Ovaj PRD razrađuje proizvod na osnovu tih izvora i novih odgovora. Radi poštovanja pravila „samo aktivni fajl faze”, neaktivni detaljni fajlovi faza nisu učitavani. Mapiranje faza ispod potiče iz zajedničkog početnog plana; nije potvrda potpunog prenosa njihovih pojedinačnih acceptance kriterijuma. Prije implementacije svake izričito aktivirane faze treba pročitati njen fajl, povezati dodatne kriterijume i prijaviti eventualno neslaganje.

Repozitorijum trenutno ima dokumentaciju i BMAD evidenciju, bez aplikacionog koda, build konfiguracije, zavisnosti i automatizovanih testova. Svi zahtjevi, scenariji `TC-*` i priče u ovom PRD-u predstavljaju planirani rad. Nema tvrdnje da je funkcija implementirana ili da je test prošao.

### 2.2. Ciljevi i korisnik

| ID | Cilj | Obrazloženje |
|---|---|---|
| GOAL-001 | Smanjiti aktivno vrijeme pregleda pošte i pripreme odgovora | Danas vlasnik sve radi ručno; računati i provjeru i ispravljanje AI prijedloga |
| GOAL-002 | Završavati obaveze na vrijeme | Pregledati stare nepročitane poruke, rokove i potvrđeno nezavršene zadatke |
| GOAL-003 | Sačuvati kontrolu i privatnost | Vlasnik određuje naloge, modele, podatke, dozvole i vanjske akcije |
| GOAL-004 | Isporučiti cijeli upotrebljivi MVP | Sve tri lokalizacije, stabilan desktop rad i obnova podataka su dio prve verzije |

**Primarna persona:** Vlasnik privatne instalacije koji pregleda poštu, priprema odgovore i planira obaveze. Broj naloga, dnevni obim poruka, tehnička vještina i raspoloživo vrijeme nisu izmjereni. Ne uvodi se izmišljena sekundarna persona; komercijalni kupci dolaze kasnije.

**Glavni zadatak:** Kada pregledam informacije i obaveze, želim da vidim šta traži pažnju, provjerim izvor i pripremim naredni korak, da bih trošio manje vremena i poštovao rokove.

### 2.3. Prioriteti

Koristi se MoSCoW: `MUST` je obavezno za prvi MVP, `SHOULD` je preporučeno, `COULD` je opciono, `WONT` je van prve verzije. Sigurnosne zaštite i već potvrđene MVP mogućnosti imaju `MUST`; njihove prioritete nije dozvoljeno sniziti radi ranijeg izlaska. Visok broj obaveznih zahtjeva odražava izričit zahtjev vlasnika za cijeli postojeći MVP.

Redoslijed implementacije određuju faze 01–09 i njihove zavisnosti. Brojčani ciljevi pilota i radni pragovi performansi označeni su kao prijedlozi, bez obećanja rezultata ili roka. Dodatni izvještaji i optimizacije mogu čekati; pristup punoj pošti, pregled akcija, zaštita podataka i lokalizacija ne mogu.

## 3. Funkcionalni zahtjevi <!-- Functional Requirements -->

Svaki zahtjev važi za sve tri lokalizacije gdje postoji korisnički vidljiv tekst. Kriterijumi su budući uslovi provjere. Oznake `FR-*`, `EPIC-*` i `TC-*` su tehnički identifikatori.

### FR-001: Instalacija, pokretanje i početno podešavanje [MUST]

**Zahtjev:** Vlasnik mora moći pokrenuti privatnu Windows instancu, izabrati jezik i završiti početno podešavanje bez ručnog uređivanja konfiguracije.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Na čistom Windows 11 x64 uređaju instalacija otvara desktop interfejs i njegov lokalni backend; greška pokretanja prikazuje lokalizovano objašnjenje i mogućnost ponovnog pokušaja.
- Nova instanca traži uspostavljanje vaulta prije tajnih operacija; postojeća instanca počinje zaključana i čuva ranija podešavanja.
- Korisnik bira jednu od tri lokalizacije; napuštanje početnog podešavanja ne briše postojeće podatke.

**Veza:** EPIC-001; GOAL-004; faze 01, 08, 09; TC-001.

### FR-002: Uspostavljanje i zaključavanje vaulta [MUST]

**Zahtjev:** Vlasnik mora moći uspostaviti master lozinku, otključati i zaključati šifrovano skladište.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Ispravna master lozinka otključava bazu; pogrešna ne otkriva podatke i vraća lokalizovanu grešku bez tajni.
- Ručno zaključavanje, sleep i 15 minuta neaktivnosti zaključavaju vault; backend odbija nove tajne pozive i obustavlja pristup tajnama u aktivnim operacijama.
- Ključ za dekripciju ne čuva se na disku radi automatskog rada; promjena lozinke čuva podatke i provjerava pristup novom lozinkom.

**Veza:** EPIC-002; GOAL-003; faza 02; TC-002.

### FR-003: Recovery bez zaobilaženja zaštite [MUST]

**Zahtjev:** Vlasnik mora dobiti jednokratni recovery code i moći obnoviti pristup bez obaveznog OS keychaina.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Recovery code se prikazuje u kontrolisanom toku uz potvrdu čuvanja; nije u logovima, telemetry ili UI bootstrap datoteci.
- Važeći kod omogućava postavljanje nove master lozinke uz očuvanje podataka; iskorišćeni kod više ne može ponovo otključati vault, a zamjenski se izdaje kroz kontrolisan tok.
- Bez master lozinke i recovery koda nema resetovanja koje dekriptuje postojeće podatke; poruka objašnjava granicu i ne briše bazu.

**Veza:** EPIC-002; GOAL-003; faza 02; TC-003.

### FR-004: Šifrovani backup i bezbjedni restore [MUST]

**Zahtjev:** Vlasnik mora moći napraviti i obnoviti šifrovani backup privatne instance.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Backup ne sadrži čitljive tajne ili privatni sadržaj; restore sa odgovarajućim pristupnim materijalom vraća podatke i podešavanja na testnoj novoj instanci.
- Pogrešna lozinka, oštećen ili nepodržan backup odbija se bez prepisivanja važeće baze; zamjena postojeće baze zahtijeva backup i potvrdu prije destruktivnog koraka.
- Restore čuva locale i AI jezik, primjenjuje kompatibilnu migraciju i počinje zaključanim vaultom; dozvole i zakazani poslovi se preispituju prije prvog izvršavanja na novoj instanci.

**Veza:** EPIC-002; GOAL-003, GOAL-004; faze 02, 09; TC-004.

### FR-005: Više AI provajdera i BYOK [MUST]

**Zahtjev:** Vlasnik mora moći podesiti OpenAI, Anthropic, Gemini, Ollama i eksplicitno odobren kompatibilni endpoint, odabrati model i testirati vezu kroz zajednički ugovor.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Svaki cloud provajder koristi vlasnikov ključ iz vaulta; Ollama radi bez obaveznog API ključa. Test veze daje rezultat i ograničenja sposobnosti modela.
- Tajna je maskirana u interfejsu; renderer ne prima sačuvani ključ. Zamjena ili uklanjanje provajdera ne briše istoriju razgovora.
- Nepostojeći model, nevažeći ključ i nedostupan endpoint daju stabilan kod greške, bez lažne potvrde veze i bez zapisa tajni.

**Veza:** EPIC-003; GOAL-003; faza 03; TC-005.

### FR-006: Povjerenje endpointu i rezervni model [MUST]

**Zahtjev:** Vlasnik mora moći izabrati primarni i rezervni model, uz kontrolu odredišta i podataka.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Novi kompatibilni endpoint i promjena odredišta zahtijevaju eksplicitno povjerenje i validaciju backend politike; nevažeće odredište se odbija.
- Prelazak na rezervni model vidljiv je u razgovoru i dozvoljen samo ako već odobrena politika obuhvata tog provajdera i taj sadržaj; inače zahtijeva odobrenje.
- Pad primarnog provajdera ne ponavlja automatski vanjsku promjenu i ne šalje ključ drugom provajderu.

**Veza:** EPIC-003; GOAL-003; faza 03; TC-006.

### FR-007: Razgovor i istorija [MUST]

**Zahtjev:** Vlasnik mora moći voditi razgovore sa izabranim modelom, sačuvati istoriju i prekinuti generisanje.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Poslije restarta i otključavanja prethodni razgovor i izabrani model ostaju dostupni; istorija je u šifrovanom skladištu.
- Prekid i greška označavaju nepotpun odgovor; korisnik razlikuje prijedlog alata, odobrenu akciju i stvarni rezultat izvršavanja.
- Model dobija nezavisno podešen jezik odgovora; eksplicitni zahtjev vlasnika u razgovoru ima prednost, a vanjski citirani tekst ne mijenja tu postavku.

**Veza:** EPIC-004; GOAL-001, GOAL-003; faza 04; TC-007.

### FR-008: Registrovani alati i dozvole [MUST]

**Zahtjev:** Svaka akcija agenta mora proći kroz registrovan, validiran alat i važeću korisničku dozvolu.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Nepoznat alat, nevažeći argumenti, nedostatak dozvole i zaključan vault odbijaju se prije poziva konektora; početna dozvola je odbijanje.
- Vlasnik vidi svrhu i opseg pristupa i može opozvati dozvolu; opoziv se ponovo provjerava neposredno prije izvršavanja.
- Poruka, dokument ili odgovor modela ne mogu dodati alat, odobriti dozvolu, izvršiti proizvoljnu komandu ili otvoriti direktni SQL pristup.

**Veza:** EPIC-004; GOAL-003; faza 04; TC-008.

### FR-009: Pregled i odobravanje vanjskih promjena [MUST]

**Zahtjev:** Prije vanjske promjene vlasnik mora dobiti konkretan pregled akcije i dati propisanu autorizaciju; slanje i brisanje uvijek traže izričito odobrenje.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Pregled sadrži nalog, alat, cilj, primaoce kada postoje, konačni sadržaj i posljedicu; odbijanje ili zatvaranje dijaloga ne izvršava akciju.
- Odobrenje je vezano za prikazane argumente; promjena primaoca, sadržaja ili cilja poništava ranije odobrenje. Opoziv dozvole i zaključavanje prije izvršavanja zaustavljaju akciju.
- Ponovljen zahtjev i timeout ne izazivaju slijepo ponovno slanje ili brisanje; nepoznat ishod ostaje vidljiv do provjere.

**Veza:** EPIC-004; GOAL-003; faze 04, 05, 07; TC-009.

### FR-010: Povezivanje Microsoft 365 naloga [MUST]

**Zahtjev:** Vlasnik mora moći povezati i opozvati Microsoft nalog preko public client OAuth toka sa PKCE i delegiranim pravima.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Korisnik vidi tražena prava i daje saglasnost; ne unosi Microsoft aplikacioni secret, a tokeni se čuvaju u vaultu.
- Odbijena saglasnost i opozvana ili istekla sesija imaju vidljivo stanje i bezbjedan ponovni login; nema širih prava kao fallback-a.
- Uklanjanje veze zaustavlja pristup i povezane poslove; UI ne predstavlja zastarjele lokalne podatke kao nov uspješan sync.

**Veza:** EPIC-005; GOAL-001, GOAL-003; faza 05; TC-010.

### FR-011: Pregled, pretraga i izvor pošte [MUST]

**Zahtjev:** Vlasnik mora moći pronaći i pregledati dostupnu poštu povezanog naloga i original poruke.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Pretraga i listanje podržavaju straničenje i stabilnu vezu sa izvornom porukom; prikazuju nalog i vrijeme posljednjeg uspješnog osvježavanja.
- Pregled preporučene pošte uvijek omogućava prelazak na punu dostupnu poštu; nedostupan izvor i nepotpun sync jasno su označeni.
- Selekcija važnosti i čitanje za potrebe agenta ne brišu niti automatski mijenjaju status poruke; eventualna promjena prolazi kroz alat i dozvole.

**Veza:** EPIC-005; GOAL-001; faza 05; TC-011.

### FR-012: Važnost pošte prema pravilima vlasnika [MUST]

**Zahtjev:** Vlasnik mora moći postaviti pravila i oznake važnosti, uz dodatnu AI preporuku i objašnjenje.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Podržani su eksplicitna oznaka važno/nevažno i podesiva pravila po pošiljaocu, domenu i temi. Redoslijed je ručna oznaka, zatim korisnički poredana pravila, zatim AI preporuka; prvo primjenjivo pravilo određuje rezultat.
- AI preporuka ima razlog i vezu na izvor, ne prepisuje oznaku ili pravilo i može se isključiti. Nepouzdana ili nedostupna procjena ostaje označena kao neprocijenjena, uz pristup originalu.
- Izmjena pravila ponovo procjenjuje pregled bez uklanjanja poruka iz pune pošte; sadržaj mejla ne može sam dodati pravilo. Vlasnik može ispraviti klasifikaciju.

**Veza:** EPIC-005; GOAL-001, GOAL-002; faza 05; TC-012. Kombinovani pristup je potvrđen; konkretna polja pravila i razrješenje sukoba su projektantska razrada za provjeru u UX-u.

### FR-013: Prijedlog odgovora sa provjerljivim kontekstom [MUST]

**Zahtjev:** Vlasnik mora moći dobiti i urediti nacrt odgovora na izabrani mejl.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Prije cloud obrade vidljivi su provajder i odabrani kontekst prema važećoj politici slanja podataka; nepovezane poruke ili dokumenti ne dodaju se samovoljno.
- Nacrt prikazuje original ili vezu na njega, omogućava uređivanje i označava nepoznate podatke umjesto izmišljanja rokova, činjenica ili priloga.
- Generisanje nacrta ne šalje poruku. Nacrt se čuva lokalno šifrovano; zapis vanjskog nacrta, ako se koristi, predstavlja posebnu autorizovanu promjenu.

**Veza:** EPIC-005; GOAL-001, GOAL-003; faza 05; TC-013.

### FR-014: Slanje i brisanje pošte [MUST]

**Zahtjev:** Vlasnik mora moći odobriti slanje pregledanog nacrta ili brisanje precizno odabrane poruke.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Prije slanja prikazuju se nalog, To/CC/BCC, predmet, tekst i prilozi; prije brisanja prikazuje se konkretna poruka i posljedica.
- Bez novog izričitog odobrenja nema slanja ili brisanja, uključujući automatizacije; promijenjen nacrt zahtijeva novi pregled.
- Potvrđen rezultat konektora razlikuje se od zahtjeva koji je samo poslat; nepoznat ishod ne prikazuje se kao uspjeh i ne pokreće automatski duplikat.

**Veza:** EPIC-005; GOAL-001, GOAL-003; faza 05; TC-014.

### FR-015: Kalendar i vremenska zona [MUST]

**Zahtjev:** Vlasnik mora moći pregledati povezani kalendar i odobriti predložene promjene događaja.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Prikaz dana i događaja poštuje korisničku zonu, locale i izvorni status; prelaz na ljetnje/zimsko vrijeme i cjelodnevni događaji imaju provjerene scenarije.
- Predloženo kreiranje, promjena ili otkazivanje prikazuje kalendar, vrijeme, učesnike i posljedice prije autorizacije.
- Nedostupnost ili sukob izmjene ne proizvodi lažno potvrđen događaj niti tiho prepisuje noviju verziju izvora.

**Veza:** EPIC-005; GOAL-002; faza 05; TC-015.

### FR-016: Microsoft To Do i pouzdan status [MUST]

**Zahtjev:** Vlasnik mora moći pregledati i kroz autorizovane alate mijenjati podržane To Do zadatke.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Izvorni identitet, status i rok zadatka ostaju vezani za povezani nalog; osvježavanje i greške vidljivi su korisniku.
- Kreiranje, promjena roka i završavanje su vanjske promjene i prolaze FR-008/FR-009; AI tvrdnja sama ne završava zadatak.
- Noviji izvorni status ima prednost nad zastarjelom kopijom; završen ili otkazan zadatak ne ostaje u narednom potvrđenom pregledu nezavršenog rada.

**Veza:** EPIC-005; GOAL-002; faza 05; TC-016.

### FR-017: OneDrive kao izvor dokumenata [MUST]

**Zahtjev:** Vlasnik mora moći pronaći i odabrati dozvoljen OneDrive dokument kao izvor za pretragu ili razgovor.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Listanje i preuzimanje poštuju delegirana prava i eksplicitno odabran opseg; ne indeksira se cijeli nalog bez saglasnosti.
- Prikazuju se identitet, porijeklo i verzija izvora; opozvan pristup ili uklonjen dokument označava se kao nedostupan.
- Nedozvoljen fajl, greška preuzimanja i prekoračenje podržanog limita daju lokalizovanu grešku bez izvršavanja sadržaja dokumenta.

**Veza:** EPIC-005; GOAL-001, GOAL-003; faza 05; TC-017.

### FR-018: Uvoz i upravljanje dokumentima [MUST]

**Zahtjev:** Vlasnik mora moći odabrati lokalne dokumente, učitati ih u kontrolisanom opsegu i pratiti obradu.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- UI unaprijed navodi podržane formate i veličine; svaki uvoz ima stanje čekanja, obrade, uspjeha ili greške. Tačna matrica formata i limita utvrđuje se iz aktivne faze 06.
- Duplikat i izmijenjena verzija imaju determinističan tretman i porijeklo; neuspio uvoz ne kvari ranije učitane dokumente.
- Korisnik može ukloniti dokument i izvedene indekse; uvoz ne izvršava ugrađene skripte, makroe ili instrukcije.

**Veza:** EPIC-006; GOAL-001, GOAL-003; faza 06; TC-018.

### FR-019: Pretraga dokumenata i odgovori sa izvorima [MUST]

**Zahtjev:** Vlasnik mora moći pretražiti dozvoljene dokumente i dobiti odgovor koji pokazuje korišćene izvore.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Rezultat i odgovor prikazuju dokument, verziju i lociranje korišćenog odlomka gdje format to podržava; odsustvo dokaza jasno se navodi.
- Uklonjen ili nedozvoljen dokument ne ulazi u novu pretragu; zastarjela referenca nije predstavljena kao aktuelan dokaz.
- Ako embeddings ili odgovor koriste cloud provajdera, isti izbor i kontrola slanja važe prije prenosa teksta. Nedostupan model ne širi opseg pristupa.

**Veza:** EPIC-006; GOAL-001, GOAL-003; faza 06; TC-019.

### FR-020: Kontrolisana memorija [MUST]

**Zahtjev:** Vlasnik mora moći uključiti, pregledati, ispraviti i izbrisati trajnu memoriju agenta.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- UI razlikuje memorijski prijedlog od sačuvane stavke, prikazuje porijeklo i omogućava vlasniku odobravanje prema izabranoj politici.
- Isključena memorija ne prima nove trajne stavke; brisanje uklanja stavku iz budućih upita i izvedenih indeksa, uz objašnjenje da postojeći offline backup ne može biti retroaktivno izmijenjen.
- API ključevi, tokeni i lozinke nisu memorijske činjenice; vanjski sadržaj ne može promijeniti dozvole ili pretvoriti se u sistemsku instrukciju.

**Veza:** EPIC-006; GOAL-001, GOAL-003; faza 06; TC-020.

### FR-021: Lokalne obaveze i pregled nezavršenog rada [MUST]

**Zahtjev:** Vlasnik mora moći kreirati lokalnu obavezu sa statusom, opcionim rokom i izvorom, te pregledati lokalne i povezane nezavršene zadatke.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Prikaz razlikuje lokalni zadatak od To Do zadatka i zadržava njihov identitet; oznaka završeno potiče od korisnika ili potvrđenog konektora.
- Rok bez vremena dobija jasno prikazanu konvenciju kraja dana u korisničkoj zoni; zadatak bez roka nema izmišljen datum dospijeća.
- Potencijalni zadatak izveden iz mejla prvo je prijedlog; korisnik potvrđuje tekst i rok prije čuvanja. Ista izvorna stavka ne proizvodi automatski duplikat.

**Veza:** EPIC-007; GOAL-002; faza 07; TC-021. Konvencija roka bez vremena je razrada za UX provjeru.

### FR-022: Podesiv dnevni pregled i podsjetnici [MUST]

**Zahtjev:** Vlasnik mora moći podesiti dnevni objedinjeni pregled stare nepročitane pošte i nezavršenih zadataka.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Početni profil obuhvata dostupne nepročitane poruke sa vremenom prijema strogo starijim od 72 sata i nezavršene zadatke čiji je rok prošao ili je između sada i naredna 24 sata. Granične vrijednosti se testiraju; zadaci bez roka ostaju u opštem pregledu.
- Pregled odvaja važnu poštu od ostale podobne pošte, uz pristup svim podobnim stavkama; preporuka važnosti ne potiskuje neprocijenjene poruke. Korisnik može podesiti opseg, starost, horizont rokova, kanal, vrijeme i isključivanje podsjetnika.
- Vrijeme dnevnog pregleda bira se pri podešavanju u korisničkoj zoni. Ista stavka pojavljuje se najviše jednom u jednom pregledu; sljedeći dnevni pregled je dozvoljen dok je stavka i dalje podobna.
- Poslije osvježavanja pročitana poruka i završen ili otkazan zadatak nisu novi podsjetnik. Odgađanje utiče na podsjetnik, ne mijenja rok ni izvorni status.

**Veza:** EPIC-007; GOAL-002; faza 07; TC-022. Početni dnevni profil je potvrđen odgovorom vlasnika.

### FR-023: Zakazani poslovi, pauza i naknadni pregled [MUST]

**Zahtjev:** Zakazani poslovi moraju raditi samo dok su backend aktivan i vault otključan; korisnik mora vidjeti stanje i propuštena izvršavanja.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Zaključan vault ili ugašen backend ne čitaju tajne i ne izvršavaju poslove. UI razlikuje posljednji uspjeh, čekanje, grešku i propušten termin.
- Nakon otključavanja ili ponovnog pokretanja više propuštenih dnevnih pregleda sabira se u jedan pregled aktuelno podobnih stavki; ne prikazuje se zaseban pregled za svaki propušten dan.
- Statusi se provjeravaju prije podsjećanja. Ako izvor nije dostupan, prikazuje se upozorenje o nepotpunosti i posljednjem osvježavanju; zastarjela kopija nije predstavljena kao potvrđeno nepročitana ili nezavršena stavka.
- Neuspjelo izvršavanje ima ograničene pokušaje, otkazivanje i evidenciju bez tajni; vanjske promjene ostaju pod FR-009/FR-014 i nisu dio automatskog naknadnog pregleda.

**Veza:** EPIC-007; GOAL-002, GOAL-003; faza 07; TC-023.

### FR-024: Tri jezika i nezavisan jezik AI odgovora [MUST]

**Zahtjev:** Vlasnik mora moći odvojeno izabrati i trajno sačuvati jezik interfejsa i jezik AI odgovora.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- UI ima `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk` i `en-US`; AI ima ista tri izbora i `follow-user`. Promjena UI jezika odmah mijenja vidljive tekstove bez gubitka stanja i ne mijenja podešen AI jezik.
- Sva tri izbora opstaju kroz cold start, update, lock/unlock i restore. Zaključani ekran koristi minimalnu nesenzitivnu bootstrap kopiju `ui.locale`, bez `agent.responseLanguage` i privatnih podataka.
- Poslije faze 02 kanonska vrijednost je u šifrovanoj SQLite bazi; uspješan upis prethodi potvrdi promjene i atomarnom osvježavanju bootstrap kopije. Pri otključavanju kanonska vrijednost ima prednost.
- Srpski AI izbor eksplicitno navodi latinicu i varijantu; ijekavica prati upotrebu u Republici Srpskoj: nisu, nisam, sutra, vrijeme, sljedeći, izvještaj. Izvorni sadržaj ostaje u svom jeziku.

**Veza:** EPIC-001, EPIC-008; GOAL-004; faze 01, 02, 08; TC-024; NFR-007/NFR-008.

### FR-025: Potpuna podešavanja i obavještenja [MUST]

**Zahtjev:** Vlasnik mora kroz UI podešavati model, provajdere, integracije, dozvole, memoriju, obavještenja, vremensku zonu i trajanje podataka.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Validne promjene ostaju sačuvane nakon restarta; nevažeća vrijednost ili neuspješan upis ne zamjenjuju posljednju važeću postavku.
- Izmjena zone mijenja prikaz i raspored dnevnih podsjetnika, uz očuvanje stvarnih trenutaka postojećih događaja. UI jasno razlikuje zone, locale i jezik AI odgovora.
- Trajanje podataka i lokalno uklanjanje zahtijevaju jasan pregled obuhvata, potvrdu i provjeru izvedenih indeksa; ne brišu vanjske izvore samovoljno. Obavezne sigurnosne zaštite ne mogu se isključiti.
- Desktop obavještenje na zaključanom vaultu ne sadrži privatne naslove, tekst mejla, dokumenta ili zadatka; detalji su dostupni tek nakon otključavanja.
- Korisnik bira svijetlu ili tamnu temu (`ui.theme = light | dark`) koja ostaje sačuvana kroz ponovno pokretanje, zaključavanje/otključavanje, update i restore, nezavisno od UI/AI jezika. Od faze 02 kanonski zapis je šifrovan, a minimalna nesenzitivna bootstrap kopija omogućava isti izgled prije unlock-a; neuspješan upis ne poništava prethodni izbor. Zahtjev je potvrđen tokom UX razgovora 12.10.2026; arhitektonska razrada je [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md), a planirane provjere `UX-THEME-01–06` u [UX specifikaciji](ux-design.md).

**Veza:** EPIC-008; GOAL-003, GOAL-004; faza 08; TC-025.

### FR-026: Pregled upotrebe i kontrola troška [MUST]

**Zahtjev:** Vlasnik mora moći vidjeti dostupnu upotrebu AI provajdera i podesiti ograničenja prije nastavka obrade.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Upotreba razlikuje mjeren podatak provajdera, lokalnu procjenu i nepoznatu vrijednost; procjena troška ne predstavlja račun ili garantovanu cijenu.
- Prekoračeno korisničko ograničenje zaustavlja naredni zahtjev ili traži eksplicitnu izmjenu politike; nema tihog prelaska na drugi plaćeni provajder.
- Evidencija sadrži minimalne tehničke metapodatke, bez ključeva, sadržaja prompta, privatnih poruka ili dokumenata.

**Veza:** EPIC-003, EPIC-008; GOAL-003; faze 03, 08; TC-026. Precizna jedinica limita i prikaz cijena utvrđuju se prije implementacije iz aktivnog plana i dostupnih ugovora provajdera.

### FR-027: Pregled akcija i dijagnostika [MUST]

**Zahtjev:** Vlasnik mora moći provjeriti predložene, odbijene, odobrene, izvršene i neuspjele akcije bez izlaganja privatnog sadržaja kroz logove.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Evidencija povezuje alat, nalog kroz nesenzitivni identifikator, vrijeme, odluku i ishod; privatni detalji pregleda dostupni su samo u otključanom interfejsu i šifrovanom skladištu gdje su potrebni.
- Greška, prekid i nepoznat ishod razlikuju se od uspješnog izvršavanja; dijagnostički izvoz ima pregled i redakciju prije lokalnog čuvanja.
- Konzola, crash dump i telemetry ne sadrže stvarne tajne ni privatni sadržaj; sigurnosni testovi koriste kontrolisane sintetičke podatke.

**Veza:** EPIC-004, EPIC-009; GOAL-003; faze 04, 09; TC-027.

### FR-028: Windows izdanje i provjera cijelog MVP-a [MUST]

**Zahtjev:** Prva potpuna verzija mora imati provjeren Windows instalacioni paket, dokumentaciju i dokaze svih odobrenih faza 01–09.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Čista instalacija, nadogradnja i ponovno pokretanje provjeravaju se na Windows 11 x64; ažuriranje ne briše bazu, tajne ni locale. Deinstalacija ima jasan tretman zadržavanja korisničkih podataka.
- Demonstracija prolazi od uspostavljanja vaulta i izbora modela do pošte, nacrta, kalendara, dokumenta, zadatka, odobravanja, memorije, podsjetnika, promjene jezika i obnove backupa.
- Postoje stvarne komande i rezultati build/test provjera, dokazi kriterijuma svake aktivirane faze, pregled rizika, ažuriran README i arhitektonska dokumentacija; nema preostalih poznatih propusta koji krše sigurnosne invarijante.

**Veza:** EPIC-009; GOAL-004; faza 09; TC-028.

## 4. Nefunkcionalni zahtjevi <!-- Non-Functional Requirements -->

### NFR-001: Izolovan lokalni API [MUST]

**Zahtjev:** Backend mora slušati isključivo na `127.0.0.1` sa nasumičnim portom, sesijskim tokenom i validacijom Origin/CORS politike.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Test potvrđuje bind adresu i odsustvo LAN/javnog listenera; nedostajući, nevažeći ili zastarjeli token i nedozvoljen Origin odbijaju se.
- CORS nije zamjena za autentifikaciju; provjera obuhvata čitanja i mutacije, preflight, direktne HTTP pozive i transport za stream ako postoji. Ne uvodi se wildcard ili izuzetak koji zaobilazi zaštitu.

**Način provjere:** Unit i integracioni negativni testovi i pregled konfiguracije transporta. **Veza:** EPIC-001; TC-101.

### NFR-002: Šifrovanje i zaštita tajni [MUST]

**Zahtjev:** SQLite mora biti šifrovan SQLCipher-om, tajne dodatno AES-GCM zaštitom kolona, a materijal izveden iz master lozinke Argon2id postupkom prema mjerodavnom planu.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Baza, journal/WAL, backup i aplikacioni privremeni fajlovi ne otkrivaju kontrolisane oznake tajni ili privatni tekst; izmijenjen šifrat i pogrešna lozinka odbijaju se bez djelimičnog čitanja.
- Ako odgovarajući native SQLCipher build nije dostupan, vault dio je blokiran; nema plaintext SQLite fallback-a. Kriptografski parametri i upravljanje ključevima zahtijevaju arhitektonsku razradu i negativne testove prije upotrebe.

**Način provjere:** Kriptografski unit/integracioni testovi i kontrolisan pregled artefakata na disku. **Veza:** EPIC-002; TC-102.

### NFR-003: Granica renderera i privatne instance [MUST]

**Zahtjev:** Renderer ne smije direktno pristupati SQLite-u, sačuvanim ključevima ili SDK-ovima provajdera; privatne instance moraju imati odvojene podatke i integracije.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Pregled zavisnosti, IPC/API ugovora i build artefakata potvrđuje zabranu privilegovanih pristupa i odsustvo trajnih tajni u frontendu.
- Test dvije kontrolisane instance ne dijeli bazu, ključeve, tokene, podešavanja ili dozvole; argumenti za putanje ne dozvoljavaju čitanje druge instance.

**Način provjere:** Arhitektonski pregled i test izolacije sa sintetičkim podacima. **Veza:** EPIC-001, EPIC-002; TC-103.

### NFR-004: Najmanje privilegije i nepouzdan sadržaj [MUST]

**Zahtjev:** Model i vanjski tekst ne smiju zaobići dozvole, validaciju alata ili pregled akcije.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Sintetičke instrukcije u mejlu, dokumentu, memoriji i rezultatu alata ne odobravaju akciju, ne proširuju opseg i ne otkrivaju tajnu.
- Negativni testovi pokrivaju nepoznat alat, izmijenjene argumente poslije odobrenja, opoziv dozvole, lock tokom poziva, duplikate i proizvoljne putanje; izvršavanje se provjerava i na backendu.

**Način provjere:** Unit i integracioni scenariji za granice povjerenja. **Veza:** EPIC-004, EPIC-005, EPIC-006, EPIC-007; TC-104.

### NFR-005: Kontrola cloud obrade [MUST]

**Zahtjev:** Slanje odabranog sadržaja cloud modelu mora biti vidljivo, ograničeno i usklađeno sa odobrenom politikom za odredište i vrstu podataka.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- UI prije prve relevantne obrade objašnjava odredište i opseg; dodavanje novog odredišta ili šireg sadržaja zahtijeva novu odluku. Embeddings nisu izuzetak.
- Odbijanje ne šalje sadržaj; lokalni model nije zamijenjen cloud modelom bez autorizacije. Korisnik može raditi sa dostupnim lokalnim pregledom uz označenu nedostupnu AI obradu.

**Način provjere:** Simuliran transport i provjera tačnih poziva i zabranjenih prenosa. **Veza:** EPIC-003, EPIC-005, EPIC-006; TC-105. Ovo nije tvrdnja da je urađena pravna ili regulatorna provjera.

### NFR-006: Migracije, prekidi i integritet podataka [MUST]

**Zahtjev:** Migracije i upisi moraju očuvati podatke pri grešci, prekidu i kompatibilnoj nadogradnji.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Kontrolisani prekid migracije/upisa i ponovni pokušaj ne ostavljaju djelimičnu kanonsku postavku ili nečitljivu bazu; restore provjerava integritet prije zamjene.
- Predviđena putanja unazad testira se; nepodržan downgrade bezbjedno se odbija uz očuvanje backupa. Destruktivna migracija traži backup/potvrdu.

**Način provjere:** Integracioni testovi migracije unaprijed/unazad gdje su podržane, prekida i restore-a. **Veza:** EPIC-002, EPIC-009; TC-106.

### NFR-007: Potpuna tri kataloga i formatiranje [MUST]

**Zahtjev:** Sav UI tekst, validacija, accessibility opisi, sistemska obavještenja i dijalozi moraju koristiti tri zasebna kataloga prema I18N_SPEC.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- CI provjerava iste semantičke ključeve i imenovane placeholdere u sva tri kataloga; pluralni sufiksi prate CLDR svakog jezika, bez zahtjeva da engleski i srpski imaju isti skup sufiksa.
- Obje srpske varijante koriste `sr-Latn-RS` za regionalno formatiranje, a zadržavaju puni kod za katalog i postavku. Datumi, brojevi, relativno vrijeme i množine imaju provjerene scenarije i jezički odgovarajući tekst.
- Nema automatske konverzije ekavice u ijekavicu, hardkodovanog UI teksta ili sirovih ključeva u izdanju; backend greške su stabilni kodovi i parametri. Ne renderuje se nepouzdan tekst kao HTML kroz prevode.

**Način provjere:** Paritet kataloga, provjera formatiranja i E2E svake lokalizacije, uz ručni jezički pregled. **Veza:** Svi epici; TC-107.

### NFR-008: Očuvanje jezika i kompatibilnost [MUST]

**Zahtjev:** Sačuvani korisnički izbor mora imati prednost nad OS prijedlogom i opstati kroz bootstrap, SQLite i restore.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Nova instalacija: srpski OS bez varijante predlaže ekavicu; eksplicitna podržana varijanta se zadržava; engleski i nepodržan OS predlažu `en-US`. Ne zaključuje se srpska varijanta iz države ili hrvatskog/bosanskog jezika.
- Stari sačuvani `sr-Latn-RS` migrira idempotentno u `sr-Latn-RS-ijekavsk`, zasebno za UI i AI jezik; `en-US`, `follow-user` i puni novi kodovi ostaju očuvani. Bootstrap uvoz ne prepisuje postojeći kanonski zapis.
- Testovi pokrivaju nevažeći bootstrap kod sa bezbjednim `en-US` prikazom, odbijanje proizvoljnih polja/putanja, kvar kopiranja, sukob sa kanonskom vrijednošću i odbijanje tihog downgrade-a ekavice u ijekavicu.
- Provjeravaju se sve tri UI lokalizacije i svih 12 kombinacija UI/AI postavki kroz restart i lock/unlock, uz eksplicitno uputstvo za jezički stil Republike Srpske i prednost korisničke instrukcije.

**Način provjere:** Unit, integracioni i E2E testovi svih putanja iz I18N_SPEC. **Veza:** EPIC-001, EPIC-002, EPIC-008; TC-108.

### NFR-009: Pouzdani poslovi i stanje izvora [MUST]

**Zahtjev:** Kvar mreže, sleep, lock i restart ne smiju stvoriti duplikat vanjske akcije ili sakriti nepoznat ishod.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Testovi ponavljanja i prekida potvrđuju jedan pregled po terminu i jedan naknadni pregled, uz novo provjeravanje podobnosti. Ako se izvor ne može provjeriti, prikaz je označen kao nepotpun.
- Prelazi vremena i promjena zone ne stvaraju dva dnevna pregleda istog termina; politika izbora vremena tokom DST prelaza dokumentuje se u arhitekturi prije implementacije.
- Timeout mutacije čuva stanje nepoznatog ishoda do provjere. Ne tvrdi se da lokalna aplikacija ima server SLA ili da radi dok je računar ugašen.

**Način provjere:** Integracioni testovi rasporeda, grešaka konektora, idempotentnosti i DST granica. **Veza:** EPIC-005, EPIC-007; TC-109.

### NFR-010: Odziv i kontrolisani profil opterećenja [SHOULD]

**Zahtjev:** Lokalni interfejs treba da ostane odzivan tokom generisanja, indeksiranja i osvježavanja.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Radni prijedlog za pilot: lokalno listanje/pretraga indeksiranih podataka p95 ≤ 2 sekunde i vidljiva reakcija na prekid generisanja ≤ 1 sekunda, na dogovorenom Windows 11 x64 uređaju sa 10.000 sintetičkih poruka, 1.000 zadataka i 100 dokumenata, uz jedan aktivan razgovor.
- Izvještaj navodi CPU, RAM, disk, veličine dokumenata, verzije i broj mjerenja; odziv provajdera i vrijeme Argon2id nisu skriveni u lokalnom rezultatu. Limit uvoza i konkurentnosti se utvrđuje prije mjerenja.

**Način provjere:** Lokalni benchmark nakon implementacije; pragovi i profil su prijedlozi za potvrdu, ne izmjeren rezultat. **Veza:** EPIC-001, EPIC-006, EPIC-008; TC-110.

### NFR-011: Pristupačnost i jasnoća odluka [MUST]

**Zahtjev:** Osnovni tokovi moraju biti upotrebljivi tastaturom i pružati razumljive lokalizovane informacije o stanju i riziku akcije.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Onboarding, unlock, chat, pregled pošte/zadataka, podešavanja i odobravanje imaju vidljiv fokus, lokalizovane pristupačne nazive i logičan redoslijed tastature; dijalog odobrenja ne potvrđuje se slučajnim nastavkom kucanja.
- Prazno stanje, greška, nepotpun sync, zaključan vault i nepoznat ishod imaju različite prikaze; opasna odluka prikazuje posljedicu prije potvrde u sve tri lokalizacije.

**Način provjere:** Ručni test tastature i pomoćnih tehnologija uz automatizovane provjere gdje su dostupne. Potpuna WCAG sertifikacija se ne tvrdi. **Veza:** EPIC-008; TC-111.

### NFR-012: Održivost arhitekture i provjerljive zavisnosti [MUST]

**Zahtjev:** Implementacija mora koristiti .NET 10, TypeScript `strict`, zaključane verzije i odvojene interfejse za modele, konektore, skladište, alate, vault i autentifikaciju.

**Kriterijumi prihvatanja:** <!-- Acceptance Criteria -->
- Zamjena model adaptera ili konektora ne zahtijeva direktan provajder SDK poziv iz poslovnog servisa ili renderera; ugovori imaju testove ponašanja i grešaka.
- Prije izbora ili nadogradnje paketa evidentiraju se aktuelna podrška, porijeklo i licenca; CI sprovodi build, tipove, neophodne testove i pregled zavisnosti. PRD ne bira nove pakete niti potvrđuje njihove današnje licence.
- Sigurnosno kritične putanje imaju unit i integracione negativne scenarije; README i `docs/architecture/` prate stvarne promjene arhitekture.

**Način provjere:** Build/CI, pregled ugovora i zavisnosti i stvarni test izvještaji po aktivnoj fazi. **Veza:** Svi epici; TC-112.

## 5. Epici i kandidati za priče <!-- Epics and User Stories -->

Priče su kandidati za kasniju razradu kroz `bmad:create-story`, bez procjene bodova, rezervisanog kapaciteta ili aktivnog sprinta. Svaka priča nasljeđuje pune kriterijume povezanih FR/NFR; sažeti scenariji ispod ne zamjenjuju njihove negativne testove. Prilikom planiranja podijeliti veće kandidate na nezavisno provjerljive cjeline za približno 1–3 dana rada.

| Epic | Vrijednost i mjera izlaska | Zahtjevi | Zavisnosti | Faza |
|---|---|---|---|---|
| EPIC-001 — Desktop osnova i jezici | Pokretna privatna instanca; dokaz lokalnog transporta i tri jezika | FR-001, FR-024; NFR-001, NFR-003, NFR-007, NFR-008, NFR-012 | Razvojni Windows alati | 01 |
| EPIC-002 — Vault i oporavak | Nema čitljivih tajni; uspješan kontrolisani restore | FR-002–FR-004; NFR-002, NFR-003, NFR-006, NFR-008 | EPIC-001, provjeren native SQLCipher | 02 |
| EPIC-003 — Modeli i BYOK | Test veze svakog adaptera i kontrolisan failover | FR-005, FR-006, FR-026; NFR-005, NFR-012 | EPIC-002, vlasnikov model/ključ | 03 |
| EPIC-004 — Razgovor i alati | Odbijene neautorizovane akcije i provjerljiv ishod | FR-007–FR-009, FR-027; NFR-004, NFR-012 | EPIC-003 | 04 |
| EPIC-005 — Microsoft 365 i pošta | Izvorna pošta, uredljiv nacrt, provjeren kalendar/To Do/OneDrive | FR-010–FR-017; NFR-004, NFR-005, NFR-009 | EPIC-004, OAuth registracija i saglasnost | 05 |
| EPIC-006 — Dokumenti i memorija | Odgovori sa izvorima; obrisana memorija nije u novom kontekstu | FR-018–FR-020; NFR-004, NFR-005, NFR-010 | EPIC-005, podržani formati | 06 |
| EPIC-007 — Obaveze i podsjetnici | Sve podobne kontrolisane stavke jednom po pregledu | FR-021–FR-023; NFR-004, NFR-009 | EPIC-006, provjeren status izvora | 07 |
| EPIC-008 — Podešavanja i desktop UX | Podešavanja opstaju; svi osnovni tokovi rade tastaturom | FR-024–FR-026; NFR-007, NFR-008, NFR-010, NFR-011 | EPIC-007 | 08 |
| EPIC-009 — Provjera i izdanje | Cijeli MVP demonstriran i instalacija/upgrade/restore provjereni | FR-027, FR-028; NFR-006, NFR-012 i ostale sigurnosne provjere | EPIC-008 i dokazi prethodnih faza | 09 |

Sve epike koristi primarni vlasnik. NFR-007 i NFR-012 važe za sve epike. Zavisnosti slijede mjerodavni redoslijed; prisustvo zahtjeva za jezik u fazi 08 ne odlaže lokalizaciju iz faze 01.

### STORY-001: Pokretanje privatne instance

Kao vlasnik želim početno podešavanje desktop aplikacije da bih započeo rad bez uređivanja datoteka. **Veza:** EPIC-001; FR-001; NFR-001, NFR-003; `MUST`.
- **Given** nova instalacija, **when** otvorim aplikaciju, **then** dobijam izbor tri jezika i tok uspostavljanja vaulta.
- **Given** nedostupan sidecar, **when** pokretanje ne uspije, **then** vidim lokalizovanu grešku i nema alternativnog javnog endpointa.

### STORY-002: Čuvanje jezičkog izbora

Kao vlasnik želim nezavisne UI/AI jezike da bih zadržao željeni način rada poslije ponovnog pokretanja. **Veza:** EPIC-001, EPIC-008; FR-024; NFR-007, NFR-008; `MUST`.
- **Given** bilo koji od tri sačuvana UI izbora, **when** pokrenem i otključam aplikaciju, **then** odgovarajući katalog i puni kod ostaju isti.
- **Given** sačuvan AI jezik i stari UI kod `sr-Latn-RS`, **when** migracija i UI promjena završe, **then** stari UI postaje ijekavica, a nezavisni AI izbor je očuvan.

### STORY-003: Zaključavanje i master lozinka

Kao vlasnik želim kontrolisan pristup vaultu da bih zaštitio tajne. **Veza:** EPIC-002; FR-002; NFR-002, NFR-004; `MUST`.
- **Given** postojeći vault, **when** unesem pogrešnu lozinku, **then** baza ostaje zaključana bez otkrivanja sadržaja.
- **Given** aktivan rad, **when** nastupi sleep ili 15 minuta neaktivnosti, **then** novi tajni pozivi se odbijaju i tajne nisu dostupne aktivnim poslovima.

### STORY-004: Jednokratni recovery

Kao vlasnik želim rezervni način otključavanja da bih oporavio pristup bez brisanja podataka. **Veza:** EPIC-002; FR-003; NFR-002; `MUST`.
- **Given** sačuvan važeći recovery code, **when** obnovim master lozinku, **then** podaci ostaju dostupni kroz novu lozinku.
- **Given** iskorišćen ili nevažeći kod, **when** pokušam recovery, **then** pristup je odbijen i baza ostaje očuvana.

### STORY-005: Backup, restore i migracije

Kao vlasnik želim provjerljivu obnovu instance da bih zaštitio svoj rad. **Veza:** EPIC-002; FR-004; NFR-006, NFR-008; `MUST`.
- **Given** kontrolisan šifrovani backup, **when** obnovim novu instancu, **then** podaci i jezici odgovaraju izvoru, a vault je zaključan.
- **Given** oštećen backup ili prekid migracije, **when** operacija ne uspije, **then** postojeća baza nije zamijenjena djelimičnim podacima.

### STORY-006: Dodavanje modela

Kao vlasnik želim svoje provajdere da bih birao model i trošak. **Veza:** EPIC-003; FR-005; NFR-003, NFR-012; `MUST`.
- **Given** podržan adapter i kontrolisan credential, **when** testiram vezu, **then** vidim stvaran rezultat i podržane sposobnosti.
- **Given** nevažeći credential, **when** test ne uspije, **then** dobijam redigovanu grešku i renderer ne prima sačuvanu tajnu.

### STORY-007: Povjerenje i failover

Kao vlasnik želim kontrolisani rezervni model da bih nastavio rad uz poznato odredište podataka. **Veza:** EPIC-003; FR-006; NFR-005; `MUST`.
- **Given** pad primarnog modela, **when** rezervni zahtijeva novu politiku prenosa, **then** sadržaj čeka moju odluku.
- **Given** neodobren kompatibilni endpoint, **when** pokušam poziv, **then** backend ga odbija prije prenosa tajni ili sadržaja.

### STORY-008: Istorija i prekid razgovora

Kao vlasnik želim nastavak razgovora i prekid odgovora da bih kontrolisao rad agenta. **Veza:** EPIC-004; FR-007; `MUST`.
- **Given** sačuvan razgovor, **when** restartujem i otključam instancu, **then** vidim istoriju i izabrani model.
- **Given** generisanje, **when** ga prekinem, **then** odgovor je označen kao nepotpun i nijedna predložena akcija nije predstavljena kao izvršena.

### STORY-009: Dozvole i validacija alata

Kao vlasnik želim ograničene alate da bih kontrolisao pristup podacima. **Veza:** EPIC-004; FR-008; NFR-004; `MUST`.
- **Given** nedozvoljen alat ili argument, **when** model predloži poziv, **then** backend odbija akciju prije konektora.
- **Given** instrukcija o proširenju prava u mejlu, **when** agent obradi sadržaj, **then** moje dozvole ostaju iste.

### STORY-010: Odobrenje i ishod akcije

Kao vlasnik želim konkretan pregled akcije da bih odobrio tačnu promjenu. **Veza:** EPIC-004; FR-009, FR-027; NFR-004; `MUST`.
- **Given** pregled akcije, **when** je odbijem, **then** nema vanjskog poziva za tu promjenu.
- **Given** ranije odobrenje, **when** se promijene argumenti ili ishod ostane nepoznat, **then** nema slijepog ponavljanja i stanje je vidljivo.

### STORY-011: Microsoft veza i puna pošta

Kao vlasnik želim povezivanje naloga i pregled izvora da bih imao provjerljiv kontekst. **Veza:** EPIC-005; FR-010, FR-011; `MUST`.
- **Given** dodijeljena delegirana prava, **when** pretražim poštu, **then** vidim stranice rezultata, izvor i vrijeme osvježavanja.
- **Given** opozvan pristup, **when** sync ne uspije, **then** vidim prekid veze i kopija nije predstavljena kao aktuelan rezultat.

### STORY-012: Pravila važnosti pošte

Kao vlasnik želim pravila i oznake uz AI preporuku da bih lakše izabrao relevantnu poštu. **Veza:** EPIC-005; FR-012; `MUST`.
- **Given** moja oznaka ili primjenjivo pravilo, **when** AI preporuči drugi rezultat, **then** moj izbor zadržava prednost i preporuka je jasno odvojena.
- **Given** neprocijenjena poruka, **when** otvorim punu poštu, **then** mogu je pregledati i ručno ispraviti važnost.

### STORY-013: Nacrt, slanje i brisanje

Kao vlasnik želim uredljiv nacrt i konačni pregled da bih brže odgovorio uz kontrolu. **Veza:** EPIC-005; FR-013, FR-014; NFR-005; `MUST`.
- **Given** odabrana poruka i dozvoljen kontekst, **when** generišem nacrt, **then** mogu ga urediti i poruka nije poslata.
- **Given** promijenjen primalac ili tekst poslije odobrenja, **when** zatražim slanje, **then** potreban je novi pregled; brisanje takođe traži zasebno izričito odobrenje.

### STORY-014: Kalendar

Kao vlasnik želim događaje u svojoj zoni da bih planirao obaveze. **Veza:** EPIC-005; FR-015; NFR-009; `MUST`.
- **Given** događaj sa UTC vremenom, **when** promijenim UI jezik ili zonu, **then** format i prikaz se prilagode bez promjene stvarnog trenutka događaja.
- **Given** predložena promjena kalendara, **when** je ne odobrim, **then** izvorni događaj ostaje isti.

### STORY-015: To Do zadaci

Kao vlasnik želim potvrđen status povezanih zadataka da bih znao šta je ostalo. **Veza:** EPIC-005; FR-016; `MUST`.
- **Given** zadatak završen u izvoru, **when** osvježavanje uspije, **then** više nije u potvrđenom pregledu nezavršenih zadataka.
- **Given** AI tvrdnja da je zadatak završen, **when** nema korisničke akcije i potvrde konektora, **then** status se ne mijenja.

### STORY-016: OneDrive izvor

Kao vlasnik želim izabrane OneDrive dokumente da bih koristio kontekst u dozvoljenom opsegu. **Veza:** EPIC-005; FR-017; NFR-005; `MUST`.
- **Given** odobren folder ili dokument, **when** ga odaberem, **then** vidim porijeklo i verziju bez automatskog indeksiranja cijelog naloga.
- **Given** opozvan pristup, **when** preuzimanje ne uspije, **then** dokument je označen kao nedostupan i nema širenja prava.

### STORY-017: Lokalni uvoz i pretraga sa izvorima

Kao vlasnik želim kontrolisanu obradu dokumenata da bih našao provjerljiv odgovor. **Veza:** EPIC-006; FR-018, FR-019; NFR-004, NFR-005; `MUST`.
- **Given** podržan sintetički dokument, **when** uvoz i pretraga uspiju, **then** odgovor pokazuje korišćeni dokument i odlomak.
- **Given** nedozvoljen format ili uklonjen dokument, **when** pokrenem obradu ili novu pretragu, **then** nema izvršavanja sadržaja niti korišćenja uklonjenog izvora.

### STORY-018: Kontrola memorije

Kao vlasnik želim pregled i brisanje memorije da bih birao trajni kontekst. **Veza:** EPIC-006; FR-020; NFR-004; `MUST`.
- **Given** memorijski prijedlog, **when** ga ne odobrim prema politici, **then** nije sačuvana trajna činjenica.
- **Given** obrisana stavka, **when** započnem novi upit, **then** stavka nije u novom memorijskom kontekstu ni pretrazi.

### STORY-019: Lokalne obaveze

Kao vlasnik želim kontrolisan unos zadatka i roka da bih planirao rad bez izmišljenih obaveza. **Veza:** EPIC-007; FR-021; `MUST`.
- **Given** prijedlog zadatka iz mejla, **when** potvrdim tekst i rok, **then** čuva se jedna obaveza povezana sa izvorom.
- **Given** zadatak bez roka, **when** otvorim pregled dospjelih, **then** nema izmišljen rok, ali ostaje u opštem pregledu.

### STORY-020: Dnevni podsjetnici

Kao vlasnik želim podesiv objedinjeni pregled da bih uočio staru poštu i bliske rokove. **Veza:** EPIC-007; FR-022; NFR-009; `MUST`.
- **Given** početni profil i podobne kontrolisane stavke, **when** dođe izabrano vrijeme, **then** poruke starije od 72 sata i nezavršeni zadaci sa prošlim ili rokom u naredna 24 sata pojavljuju se jednom po pregledu.
- **Given** pročitan mejl, završen zadatak ili odgađanje podsjetnika, **when** provjera izvora uspije, **then** pregled prati novo stanje bez promjene roka usljed odgađanja.

### STORY-021: Pauza i naknadno izvršavanje

Kao vlasnik želim vidljivo stanje propuštenih pregleda da bih nastavio rad poslije zaključavanja. **Veza:** EPIC-007; FR-023; NFR-009; `MUST`.
- **Given** zaključan vault tokom više dnevnih termina, **when** ga otključam, **then** dobijam jedan naknadni pregled aktuelno provjerenih stavki.
- **Given** nedostupan izvor ili DST prelaz, **when** raspored radi, **then** nema lažne potvrde statusa, duplog termina ili tajnog poziva dok je zaključano.

### STORY-022: Podešavanja i trošak

Kao vlasnik želim dostupne postavke i kontrolu upotrebe da bih upravljao radom agenta. **Veza:** EPIC-008, EPIC-003; FR-025, FR-026; NFR-010, NFR-011; `MUST`.
- **Given** validna postavka, **when** je sačuvam i restartujem, **then** izbor opstaje; sigurnosne zaštite nemaju prekidač za isključivanje.
- **Given** dostignut korisnički limit ili zaključan ekran, **when** nastupi novi zahtjev ili obavještenje, **then** zahtjev čeka novu politiku, a obavještenje ne otkriva privatni sadržaj.

### STORY-023: Dijagnostika i izdanje

Kao vlasnik želim provjerenu instalaciju i redigovanu dijagnostiku da bih pouzdano koristio cijeli MVP. **Veza:** EPIC-009; FR-027, FR-028; NFR-006, NFR-012; `MUST`.
- **Given** release kandidat, **when** izvršim instalaciju, nadogradnju i kontrolisani restore, **then** podaci i jezici su očuvani uz evidentirane rezultate svih provjera.
- **Given** greška ili nepoznat ishod akcije, **when** pregledam ili izvezem dijagnostiku, **then** status je tačan, a tajne i privatni sadržaj nisu u izvozu.

## 6. Korisnički tokovi i interfejs <!-- User Experience Requirements -->

### FLOW-001: Prvo pokretanje i nastavak rada <!-- Flow 1 -->

1. Vlasnik instalira aplikaciju, bira UI jezik i uspostavlja vault.
2. Čuva recovery code, bira AI jezik odvojeno, dodaje model i provjerava vezu.
3. Povezuje Microsoft nalog uz vidljiva prava i bira opseg obrade.
4. Podešava vrijeme i zonu dnevnog pregleda; automatski pregled ne počinje bez završenog podešavanja i dozvola.
5. Poslije restarta zaključani ekran koristi sačuvani jezik; otključavanje sinhronizuje kanonsku postavku.

**Greške:** Prekid ne briše završene korake; nevažeći credential ne blokira lokalni pregled i podešavanja. Nema pristupa tajnama dok je vault zaključan.

### FLOW-002: Važan mejl i odgovor <!-- Flow 2 -->

1. Vlasnik otvara pregled važnih poruka i vidi razlog: oznaka, pravilo ili AI preporuka.
2. Otvara original; puna pošta i neprocijenjene poruke uvijek su dostupne.
3. Bira kontekst i dobija nacrt preko dozvoljenog provajdera, pa ga uređuje.
4. Pregleda konačne primaoce, tekst i priloge i izričito odobrava slanje.
5. Dobija potvrđen ishod ili vidljivo stanje nepoznatog ishoda.

**Greške:** Odbijena cloud obrada ne prenosi sadržaj; promijenjen nacrt poništava odobrenje; timeout ne šalje ponovo bez provjere.

### FLOW-003: Dnevni i naknadni pregled <!-- Flow 3 -->

1. Vlasnik postavlja dnevno vrijeme i podesive pragove; početni profil je 72 sata za poštu i 24 sata unaprijed za rokove.
2. Otključan backend osvježava izvore, zatim pravi jedan objedinjeni pregled podobnih stavki.
3. Vlasnik otvara izvor, odgađa podsjetnik ili potvrđuje promjenu zadatka kroz odgovarajući alat.
4. Vault se zaključa; poslovi čekaju bez tajnih poziva.
5. Po otključavanju dobija jedan naknadni pregled trenutnih stavki umjesto niza propuštenih dnevnih obavještenja.

**Greške:** Nepotpun izvor ima upozorenje i vrijeme posljednjeg osvježavanja; zadatak se ne smatra završenim na osnovu AI teksta.

### FLOW-004: Dokument, memorija i obnova <!-- Flow 4 -->

1. Vlasnik bira lokalni ili OneDrive dokument i opseg, vidi stanje uvoza i pretražuje ga.
2. Odgovor pokazuje izvor; korisnik pregledava memorijske prijedloge i odlučuje šta čuva.
3. Pravi šifrovani backup i kroz kontrolisan restore provjerava obnovu podataka i jezika.

**Greške:** Nepodržan format i nedozvoljen pristup ne izvršavaju sadržaj; oštećen backup ne mijenja postojeću bazu.

| UI zahtjev | Obavezno ponašanje | Dokaz pri implementaciji |
|---|---|---|
| UI-001 | Početno podešavanje, unlock i izbor tri jezika prije pristupa podacima | Test svih locale putanja; bez tajni u bootstrap-u |
| UI-002 | Pregled pošte/obaveza sa razlogom važnosti, izvorom, statusom i vremenom osvježavanja | Kontrolisan uzorak i ručni pregled pune pošte |
| UI-003 | Dijalog akcije sa konačnim argumentima i jasnom posljedicom | Odbijanje, promjena nacrta i lock prije izvršavanja |
| UI-004 | Jasno odvojeni UI/AI jezici, zona, dozvole, privatnost, memorija i upotreba | Tastatura, svi jezici i restart |
| UI-005 | Stanje poslova i obavještenja koja ne otkrivaju sadržaj dok je zaključano | Sleep, propušteni termini i naknadni pregled |

Wireframe-ovi nisu izrađeni; to je ulaz za zaseban `bmad:ux-design`. Nazivi ekrana u ovom dokumentu opisuju tok i ne predstavljaju hardkodovane UI stringove.

## 7. Uspjeh i lični pilot <!-- Success Metrics -->

Mjere su dio provjere proizvoda, bez prikupljanja privatnih poruka kroz telemetry. Početno stanje nije izmjereno. Brojčani ciljevi iz opisa proizvoda ostaju **prijedlozi za potvrdu prije pilota**; nisu uslov koji je vlasnik već prihvatio.

| Mjera | Početno stanje | Predloženi cilj | Definicija i način mjerenja | Period |
|---|---|---|---|---|
| Vrijeme aktivnog rada | Neizmjereno | ≥ 30% manje | Porediti minute po relevantnoj obrađenoj poruci i zadatku pri sličnom obimu; uključiti čitanje, nacrt, provjeru, ispravke i obradu podsjetnika | 5 radnih dana prije, 10 dana tokom pilota |
| Zadaci završeni do roka | Neizmjereno | ≥ 90% | Broj zadataka završenih do eksplicitnog roka / broj zadataka koji dospijevaju; odgođene i otkazane evidentirati odvojeno, bez naknadnog uljepšavanja nazivnika | Prve 4 sedmice |
| Obuhvat važnih poruka | Nema označenog uzorka | ≥ 95% | Važne poruke u preporučenom pregledu / sve poruke koje vlasnik označi važnim u pregledanom uzorku; posebno bilježiti propuštene i pogrešno preporučene | Pregled označenog uzorka tokom pilota |
| Potpunost podsjetnika | Nema implementacije | 100% kontrolisanog scenarija | Prikazane potvrđeno podobne stavke / sve podobne stavke; nula duplikata u istom pregledu | Test prije pilota, pa provjera promjene statusa |
| Kontrola vanjskih akcija | Nema implementacije | Nula neautorizovanih promjena | Negativni scenariji i dokaz da slanje/brisanje čekaju tačno odobrenje | Svaka relevantna faza i release |
| Obnova i jezički izbor | Nema implementacije | Svi kontrolisani scenariji prolaze | Restore, tri locale putanje, 12 UI/AI kombinacija i kompatibilnost | Faze 02, 08 i release |

Prve tri mjere provjeravaju korisničku korist. Posljednje tri su kriterijumi kvaliteta, a sigurnosna autorizacija, integritet podataka i puna lokalizacija obavezni su bez obzira na rezultat pilota. Preciznost preporučene pošte se dodatno prikazuje kao broj ispravno preporučenih važnih / sve preporučene poruke, bez izmišljenog cilja.

Nula podobnih zadataka, nula označenih važnih poruka ili nedovoljan uporediv uzorak daju „nedovoljno podataka”, ne 100% uspjeha. Pilot ima jednog korisnika i ne dokazuje tržišnu potražnju, prihod ili korist za druge korisnike. Lokalna evidencija je dobrovoljna i sadrži vrijeme, brojeve i nesenzitivne identifikatore, bez kopiranja sadržaja u repozitorijum.

## 8. Pretpostavke, zavisnosti i otvorene odluke <!-- Assumptions and Dependencies -->

### 8.1. Potvrđeno i razrađeno

**Potvrđeno:** Jedan vlasnik za početak; cijeli MVP 01–09; sopstveni AI ključevi; Microsoft 365 kao prva integracija; tri lokalizacije; ušteda vremena i pravovremene obaveze; pravila/oznake uz AI preporuku; podesiv dnevni profil 3 dana/24 sata i jedan naknadni pregled.

**Projektantska razrada u PRD-u:** Prednost ručne oznake i prvog primjenjivog poredanog pravila, početna polja pravila, prikaz ostale podobne pošte, granica starosti od 72 sata, rok bez vremena kao kraj dana i odvojeno stanje nepoznatog ishoda. Ovo su konkretna pravila za provjeru u UX/arhitekturi, ne dodatne izjave vlasnika. Bezbjednosna razrada ostaje konzervativna: nepoznato nije dozvola niti potvrda uspjeha.

| ID | Otvorena tačka | Privremeni tretman | Kada se razrješava |
|---|---|---|---|
| OPEN-001 | Vrijeme dnevnog pregleda i korisnička zona | Korisnik ih bira; nema proizvoljno aktiviranog termina | UX i podešavanje pilota |
| OPEN-002 | Privatni Microsoft nalog ili poslovni tenant, broj naloga i prava | Testirati stvarni tip vlasnikovog naloga; ne obećavati nedostupnu funkciju | Prije faze 05 |
| OPEN-003 | Količina podataka, formati i limiti uvoza | Tačnu matricu preuzeti iz aktivne faze 06 i dokumentovati prije razvoja parsera | Arhitektura i faza 06 |
| OPEN-004 | Pragovi uspjeha, početno mjerenje i radni profil performansi | Brojčani ciljevi su prijedlozi, rezultati nisu izmjereni | Prije ličnog pilota |
| OPEN-005 | Budžet, tim i datum isporuke | Nema procjene datuma ni story bodova bez kapaciteta i preduslova | Planiranje priča/sprinta |
| OPEN-006 | Granularna politika cloud obrade i memorijskog odobravanja | Odbijanje prije odobrenja odredišta/opsega; memorijski prijedlog nije automatski zapis | UX i arhitektura |
| OPEN-007 | Precizna jedinica limita i upotreba cijena provajdera | Razlikovati izmjereno, procjenu i nepoznato; bez obećanja tačnog računa | Aktivne faze 03/08 |

| Zavisnost | Odgovornost | Trenutno stanje | Rizik i postupak |
|---|---|---|---|
| Windows 11 x64 i razvojni alati | Razvoj | Aplikacioni build nije provjeren | Provjeriti u fazi 01 |
| Native SQLCipher i kriptografski ugovori | Razvoj/arhitektura | Nije testirano | Blokirati vault dio ako uslov nije ispunjen |
| Microsoft registracija i delegirana saglasnost | Vlasnik i razvoj | Nije provjeren tip naloga ni tenant politika | Testni nalog i minimalna prava u fazi 05 |
| Vlasnikov provajder ili Ollama | Vlasnik | Credential/model nije provjeravan | Kontrolisan test veze u fazi 03; bez ključa izdavača |
| Kontrolisani sintetički podaci | Razvoj/testiranje | Aplikacioni fixture-i ne postoje | Bez stvarnog privatnog sadržaja u testovima |
| Izričito aktiviranje implementacionih faza | Vlasnik | Nijedna aktivna faza | BMAD status nije odobrenje za razvoj |

Priprema PRD-a nije blokirana. Ovi implementacioni preduslovi još nisu dokazi raspoloživosti.

## 9. Ograničenja i opseg prve verzije <!-- Constraints -->

Preuzeti stack je Windows 11 x64, Tauri 2, React/TypeScript `strict`, ASP.NET Core/.NET 10 sidecar i SQLCipher SQLite. Ovaj PRD ne bira novi paket ili algoritamske parametre. Provjera aktuelne podrške i licence obavlja se prije konkretnog izbora zavisnosti.

Lokalni API ostaje na loopback adresi; outbound pozivi dozvoljenim AI i Microsoft servisima idu kroz backend i njegove apstrakcije. Centralna komercijalna infrastruktura ne čita privatnu bazu. Cloud AI obrada odabranog sadržaja razlikuje se od lokalnog čuvanja i mora biti vidljiva.

Datumi i mašinske vrijednosti ostaju UTC/ISO i engleski identifikatori; UI ih formatira prema locale-u i korisničkoj zoni. UI bootstrap nije skladište privatnog profila, AI jezika ili tajni. Kriptografski metapodaci vaulta imaju zaseban ugovor.

Vlasnik želi cijeli MVP što ranije, ali nema ugovorenog roka, budžeta ili raspoloživog kapaciteta. Skraćivanje obaveznih provjera ili izbacivanje potvrđenih MVP funkcija nije način da se ispuni taj zahtjev.

### Van MVP-a [WONT] <!-- Out of Scope -->

- Javni server, LAN pristup, mobilni klijent i cloud sinhronizacija: zasebna kasnija sigurnosna i projektantska faza.
- Više istovremenih korisnika iste instance i centralni pristup privatnim podacima: nije model prve verzije.
- Govor, autonoman browser/desktop control, proizvoljno izvršavanje komandi i direktni SQL nad korisničkim bazama: nema registrovanog MVP opsega.
- Rad sa tajnama i automatizacije dok je vault zaključan ili backend ugašen: zabranjeno postojećom invarijantom.
- Naplata pretplata, komercijalni hosting, macOS/Linux i dodatni konektori poput Gmail-a: kasniji opseg, bez obećanog datuma.

**Opcione dopune:** Dodatni pregled trendova lične produktivnosti `[COULD]` i napredno podešavanje rangiranja preporuka `[COULD]` mogu se razmotriti poslije pilota. Ne predstavljaju novu obavezu ni razlog za odlaganje osnovnih pravila, oznaka ili podsjetnika.

## 10. Plan isporuke i izlazni kriterijumi <!-- Release Planning -->

Razvojni build može provjeravati jednu fazu; prva potpuna verzija obuhvata sve faze 01–09. Ne uvode se nove implementacione faze, datumi ili automatski prelazi. Svaka faza traži izričit zahtjev vlasnika.

| Razvojna faza | PRD opseg | Predaja i potrebni dokaz |
|---|---|---|
| 01 — Osnova | FR-001, FR-024; NFR-001, NFR-003, NFR-007, NFR-008, NFR-012 | Desktop/sidecar build, lokalni transport, tri kataloga i početno pamćenje jezika |
| 02 — Baza/vault | FR-002–FR-004 i kanonski dio FR-024 | Šifrovanje, recovery, backup/restore i migracija jezika sa negativnim testovima |
| 03 — Provajderi | FR-005, FR-006 i provajderski dio FR-026 | Adapteri, BYOK, povjerenje i kontrolisan rezervni model |
| 04 — Agent/dozvole | FR-007–FR-009 i osnovna evidencija FR-027 | Razgovor, validacija, autorizacija i dokaz odbijanja nebezbjednih poziva |
| 05 — Microsoft 365 | FR-010–FR-017 | OAuth, pošta/pravila/nacrt, potvrđeno slanje, kalendar, To Do, OneDrive |
| 06 — Dokumenti/memorija | FR-018–FR-020 | Matrica formata, uvoz, pretraga sa izvorima, kontrola memorije |
| 07 — Obaveze/automatizacije | FR-021–FR-023 | Statusi/rokovi, dnevni profil, deduplikacija, lock i naknadni pregled |
| 08 — Podešavanja/UX | FR-024–FR-026 | Potpuna podešavanja, lokalizacija i tastatura kroz sve tokove |
| 09 — Provjera/izdanje | FR-027, FR-028 i svi relevantni NFR | Sigurnosna provjera, stvarni test/build izvještaji, Windows paket i kompletna demonstracija |

**Izlaz iz MVP-a:** Dokazi pojedinačnih kriterijuma aktiviranih faza, potpuna provjera FR/NFR, demonstracija minimalnog gotovog proizvoda iz početnog plana, očuvanje podataka pri upgrade/restore i nula poznatih kršenja sigurnosnih invarijanti. Usvajanje brojčanih pilot ciljeva je zasebna odluka; dobri korisnički rezultati ne zamjenjuju sigurnosnu provjeru.

## 11. Rizici i mjere <!-- Risks and Mitigations -->

Vjerovatnoće i novčane posljedice nisu izmjerene; procjena je kvalitativna. Odgovornost za provjeru ima razvoj, uz odluke vlasnika gdje su navedene.

| Rizik | Posljedica | Mjera i uslov provjere |
|---|---|---|
| AI propusti važnu poruku | Propuštena pažnja ili rok | Pravila/oznake imaju prednost; puna pošta, razlog i ručna ispravka; izmjeriti obuhvat i preciznost |
| Nacrt izmisli obavezu ili podatak | Netačan odgovor | Izvor, označene nepoznate činjenice, uređivanje i konačno odobrenje |
| Instrukcije u sadržaju zaobiđu kontrolu | Neovlašćena akcija ili otkrivanje | Backend validacija i negativni testovi granice povjerenja |
| Lock, sleep ili gašenje prekine pregled | Zakašnjeli podsjetnik | Vidljivo stanje; jedan naknadni pregled; nema ključa na disku radi zaobilaženja lock-a |
| Izvorni status je zastario | Pogrešan ili suvišan podsjetnik | Ponovna provjera i označena nepotpunost; bez AI zaključivanja završenosti |
| Timeout uzrokuje duplikat promjene | Ponovljeno slanje ili promjena | Provjera ishoda, vezivanje odobrenja za argumente i kontrolisano ponavljanje |
| Native SQLCipher ili OAuth nije raspoloživ | Blokirana odgovarajuća funkcija | Rano provjeriti u njenoj fazi; nema nesigurnog fallback-a |
| Cloud obrada ode na novo odredište | Prenos privatnog sadržaja bez odluke | Posebno povjerenje i politika sadržaja, uključujući failover i embeddings |
| Jezička migracija resetuje varijantu | Gubitak izbora ili netačan zaključani ekran | Idempotentna migracija, kanonski prioritet, restore i downgrade testovi |
| Cijeli MVP prevaziđe kapacitet | Nerealno obećanje roka | Procjena poslije razrade priča, izričite faze i obavezni uslovi kvaliteta |
| Kompromitovan uređaj ili izgubljen recovery | Privatnost ili trajno izgubljen pristup | Jasne granice lokalne zaštite, kontrolisan recovery/backup; nema obećanja zaštite od kompromitovanog OS-a |

## 12. Sljedivost i planirani scenariji <!-- Traceability Matrix -->

Svaki red ima status **planirano**. `TC-*` označava budući paket scenarija iz kriterijuma zahtjeva, ne postojeći test fajl. Povezani NFR primjenjuju se uz funkcionalne scenarije. Tačne test putanje dodaju se kada se odgovarajuća faza implementira.

| Zahtjev | Cilj | Epic | Priča | Planirani scenario |
|---|---|---|---|---|
| FR-001 | GOAL-004 | EPIC-001 | STORY-001 | TC-001 |
| FR-002 | GOAL-003 | EPIC-002 | STORY-003 | TC-002 |
| FR-003 | GOAL-003 | EPIC-002 | STORY-004 | TC-003 |
| FR-004 | GOAL-003, GOAL-004 | EPIC-002 | STORY-005 | TC-004 |
| FR-005 | GOAL-003 | EPIC-003 | STORY-006 | TC-005 |
| FR-006 | GOAL-003 | EPIC-003 | STORY-007 | TC-006 |
| FR-007 | GOAL-001, GOAL-003 | EPIC-004 | STORY-008 | TC-007 |
| FR-008 | GOAL-003 | EPIC-004 | STORY-009 | TC-008 |
| FR-009 | GOAL-003 | EPIC-004 | STORY-010 | TC-009 |
| FR-010 | GOAL-001, GOAL-003 | EPIC-005 | STORY-011 | TC-010 |
| FR-011 | GOAL-001 | EPIC-005 | STORY-011 | TC-011 |
| FR-012 | GOAL-001, GOAL-002 | EPIC-005 | STORY-012 | TC-012 |
| FR-013 | GOAL-001, GOAL-003 | EPIC-005 | STORY-013 | TC-013 |
| FR-014 | GOAL-001, GOAL-003 | EPIC-005 | STORY-013 | TC-014 |
| FR-015 | GOAL-002 | EPIC-005 | STORY-014 | TC-015 |
| FR-016 | GOAL-002 | EPIC-005 | STORY-015 | TC-016 |
| FR-017 | GOAL-001, GOAL-003 | EPIC-005 | STORY-016 | TC-017 |
| FR-018 | GOAL-001, GOAL-003 | EPIC-006 | STORY-017 | TC-018 |
| FR-019 | GOAL-001, GOAL-003 | EPIC-006 | STORY-017 | TC-019 |
| FR-020 | GOAL-001, GOAL-003 | EPIC-006 | STORY-018 | TC-020 |
| FR-021 | GOAL-002 | EPIC-007 | STORY-019 | TC-021 |
| FR-022 | GOAL-002 | EPIC-007 | STORY-020 | TC-022 |
| FR-023 | GOAL-002, GOAL-003 | EPIC-007 | STORY-021 | TC-023 |
| FR-024 | GOAL-004 | EPIC-001, EPIC-008 | STORY-002 | TC-024 |
| FR-025 | GOAL-003, GOAL-004 | EPIC-008 | STORY-022 | TC-025 |
| FR-026 | GOAL-003 | EPIC-003, EPIC-008 | STORY-022 | TC-026 |
| FR-027 | GOAL-003 | EPIC-004, EPIC-009 | STORY-010, STORY-023 | TC-027 |
| FR-028 | GOAL-004 | EPIC-009 | STORY-023 | TC-028 |
| NFR-001 | GOAL-003 | EPIC-001 | STORY-001 | TC-101 |
| NFR-002 | GOAL-003 | EPIC-002 | STORY-003, STORY-004, STORY-005 | TC-102 |
| NFR-003 | GOAL-003 | EPIC-001, EPIC-002 | STORY-001, STORY-003, STORY-006 | TC-103 |
| NFR-004 | GOAL-003 | EPIC-004–EPIC-007 | STORY-009, STORY-010, STORY-013, STORY-017, STORY-018, STORY-021 | TC-104 |
| NFR-005 | GOAL-003 | EPIC-003, EPIC-005, EPIC-006 | STORY-007, STORY-013, STORY-016, STORY-017 | TC-105 |
| NFR-006 | GOAL-003, GOAL-004 | EPIC-002, EPIC-009 | STORY-005, STORY-023 | TC-106 |
| NFR-007 | GOAL-004 | EPIC-001–EPIC-009 | STORY-002 i svaki UI scenario | TC-107 |
| NFR-008 | GOAL-004 | EPIC-001, EPIC-002, EPIC-008 | STORY-002, STORY-005 | TC-108 |
| NFR-009 | GOAL-002, GOAL-003 | EPIC-005, EPIC-007 | STORY-014, STORY-020, STORY-021 | TC-109 |
| NFR-010 | GOAL-001, GOAL-004 | EPIC-001, EPIC-006, EPIC-008 | STORY-001, STORY-017, STORY-022 | TC-110 |
| NFR-011 | GOAL-004 | EPIC-008 | STORY-022 i svi osnovni tokovi | TC-111 |
| NFR-012 | GOAL-003, GOAL-004 | EPIC-001–EPIC-009 | STORY-001–STORY-023 | TC-112 |

## 13. Predaja arhitekturi i naredni korak

Preporučeni naredni intent je **`bmad:architecture`**. Preporučeni `bmad:ux-design` može dodatno razraditi pravila, početno podešavanje i odobravanja prije planiranja sprinta. Ovaj dokument ne započinje te tokove automatski.

Arhitektura treba da razradi: sidecar transport i sesijsku autentifikaciju; SQLCipher/native distribuciju i recovery; registraciju alata i vezivanje odobrenja za argumente; politiku cloud odredišta i sadržaja; OAuth prava i ugovore konektora; izvore/verzije i brisanje indeksa; migracije jezika; idempotentnost i DST pravila rasporeda; matrice formata i limita; strategiju instalacije i upgrade-a. Interfejsi za modele, konektore, skladište, alate, vault i autentifikaciju ostaju odvojeni.

Postojeći [ADR-001_LOCALIZATION.md](../architecture/ADR-001_LOCALIZATION.md) je ulaz, ne dokaz implementacije. Novi ADR-ovi se dodaju kada budu donesene arhitektonske odluke; ovaj PRD nije promijenio postojeću arhitekturu. Prije sprinta potrebno je razložiti veće kandidate, provjeriti preduslove i kapacitet, a prije razvoja tražiti izričito aktiviranje faze. Prvi mogući implementacioni korak ostaje faza 01.

### Provjere ovog dokumenta — 11.10.2026.

- `python -` sa provjerom strukturisanih definicija, kriterijuma, prioriteta, referenci, lokalnih veza, jezičkih oblika i potvrđenih odgovora: izlazni kod 0; 28 jedinstvenih FR, 12 NFR, 23 priče, 9 epika i 40 kompletnih redova sljedivosti.
- BMAD `bmad-product-manager/scripts/validate-prd.sh` kroz Git Bash, sa privremenom kopijom i samo sedam obrazaca naslova prilagođenih postojećim lokalizovanim markerima sekcija: 20/20 uspješnih provjera, nula grešaka i upozorenja, izlazni kod 0. Izvor vještine nije mijenjan. Brojevi ID-jeva u njegovom izlazu broje pojavljivanja, dok je prethodna provjera brojala jedinstvene definicije.
- BMAD `bmad-shared/scripts/update-workflow-status.sh --workflow prd --status-file bmad/workflow-status.yaml --output docs/bmad/prd.md` kroz Git Bash, sa privremenom LF kopijom skripte: izlazni kod 0; završena 2 od 10 tokova, ukupno 20% BMAD evidencije. Sprint i implementacioni status ostaju bez početka.
- `git diff --check`: izlazni kod 0 u provjeri promjena; to je provjera formata izmjena, ne funkcionalnosti proizvoda.

Ove provjere potvrđuju strukturu dokumenta i evidenciju, ne ispunjenje acceptance kriterijuma aplikacije. Build, unit, integracioni, sigurnosni i E2E testovi aplikacije nisu pokretani jer aplikacioni kod i konfiguracija još ne postoje. Detaljni kriterijumi neaktivnih faza čekaju njihovo izričito aktiviranje i usklađivanje iz odjeljka 2.1.

### Rječnik i evidencija

| Pojam | Značenje |
|---|---|
| BYOK | Vlasnik koristi sopstveni API ključ, bez zajedničkog ključa izdavača |
| Vault | Zaključivo šifrovano skladište privatne instance |
| Podobna stavka | Stavka koja prema potvrđenom statusu, opsegu i vremenskom pravilu ulazi u pregled |
| Nepoznat ishod | Poziv nema potvrđen rezultat; nije ni potvrđen uspjeh ni dozvola za slijepo ponavljanje |
| Locale | Puni identifikator UI kataloga i sačuvanog jezičkog izbora |

Izvori su lokalni dokumenti iz odjeljka 2 i odgovori vlasnika iz ovog razgovora, 11.10.2026. Tržišno istraživanje, pregled aktuelnih SDK-ova/licenci i test stvarnih naloga nisu rađeni u ovom PRD toku. Tehnički komentari sekcija su stabilni identifikatori za provjeru strukture; proza je na srpskoj latinici i ijekavici u upotrebi u Republici Srpskoj.

| Verzija | Datum | Izmjena |
|---|---|---|
| 1.0 | 11.10.2026. | Cijeli MVP, 28 FR, 12 NFR, 9 epika, 23 kandidata za priče, potvrđena pravila/AI kombinacija i dnevni profil podsjetnika |
