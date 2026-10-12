# Personal AI Agent

Plan privatnog AI agenta za jednog vlasnika po instalaciji: razgovori, mejlovi, kalendar, dokumenti, obaveze, memorija i automatizacije, uz sopstvene API ključeve (BYOK).

## Trenutno stanje

STORY-026 je implementirana i ostaje `in_progress` do stvarnog Windows sleep/resume-a i vizuelne native provjere. Sidecar se kreira atomarno u privatnom Windows Job Object-u, profil ima kernel lease naslijeđen u dijete, a prekid kanala/crash/suspend/resume opoziva sesiju i HTTP operacije. Resume ostavlja aplikaciju zaključanom i zahtijeva restart sa novim handshakom. Automatizovani dokazi obuhvataju cleanup, prekinuti startup, restart/stari token, konkurentne instance, izolaciju profila, sintetički stream u test hostu i power poruke poslate stvarnom prozoru. [Windows izvještaj STORY-026](docs/architecture/STORY-026_WINDOWS_VERIFICATION.md) sadrži rezultate, granice i preostale korake. G-002 je otvoren; Faza 02 nije aktivirana.

STORY-025 je `completed`: zaštićeni health, token samo u Rust/.NET procesu, strogi Origin/Host, bezbjedne greške i tri UI prevoda. Lokalni testovi i vlasnikova manuelna potvrda AC-06 od 12.10.2026. ostaju u [zasebnom istorijskom izvještaju](docs/architecture/STORY-025_WINDOWS_VERIFICATION.md).

Vlasnik je 12.10.2026. izričito aktivirao **Fazu 01 — Foundation** i odobrio početak implementacije. **STORY-024 je završena:** .NET 10 solution, domenski portovi, test projekti, React/Vite navigacija sa tri lokalizacije i ograničeni Tauri shell imaju [Windows dokaze svih šest kriterijuma](docs/architecture/STORY-024_WINDOWS_VERIFICATION.md). Prošli su native debug build, svih osam navigacija tastaturom, tri jezička izbora, 5 .NET / 10 UI / 2 Rust testa, lint/typecheck i clippy. Sa završenom STORY-025, Faza 01 ima 2/7 završenih priča (10/33 boda) i nije završena; Faza 02 nije aktivirana.

[BMAD opis proizvoda](docs/bmad/product-brief.md), [PRD](docs/bmad/prd.md), [arhitektura](docs/bmad/architecture.md) i [UX specifikacija](docs/bmad/ux-design.md) izrađeni su za cijeli definisani MVP iz faza 01–09. Početni korisnik je samo vlasnik; cilj su ušteda vremena i pravovremeno završavanje obaveza, sa fokusom na važne mejlove, prijedloge odgovora i podsjetnike. Rok i brojčani ciljevi još nisu potvrđeni. UX definiše 16 ekrana, osam glavnih tokova, stanja greške/odobrenja, pristupačnost i tri lokalizacije. Na zahtjev vlasnika pripremljen je [uslovni plan sprinta 1](docs/bmad/sprint-plan.md), bez kalendarskog roka.

PRD sadrži 28 funkcionalnih i 12 nefunkcionalnih zahtjeva, 9 epika, 23 kandidata za priče i mapiranje na planirane scenarije provjere. Potvrđeni su vlasnikova pravila i oznake uz AI preporuku važnosti, kao i podesiv dnevni pregled nepročitanih mejlova starijih od 3 dana i nezavršenih zadataka koji kasne ili dospijevaju u naredna 24 sata. Vrijeme bira vlasnik; poslije otključavanja dobija jedan naknadni pregled. Kandidati za priče nisu aktivan sprint, a planirani scenariji nisu izvršeni aplikacioni testovi.

Planirani stack je Windows 11 x64, Tauri 2, React/TypeScript strict, ASP.NET Core/.NET 10 i SQLite uz SQLCipher. Backend je isključivo lokalni, na `127.0.0.1` sa nasumičnim portom i zaštitom sesijskim tokenom i Origin/CORS politikom.

Arhitektura bira modularni .NET backend i ograničeni Rust IPC most; renderer ne prima sačuvane tajne ili sesijski token. Razrađeni su odobrenje vezano za konačne argumente, cloud politika, šifrovani model podataka, jedan naknadni pregled i DST pravila. [Provjera spremnosti](docs/bmad/gate-check.md) daje `CONDITIONAL PASS`: svih 28 FR i 12 NFR imaju projektantsko mapiranje, uz dva djelimično razrađena NFR. Native SQLCipher distribucija/licenca, konačni KDF izbor i recovery/crash dokaz ostaju kritičan uslov prije implementacije vaulta; nema plaintext fallback-a. Ta istorijska gate provjera potvrđuje dizajn; zasebni aplikacioni dokazi osnove sada postoje.

Trajne odluke: [ADR-002 — lokalni transport](docs/architecture/ADR-002_LOCAL_TRANSPORT.md), [ADR-003 — vault i izvršavanje](docs/architecture/ADR-003_VAULT_EXECUTION.md), [ADR-004 — raspored i izvori](docs/architecture/ADR-004_SCHEDULER_SOURCES.md) i [ADR-005 — pamćenje teme](docs/architecture/ADR-005_THEME_PERSISTENCE.md). Postojeći ADR-001 ostaje mjerodavan za jezike. BMAD evidencija razlikuje završenu provjeru od bezuslovnog prolaza; STORY-024 je završena, a sprint ostaje uslovan bez potvrđenog kapaciteta. Nova gate provjera dopunjene bootstrap allowlist-e potrebna je prije pogođenih priča.

Vlasnik je potvrdio dvije teme, svijetlu i tamnu, uz trajno pamćenje izbora za buduća prijavljivanja. `ui.theme` je nezavisna od oba jezika, čuva se kanonski u šifrovanoj bazi i ima nesenzitivnu bootstrap kopiju za zaključani ekran. Minimalna bootstrap allowlist-a sada obuhvata samo `ui.locale` i `ui.theme`. Promjena teme/jezika ne mijenja dozvole; neuspješan upis čuva prethodni izbor. Ova dopuna je projektovana, još nije implementirana.

[Nacrt tri kataloga](docs/bmad/ux-copy.json) ima 200 semantičkih ključeva, a [vizuelni tokeni](docs/bmad/ux-tokens.json) obje palete. Dokumentaciona provjera `node docs/bmad/validate-ux.cjs` prolazi: paritet/placeholderi/CLDR, 44 kontrastna para, formatiranje i sljedivost 28 FR + 12 NFR. To nije aplikacioni test, audit stvarnog webview-a ili dokaz korisničke upotrebljivosti. Kompletan zapis provjera i plan testova nalazi se u UX dokumentu.

## Podržane lokalizacije od prve funkcionalne verzije

| Izbor | Kod | Pismo |
|---|---|---|
| SRB (Ekavica) | `sr-Latn-RS-ekavsk` | Latinica, sa dijakritičkim znakovima |
| SRB (Ijekavica) | `sr-Latn-RS-ijekavsk` | Latinica, sa dijakritičkim znakovima |
| ENG | `en-US` | Engleski (SAD) |

Svaka lokalizacija ima zaseban katalog. Izbor jezika i srpske varijante pamti se kroz restart, ažuriranje i zaključavanje. Jezik UI-ja i jezik AI odgovora su odvojene postavke; AI odgovori dodatno podržavaju `follow-user`.

Vlasnik je iz Republike Srpske. Komunikacija, dokumentacija, SRB (Ijekavica) katalog i AI odgovori sa izabranom ijekavicom koriste srpsku ijekavicu u upotrebi u Republici Srpskoj: „nisu”, „nisam” i „sutra”, uz „vrijeme”, „sljedeći” i „izvještaj”. Preciziranje jezičkog stila zadržava postojeće kodove lokalizacija i regionalno formatiranje.

Stari sačuvani `sr-Latn-RS` označavao je ijekavicu i pri budućoj migraciji postaje `sr-Latn-RS-ijekavsk`. Na novoj instalaciji srpski OS bez navedene varijante predlaže ekavicu; sačuvani korisnički izbor uvijek ima prednost. Detalji su u [I18N_SPEC.md](docs/codex-plan/I18N_SPEC.md) i [arhitektonskoj odluci o lokalizaciji](docs/architecture/ADR-001_LOCALIZATION.md).

## Pravila i naredni korak

BMAD vještine su već instalirane u lokalnom Codex okruženju. Projekat je inicijalizovan kroz `bmad-orchestrator`: [projektna konfiguracija](bmad/project.yaml), [status tokova rada](bmad/workflow-status.yaml) i [status sprinta](bmad/sprint-status.yaml). Komunikacija i BMAD dokumenti podešeni su na srpski, latinicom i ijekavicom u upotrebi u Republici Srpskoj; isto pravilo trajno je zapisano u `AGENTS.md`.

Za provjeru stanja pošalji `bmad:status`, a za preporuku narednog BMAD koraka `bmad:next`. Nivo projekta je 3 zbog opsega desktop aplikacije, vaulta i integracija; tip `other` označava desktop aplikaciju. BMAD konfiguracija ne mijenja postojeći razvojni plan niti označava njegove faze završenim. Planiranje sprinta je završeno uslovno; datumi i potvrđeni kapacitet ostaju nepoznati, a obavezani obim je nula.

Osnova je razložena na [sedam priča STORY-024–030](docs/bmad/sprint-plan.md): ukupno 33 relativna boda, od čega su prve tri kandidat sprinta 1 sa 15 bodova, a četiri preostale imaju 18 bodova bez dodijeljenog budućeg sprinta. Procjene i scenario kapaciteta od 16 bodova služe planiranju i nisu obećanje isporuke. STORY-024 i STORY-025 su `completed`, STORY-026 je `in_progress`, a STORY-027–030 su `not_started`; završeno je 10/33 boda. Postojeći PRD kandidati STORY-001–023 nisu prepisani. Struktura prati PersonalAI.* i apps/web + apps/desktop iz mjerodavne Faze 01.

Dokumentacioni plan se provjerava komandom `python docs/bmad/validate-sprint-plan.py --self-test`; potrebno je Python okruženje sa PyYAML 6.0.3 iz [zaključane zavisnosti validatora](docs/bmad/requirements-validation.txt). Izolovane komande i stvarni rezultati su u [planu sprinta](docs/bmad/sprint-plan.md). Provjera pokriva YAML, priče, zbir, zavisnosti, acceptance mapiranje, lokalne veze i usklađenu autorizaciju Faze 01. Pozitivne i negativne kontrole pokrivaju plan prije aktivacije i aktivaciju bez početka sprinta. Ovo nije aplikacioni build ili dokaz završene faze.

Prije implementacije pročitati [AGENTS.md](AGENTS.md), [početni plan](docs/codex-plan/00_START_HERE.md), [odluke](docs/codex-plan/DECISIONS.md), [specifikaciju lokalizacije](docs/codex-plan/I18N_SPEC.md) i samo aktivni fajl faze. Pravila zahtijevaju sva tri prevoda, paritet kataloga i testove čuvanja jezika, formatiranja i kompatibilnosti.

Zahtjev vlasnika „Sljedeći korak: izričito aktiviranje Faze 01” i odgovor „Aktivacija i početak implementacije” odobravaju tekući rad na Fazi 01. [STORY-024](docs/stories/STORY-024.md) i STORY-025 imaju završene dokaze. Sljedeći korak je završni manuelni dokaz STORY-026: stvarni Windows sleep/resume, zaključano stanje i lokalizovana nedostupnost; detaljni koraci su u njenom izvještaju. Kapacitet i rok ostaju nepotvrđeni. G-001 za vault ostaje otvoren. Sve tri lokalizacije važe od prvog builda; trajni jezik, tema i Windows installer dokaz dio su preostalog obima iste faze. Faza 01 završava tek sa svih šest acceptance dokaza i potvrđenom dopunom teme. Ne prelaziti na narednu fazu bez zahtjeva vlasnika.

## Razvoj osnove

Potrebni alati: .NET SDK **10.0.401**, Node.js **24.19.0** i Rust **1.99.0**. Verzije paketa su zaključane u NuGet, npm i Cargo lockfile fajlovima; NuGet verzije su centralizovane u `Directory.Packages.props`. Licence i kriterijumi Faze 01 nalaze se u [izvještaju osnove](docs/architecture/FOUNDATION_STATUS.md), a stvarne Windows komande, pregled koda i rezultati u [Windows provjeri STORY-024](docs/architecture/STORY-024_WINDOWS_VERIFICATION.md).

U pripremljenom Linux cloud okruženju:

```bash
cd /workspace/PersonalAiAgent
source /workspace/.personal-ai-agent-setup/activate.sh
bash scripts/verify-foundation.sh
npm --prefix apps/web run dev
```

Frontend razvojni server radi samo na loopback adresi. Desktop prikazuje prazne module, zaključano stanje i zaštićeni health kroz Rust IPC/sidecar. Samostalni browser prikazuje lokalizovanu nedostupnost servisa. Otključavanje nije implementirano. Izbor jezika važi tokom tekućeg pokretanja; trajni bootstrap i tema još nisu implementirani.

Za instalaciju zavisnosti na računaru sa navedenim alatima:

```text
dotnet restore PersonalAI.sln --locked-mode
npm ci --prefix apps/web
npm ci --prefix apps/desktop
dotnet build PersonalAI.sln --no-restore
dotnet test PersonalAI.sln --no-build --no-restore
npm --prefix apps/web run build
npm --prefix apps/web run lint
npm --prefix apps/web run test:catalogs
npm --prefix apps/web test
npm --prefix apps/web run test:coverage
```

Za Windows 11 x64 dodatno su potrebni Microsoft C++ Build Tools sa Desktop development with C++, MSVC Rust toolchain i WebView2, prema službenim Tauri preduslovima. U ovoj provjeri postojeći Visual Studio 2022 Community dopunjen je C++ workload-om (MSVC 14.44.35207, Windows SDK 10.0.26100.0); installer je vratio kod 0. Iz korijena projekta `npm --prefix apps/desktop run dev` pokreće Vite razvojni shell; `npm --prefix apps/desktop run build -- --debug --no-bundle -- --locked` priprema provjereni native debug executable sa ugrađenim UI-jem. Pokrenuti `apps/desktop/src-tauri/target/debug/personal-ai-desktop.exe`. U ovoj priči izvršeni su debug build i smoke tog executable-a; STORY-025 sada dodaje framework-dependent .NET sidecar uz executable preko scripts/prepare-sidecar.ps1. Release/installer dokaz ostaje STORY-030. Trenutno ne postoji paket za krajnju instalaciju.

Istorijski Linux cloud rezultati obuhvataju .NET build/4 testa i frontend provjere, uz nedostajuće GLib/GTK/WebKit native preduslove. Istorijska STORY-024 Windows provjera prolazi: .NET build/5 testova, frontend build/typecheck/lint, 3 kataloga sa 28 ključeva, 10 UI testova, coverage 90,9% linija / 100% grana, Tauri native debug build, 2 Rust testa, fmt/clippy i desktop navigacija/jezici. GitHub Actions konfiguracija je dopunjena, ali CI još nije izvršen; installer nije testiran.

Windows PowerShell skripta `./scripts/verify-foundation.ps1` zaustavlja se na prvoj neuspješnoj provjeri; opcija `-Desktop` dodaje razvojni Tauri build, Rust testove i clippy. Potrebne su prethodno instalirane npm zavisnosti i navedeni alati u PATH-u. U ovoj lokalnoj pripremi Node/Rust su izolovani; za novu PowerShell sesiju:

```powershell
$taskTools = Join-Path $env:LOCALAPPDATA 'PersonalAI/dev-tools'
$env:CARGO_HOME = Join-Path $taskTools 'cargo'
$env:RUSTUP_HOME = Join-Path $taskTools 'rustup'
$env:Path = (Join-Path $taskTools 'node-v24.19.0-win-x64') + ';' + (Join-Path $taskTools 'cargo/bin') + ';' + $env:Path
./scripts/verify-foundation.ps1 -Desktop
```

Ovaj blok koristi postojeću lokalnu pripremu, ne preuzima alate. Na drugom računaru instalirati iste verzije i Tauri Windows preduslove. Frontend coverage prag je 80% za linije, grane, funkcije i izraze. Generisani izvještaji ostaju izvan Git-a; njihove vrijednosti i tačan opseg zapisani su u Windows izvještaju.

Poslije native build-a `./scripts/verify-lifecycle-windows.ps1` pokreće zasebne parove aplikacije, provjerava normalni izlaz, pad desktop-a/sidecar-a, restart, odbijanje druge instance i sintetički suspend/resume preko Windows poruka. Skripta prekida samo procese koje je sama pokrenula i ne briše profil. Ne uspavljuje računar i ne predstavlja dokaz stvarnog sleep-a. Produkcioni profil je `profiles/default` ispod aplikacionog lokalnog data direktorijuma; postojeći `instance.lock` se zadržava i njegovo vlasništvo određuje kernel handle. Renderer ne bira putanju. Bootstrap sadržaj i trajni jezik/tema još nisu implementirani.
