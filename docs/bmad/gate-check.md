# Provjera arhitekture — Personal AI Agent

- **Datum:** 11.10.2026. (`2026-10-11`)
- **Provjerio:** Codex, BMAD arhitektura; samoprovjera dokumentacije, bez nezavisne sigurnosne revizije.
- **Arhitektura:** [architecture.md](architecture.md), verzija 1.0.
- **Zahtjevi:** [prd.md](prd.md), verzija 1.0.
- **Verzija izvještaja:** 1.0.
- **Kriterijumi:** `bmad-architect/resources/gate-check-criteria.md` i šablon `templates/gate-check.template.md` iz instalirane BMAD vještine.

**Dopuna 12.10.2026:** [UX specifikacija](ux-design.md) je izrađena, a zahtjev za trajno pamćenje svijetle/tamne teme dodaje `ui.theme` u minimalnu bootstrap allowlist-u prema [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md). Ovaj izvještaj ostaje istorijska provjera od 11.10.2026; odluka `CONDITIONAL PASS`, `passed: false` i G-001 nisu automatski promijenjeni. Novu bootstrap šemu i njene negativne scenarije uključiti u narednu gate provjeru prije pogođene implementacije.

## 1. Sažetak <!-- Executive Summary -->

**Odluka: `CONDITIONAL PASS`.**

Arhitektura je dovoljno razrađena za dalje planiranje, uz obavezno rješavanje G-001 prije implementacije vaulta. Dokumentuje sve zahtjeve, granice modula, podatke, API, sigurnosne kontrole, raspored i plan provjera. Native SQLCipher distribucija/licenca, konačan KDF izbor i dokaz recovery/crash putanja ostaju kritičan uslov; aplikacioni build/testovi nisu izvršeni.

Odluka ne aktivira Fazu 01 ili bilo koju drugu fazu. Mjerodavni detaljni acceptance kriterijumi neaktivnih faza nisu učitani i moraju se uskladiti pri njihovom izričitom aktiviranju.

## 2. Pokrivenost zahtjeva <!-- Requirements Coverage -->

### 2.1. Funkcionalni zahtjevi

Ukupno **28 FR**, pokriveno **28**, djelimično **0**, nedostaje **0**. Formula: `28 / 28 × 100 = 100%`. Pokrivenost znači eksplicitnog vlasnika komponente i pristup implementaciji, prema BMAD definiciji, a ne završenu funkciju ili potpuno zatvorene implementacione preduslove.

Svih 28 pojedinačnih redova sa komponentom, odjeljkom i TC identifikatorom nalazi se u [arhitekturi §6.1](architecture.md#61-fr--komponenta--rješenje--provjera). FR-018 ima vlasnika/pristup i eksplicitnu granicu za formate/limite; njihov konačan izbor čeka aktivnu fazu 06.

### 2.2. Nefunkcionalni zahtjevi

Ukupno **12 NFR**, potpuno **10**, djelimično **2**, nedostaje **0**. BMAD formula je `(10 + 2) / 12 × 100 = 100%`. Posebno prikazana puna razrada: `10 / 12 × 100 = 83,33%`.

| NFR | Kategorija i cilj | Pokrivenost | Kvalitet rješenja | Validacija / dokaz |
|---|---|---|---|---|
| NFR-001 | Lokalna autentifikacija, bez LAN/javnog listenera | Full | Good | §5.1; TC-101, token/Origin/Host/stream negativni scenariji |
| NFR-002 | Šifrovana baza/tajne i kontrolisani recovery | Partial | Fair | §3.1/4.1; TC-102; G-001 paket, KDF i crash dokaz otvoreni |
| NFR-003 | Renderer bez privilegovanih adaptera; zasebne instance | Full | Good | §1/3/4.1/5; TC-103 |
| NFR-004 | Nula neautorizovanih akcija; nepouzdan sadržaj | Full | Good | §3.2; TC-104 |
| NFR-005 | Nula zabranjenih prenosa cloud sadržaja | Full | Good | §3.3/3.5; TC-105 |
| NFR-006 | Očuvanje podataka pri migraciji/prekidu/restore-u | Full | Good | §4.2; TC-106 i G-001 dokaz prije upotrebe formata |
| NFR-007 | Tri potpuna kataloga i CLDR formatiranje | Full | Good | §7.1/ADR-001; TC-107 |
| NFR-008 | Trajan izbor i svih 12 UI/AI kombinacija | Full | Good | §7.1; TC-108 |
| NFR-009 | Jedan dnevni/catchUp pregled; unknown bez slijepog retry-a | Full | Good | §3.2/3.6; TC-109 |
| NFR-010 | Predloženi p95 ≤2s i reakcija na prekid ≤1s | Partial | Fair | §3.6/9; TC-110; profil/uređaj/pragovi nisu potvrđeni |
| NFR-011 | Osnovni tokovi tastaturom i jasne posljedice | Full | Good | §7.1; TC-111, bez tvrdnje o WCAG sertifikaciji |
| NFR-012 | Odvojeni portovi, zaključane provjerene zavisnosti | Full | Good | §2/3/7/9; TC-112 |

Slabije tačke su NFR-002 i NFR-010. `Good` ocjenjuje konkretnost projektantske kontrole, bez tvrdnje o izmjerenoj efikasnosti.

## 3. Kvalitet arhitekture <!-- Architecture Quality Assessment -->

### 3.1. Rezultat

**12 provjera**, **12 uspješnih**, **0 neuspješnih**; `12 / 12 × 100 = 100%`. Ova stručna kontrolna lista je odvojena od skripte koja traži sekcije/ključne riječi.

### 3.2. Kontrolna lista i dokaz

| # | Provjera | Ocjena | Konkretan dokaz u arhitekturi |
|---|---|---|---|
| 1 | Obrazac je obrazložen | PASS | §2, lokalni modularni monolit i alternative |
| 2 | Komponente/granice su jasne | PASS | §1–3, vlasništvo podataka i renderer granica |
| 3 | Interfejsi/zavisnosti su eksplicitni | PASS | §3, odvojeni portovi i tipovi ishoda |
| 4 | Stack ima razlog izbora | PASS | §7, preuzeta ograničenja i granica konkretnog paketa |
| 5 | Kompromisi su dokumentovani | PASS | §8, cijena i uslov ponovne procjene |
| 6 | Model podataka je eksplicitan | PASS | §4, ključevi, relacije, indeksi i invarijante |
| 7 | API i autentifikacija/autorizacija su definisani | PASS | §5 i §3.2, transport/rute/fingerprint |
| 8 | Sigurnosne kontrole su eksplicitne | PASS | §3.1–3.3/4.1; neprovjereni detalji pod G-001 |
| 9 | Pouzdanost/obnova/monitoring su razrađeni | PASS | §3.6/4.2/9; lokalna availability granica i redakcija |
| 10 | Strategija testiranja je definisana | PASS | §6/9, negativni/unit/integration/contract/E2E |
| 11 | Distribucija i okruženja su definisani | PASS | §9, Windows paket, sintetički test i CI/update |
| 12 | Postoji FR/NFR sljedivost | PASS | §6/6.1, 40 redova i TC paketi |

### 3.3. Neuspješne provjere

Nema neuspješne provjere strukture dizajna. Kritičan preduslov ostaje u klasifikaciji rizika; 100% ove liste ne znači da je G-001 zatvoren.

## 4. Problemi i težina <!-- Issues -->

### 4.1. Kritičan uslov prije pogođene implementacije

**G-001 — Native SQLCipher i ključevski format: otvoren, sa mjerom.** Odgovornost: Codex/razvoj za provjeru, vlasnik za izbor/nabavku ako se razmatra plaćena distribucija. Predloženi kontrolni datum je **18.10.2026.**, uz obaveznu granicu prije implementacije faze 02. To je projektantski prijedlog datuma pregleda uslova, nije dogovoren rok, automatizacija ili aktiviranje faze.

Mjera: poslije izričitog aktiviranja faze 02 uskladiti njen fajl, provjeriti licencu/porijeklo i .NET 10 Windows x64 native build, odabrati podržan Argon2id paket/parametre, pa sintetički dokazati enkripciju baze/WAL/temp, AEAD tamper, recovery rotaciju i prekide commit-a. Ako dokaz nije moguć, vault dio ostaje blokiran; nema plaintext SQLite ili drugačijeg nesigurnog fallback-a.

### 4.2. Značajna pitanja

- **G-002:** Rust/pipe/Origin/stream/sleep dokaz, odgovornost Codex/razvoj, prije izlaza iz aktivirane faze 01.
- **G-003:** Tip Microsoft naloga, registracija/prava i provjera unknown mutacija, odgovornost razvoj/vlasnik, prije faze 05 konektora.
- **G-004:** Formati/limiti i parser isolation, odgovornost razvoj, prije faze 06 uvoza.
- **G-006:** Prompt injection, lock/revoke trke i nonce/ključ lifecycle, odgovornost razvoj, negativni testovi u relevantnim fazama i prije izdanja.

Ove granice nisu ugovoreni kalendarski rokovi. Poznati implementacioni preduslov sa konzervativnom blokadom nije prećutno zadovoljen.

### 4.3. Manja otvorena pitanja za planiranje

**G-005:** UX odluke, vrijeme/zona, jedinice limita, profil podataka i pragovi pilota; odgovornost vlasnik/UX/razvoj prije relevantne priče ili pilota. NFR-010 je `SHOULD` i prijedlog, pa nepotvrđena mjera ne mijenja obavezne sigurnosne zahtjeve. Kapacitet i rok isporuke nisu poznati.

## 5. Preporuke <!-- Recommendations -->

1. Razraditi `bmad:ux-design` za onboarding, pravila pošte, konačna odobrenja, nepotpune izvore i zasebne jezike.
2. Planirati samo izričito traženu implementacionu fazu; učitati njen fajl i povezati dodatne kriterijume sa FR/NFR prije priča.
3. Voditi G-001 kao obavezan uslov vault priča; riješiti ga dokazom, bez snižavanja zaštite.
4. Potvrditi radni profil i pragove NFR-010 prije mjerenja/pilota; ne obećavati rok bez kapaciteta.

## 6. Odluka prema pragovima <!-- Gate Decision -->

| Mjera | Stvarni projektantski rezultat | PASS prag | CONDITIONAL PASS prag | Ocjena |
|---|---|---|---|---|
| FR pokrivenost | 28/28 = 100% | ≥90% | ≥80% | Zadovoljava oba |
| NFR pokrivenost po BMAD formuli | (10+2)/12 = 100% | ≥90% | ≥80% | Zadovoljava oba; 2 Partial |
| Kvalitet dizajna | 12/12 = 100% | ≥80% | ≥70% | Zadovoljava oba |
| Kritični uslovi | G-001 otvoren | Nema otvorenih kritičnih uslova | Svaki ima mjeru, odgovornost i datum | Samo uslovno |

**Konačna odluka: `CONDITIONAL PASS`.** `PASS` nije primjenjiv dok je G-001 otvoren. `FAIL` nije primijenjen jer su metrički pragovi zadovoljeni i kritičan uslov ima konkretnu mjeru, odgovornost i kontrolni datum, prema redoslijedu procjene iz kriterijuma. Ovo tumačenje dozvoljava dalje planiranje uz zabranu nesigurne implementacije.

Ako G-001 ostane bez izvodljive mjere/odgovornosti/datuma, odluka postaje `FAIL` na narednoj provjeri. Po zatvaranju dokazima treba ponoviti `bmad:gate-check`; ovaj izvještaj ne unapređuje stanje automatski.

## 7. Sljedeći koraci <!-- Next Steps -->

Preporučeni sljedeći zahtjev je `bmad:ux-design`. `bmad:sprint-plan` može razložiti rad i pratiti uslove, ali ne smije aktivirati implementacionu fazu. Za razvoj vlasnik izričito traži Fazu 01; faze 02–09 čekaju zasebne zahtjeve. Nijedan sprint nije planiran niti početak razvoja upisan ovom provjerom.

## 8. Dodatak: detaljni dokazi <!-- Appendix -->

### 8.1. FR sljedivost

Normativna matrica [arhitektura §6.1](architecture.md#61-fr--komponenta--rješenje--provjera) ima 28 jedinstvenih FR redova, po jedan za svaku PRD definiciju. Svaki sadrži vlasnika, pristup i planirani TC-001–TC-028. Kriterijumi zahtjeva ostaju u PRD-u; matrica ih ne zamjenjuje.

### 8.2. NFR sljedivost

[Arhitektura §6](architecture.md#6-nfr-i-sljedivost) ima svih 12 NFR i TC-101–TC-112. Full/Partial klasifikacija je ista kao u §2.2 ovog izvještaja. Native/crypto detalji i nepotvrđen benchmark nisu skriveni ukupnom formulom.

### 8.3. Izvori kontrolne liste

Dokaz za svaku od 12 stavki nalazi se u tabeli §3.2. Kontrolna lista preuzeta je iz BMAD gate šablona; ocjene su samoprovjera. [SQLCipher .NET dokumentacija](https://www.zetetic.net/sqlcipher/sqlcipher-for-dotnet/) i [licence](https://www.zetetic.net/sqlcipher/license/) podržavaju G-001 razdvajanje distribucije/native paketa; provjera web izvora nije native build test.

### 8.4. Izvršene dokumentacione provjere

Provjere su izvršene 11.10.2026:

| Komanda / provjera | Stvarni rezultat |
|---|---|
| `bash bmad-architect/scripts/nfr-checklist.sh` | Izlazni kod 0; ispisana lista kategorija i pregledana primjenjivost za lokalni MVP. Skripta ne provjerava implementaciju. |
| `bash bmad-architect/scripts/validate-architecture.sh docs/bmad/architecture.md` | Izlazni kod 0; 24/24 provjere, 0 grešaka i 0 upozorenja. Provjera sekcija/ključnih riječi je odvojena od 12 stručnih stavki iz §3. |
| `python validate-documents.py` u privremenom radnom direktorijumu | Izlazni kod 0; 28 FR i 12 NFR sa jedinstvenim redovima i paritetom PRD-a, 10 Full/2 Partial, 12 kontrolnih stavki, 40 TC oznaka, 35 lokalnih veza i odredišnih naslova, jezička provjera. |
| `bash bmad-shared/scripts/update-workflow-status.sh --workflow architecture --status-file bmad/workflow-status.yaml --output docs/bmad/architecture.md` | Izlazni kod 0; arhitektura evidentirana kao izrađen dokument. |
| `bash bmad-shared/scripts/update-workflow-status.sh --workflow gate_check --status-file bmad/workflow-status.yaml --output docs/bmad/gate-check.md --passed false` | Izlazni kod 0; 4/10 tokova, 40%. Dodatno upisana `decision: CONDITIONAL PASS`; `passed: false` konzervativno znači da nema bezuslovnog PASS. |
| `git diff --check` i provjera novih Markdown fajlova sa `git diff --no-index --check` | `git diff --check`: kod 0. Za nove fajlove `--no-index` vraća 1 jer se razlikuju od praznog fajla, uz prazan izlaz provjere whitespace-a; provjereno svih 5 novih Markdown fajlova. |

BMAD Bash skripte pokrenute su kroz Git Bash iz privremenih kopija normalizovanih na LF; njihova logika i izvor vještine nisu mijenjani. Prefiksi `bmad-architect/` i `bmad-shared/` u tabeli označavaju instalirane direktorijume vještina u `C:/Users/Korisnik/.agents/skills/`, ne projektne skripte. Privremena Python provjera nalazi se u `C:/Users/Korisnik/AppData/Local/Temp/personal-agent-bmad-architecture-gmy525oe/validate-documents.py`; nije aplikacioni test.

YAML provjera potvrđuje `phase_4_implementation: not_started` i nula aktivnih priča. `current_phase: 2` ostaje zbog nezavršenog preporučenog UX toka; `phase_3_solutioning: completed` znači izrađene dokumente/provjeru, bez zatvaranja G-001. Sprint evidencija nije mijenjana.

Aplikacioni build, unit, integration, sigurnosni i E2E testovi nisu pokretani: nema koda, build konfiguracije ili test projekata. Nijedna implementaciona faza nema novi acceptance dokaz.
