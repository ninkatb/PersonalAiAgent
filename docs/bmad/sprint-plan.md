# Uslovni plan sprinta 1 — Personal AI Agent

- **Datum planiranja:** 12.10.2026.
- **Status:** Planiranje završeno; sprint i razvoj nisu počeli.
- **Odluka vlasnika:** Uslovni plan bez kalendarskog roka. Kapacitet, sastav tima i dostupni radni dani nisu potvrđeni.
- **Početak / kraj:** Nisu određeni.
- **Implementaciona faza:** Nijedna nije aktivirana. Kandidat za rad je prva cjelina Faze 01; ovaj plan nije odobrenje za razvoj.
- **Cilj sprinta:** Pokrenuti praznu desktop aplikaciju u tri lokalizacije, provjeriti zaštićeni lokalni health poziv kroz Rust most i bezbjedno upravljati sidecar procesom.

## 1. Izvori i granice

Mjerodavni su [početni plan](../codex-plan/00_START_HERE.md), [odluke](../codex-plan/DECISIONS.md), [I18N_SPEC](../codex-plan/I18N_SPEC.md) i [AGENTS.md](../../AGENTS.md). Ulazi su [PRD](prd.md), [arhitektura](architecture.md), [provjera spremnosti](gate-check.md) i [UX](ux-design.md). Detaljni fajl [Faze 01](../codex-plan/01_FOUNDATION.md) pregledan je za uslovno razlaganje prvog kandidata; ostali detaljni fajlovi faza nisu učitavani u ovom toku. Prije razvoja treba izričito aktivirati Fazu 01 i ponovo potvrditi njenu usklađenost sa odlukama.

Plan obuhvata EPIC-001 i samo početni dio zahtjeva FR-001/FR-024, uz potvrđenu dopunu FR-025 za temu iz ADR-005. Pun onboarding sa master lozinkom, recovery kodom, AI jezikom i modelom pripada kasnijim odobrenim fazama. Završetak sprinta 1 ne znači završetak Faze 01 ili cijelog FR-001/FR-024/FR-025.

U ovom opsegu nema SQLCipher implementacije, stvarnog unlock-a, tajni, OAuth-a, AI poziva, produktivnog stream-a, rasporeda poslova ili javnog API-ja. `Locked/Unlocked` su tipizovana stanja ugovora; produkcioni build počinje zaključan i ne daje lažni unlock. `Unlocked` se provjerava samo sintetički u testovima, bez produkcionog prekidača za zaobilaženje zaštite.

## 2. Kapacitet i procjene

Vlasnik je izabrao uslovno planiranje bez roka. Potvrđeni kapacitet je **nepoznat**, a obavezani obim je **0 bodova**. Relativne procjene služe razlaganju i treba ih kalibrisati nakon prvih stvarnih rezultata; nisu preračunati sati, ugovoreni rok ili izmjerena brzina.

Za provjeru izvodljivosti koristi se označeni scenario: jedan razvojni tok × 10 raspoloživih radnih dana × 2 boda po danu = 20 bodova; rezerva od 20% za postavljanje okruženja i provjere daje **16 bodova**. Ovo nisu potvrđeni tim, trajanje sprinta ili dostupnost. Kandidat sprinta od **15 bodova** staje u taj scenario i ostavlja 1 bod dodatne rezerve. Ako stvarni kapacitet bude manji, smanjuje se broj priča, uz očuvanje svih sigurnosnih i jezičkih kriterijuma.

Nema završene istorije sprintova: brzina i pokretni prosjek su nepoznati. Ne upisivati nultu brzinu kao rezultat rada koji nije sproveden. Datumi, dnevni burndown i procjena završetka MVP-a ostaju prazni.

## 3. Razloženi obim

`STORY-001–STORY-023` ostaju PRD kandidati. Nove priče imaju zasebne ID-jeve `STORY-024–STORY-030`, povezane sa roditeljima. Time postojeća sljedivost ostaje sačuvana.

| Red | Priča | Bodovi | Zavisnosti | Raspored |
|---|---|---:|---|---|
| 1 | [STORY-024 — Pokretna osnova i lokalizovana navigacija](../stories/STORY-024.md) | 5 | Nema priča; potrebno razvojno okruženje | Uslovni sprint 1 |
| 2 | [STORY-025 — Zaštićeni lokalni health kroz Rust most](../stories/STORY-025.md) | 5 | STORY-024 | Uslovni sprint 1 |
| 3 | [STORY-026 — Životni ciklus i izolacija sidecar procesa](../stories/STORY-026.md) | 5 | STORY-025 | Uslovni sprint 1 |
| 4 | [STORY-027 — Jezičko formatiranje i potpuna provjera kataloga](../stories/STORY-027.md) | 5 | STORY-024 | Preostali obim Faze 01 |
| 5 | [STORY-028 — Trajni UI jezik i bezbjedan bootstrap](../stories/STORY-028.md) | 5 | STORY-024 | Preostali obim Faze 01 |
| 6 | [STORY-029 — Svijetla i tamna tema sa trajnim izborom](../stories/STORY-029.md) | 3 | STORY-027, STORY-028 | Preostali obim Faze 01 |
| 7 | [STORY-030 — CI, Windows paket i dokazi Faze 01](../stories/STORY-030.md) | 5 | STORY-024–STORY-029 | Preostali obim Faze 01 |

Sve priče imaju prioritet `MUST` za konačnu predaju osnove, ali samo prve tri su kandidat sprinta 1. Ukupno: **7 priča / 33 boda**, sprint 1 **3 / 15**, preostali obim **4 / 18**. Preostalim pričama nije dodijeljen broj ili datum budućeg sprinta. Nema opcione priče koja bi skratila obavezne kontrole.

## 4. Redoslijed, zavisnosti i uslovi ulaska

Prvo se provjeravaju alati, licence, zaključavanje zavisnosti i minimalni build. STORY-024 već uključuje stvarne prevode svih uvedenih tekstova i osnovni paritet tri kataloga. STORY-027 dopunjava formatiranje i regresione scenarije; ne opravdava build sa jednim jezikom.

Zatim STORY-025 dodaje token, tačan Origin/Host i poznatu Rust IPC operaciju. STORY-026 provjerava prekide, restart, jednu instancu i sleep. Tek sa dokazima sve tri priče kandidat sprinta ispunjava cilj. STORY-027 i STORY-028 mogu se razvijati nezavisno nakon osnove, ali preporučeni jedan razvojni tok prvo završava dokaz transporta. STORY-029 koristi isti bootstrap servis; nema drugog pisca koji bi izgubio locale. STORY-030 objedinjeno provjerava instalirani paket i acceptance dokaze.

Prije implementacije:

1. Vlasnik izričito aktivira Fazu 01; BMAD evidencija nije autorizacija.
2. Potvrđuju se raspoloživi kapacitet i konačan obim, bez obaveze kalendarskog roka.
3. Usklađuju se bootstrap dopuna ADR-005 i transport ADR-002 sa detaljnim planom; dopunjena bootstrap šema prolazi novu gate provjeru prije implementacije STORY-028/029.
4. U STORY-024 provjeravaju se aktuelna podrška, licence i konkretne zaključane verzije alata/paketa. Ovaj plan ne bira nove verzije ili licence.

## 5. Komponente i usklađivanje

Za strukturu i nazive primjenjuje se mjerodavni fajl Faze 01: `apps/web`, `apps/desktop`, `src/PersonalAI.*` i tri .NET test projekta. Primjeri `PersonalAgent.*` i React unutar `apps/desktop` iz ranije BMAD arhitekture usklađuju se sa tom strukturom. Domenski interfejsi iz Faze 01 ostaju obavezni; dodatni portovi za autentifikaciju i bootstrap slijede arhitekturu, bez dupliranja ekvivalentnih ugovora.

ADR-002 precizira da token ostaje u Rust/backend procesu i dolazi privatnim naslijeđenim kanalom. Renderer dobija samo rezultat poznate operacije. `/health` je zaštićen kao ostale stvarne rute i ne vraća privatne detalje. Predložena arhitektura ostaje ista; konkretan Windows dokaz tek slijedi.

Bootstrap allowlist je samo `ui.locale` i `ui.theme`, na fiksnoj per-instance aplikacionoj putanji izvan repozitorijuma. STORY-028 priprema zajednički servis i idempotentnu normalizaciju starog locale-a; STORY-029 koristi taj servis za temu. Nema šeme baze ili destruktivne migracije. SQLite kanonski upis, restore i puni UI/AI testovi čekaju Fazu 02 i kasnije relevantne faze.

## 6. Rizici i mjere

| Uslov / rizik | Odgovornost | Mjera i granica |
|---|---|---|
| Nepotvrđen kapacitet i neprovjeren toolchain | Vlasnik / razvoj | Ovaj obim je uslovan; STORY-024 dokumentuje provjereno okruženje, bez obećanog roka |
| G-002: Windows pipe, Origin, cleanup, sleep i stream | Razvoj | STORY-025/026: unit, integration i desktop negativni scenariji; sintetički stream samo u test hostu, bez AI funkcije; nezatvorena stavka sprečava predaju Faze 01 |
| Dopunjena bootstrap allowlist | Razvoj / arhitektura | Nova gate provjera šeme i negativnih slučajeva prije STORY-028/029; postojeći gate ostaje istorijski `CONDITIONAL PASS` |
| G-001: native SQLCipher, licenca, KDF i recovery/crash | Razvoj; vlasnik ako treba nabavka | Ostaje otvoren za Fazu 02; nije dio ovog sprinta, nema plaintext fallback-a |
| Lokalizacija i tema izgube sačuvani izbor | Razvoj | Puni locale kodovi, jedan atomarni pisac, fault injection, stari zapis, šest UI/tema kombinacija |
| Paket radi samo na razvojnom računaru | Razvoj | STORY-030 provjerava ugrađeni sidecar, instalaciju i cleanup na čistom Windows okruženju |

Kritičan neuspjeh transporta zaustavlja pogođeni dio; nema izlaganja listenera, generičkog IPC proxy-ja ili izbacivanja provjere tokena. G-003/004/006 i otvorena pitanja cijelog MVP-a ostaju u postojećoj gate evidenciji, bez novih rokova i automatskog zatvaranja.

## 7. Sljedivost i dokazi

Šest kriterijuma Faze 01 dobijaju interne oznake F01-AC-01–06 radi evidencije. Oznake ne mijenjaju njihov tekst. Sve provjere ispod su **planirane, bez izvršenog aplikacionog dokaza**.

| Kriterijum iz Faze 01 | Priče | Potreban dokaz |
|---|---|---|
| F01-AC-01: README pokretanje, alati i Windows instalacija | STORY-024, STORY-030 | Tačne verzije, licence, komande i smoke zapis instaliranog paketa |
| F01-AC-02: frontend poziva zaštićeni health | STORY-025 | UI → Rust → sidecar poziv i odbijanje direktnog neautorizovanog poziva |
| F01-AC-03: lokalni API izolovan i odbija neautorizovan pristup | STORY-025, STORY-026 | Bind na 127.0.0.1:0; missing/wrong/stale token, Origin/Host/preflight, drugi računar i restart |
| F01-AC-04: DI/core bez direktnih provajderskih zavisnosti | STORY-024 | Projektne reference, obavezni interfejsi i provjera renderer granice |
| F01-AC-05: pokrenuti testovi i lint/typecheck, stvarni rezultati | Sve priče; predaja STORY-030 | Build/unit/integration/security/desktop dokazi i Windows smoke, zapis izlaznih kodova |
| F01-AC-06: tri stvarna kataloga, selektor, formatiranje, fallback, trajni default i legacy | STORY-024, STORY-027, STORY-028 | Paritet ključeva/placeholdera/CLDR, regionalna mapa, restart, OS i stari sr-Latn-RS |
| Potvrđena dopuna: pamćenje light/dark teme | STORY-028, STORY-029, STORY-030 | ADR-005; UX-THEME-01/02/05/06 u dijelu bez vaulta; šest UI/tema kombinacija i prekid upisa |

Roditelji: STORY-001 → STORY-024/025/026/030; STORY-002 → STORY-024/027/028; STORY-022 → STORY-029 samo za početni dio teme. FR/NFR/TC oznake su navedene u svakoj priči. Restore, kanonski SQLite konflikt, stvarni unlock i 12 UI/AI odnosno 24 UI/AI/tema kombinacije ostaju obavezni u kasnijim relevantnim fazama; nisu pokriveni simulacijom kao konačan dokaz.

## 8. Uslovi završetka i praćenje

Priča je završena tek kada su kriterijumi dokazani, relevantni testovi prolaze, pregled koda je evidentiran, dokumentacija ažurirana i nema otvorene kritične greške u njenom opsegu. Za svaki uvedeni UI tekst potrebna su sva tri prevoda. Sigurnosno kritične putanje imaju pozitivne i negativne unit/integration scenarije. Nema obaveze javnog deploy-a za lokalnu aplikaciju.

Sprint 1 je uspješan tek sa dokazima STORY-024/025/026: tri lokalizovane prazne navigacije, poznati zaštićeni health poziv, zatvoren proces nakon gašenja i odbijanje neautorizovanih/starih sesija. Ovo je isporuka dijela osnove. Faza 01 ostaje nezavršena dok STORY-027–030 i svaki njen kriterijum nisu dokazani.

Tokom rada u [sprint-status.yaml](../../bmad/sprint-status.yaml) upisivati stvarne statuse, početak/završetak priče, blokere i dokaz. Bodovi se računaju tek za potpuno završene priče; djelimičan rad ne daje završene bodove. Burndown počinje tek stvarnim početkom i vremenskim planom; trenutna prazna istorija je namjerna. Pregled rezultata i retrospektiva slijede stvarnu isporuku, bez rezervisanih termina. Zatim se kapacitet ponovo procjenjuje za preostali dio iste Faze 01, bez prelaska na Fazu 02.

## 9. Provjera plana i predaja

Dokumentaciona provjera: `python docs/bmad/validate-sprint-plan.py`. Provjerava YAML, sedam jedinstvenih priča, procjene i zbir, zavisnosti bez ciklusa, razdvajanje sprinta i preostalog obima, šest acceptance redova, lokalne veze i očuvanje neaktivirane implementacije. Validator provjerava ovaj uslovni početni plan; nakon izričitog aktiviranja i promjene stanja mora se uskladiti sa novom evidencijom. Postojeća UX provjera: `node docs/bmad/validate-ux.cjs`.

Python zavisnost za dokumentacioni validator je zaključana u [requirements-validation.txt](requirements-validation.txt). [PyYAML 6.0.3 na PyPI](https://pypi.org/project/PyYAML/6.0.3/) navodi MIT licencu, Python ≥3.8 i Windows wheel za Python 3.13; podrška/licenca provjerene su 12.10.2026. Paket nije aplikaciona zavisnost. U ovoj provjeri instaliran je samo u privremeni direktorijum, bez promjene globalnog Python okruženja.

Komande stvarno izvršene u PowerShell-u:

```powershell
$sprintValidationPackages = Join-Path $env:TEMP 'personal-agent-sprint-validation-packages'
python -m pip install --disable-pip-version-check --no-input --only-binary=:all: --target $sprintValidationPackages 'PyYAML==6.0.3'
$env:PYTHONPATH = $sprintValidationPackages
python docs/bmad/validate-sprint-plan.py --self-test
node docs/bmad/validate-ux.cjs
git diff --check
```

| Izvršena provjera — 12.10.2026. | Stvarni rezultat |
|---|---|
| BMAD generate-story-id.sh kroz privremenu LF kopiju u Git Bash-u | Kod 0; prvi slobodan ID STORY-024, uz očuvanje 23 PRD kandidata |
| Privremena instalacija PyYAML==6.0.3 | Kod 0; Windows Python 3.13 wheel instaliran samo u TEMP direktorijum |
| validate-sprint-plan.py --self-test | Kod 0; 7 priča / 33 boda, sprint 3/15, preostalo 4/18, 6 acceptance redova, 63 lokalne veze; 7/7 negativnih kontrola odbijeno |
| validate-ux.cjs | Kod 0, PASS; 3 kataloga, 200 semantičkih ključeva, 44 kontrastna para i sljedivost 28 FR + 12 NFR |
| git diff --check i whitespace pregled novih artefakata | Kod 0 za git diff --check; novi artefakti provjereni zasebno, bez grešaka whitespace-a |

Provjere potvrđuju dokumente i početnu evidenciju; nema aplikacionog acceptance dokaza. Izmijenjeni su README, BMAD arhitektura radi naziva/strukture, obje YAML evidencije; dodati su plan, sedam priča i dokumentacioni validator sa zaključanom zavisnošću. Postojeće korisničke izmjene mjerodavnih specifikacija nisu prepisivane. Nije dodata migracija baze ili implementacija aplikacije.

Planirane aplikacione komande nakon stvaranja koda su `dotnet build`, `dotnet test`, `npm run build`, `npm run typecheck` i `npm run lint`, uz Rust/desktop build i Windows smoke. Ove komande sada nisu aplikacioni dokaz jer projekti ne postoje.

Precizan sljedeći razvojni korak: nakon izričitog zahtjeva vlasnika za Fazu 01, potvrditi kapacitet i spremnost prve priče, pa započeti STORY-024. Zahtjev `bmad:sprint-plan` ovdje je potpuno obrađen; odobrenje razvoja je zasebna projektna granica iz AGENTS.md.
