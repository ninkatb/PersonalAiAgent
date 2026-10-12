# Opis proizvoda — Personal AI Agent

- **Datum:** 11.10.2026. (`2026-10-11`)
- **Verzija:** 1.0
- **Autor:** Codex, na osnovu razgovora sa vlasnikom i razvojnih specifikacija.
- **Status:** Izrađen na osnovu odgovora vlasnika; spreman kao ulaz za PRD. Brojčani ciljevi ispod su prijedlozi, bez izmjerenih rezultata.
- **Mjerodavni plan:** `docs/codex-plan/00_START_HERE.md`, `DECISIONS.md` i `I18N_SPEC.md`. Aktivna implementaciona faza nije određena.

## 1. Sažetak

Personal AI Agent je privatna desktop aplikacija koja vlasniku pomaže da prepozna važne mejlove, pripremi odgovore i završi obaveze na vrijeme. Danas vlasnik ručno pregleda poštu, sastavlja odgovore i prati stare nepročitane poruke i nezavršene zadatke. Željeni rezultat je manje vremena utrošenog na taj rad i pouzdanije izvršavanje obaveza.

Prvi korisnik je isključivo vlasnik. Prva upotrebljiva verzija obuhvata cijeli već definisani MVP iz razvojnih faza 01–09, uključujući razgovore, Microsoft 365, dokumente, memoriju, automatizacije, podešavanja i bezbjednu Windows instalaciju. Prioritet pošte i obaveza određuje glavne scenarije provjere, bez izbacivanja drugih MVP funkcija.

Vlasnik želi rezultat što ranije, ali konkretan datum nije dogovoren. Razvoj ide po postojećim fazama uz provjere kvaliteta i izričit zahtjev vlasnika za prelazak u narednu implementacionu fazu.

## 2. Problem i trenutni način rada <!-- Problem Statement -->

**Problem:** Ručna selekcija relevantnih mejlova, pisanje odgovora i praćenje obaveza zahtijevaju pažnju vlasnika, uključujući pregled sadržaja koji mu nije važan.

**Potvrđeno u razgovoru:** Vlasnik želi selekciju njemu bitnih mejlova, prijedloge odgovora, podsjećanje na ranije nepročitane poruke i nezavršene zadatke. Sve to trenutno radi ručno.

**Uticaj:** Potrošeno vrijeme i rizik da važna poruka ili obaveza ostanu bez pažnje. Nije potvrđeno da su rokovi već propuštani, niti je izmjereno koliko se vremena dnevno troši.

**Hitnost:** Želja za prvom verzijom što ranije; nema određenog datuma, ugovornog roka ili navedenog vanjskog događaja koji ga nameće.

**Analiza uzroka:** Vlasnik sam povezuje informacije iz pošte i zadataka, procjenjuje važnost i pamti šta zahtijeva nastavak rada. Pretpostavka za provjeru je da objedinjavanje ovog pregleda i pomoć u pripremi odgovora smanjuju ručni rad. Nije sprovedeno posmatranje stvarnog rada ni mjerenje učestalosti.

## 3. Ciljni korisnik i potrebe <!-- Target Users -->

**Primarni korisnik:** Vlasnik projekta, jedan vlasnik po privatnoj instalaciji na računaru. Početni pilot ima jednog korisnika; nema početne grupe kupaca ili testera.

**Zadatak korisnika:** Kada pregledam poštu i planiram obaveze, želim da brzo vidim šta je relevantno i šta još čeka moju reakciju, da bih manje vremena trošio na pregled i završavao obaveze na vrijeme.

**Obavezne potrebe: pristup relevantnoj pošti, prijedlozi odgovora i pregled nedovršenog rada.** Vlasnik zadržava mogućnost da provjeri originalnu poruku, ispravi procjenu važnosti i pregleda nacrt prije slanja.

Pojam „meni bitno” još nije operacionalizovan. U PRD-u treba razraditi podesive kriterijume, npr. pošiljalac, tema, korisnikove oznake i rok, uz jasan razlog preporuke. Ti kriterijumi su prijedlozi za razradu, a ne već potvrđena pravila vlasnika.

Tehnička vještina, broj naloga, dnevni broj poruka i obim dokumenata još nisu utvrđeni. Za pilot su potrebni dostupni testni nalozi i kontrolisani podaci; privatni sadržaj se ne prenosi u Git ili test fixture.

## 4. Predloženo rješenje i opseg <!-- Proposed Solution -->

Privatni agent objedinjuje pregled informacija i obaveza, koristi model koji vlasnik odabere i priprema prijedloge kroz registrovane, validirane alate. Vlasnik povezuje sopstvene naloge i unosi sopstvene API ključeve; lokalni model je predviđena opcija.

| Mogućnost | Korist za vlasnika | Granica ponašanja |
|---|---|---|
| Selekcija relevantnih mejlova | Manje ručnog pregleda nevažnih poruka | Preporuka važnosti ne podrazumijeva brisanje, skrivanje bez povratka ili promjenu statusa poruke |
| Prijedlozi odgovora | Brža priprema odgovora | Pregled nacrta i izričito odobrenje prije slanja |
| Pregled stare nepročitane pošte | Lakše uočavanje poruka koje još traže pažnju | Period pregleda, relevantnost i učestalost podsjetnika razrađuju se u PRD-u |
| Pregled nezavršenih zadataka | Manje zaboravljenih obaveza i jasni rokovi | Status zadatka se ne zaključuje proizvoljno iz AI odgovora |
| Razgovor, kalendar, dokumenti i memorija | Povezan kontekst za rad | Kontrolisan pristup podacima i provjerljivi izvori |
| Podsjetnici i automatizacije | Pravovremena pažnja bez stalnog ručnog pregleda | Izvršavanje samo uz aktivan backend i otključan vault |

**Cijeli MVP prve verzije:**

1. Desktop osnova, lokalni API i sve tri lokalizacije.
2. Šifrovana SQLite baza, vault, master lozinka, recovery i šifrovani backup/restore.
3. Više AI provajdera i BYOK: OpenAI, Anthropic, Gemini, Ollama i eksplicitno odobreni kompatibilni endpointi prema postojećem planu.
4. Razgovori, istorija, registrovani alati, dozvole i odobravanje akcija.
5. Microsoft 365: mejlovi, kalendar, To Do i OneDrive.
6. Učitavanje/pretraga dokumenata i kontrolisana memorija.
7. Obaveze, podsjetnici i zakazani poslovi.
8. Potpuna podešavanja i desktop korisničko iskustvo.
9. Bezbjednosna provjera, testovi i Windows instalacioni paket.

**Kasniji opseg:** Udaljeni pristup, telefon, cloud sinhronizacija, više istovremenih korisnika jedne instance, govor, autonoman browser/desktop control i naplata pretplata ostaju van MVP-a prema planu. Izraz „sve iz specifikacije u prvoj verziji” tumači se kao cijeli definisani MVP; faza 10 je u specifikaciji izričito označena za kasnije.

## 5. Mjere uspjeha <!-- Success Metrics -->

Vlasnik je potvrdio dva ishoda: **uštedu vremena** i **isporuku obaveza na vrijeme**. Početne vrijednosti i brojčani pragovi još nisu potvrđeni. Sljedeći pragovi su radni prijedlozi za PRD i lični pilot, bez tvrdnje o ostvarenju.

| Mjera | Početno stanje | Predloženi cilj | Način i period provjere |
|---|---|---|---|
| Vrijeme aktivnog rada na pošti i praćenju obaveza | Nije izmjereno | Najmanje 30% manje vremena | Ručno mjeriti 5 radnih dana prije pilota i 10 tokom pilota; porediti vrijeme po obrađenoj relevantnoj poruci i zadatku uz sličan obim rada |
| Zadaci završeni do roka | Nije izmjereno | Najmanje 90% zadataka sa eksplicitnim rokom | Tokom prve 4 sedmice pilota porediti broj na vrijeme završenih zadataka sa svim zadacima koji dospijevaju; evidentirati odgođene i otkazane zadatke odvojeno |
| Važne poruke u preporučenom pregledu | Nema označenog uzorka | Najmanje 95% poruka koje vlasnik označi važnim | Vlasnik pregleda kontrolisani uzorak; odvojeno bilježiti propuštene važne i pogrešno preporučene nevažne poruke |
| Potpunost podsjetnika | Nema implementacije | Sve podobne stavke iz kontrolisanog scenarija prikazane jednom prema pravilima | Testirati stare nepročitane poruke i nezavršene zadatke, promjenu statusa, duplikate i zaključavanje vaulta |

Ako nema zadataka sa rokom ili je uzorak premalen, rezultat nije ocjena uspješnosti. Ušteda vremena uključuje provjeru AI prijedloga i ispravljanje grešaka, a ne samo vrijeme generisanja odgovora.

**Obavezni uslovi kvaliteta pri izlasku iz MVP-a:** Nema slanja ili brisanja bez propisane autorizacije, nema čitljivih tajni u datotekama/logovima, sve tri lokalizacije imaju paritet i pamćenje izbora, a šifrovani backup može se obnoviti. Potrebni su stvarni dokazi iz testova i demonstracije; ovaj dokument ih ne zamjenjuje.

Mjerenje pilota je lokalno i dobrovoljno, uz minimalne podatke. Nije predviđeno prikupljanje privatnih poruka kroz telemetry.

## 6. Tržišni kontekst i alternative <!-- Market & Competition -->

Trenutna alternativa, potvrđena intervjuom, jeste ručni rad vlasnika. Drugi potencijalni pristupi su odvojeni alati za poštu, zadatke i razgovor sa AI, ali vlasnik nije naveo konkretne proizvode koje koristi ili želi zamijeniti.

Tržišno istraživanje, poređenje cijena, veličina tržišta i analiza konkurenata nisu sprovedeni. Nema potvrđenih tvrdnji o tržišnoj prednosti ili spremnosti drugih korisnika da plate.

Predložena vrijednost za lični pilot je objedinjavanje pregleda pošte i obaveza uz kontrolu modela, pristupa i vanjskih akcija. Buduća komercijalna ponuda provjerava se zasebno nakon ličnog pilota.

## 7. Poslovni model <!-- Business Model -->

Početna verzija služi vlasniku, bez prodaje, obračuna ili naplate u MVP-u. Mogućnost buduće prodaje ostaje dio vizije iz postojećih specifikacija, ali model licenciranja i cijena još nisu odabrani.

Troškove AI API-ja vlasnik snosi kroz sopstvene naloge. Nije naveden budžetski limit; to ne znači neograničen budžet. PRD treba da obuhvati pregled upotrebe i korisničke kontrole predviđene razvojnim planom. CAC, LTV i prihod se ne procjenjuju za lični pilot.

## 8. Tehnički uslovi i zaštita podataka <!-- Technical Considerations -->

Preuzeti uslovi iz mjerodavnog plana, bez nove arhitektonske odluke:

- Windows 11 x64; Tauri 2, React i TypeScript `strict`; ASP.NET Core/.NET 10 kao lokalni sidecar.
- Backend sluša samo na `127.0.0.1`, uz nasumični port, sesijski token i Origin/CORS zaštitu. Nema javnih endpointa u MVP-u.
- SQLite uz SQLCipher i dodatna enkripcija tajni u kolonama; master lozinka, Argon2id i AES-GCM prema postojećem planu. OS keychain nije obavezan.
- Početno zaključan vault, zaključavanje nakon 15 minuta neaktivnosti i na sleep; bez tajnih poziva ili izvršavanja poslova dok je zaključan.
- Odvojeni interfejsi za modele, konektore, skladište, alate, vault i autentifikaciju. Renderer nema direktan pristup bazi, ključevima ili provajder SDK-ovima.
- Podrazumijevano odbijene dozvole, validirani registrovani alati i nepouzdan tretman vanjskog teksta. Mejl nije instrukcija koja može promijeniti sistemske dozvole.
- Microsoft OAuth kroz public client i PKCE, delegirana prava i saglasnost korisnika; bez aplikacionog secreta u klijentu.
- Cloud AI obrada može zahtijevati slanje odabranog sadržaja provajderu. Lokalno čuvanje podataka samo po sebi ne znači potpuno lokalnu obradu; pristup i slanje sadržaja moraju biti vidljivi i kontrolisani.
- Bez privatnog sadržaja i pravih tajni u testovima, Git-u, konzoli, crash dump-u ili telemetry; migracije bez gubitka podataka.

**Lokalizacija:** Tri zasebna kataloga: `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk`, `en-US`. Obje srpske varijante su latinicom. Pamti se puni kod; regionalno formatiranje za obje srpske varijante koristi `sr-Latn-RS`. Jezik UI-ja i AI odgovora su odvojeni; AI podržava i `follow-user`.

Izabrani UI jezik je dostupan na zaključanom ekranu iz minimalne nesenzitivne bootstrap konfiguracije, a nakon otključavanja sinhronizuje se sa kanonskim šifrovanim podešavanjima. Stari sačuvani `sr-Latn-RS` migrira u ijekavicu. Obavezni su testovi kataloškog pariteta, formata, restart/unlock putanje i kompatibilnosti.

Obim poruka, dokumenata i očekivane performanse treba izmjeriti u pilotu. U ovoj fazi se ne bira novi paket niti tvrdi da su provjereni njegova aktuelna podrška i licenca.

## 9. Rizici, mjere i pretpostavke <!-- Risks & Mitigation -->

| Rizik | Posljedica | Mjera |
|---|---|---|
| Pogrešna procjena važnosti mejla | Važna poruka može ostati bez pažnje | Sačuvati pristup cijeloj pošti, prikazati razlog procjene, omogućiti ispravku i provjeravati propuštene važne poruke |
| Netočan ili neprikladan nacrt odgovora | Pogrešno obećanje, podatak ili ton | Prikaz izvora i pregled vlasnika prije slanja; bez samostalnog slanja |
| Instrukcije skrivene u mejlu ili dokumentu | Neovlašćeni pristup ili akcija | Nepouzdan tretman sadržaja, registrovani alati i provjere dozvola sa negativnim testovima |
| Zaključan vault ili ugašen backend | Podsjetnik nije izvršen u planirano vrijeme | Vidljivo stanje poslova i definisan pregled propuštenih izvršavanja poslije otključavanja; bez čuvanja dekripcionog ključa radi rada u pozadini |
| Previše ili ponovljeni podsjetnici | Nova smetnja i slabije povjerenje | Podesiva učestalost, uklanjanje duplikata i ponovno provjeravanje statusa stavke |
| Cijeli MVP uz želju za što ranijim izlaskom | Pogrešno obećan rok ili preskakanje provjera | Procijeniti obim kroz PRD i priče, raditi po fazama i zadržati uslove kvaliteta |
| Pristup Microsoft nalogu ili native SQLCipher build nije raspoloživ | Blokirana integracija ili vault | Rano provjeriti preduslove u odgovarajućoj fazi; bez nesigurnog fallback-a |
| Cloud obrada ili kompromitovan uređaj | Otkrivanje privatnih podataka | Kontrola slanja sadržaja, minimalna prava i šifrovanje prema planu; jasno definisati granice zaštite |

Vlasnik nije naveo dodatne rizike ili ograničenja; gornji rizici potiču iz plana i analize rješenja. Vjerovatnoće i finansijske posljedice nisu izmjerene.

Pretpostavke koje ostaju za provjeru: pristup podržanom Microsoft nalogu, raspoloživost provajdera ili lokalnog modela, prihvatljivost master lozinke u svakodnevnom radu, kriterijumi važnosti i mjerljivi pragovi uspjeha. Pitanja za kasniji komercijalni pilot iz `DECISIONS.md` ostaju otvorena.

## 10. Resursi i vremenski okvir <!-- Resource Estimates -->

Vlasnik određuje prioritete i provjerava preporuke i lični pilot. Potrebni su razvoj, provjere bezbjednosti, testiranje integracija i priprema instalacije; sastav tima i raspoloživi sati nisu potvrđeni.

Konkretan rok i budžet nisu dogovoreni. Nema osnove da se obeća datum isporuke prije PRD-a, razrade priča i provjere ključnih tehničkih preduslova. „Što ranije” je smjer prioritizacije, uz cijeli MVP kao cilj prve verzije.

Redoslijed rada ostaje razvojni plan 01–09. Djelimični razvojni buildovi mogu služiti provjeri; prva potpuna verzija traži ispunjenje kriterijuma cijelog MVP-a. Ovaj opis ne aktivira implementacionu fazu niti odobrava prelazak između njih.

## 11. Zavisnosti i otvorene tačke <!-- Dependencies -->

**Unutrašnje zavisnosti:** Razvojne specifikacije i I18N pravila, PRD sa mapiranjem zahtjeva, arhitektura i bezbjednosne provjere, priče sa acceptance kriterijumima i stvarni build/test dokazi po fazama.

**Vanjske zavisnosti:** Windows razvojni alati, kompatibilan native SQLCipher build, Microsoft OAuth registracija i saglasnosti, pristup testnom Microsoft nalogu, te sopstveni AI API nalog ili lokalni model.

**Preduslovi za razradu u PRD-u:** Pravila selekcije pošte, starost poruka za podsjećanje, rokovi i način označavanja završenosti zadataka, učestalost podsjetnika i ponašanje poslije propuštenog izvršavanja. Nepoznate vrijednosti se ne predstavljaju kao korisnikove odluke.

Nema prijavljene blokade za pripremu PRD-a. Implementacione zavisnosti još nisu testirane. Postojeće otvorene odluke za komercijalni pilot ne blokiraju početnu razradu lične verzije.

## 12. Naredni koraci i predaja <!-- Next Steps -->

1. Koristiti ovaj opis, intervju i postojeći plan kao ulaz za `bmad:prd` kada vlasnik zatraži taj korak.
2. U PRD-u povezati cijeli MVP iz faza 01–09 sa funkcionalnim i bezbjednosnim kriterijumima. Posebno razraditi selekciju pošte, nacrte odgovora i podsjećanje na ranije nepročitane poruke i nezavršene zadatke.
3. Definisati mjerljive pilot scenarije, početno mjerenje ručnog rada i potvrdu predloženih pragova; raspored i procjenu izvesti iz razrađenog obima.
4. Implementaciju započeti tek kroz izričito aktiviranu fazu postojećeg plana.

**Predaja:** `bmad-product-manager`, nivo projekta 3; završen opis proizvoda; PRD još nije započet. Očekivani naredni dokument: `docs/bmad/prd.md`.

**Preporučeni naredni intent:** `bmad:prd`.

## Dodatak — izvori i razgovor <!-- Interview Summary -->

- `../codex-plan/00_START_HERE.md`: vizija, cijeli MVP, granice i razvojni redoslijed.
- `../codex-plan/DECISIONS.md`: potvrđeni zahtjevi i tehničke pretpostavke.
- `../codex-plan/I18N_SPEC.md`: mjerodavna pravila lokalizacije i trajnog pamćenja jezika.
- `../../AGENTS.md`: pravila rada, zaštita podataka i granice implementacije.
- Razgovor sa vlasnikom u ovom chatu, 11.10.2026; pet odgovora na pitanja za opis proizvoda.

| Tema razgovora | Odgovor vlasnika |
|---|---|
| Problem i alternativa | Selekcija važnih mejlova, prijedlozi odgovora, podsjetnici na stare nepročitane poruke i nezavršene zadatke; sada sve ručno |
| Korisnici | Samo vlasnik za početak |
| Uspjeh | Ušteda vremena i isporuka obaveza na vrijeme |
| Opseg i hitnost | Što ranije, sve iz specifikacije u prvoj verziji |
| Dodatna ograničenja | Za sada nema dodatnih navedenih ograničenja |

Pokrivenost intervjua: problem, korisnik, trenutni način rada, hitnost i uspjeh potiču iz odgovora; uslovi, rizici i zavisnosti iz postojećih specifikacija i jasno označene analize; naredni korak je PRD. Nije sprovedeno tržišno istraživanje niti su provjereni rezultati proizvoda.

Tehnički komentari uz naslove služe kao stabilni identifikatori sekcija za postojeći BMAD validator. Tekst dokumenta je na srpskom, latinicom i srpskom ijekavicom u upotrebi u Republici Srpskoj. Isto pravilo važi za ijekavski UI katalog i AI odgovore: „nisu”, „nisam” i „sutra”, uz „vrijeme”, „sljedeći” i „izvještaj”.
