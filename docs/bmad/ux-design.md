# UX specifikacija — Personal AI Agent

- **Datum:** 12.10.2026. (`2026-10-12`)
- **Verzija:** 1.0
- **Autor:** Codex, BMAD UX dizajn.
- **Status:** Izrađena specifikacija i nacrt tekstova; aplikacija, prototip i korisničko testiranje nisu sprovedeni.
- **Opseg:** Planirani Windows 11 x64 MVP, faze 01–09. Aktivna implementaciona faza nije određena.
- **Potvrđen novi zahtjev:** Korisnik bira svijetlu ili tamnu temu; izbor se pamti za buduća prijavljivanja.

## 1. Osnova i granice dizajna

Izvori su [početni plan](../codex-plan/00_START_HERE.md), [odluke](../codex-plan/DECISIONS.md), [I18N_SPEC](../codex-plan/I18N_SPEC.md), [PRD](prd.md), [arhitektura](architecture.md) i [gate provjera](gate-check.md). Jezička specifikacija ima prednost za lokalizaciju. Neaktivni detaljni fajlovi faza nisu učitavani. Prije implementacije svake izričito aktivirane faze potrebno je učitati njen fajl i dodati sljedivost svih pojedinačnih kriterijuma; tabela u §10 povezuje PRD, bez tvrdnje da prenosi nepročitane kriterijume faza.

Repozitorijum nema aplikacioni kod, build konfiguraciju ili aplikacione testove. Ovaj tok proizvodi dokumentaciju, bez početka sprinta ili aktiviranja faze. `CONDITIONAL PASS` i uslovi G-001–006 ostaju važeći; izbor teme ne zatvara kriptografske ili transportne uslove.

### 1.1. Korisnik, ciljevi i projektantske pretpostavke

Primarni korisnik je vlasnik privatne instalacije iz Republike Srpske. Trenutno ručno bira važne mejlove, priprema odgovore i prati nezavršene obaveze. Broj naloga, količina podataka, zanimanje, godine i posebne potrebe pristupačnosti nisu istraženi; ne uvodi se izmišljena persona.

UX treba da skrati put od važne informacije do provjerenog narednog koraka i zadrži kontrolu nad podacima. Pošta i obaveze su dostupne kroz pregled i zasebne radne ekrane; razgovor nije jedini ulaz u proizvod. Svaka AI preporuka pokazuje izvor i razlog. Spremnost za slanje razlikuje se od poslate poruke, a prihvaćen zahtjev od potvrđene promjene.

Predloženi početni ekran poslije otključavanja je pregled; nova instanca završava uspostavljanje vaulta prije privatnih funkcija. Svijetla tema je početni prijedlog samo ako nema sačuvanog izbora. Raspored, veličine i vizuelni tokeni su projektantski prijedlozi, bez potvrde korisničkim testiranjem. Vrijeme dnevnog pregleda i zona biraju se pri podešavanju; ne podmeće se proizvoljan dnevni termin.

Mjere budućeg ličnog pilota su vrijeme pregleda pošte uključujući provjeru AI prijedloga, vrijeme pripreme odgovora, obaveze završene na vrijeme i greške u razumijevanju akcija. Početno mjerenje i brojčani ciljevi nisu potvrđeni. Nema tvrdnje o poboljšanju ili roku isporuke.

### 1.2. Artefakti i način čitanja

- [ux-copy.json](ux-copy.json): zasebni nacrti sva tri kataloga za tekstove koji su definisani u ovoj specifikaciji, sa imenovanim placeholderima i pluralima. Ovo nije kompletan produkcioni katalog svih budućih ekrana.
- [ux-tokens.json](ux-tokens.json): predložene dvije palete, razmaci, tipografija i dozvoljeni kontrastni parovi.
- [validate-ux.cjs](validate-ux.cjs): dokumentaciona provjera pariteta, placeholdera, CLDR oblika, kontrasta, sljedivosti i putanja linkova.
- [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md): arhitektonska razrada pamćenja teme prije otključavanja.

Tehnički ID-jevi `S01–S16`, `J01–J08`, `FR-*` i `UX-*` nisu UI natpisi. Nazivi ekrana u prozi služe za opis; implementacija koristi kataloške ključeve. Svaki novi natpis pri implementaciji dodaje se u sva tri produkciona kataloga. Izvorni mejlovi, dokumenti i razgovori zadržavaju svoj tekst i jezik.

## 2. Informaciona struktura i raspored

Otključani interfejs ima zajedničko zaglavlje, glavnu navigaciju i radni prostor. Zaglavlje nosi trenutni naziv ekrana, stanje izvora gdje je relevantno i dostupno dugme `vault.lock`. Navigacija redom koristi `nav.overview`, `nav.chat`, `nav.mail`, `nav.calendar`, `nav.tasks`, `nav.documents`, `nav.memory`, `nav.automations`, `nav.actions`, `nav.settings`. Podešavanja sadrže modele/provajdere, integracije, dozvole, jezike/temu, zonu/obavještenja, čuvanje podataka, backup i dijagnostiku.

Na početnoj veličini 1440 × 900 CSS piksela navigacija je široka 232 px, zaglavlje najmanje 56 px, unutrašnji razmak 24 px. Listovi imaju redove visine najmanje 56 px. Detalj izvora ima najmanje 360 px kada stoji pored liste; tijelo za čitanje najviše 800 px, forma najviše 560 px. Sve su donje granice ili maksimalne širine, bez fiksne visine teksta i bez rezanja prevoda.

Zaključani interfejs ima samo nesenzitivno zaglavlje, izbor jezika/teme, unlock/recovery i pomoć za pokretanje. Nema privatnih naslova, broja nepročitanih poruka, liste naloga, istorije, rasporeda ili detalja akcija. Ne otkriva da li je postojao osjetljiv prijedlog.

### 2.1. Strukturne skice

Skice predstavljaju položaj elemenata, nisu prototip. U uglastim zagradama su kataloški ključevi ili tehnički nazivi regiona, bez izmišljenog korisničkog sadržaja.

```text
S02 — zaključani ekran
+------------------------------------------------------------------+
| [settings.uiLanguage]                 [settings.theme]            |
|                                                                  |
|               [vault.locked]                                     |
|               [vault.masterPassword] [vault.showPassword]         |
|               [vault.unlock]                                     |
|               [vault.recovery]                                   |
|               region: validation / temporary preference          |
+------------------------------------------------------------------+

S05 — dnevni radni pregled
+----------------+-------------------------------------------------+
| navigation     | [nav.overview]                    [vault.lock]   |
|                | source freshness / completeness                 |
|                +----------------------+--------------------------+
|                | [mail.important]     | unfinished tasks         |
|                | reasons + source     | origin + due + status    |
|                | [mail.all]           | [tasks.noDueDate]        |
|                +----------------------+--------------------------+
|                | [digest.title] / [digest.catchUp]                |
|                | eligible groups / unavailable sources           |
|                | [actions.awaitingApproval]                      |
+----------------+-------------------------------------------------+

S07/S08 — pošta i lokalni nacrt
+------------+-----------------------+------------------------------+
| navigation | [mail.all]            | selected source / recipients |
|            | [common.search]       | original text                |
|            | importance + reason   | [chat.context]               |
|            | source freshness      | [mail.localDraft]            |
|            | paginated list        | editable draft + save state  |
|            |                       | [mail.reviewSend]            |
+------------+-----------------------+------------------------------+

S09 — modalni pregled slanja
+------------------------------------------------------------------+
| [actions.review]                              [common.close]     |
| [actions.account] / [actions.tool] / [actions.target]              |
| [actions.recipientsTo] / [actions.recipientsCc]                    |
| [actions.recipientsBcc] / [actions.subject]                        |
| [actions.body] / [actions.attachments]                            |
| [actions.consequence] / proposal version / expiry                 |
| [actions.reject]                   [actions.approveSend]          |
+------------------------------------------------------------------+
```

## 3. Ključni korisnički tokovi

### J01 — prvo pokretanje i bezbjedan početak

**Cilj:** Uspostaviti privatnu instancu i obaviti prvi razgovor; FR-001–006/024/025.

`S01 jezik i tema → S02 uspostavljanje vaulta → S03 recovery kod i potvrda čuvanja → izbor AI jezika → S04 model/test veze i politika konteksta → opcioni S15 integracija/dnevni pregled → S05 pregled → S06 razgovor`.

Nova instanca predlaže UI locale iz OS-a prema I18N_SPEC-u. AI jezik se jednom predlaže iz izabranog UI-ja, zasebno potvrđuje i dalje ne prati njegove promjene. Unos lozinke dozvoljava lijepljenje i password manager; nema izmišljenog pravila sastava lozinke. Validacija dolazi iz odobrene vault politike. Recovery kod se prikazuje samo u kontrolisanom koraku; potvrda čuvanja nije unaprijed označena. Ne dodaje se automatsko kopiranje u clipboard.

Korisnik može odložiti model, integraciju i dnevni pregled uz `onboarding.skipOptional`. Tada lokalni UI ostaje dostupan, a zavisna funkcija objašnjava šta nedostaje. Uspostavljanje zaštite se ne preskače da bi cloud obrada proradila. Zatvaranje nakon uspješnog stvaranja vaulta ne briše instancu; pri nastavku prepoznaje se sačuvano stanje, bez automatskog ponovnog stvaranja.

Neuspio test modela ostavlja nepotvrđenu vezu i sačuvanu nesenzitivnu konfiguraciju prema rezultatu upisa. Odbijena cloud obrada ne šalje izabrane izvore. Odbijena OAuth saglasnost ne povezuje nalog i ne traži šira prava.

### J02 — važno pismo, nacrt i odobreno slanje

**Cilj:** Pregledati provjerljiv kontekst, urediti odgovor i namjerno poslati; FR-009/011–014.

`S05 ili S07 → važnost/razlog → original → S08 odabrani kontekst → provjera cloud politike → lokalni nacrt → uređivanje i čuvanje → S09 konačni pregled → izričita potvrda → evidencija ishoda`.

Ručna oznaka ima prednost, zatim prvo primjenjivo poredano pravilo, pa odvojena AI preporuka. Link `mail.all` je stalno dostupan; neprocijenjene poruke su vidljive. Otvaranje za čitanje i računanje važnosti ne mijenjaju automatski izvorni read status. Lokalna oznaka važnosti ne tvrdi da je izmijenjena oznaka na Microsoft nalogu.

Nacrt jasno nosi `mail.localDraft`. Nepotvrđene činjenice označava `mail.unknownFact`. To/CC/BCC, tekst, predmet i prilozi mogu se urediti prije pregleda; svaka izmjena stvara novu verziju. S09 dobija konačni prikaz sa backenda. Odbijanje, Escape ili zatvaranje ne izvršavaju slanje. Promjena argumenta, lock, opoziv ili novija verzija izvora zahtijevaju novi pregled.

Timeout vodi u `actions.unknown`, sa `actions.checkOutcome`; nema dugmeta koje slijepo ponavlja slanje. Prihvatanje slanja od strane servisa koristi `actions.acceptedSend` i ne tvrdi isporuku primaocu. Potvrđeno brisanje koristi zaseban pregled konkretne poruke i posljedice. Vanjsko čuvanje nacrta je posebno odobrenje, dostupno samo uz podržanu sposobnost i dozvolu.

### J03 — pravila važnosti bez skrivanja pošte

**Cilj:** Urediti determinističan prioritet; FR-011/012.

`S07 → pravila → izbor polja/operatora/vrijednosti i važno/nevažno → poredak → lokalni pregled uticaja → čuvanje → osvježavanje klasifikacije`.

Polja su pošiljalac, domen i tema; UI nudi samo operatore koje backend ugovor podržava, bez izvršnog izraza. Domen/pošiljalac koriste jasan prikaz poređenja, a tema samo podržani doslovni/prefix/contains oblik. Poredak se mijenja i dugmadima `mail.moveRuleUp`/`mail.moveRuleDown`, bez obaveznog drag-and-drop-a. Pregled prikazuje prvo podudarno pravilo, eventualnu ručnu oznaku i zasebnu AI preporuku. Uklanjanje ručne oznake vraća procjenu pravila; pravilo nikad ne dodaje sam sadržaj mejla. Neuspješan upis zadržava prethodni poredak.

### J04 — dnevni pregled i jedan naknadni pregled

**Cilj:** Vidjeti stvarno podobne poruke i zadatke; FR-021–023.

`S14 izbor vremena/zone/opsega/kanala → sačuvan raspored → dnevni termin dok je otključano → provjera statusa → S05 pregled`. Ako se termin propusti: `unlock/start → izračun propuštenih termina → osvježavanje izvora → jedan naknadni pregled aktuelnih stavki`.

Početni profil je pošta strogo starija od 72 sata i zadaci koji kasne ili dospijevaju između sada i naredna 24 sata, uključujući oba kraja horizonta. Tačno 72 sata ne ulazi. Bez roka se vidi u opštem pregledu obaveza. Važna, ostala podobna i neprocijenjena pošta ostaju dostupne; ista stavka je najviše jednom u jednom pregledu. I dalje podobna stavka može biti u sljedećem dnevnom pregledu.

Pročitana/završena/otkazana stavka poslije osvježavanja ne ulazi. Neprovjeren cache je izdvojen uz `common.stale`, vrijeme posljednjeg uspjeha i `common.partial`; nije novi potvrđeni podsjetnik. Potvrđene lokalne obaveze rade bez mreže. Odgađanje bira trenutak sljedećeg podsjetnika i ne mijenja rok/status. Dnevni i naknadni posao ne proizvode paralelne kopije istog pregleda. Naknadni pregled nikad ne izvršava propuštena slanja/brisanja.

### J05 — zaključavanje, povratak i oporavak pristupa

**Cilj:** Sačuvati zaštitu i razumjeti nastavak; FR-002/003/023/025.

`ručni lock / sleep / 15 min neaktivnosti → opoziv pristupa → brisanje privatnih UI prikaza → S02 → unlock → kanonska sinhronizacija jezika/teme → ponovno učitavanje dozvoljenog stanja → provjera unknown akcija/naknadnog pregleda`.

AI stream i poslovi ne održavaju sesiju aktivnom. Predloženo upozorenje jednu minutu prije idle lock-a koristi `vault.lockWarning` i dostupno `vault.keepWorking`; upozorenje ne omogućava isključivanje zaštite niti odgađa sleep/ručni lock. Ako se šifrovani nacrt već sačuvao, ponovo se učita; nesačuvano stanje se uklanja i ne obećava povratak. Postoji trajna pomoć `vault.unsavedOnLock` na uređivaču nacrta. Zaključavanje ne čeka neuspješno čuvanje.

Nove privatne operacije se odbijaju. Već poslat vanjski zahtjev ne može se povući zaključavanjem; bez potvrde ostaje unknown, a provjera traži unlock. Rezultat stare sesije ne obnavlja privatni prikaz. Pogrešna lozinka čuva S02 i lokalizovanu grešku; retry ograničenje prikazuje `vault.rateLimited` prema backend parametrima. Recovery koristi važeći kod, novu lozinku i novi kod uz rotaciju prema arhitekturi. Bez pristupnog materijala nema dekripcije ili brisanja baze kao prečice. Granica starih backupa je objašnjena preko `vault.backupBoundary`.

### J06 — dokument, izvor i kontrolisana memorija

**Cilj:** Dobiti provjerljiv odgovor i odabrati šta ostaje u memoriji; FR-017–020.

`S12 lokalni izbor ili odabrani OneDrive opseg → podržani formati/limiti → uvoz i verzija → pretraga → odabrani odlomci i cloud politika → odgovor sa izvorima → opcioni S13 memorijski prijedlog → pojedinačno odobrenje`.

Uvoz ne označava cijeli OneDrive nalog kao dozvoljen. Napredak razlikuje čekanje, obradu, uspjeh, grešku i otkazivanje. Duplikat dobija `documents.duplicate`; neuspjela nova verzija ostavlja prethodnu aktivnu. Svaki citat otvara identitet/verziju i lociranje odlomka gdje format omogućava. Nepostojeći dokaz nosi `documents.noEvidence`. Uklonjen/nedozvoljen izvor nije aktuelni dokaz.

Cloud embeddings imaju zasebno vidljivu svrhu i ista pravila prenosa. Memorija početno traži odobrenje svake činjenice; isključivanje blokira nove upise. Korisnik može ispraviti i ukloniti stavku i izvedene indekse. Uklanjanje lokalne kopije ne briše original na disku/OneDrive-u, a raniji offline backup nije retroaktivno izmijenjen. Tajne nisu činjenice za memoriju.

### J07 — obaveze i kalendar sa stvarnim statusom

**Cilj:** Potvrditi rok i izvor promjene; FR-015/016/021.

`S11 kreiranje lokalne obaveze ili prijedlog iz pošte → potvrda teksta/roka/izvora → čuvanje`. Za povezani To Do ili događaj: `S10/S11 → prijedlog kreiranja/promjene/završavanja/otkazivanja → S09 pregled → odobrenje → potvrda izvora`.

Lokalni i povezani zadatak imaju različite oznake porijekla; AI tvrdnja ne završava zadatak. Rok bez vremena je kraj izabranog dana, odnosno granica početka sljedećeg dana u prikazanoj zoni, a kašnjenje počinje od te granice. Datum bez vremena i cjelodnevni događaj nisu ponoćni UTC trenutak. Promjena zone mijenja prikaz i raspored, bez promjene stvarnih trenutaka događaja. Sukob izmjene traži osvježavanje i novi pregled, bez tihog prepisivanja.

### J08 — backup, restore i povratak na sigurnu instancu

**Cilj:** Obnoviti podatke bez gubitka važeće instance; FR-004/025/027/028.

`S16 izbor šifrovanog backupa → staging i provjera pristupnog materijala/formata/integriteta → pregled obuhvata → backup postojeće instance → izričita potvrda zamjene → kontrolisani commit → zaključana obnovljena instanca → pregled naloga/dozvola/poslova`.

Oštećen, nepodržan backup ili pogrešna lozinka dobijaju `backup.invalid`; postojeća instanca ostaje važeća. Zamjena nije aktivna prije verifikacije i backupa. Tok ne obećava trenutno otkazivanje poslije početka atomarnog preklopa; ako nije bezbjedno, prikazuje napredak i čeka rezultat oporavka. Privremeni fajlovi i generacije ostaju pod kontrolom backend servisa. Restore čuva oba jezika i temu, migrira stari locale i ne obnavlja prethodna odobrenja kao važeća. Poslovi ostaju pauzirani do preispitivanja; poruka koristi `backup.reviewPermissions`.

## 4. Specifikacija ekrana

Svaki ekran nasljeđuje stanja iz §5, pristupačnost iz §7 i lokalizaciju iz §8. Ispod su dodatni regioni, akcije i ponašanje fokusa. Nova rutinska navigacija stavlja fokus na naslov glavnog prostora; povratak iz detalja vraća odabranu stavku i položaj liste kada podaci to omogućavaju.

| ID | Ekran i raspored | Interakcija i posebna stanja | Ulaz/fokus i provjera |
|---|---|---|---|
| S01 | Početno podešavanje; jedna forma do 560 px, jezik/tema, napredak i povratak | Prijedlog OS-a samo bez sačuvanog izbora; odlaganje samo opcionih koraka; nastavak poslije prekida | Naslov pa izbor; J01, TC-001/024 |
| S02 | Uspostavljanje ili otključavanje; label, maskirani unos, recovery, jezik/tema | Locked/unlocking/error/ograničen retry; privremeni jezik/tema jasno označeni; bez privatnog sadržaja | Naslov/unos; potvrda samo unlock operacije; J05, TC-002 |
| S03 | Recovery i promjena lozinke; kontrolisana forma sa granicom oporavka | Kod samo u zaštićenom toku, potvrda čuvanja, nova generacija i kod; nema auto clipboard kopije | Naslov pa kod/unos; bez čitanja koda kroz status poruku; TC-003 |
| S04 | Modeli/provajderi; lista konfiguracija i detaljna forma | BYOK jednokratan unos, status sačuvanog ključa bez vrijednosti, test/sposobnosti, primarni/rezervni model, trust odredišta i limit | Naslov pa odabrana konfiguracija; rezultat testa je trajni status; TC-005/006/026 |
| S05 | Pregled; važna pošta, obaveze i dnevni/naknadni pregled; status izvora iznad grupa | Puna pošta dostupna; aktuelne i neprovjerene grupe odvojene; prazno nije isto što i nepotpuno | Naslov pa grupe; otvaranje izvora ne mijenja status; TC-011/022/023 |
| S06 | Razgovor; istorija sa strane kada ima mjesta, glavni tok, composer i izabrani kontekst | Vidljiv model/odredište, prekid, nepotpun odgovor, izvor i prijedlog akcije; jedan aktivan chat po početnoj arhitekturi | Composer ima label; stream ne pomjera fokus; TC-007/009 |
| S07 | Pošta i pravila; filter/pretraga, lista i detalj, pravila u zasebnom panelu | Straničenje, nalog, razlog i izvor, AI odvojeno, ručna oznaka, poredak pravila; puni pregled ostaje dostupan | Lista sa jasnim linkom na detalj, bez pretrpanih tab stopova; TC-011/012 |
| S08 | Lokalni nacrt; original/kontekst uz editor, primaoci i prilozi prije pregleda | Save state/version, nepoznate činjenice, greška upisa, opozvan izvor, pregled slanja ili posebnog vanjskog drafta | Editor sa labelom; izlaz sa nesačuvanim izmjenama ima sačuvaj/odbaci/ostani; TC-013/014 |
| S09 | Konačni pregled akcije, modal; naslov, identitet/cilj, kompletni argumenti, posljedica i odluka | Fingerprint/verzija/istek; promjena/lock/revoke poništava odobrenje; unknown nema slijepi retry | Fokus na naslov pregleda; potvrda bez autofocus-a i bez globalnog Enter handlera; TC-009/014 |
| S10 | Kalendar; dnevna agenda i detalj događaja; zona uz datume | Cjelodnevni odvojeni; promjene prolaze S09; source conflict i nepotpun refresh vidljivi | Agenda dostupna tastaturom i pri maloj širini; TC-015 |
| S11 | Obaveze; filter lokalni/To Do/status/rok, lista i forma | Bez roka jasno; kraj dana objašnjen; prijedlog iz mejla nije sačuvana obaveza; To Do promjene kroz S09 | Kontrole završavanja imaju naziv i izvor; TC-016/021 |
| S12 | Dokumenti; lokalni/OneDrive izbor, lista uvoza, verzija i čitač izvora | Prije izbora formati/limiti iz capabilities; unsupported/error/duplicate/cancelled, lokacija citata, cloud embeddings | Izbor fajla kroz broker; fokus poslije odlomka ima povratak; TC-017–019 |
| S13 | Memorija; stanje uključivanja, prijedlozi i sačuvane činjenice sa porijeklom | Pojedinačna potvrda, ispravka, brisanje/indeksi, offline backup granica | Uključivanje ne odobrava sve prijedloge; TC-020 |
| S14 | Automatizacije; raspored, sljedeći termin/zone, posljednji rezultat i kontrola | Opseg/starost/horizont/kanal/vrijeme, pauza, missed/failed/catchUp; bez tajnih poslova tokom lock-a | Objašnjenje runtime granice trajno dostupno; TC-022/023 |
| S15 | Podešavanja/integracije; odjeljci i jedna forma; pregled promjena prije upisa | Dva jezika i tema nezavisni, čuvanje poslije DB uspjeha, zona, dozvole, retention, OAuth i opoziv | Radio grupa svijetla/tamna, vidljiv save state; TC-010/024/025 |
| S16 | Backup/restore i dijagnostika; koraci verifikacije, obuhvat i lokalni izbor | Neuspjela provjera čuva bazu; zamjena poslije backupa; redakcija izvoza; jasan tretman podataka pri deinstalaciji | Fokus na pregled posljedice prije potvrde; TC-004/027/028 |

## 5. Zajednička stanja i pravila prelaza

| Stanje | Prikaz i sljedeća radnja | Pravilo integriteta |
|---|---|---|
| Učitavanje | `common.loading`, stabilan raspored/skeleton, relevantni region `aria-busy` | Spinner nije potvrda uspjeha; drugi nezavisni prikazi ostaju dostupni |
| Prazno | `common.empty`, po potrebi akcija povezivanja/uvoza | Važi tek za uspješno poznat opseg; ne koristi se za pad mreže |
| Nema rezultata | `common.noResults` i izmjena/uklanjanje filtera | Postojeći podaci se ne brišu |
| Izvor nedostupan | `common.unavailable`, ponovni login/refresh i posljednji uspjeh | Lokalni cache se ne proglašava svježim |
| Nepotpuno/zastarjelo | `common.partial`/`common.stale`, posebno neprovjerene stavke i `common.lastSync` | Neprovjeren status nije novi potvrđeni podsjetnik |
| Greška | Stabilan kod preveden u `common.error` ili poseban ključ; opisana naredna radnja | Sanitizovani parametri; nema tajne, stack trace-a ili sirovog SDK teksta |
| Nesačuvano/čuvanje | `common.unsaved`/`common.saving`; potvrda tek nakon upisa | Neuspješan upis zadržava prethodni kanonski zapis |
| Zaključano | `vault.locked`, unlock i nesenzitivni izbori | Privatni regioni se uklanjaju, uključujući accessibility stablo i stale stream |
| Odbijeno/isteklo/otkazano | `actions.rejected`/`actions.expired`/`actions.cancelled` | Odbijanje i zatvaranje ne izvršavaju promjenu |
| Nepoznat ishod | `actions.unknown`, read provjera izvora poslije unlock-a | Bez automatskog retry-a ili lažne oznake poslato |

Vault prati `Uninitialized → Locked → Unlocking → Unlocked → Locking → Locked`; neuspjeli unlock ostaje Locked. UI ne izvodi uspjeh iz lokalnog završetka animacije.

Akcija prati `proposed → awaitingApproval → approved → executing → succeeded | failed | unknown`, uz rejected/expired/cancelled. Odobrenje se odnosi na tačno backend prikazane argumente. Prozor S09 se ne otvara preko composera samovoljno: prijedlog prvo dobija dostupnu karticu, a vlasnik namjerno bira pregled. Otvaranje ne dozvoljava nastavku kucanja da potvrdi akciju. Samo fokusirano dugme može se aktivirati novim Enter/Space događajem ili namjernim klikom; držani taster i globalni form submit ne smiju odobriti. Backend ponovo provjerava fingerprint, sesiju, dozvole i izvor.

Posao ima queued/running/waitingForUnlock/missed/completed/failed/cancelled. Dok je zaključano ne čita se privatni raspored iz baze radi prikaza statusa. Generičko obavještenje koristi `notifications.private`; detalji se prikazuju tek uz provjeren unlock. Timeout sync-a ima ograničen retry, dok vanjska promjena ostaje u evidenciji nepoznatog ishoda do provjere.

Nacrt se čuva lokalno šifrovano tokom rada kroz podržani servis. Prijedlog implementacije je odloženo čuvanje poslije uređivanja i izričito dugme; tačan debounce i politika konflikta utvrđuju se u aktivnoj fazi. UI nikad ne predstavlja nepotvrđen upis kao sačuvan. Navigaciona potvrda ne sprečava sigurnosni lock.

## 6. Komponente i vizuelni tokeni

| Komponenta | Izgled i stanja | Pravila interakcije |
|---|---|---|
| Dugme | Visina/target najmanje 44 px, razmak 8 px, radijus 8 px; default/hover/focus/active/disabled/loading | Primarno je jedna glavna radnja regiona; u potvrdi konkretna posljedica. Disabled prati vidljiv razlog, bez oslanjanja na tooltip |
| Forma | Label iznad unosa, najmanje 44 px visina; helper/error povezani preko `aria-describedby` | Provjera na blur/submit; greške nisu samo boja. Submit omogućava otkrivanje grešaka osim dok traje zahtjev; fokus na prvu grešku |
| Osjetljivi unos | Lozinka maskirana, dostupno pokaži/sakrij samo trenutni unos; API ključ bez čitanja sačuvane vrijednosti | Unos se šalje jednokratno i čisti po završetku/lock-u; nema localStorage/sessionStorage/debug zapisa; nema automatskog clipboard-a |
| Izbor teme/jezika | Native radio/select sa povezanim opisom; duži nazivi se prelamaju | Tema ima samo light/dark; tri UI opcije i četiri AI opcije. Promjena jednog ne dira druga dva izbora |
| Lista izvora | Naslov, nalog/porijeklo, status, vrijeme, razlog i otvori izvor | Straničenje ostaje dostupno; keyboard detalj vraća fokus. Virtualizacija tek uz dokazan screen reader obrazac |
| Kartica akcije | Prijedlog/odobrenje/izvršavanje/ishod imaju tekst i ikonu | Kartica nije dokaz izvršenja. S09 uz detalje omogućava odbijanje i namjernu potvrdu |
| Modal/panel | Naslov, sadržaj i akcije, do 800 px za pregled; dug sadržaj se skroluje | Fokus ograničen na modal, background inert, Escape odbija/otkazuje prije izvršavanja; povratak na pokretač |
| Status/toast | Tekst + ikona, uspjeh i greška različiti; kritični status ostaje u ekranu | `role=status` za rutinu, `role=alert` za hitnu grešku; toast nije jedino mjesto greške, ne krade fokus |
| Citirani sadržaj | Original označen izvorom/verzijom i vlastitim jezikom | Sanitizovan prikaz bez aktivnih instrukcija; spoljni tekst nije kontrola ili odobrenje |

Palete u `ux-tokens.json` imaju tekst, sekundarni tekst, accent, success, warning, danger, border i focus za svijetlu/tamnu pozadinu. Svi deklarisani parovi običnog teksta ciljaju najmanje 4,5:1; nužne granice i fokus najmanje 3:1. Button hover/active i tekst na njima provjeravaju se zasebno. Fokus se iscrtava spolja na canvas/surface, sa razmakom od ispunjenog dugmeta. Semantička boja se uvijek dopunjava tekstom/ikonom. Nisu dozvoljene proizvoljne kombinacije tokena bez nove provjere kontrasta.

Osnova je sistemski font sa podrškom za č, ć, š, đ, ž; body 1 rem, pomoćni tekst 0,875 rem, naslov 1,75 rem i podnaslov 1,25 rem, line-height 1,5. Razmaci su 4/8/12/16/24/32/48/64 px. Fontovi ne zahtijevaju cloud preuzimanje. Animacije služe povratnoj informaciji; smanjeni pokreti isključuju dekorativne prelaze. Ne uvodi se novi paket ikona ili fontova dok aktivna faza ne provjeri podršku/licencu.

## 7. Pristupačnost i ponašanje pri promjeni veličine

Projektantski cilj je WCAG 2.2 AA za webview, uz obavezne projektne tastaturne kriterijume. To je cilj za implementaciju, bez tvrdnje o sertifikaciji. W3C definiše tekstualni kontrast, reflow, tastaturu i pristupačnu autentifikaciju; ovdje se za sav deklarisani tekst koristi stroži jedinstven prag 4,5:1. Projektni target od 44 px je izbor dizajna, iznad minimuma 24 px uz izuzetke iz 2.5.8. [W3C WCAG 2.2](https://www.w3.org/TR/WCAG22/)

| ID | Testabilan UX uslov | Planirana provjera |
|---|---|---|
| UX-A11Y-01 | Semantički header/nav/main, jedan glavni naslov, logična hijerarhija; skip link | Tastatura i accessibility stablo za S01–S16 |
| UX-A11Y-02 | Sve akcije Tab/Shift+Tab/Enter/Space; fokus vidljiv i nezaklonjen; nema pozitivnog tabindex-a | Proći osnovne tokove bez miša, oba kontrastna režima |
| UX-A11Y-03 | S09 ne prima slučajno odobrenje iz composera; fokus na naslov, tab ostaje u dijalogu i vraća se poslije zatvaranja | Držani Enter, novi Enter, Escape, zatvaranje, lock i nestanak pokretača |
| UX-A11Y-04 | Label/name/value lokalizovani; greške povezane s poljem i rutinski statusi čitljivi | Windows Narrator i NVDA na stvarnom Tauri webview-u, sva tri jezika |
| UX-A11Y-05 | Stream ne čita svaki token; početak/prekid/kraj imaju kratke status poruke, cijeli odgovor ostaje dostupan | Chat tastaturom/čitačem, bez preplavljivanja live regiona |
| UX-A11Y-06 | Bez funkcije koja zavisi isključivo od boje, hover-a ili povlačenja | Pravila pomjerati dugmadima; tooltip dostupan na fokus i može se zatvoriti |
| UX-A11Y-07 | Tekst 200%, reflow ekvivalent 320 CSS px, promjena text spacing-a bez gubitka funkcije | 1280 px na 400% za reflow; ne poistovjećivati sa fizičkim mobilnim klijentom |
| UX-A11Y-08 | Paste/password manager dozvoljeni; autentifikacija bez memorijskog zagonetnog zadatka | Unlock/recovery tastaturom; vidljiva pomoć i sanitizovane greške |
| UX-A11Y-09 | Idle upozorenje dostupno, nastavak radom; privatni sadržaj nestaje na lock-u i iz accessibility stabla | Sleep/idle/manual lock usred dijaloga i stream-a |
| UX-A11Y-10 | Reduced motion i Windows forced-colors zadržavaju fokus, kontrole i statuse | Ručni pregled u OS režimima i obje teme |

Modal koristi pravilno imenovan `role=dialog`, `aria-modal=true`, Tab ciklus i povratak fokusa; dug strukturisan pregled počinje fokusom na naslovu. Escape je otkazivanje odluke, bez izvršavanja. Ova primjena slijedi [W3C APG modalni obrazac](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/); posljedice, fingerprint i namjerno odobrenje su dodatna pravila proizvoda.

| Efektivna širina | Raspored |
|---|---|
| 320–767 CSS px | Jedna kolona; navigacija iza dugmeta, lista i detalj kao odvojeni koraci sa povratkom; akcije se prelamaju, modal koristi raspoloživu širinu |
| 768–1023 CSS px | Sklopiva navigacija; glavni prostor i detalj samo ako stvarno staju, inače detalj zamjenjuje listu |
| 1024–1439 CSS px | Navigacija i glavni prostor; lista/detalj se spajaju samo uz minimalne širine; bez forsiranja treće kolone |
| ≥1440 CSS px | Navigacija, lista i čitač/editor; pregled može imati dvije sadržajne kolone |

Pravila se primjenjuju na mali Windows prozor i uvećanje webview-a. Mobilni klijent i udaljeni web pristup ostaju van MVP-a. Duge adrese/URL-ovi se bezbjedno prelamaju; pretraga i filteri se slažu vertikalno. Agenda je alternativa prostornom kalendaru; obični tekst i forme ne traže dvodimenzionalni scroll. Sticky zaglavlje i footer ne smiju zakloniti fokus ili dio konačne odluke.

## 8. Jezici, tema i vrijeme

### 8.1. Tri lokalizacije i nacrt tekstova

UI ima `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk`, `en-US`; AI ima ista tri i `follow-user`. `settings.uiLanguage`, `settings.aiLanguage` i `settings.languagesIndependent` razdvajaju izbor. Nazivi opcija dolaze iz `settings.languageEkavsk`, `settings.languageIjekavsk`, `settings.languageEnglish`, uz `settings.latinScript`.

`ux-copy.json` sadrži zasebno napisane vrijednosti, bez automatskog izvođenja jedne srpske varijante iz druge. U ijekavici se koriste nisu/nisam/sutra i vrijeme/sljedeći/izvještaj. Svi tekstovi potvrda, grešaka, obavještenja, labela i accessibility opisa pri implementaciji moraju proći kroz tri kataloga. Parametri su imenovani `{{time}}`, `{{zone}}`, `{{count}}` i slično; nepouzdan sadržaj se ne interpolira kao HTML. Backend šalje stabilne kodove i sanitizovane parametre.

Obje srpske varijante mapiraju samo formatiranje na `sr-Latn-RS`; sačuvani kod i katalog ostaju puni. `digest.items` ima srpske one/few/other i engleske one/other oblike prema CLDR-u. Broj se formatira `Intl.NumberFormat`, datum/vrijeme `Intl.DateTimeFormat` u zasebnoj korisničkoj zoni. Relativni tekst i nazivi dana/mjeseci moraju jezički odgovarati varijanti; pogrešan runtime tekst zamjenjuje se provjerenim kataloškim oblikom, bez mijenjanja sačuvanog locale-a.

Sačuvani izbor ima prednost nad OS prijedlogom. Stari sačuvani `sr-Latn-RS` postaje ijekavica, dok isti OS kod na novoj instalaciji predlaže ekavicu. Promjena jezika čuva editor i selekciju, bez tihe promjene AI jezika ili sadržaja izvora. Kanonski upis prethodi potvrdi promjene; greška bootstrap kopije je zasebna poruka. Zaključani jezik je privremen do otključavanja i izričitog čuvanja, prema `vault.temporaryLocale`/`vault.keepLocale`.

### 8.2. Pamćenje svijetle/tamne teme

Korisnik vidi radio grupu `settings.theme` sa `settings.light`/`settings.dark` i opisom `settings.themeIndependent`. Izbor je `ui.theme = light | dark`; ne zavisi od jezika, OS teme ili login sesije. Poslije uspješnog čuvanja važi na narednom startu i zaključanom ekranu. Kanonska šifrovana postavka i nesenzitivna bootstrap kopija slijede ADR-005; upis locale-a ne smije izbaciti temu ili obratno.

U zaključanom stanju može se privremeno promijeniti izgled radi čitljivosti. Prikazuju se `vault.temporaryTheme` i, poslije unlock-a, opcija `vault.keepTheme`; bez izričitog čuvanja vraća se kanonski izbor. Neuspjeli DB upis koristi `settings.themeSaveFailed`, a neuspjela bootstrap sinhronizacija `settings.themeBootstrapFailed`. UI čuva prethodni izbor i ne predstavlja nesinhronizovanu kopiju kao garantovano pamćenje zaključanog prikaza.

### 8.3. Datumi, zone i raspored

Vrijeme dnevnog pregleda nema proizvoljan default: korisnik potvrđuje lokalno vrijeme i podržanu zonu; prikazuje se sljedeći stvarni termin preko `digest.nextRun`. Nepodržana zona se odbija uz očuvanje posljednje važeće vrijednosti. `digest.dst` objašnjava preskok na prvi važeći lokalni trenutak i raniji od dva trenutka pri ponavljanju. Promjena zone ne izvršava ponovo obrađeni dnevni termin.

Relativni rok uvijek ima dostupno apsolutno vrijeme i zonu. Lokalni date-only rok prikazuje `tasks.endOfDay`, bez skraćivanja na izmišljeno 23:59. Vrijeme UTC/ISO ostaje u API-ju; UI locale formatira prikaz, AI jezik to ne određuje.

## 9. Plan provjera i predaja razvoju

Sljedeći scenariji su planirani, bez tvrdnje da su izvršeni na proizvodu. Dva nivoa se odvajaju: dokumentacioni validator sada i unit/integracioni/E2E/ručni testovi kada postoje kod i aktivna faza.

| ID | Buduća provjera | Očekivani dokaz |
|---|---|---|
| UX-CORE-01 | J01–J08 tastaturom u sve tri lokalizacije i obje teme | Zabilježeni koraci, stanje/ishod, fokus, screenshots bez privatnih podataka |
| UX-CORE-02 | Pogrešan unlock, mrežni kvar, nepotpun izvor, prekid stream-a, revoke/lock usred akcije | Jasna različita stanja i nula neautorizovanih poziva |
| UX-CORE-03 | Argument izmijenjen nakon approval-a, nastavak kucanja, dvostruki klik, timeout mutacije | Odbijeni stari approval/replay; unknown bez slijepog retry-a |
| UX-CORE-04 | Odbijanje cloud scope-a/embeddings i novi rezervni provajder | Transport dokaz nula zabranjenih prenosa; lokalni pregled ostaje |
| UX-CORE-05 | 72h/72h+1s, now, +24h/+24h+1s, bez roka, više propuštenih dana, DST/zone | Tačna podobnost, jedan catchUp i bez potvrđenog podsjetnika iz stale cache-a |
| UX-I18N-01 | Paritet svih produkcionih ključeva/placeholdera/CLDR i srpski jezički pregled | CI dokaz bez sirovih ključeva/HTML-a; runtime fallback nije release prolaz |
| UX-I18N-02 | Svih 12 UI/AI kombinacija, start/update/lock/restore, legacy i prekid upisa | Puni kodovi očuvani; izvori nisu nepotrebno prevedeni |
| UX-THEME-01 | Light i dark izbor, zatvaranje i cold start | Isti sačuvani izbor prije i poslije unlock-a |
| UX-THEME-02 | Lock/sleep/ponovno prijavljivanje i update | Tema nije resetovana; privatni podaci su uklonjeni |
| UX-THEME-03 | Restore na novu ili postojeću instancu | Kanonska tema i bootstrap usklađeni; poslovi nisu automatski aktivirani |
| UX-THEME-04 | Greška DB upisa/bootstrap kopije i konflikt vrijednosti | Prethodni kanonski izbor očuvan, upozorenje, ponovna sinhronizacija |
| UX-THEME-05 | Stari zapis bez teme, nevažeća tema, proizvoljna polja i privremeni locked izbor | Bez gubitka locale-a, bez pristupa tajnama i bez tihog kanonskog prepisivanja |
| UX-THEME-06 | Svih šest kombinacija UI/teme i svih 24 kombinacije UI/AI/teme | Tema, UI jezik i AI jezik su nezavisni |
| UX-DATA-01 | Prekid upisa nacrta, recovery rotacije i restore commit-a | Očuvana posljednja važeća generacija; ispravno stanje poslije restarta |

Implementacija zadržava postojeće portove i backend ovlašćenja. React/Tauri renderer ne pristupa SQLite-u, sačuvanim ključevima ili SDK-ovima. UI dobija status/capabilities/konačni pregled, a backend odlučuje da li poziv smije da se izvrši. Formati/limiti dokumenata, jedinice limita troška, vault politika i OAuth prava ne izmišljaju se u UI-ju; dolaze iz provjerenog ugovora relevantne aktivne faze.

Plan rada po fazama ostaje 01–09. U Fazi 01, kada bude zatražena, primjenjuju se shell, jezik, tema i nesenzitivni startup/error obrasci. Vault/kanonski storage u Fazi 02 čeka G-001 i dokaz rotacije/prekida. Ostali ekrani dolaze u svojim izričito aktiviranim fazama. Ovo nije odobrenje za njihovo započinjanje niti dokaz acceptance kriterijuma neaktivnih fajlova.

## 10. Sljedivost PRD zahtjeva

### 10.1. Funkcionalni zahtjevi

| Zahtjev | UX ekran/tok | Pravilo i planirana provjera |
|---|---|---|
| FR-001 | S01/S02; J01 | Nastavak setup-a i startup greška, bez brisanja; TC-001 |
| FR-002 | S02/S03; J05 | Lock/idle/sleep, pogrešna lozinka, bez diskovnog ključa; TC-002 |
| FR-003 | S03; J05 | Kontrolisan recovery, novi kod i granica starih kopija; TC-003 |
| FR-004 | S16; J08 | Verifikacija prije zamjene, backup i zaključan restore; TC-004 |
| FR-005 | S04; J01 | BYOK i capabilities/test, sačuvana tajna se ne vraća; TC-005 |
| FR-006 | S04/S06; J01 | Trust odredišta i vidljiv rezervni model/politika; TC-006 |
| FR-007 | S06; J01/J05 | Istorija, prekid/nepotpun odgovor, nezavisan AI jezik; TC-007 |
| FR-008 | S09/S15; J02 | Default deny, registrovani alat i revoke/recheck; TC-008 |
| FR-009 | S09; J02/J07 | Kompletni argumenti, namjerna potvrda i unknown; TC-009 |
| FR-010 | S15; J01 | OAuth browser/saglasnost/opoziv i zastarjeli cache; TC-010 |
| FR-011 | S05/S07; J02/J03 | Puna pošta, pagination, izvor i freshness; TC-011 |
| FR-012 | S07; J03 | Ručna oznaka → prvo pravilo → AI, dostupna neprocijenjena pošta; TC-012 |
| FR-013 | S08; J02 | Odabrani kontekst, provjerljive činjenice, lokalni nacrt; TC-013 |
| FR-014 | S08/S09; J02 | Konačni send/delete, novi approval poslije izmjene; TC-014 |
| FR-015 | S10/S09; J07 | Zone/date-only i conflict bez prepisivanja; TC-015 |
| FR-016 | S11/S09; J07 | Identitet i potvrđen status To Do, autorizovane promjene; TC-016 |
| FR-017 | S12; J06 | Izabrani OneDrive opseg, verzija i nedostupan izvor; TC-017 |
| FR-018 | S12; J06 | Capabilities prije uvoza, duplicate/version/failure; TC-018 |
| FR-019 | S12/S06; J06 | Dokaz sa lokacijom/verzijom, cloud scope; TC-019 |
| FR-020 | S13; J06 | Prijedlog/odobrenje, isključenje i uklanjanje indeksa; TC-020 |
| FR-021 | S11; J07 | Lokalni/To Do razdvojeni, bez roka i kraj dana; TC-021 |
| FR-022 | S05/S14; J04 | Podesivi profil i provjerene 72h/24h granice; TC-022 |
| FR-023 | S14/S02/S05; J04/J05 | Samo unlocked, jedan catchUp i neprovjeren cache odvojen; TC-023 |
| FR-024 | S01/S02/S15; J01/J05 | Tri UI/četiri AI izbora, legacy/persistence i paritet; TC-024 |
| FR-025 | S15/S16; J08 | Validan save, zona/retention, tema i privatna obavještenja; TC-025 + UX-THEME-01–06 |
| FR-026 | S04/S15; J01 | Measured/estimated/unknown i ograničenje prije zahtjeva; TC-026 |
| FR-027 | S09/S16; J02/J08 | Status akcije, redigovan lokalni izvoz, bez tajnih logova; TC-027 |
| FR-028 | S01–S16; J01–J08 | Demo cijelog MVP-a, install/update/restore i tretman podataka; TC-028 |

### 10.2. Nefunkcionalni zahtjevi

| Zahtjev | UX primjena | Planirana provjera |
|---|---|---|
| NFR-001 | Startup bez LAN/public opcije, sanitizovana lokalna greška | TC-101 ostaje backend transport negativni dokaz |
| NFR-002 | Jednokratni unos/recovery i zaključan ekran, bez plaintext izbora | TC-102; G-001 nije riješen UX-om |
| NFR-003 | UI status/capability bez sačuvanih tajni i direktnog skladišta | TC-103; IPC/zavisnosti i privatne instance |
| NFR-004 | Izvorni sadržaj jasno odvojen, pregled i revoke/lock | TC-104; UX-CORE-02/03 |
| NFR-005 | Odredište/resurs/svrha prije cloud obrade i embeddings | TC-105; UX-CORE-04 |
| NFR-006 | Staging/commit stanja, upis prije uspjeha i čuvanje važećeg izbora | TC-106; UX-DATA-01 |
| NFR-007 | Tri kataloga, placeholder/CLDR, format i accessibility tekst | TC-107; UX-I18N-01 |
| NFR-008 | Odvojeni jezici, legacy, bootstrap/canonical i tema | TC-108; UX-I18N-02, UX-THEME-01–06 |
| NFR-009 | Freshness, missed/catchUp, DST i unknown bez retry-a | TC-109; UX-CORE-05 |
| NFR-010 | Vidljiv napredak i prekid, pagination i ograničena konkurentnost | TC-110; pragovi ostaju nepotvrđeni, bez benchmark rezultata |
| NFR-011 | Tastatura, fokus, jasne posljedice i različita failure stanja | TC-111; UX-A11Y-01–10 |
| NFR-012 | Portovi/backend granice, ugovori/capabilities i fazna predaja | TC-112; UX nije izbor novih zavisnosti |

## 11. Otvorena pitanja, rizici i sljedeći korak

- **G-001–004/006:** Kriptografska distribucija/KDF/recovery dokaz, transport, Microsoft prava i unknown provjera, formati/limiti parsera i sigurnosne trke ostaju otvoreni prema gate izvještaju. UX ih prikazuje, ne dokazuje.
- **G-005:** Vlasnik bira vrijeme/zonu tokom upotrebe. Tip Microsoft naloga, konkretne jedinice limita, podaci za pilot i brojčani ciljevi još nisu potvrđeni. UX izgled i početna svijetla tema su prijedlozi; zahtjev za dvije teme i trajno pamćenje je potvrđen.
- Potrebno je ručno potvrditi tokove sa vlasnikom na budućem prototipu, naročito razlikovanje nacrta/slanja i neprovjerenih podsjetnika. Bez toga nema tvrdnje o testiranoj upotrebljivosti.
- Bootstrap format sada ima dvije nesenzitivne UI postavke. Implementacija mora očuvati jednu pri upisu druge i dokazati ponašanje pri grešci/restore-u; to je dodatni test obima aktivirane faze.

Sljedeći preporučeni BMAD korak je `bmad:sprint-plan` tek na zahtjev vlasnika, uz izbor implementacionog obima i očuvanje gate uslova. Početak razvoja zahtijeva izričito aktiviranje Faze 01; tada se učitavaju njeni kriterijumi. Ovaj UX tok završava samo projektantski artefakt.

## 12. Evidencija stvarnih provjera

| Komanda/provjera | Stvarni rezultat 12.10.2026. |
|---|---|
| `node docs/bmad/validate-ux.cjs` | Izlazni kod 0, PASS: tri kataloga, 200 semantičkih ključeva, paritet imenovanih placeholdera, CLDR pluralne kategorije, sva korišćena referencirana imena ključeva, postojeće lokalne putanje linkova i redovi sljedivosti svih 28 FR/12 NFR. Provjereno 44 deklarisana kontrastna para obje teme; najmanji kontrast 4,55:1. |
| Isti validator, primjeri formatiranja | UTC `2026-10-12T07:05:00Z`, zona `Europe/Belgrade`: obje srpske varijante `12. 10. 2026. 09:05`, ENG `Oct 12, 2026, 9:05 AM`. Broj `12345.67`: srpski `12.345,67`, ENG `12,345.67`. Plurali provjereni za 0/1/2/5/11/21/22/101/1,5; to nisu testovi kalendara/DST aplikacije. |
| `python C:/Users/Korisnik/.agents/skills/bmad-ux-designer/scripts/contrast-check.py '#1D4ED8' '#FFFFFF'` | Izlazni kod 0, 6,70:1 za accent na svijetloj površini. |
| `python C:/Users/Korisnik/.agents/skills/bmad-ux-designer/scripts/contrast-check.py '#CBD5E1' '#111827'` | Izlazni kod 0, 11,95:1 za sekundarni tekst na tamnoj površini. Skripta ima zastarjele opise velikog teksta u px; oni nisu korišćeni za izbor pragova. |
| `bash C:/Users/Korisnik/.agents/skills/bmad-shared/scripts/update-workflow-status.sh --workflow ux_design --status-file bmad/workflow-status.yaml --output docs/bmad/ux-design.md` | Pokrenuto kroz Git Bash; izlazni kod 0. UX completed, ukupno 5/10 tokova, 50%. |
| `yq -o=json bmad/workflow-status.yaml` i provjera dobijenog strukturisanog stanja | Validan YAML; UX završen, implementation `not_started`, nula priča, gate `passed: false` / `CONDITIONAL PASS` očuvani. |
| `bash C:/Users/Korisnik/.agents/skills/bmad-shared/scripts/next-workflow.sh bmad/workflow-status.yaml` | Izlazni kod 0; `next_intent=bmad:sprint-plan`. To je preporuka, bez automatskog početka sprinta. |
| `git diff --check` | Izlazni kod 0; nema whitespace grešaka u praćenim izmjenama. Novi UX tekstovi i JSON provjeravaju se zasebnim validatorom. |

Na Windows-u Bash komande su pozvane preko `C:/Program Files/Git/bin/bash.exe -lc`; korišćen je postojeći `yq` v4.53.6. Nisu instalirane nove zavisnosti. Git prikazuje postojeća LF/CRLF upozorenja, bez greške provjere. Ranije izmjene vlasnika u radnom direktorijumu su zadržane.

BMAD `current_phase: 4` sada označava naredni preporučeni BMAD korak; `phase_4_implementation.status: not_started` i prazna sprint evidencija potvrđuju da razvoj nije počeo. Opcioni research/brainstorm/tech-spec ostaju neodrađeni, zbog čega helper drži ranije fazne zbirne statuse `in_progress`. Procenat 50% računa tokove uključujući opcione; ne predstavlja procenat implementacije proizvoda.

Aplikacioni build/test, test čuvanja teme u stvarnoj aplikaciji, ručni webview audit i korisnički pilot nisu pokrenuti jer aplikacioni kod ne postoji. UX kvalitet je provjeren na nivou dokumentacionog ugovora; svi runtime kriterijumi iz §9 ostaju planirani.
