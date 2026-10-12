# Personal AI Agent

Plan privatnog AI agenta za jednog vlasnika po instalaciji: razgovori, mejlovi, kalendar, dokumenti, obaveze, memorija i automatizacije, uz sopstvene API ključeve (BYOK).

## Trenutno stanje

Vlasnik je 12.10.2026. izričito aktivirao **Fazu 01 — Foundation** i odobrio početak implementacije. **STORY-024 je u toku:** dodani su .NET 10 solution, domenski portovi, test projekti, React/Vite navigacija sa tri lokalizacije i ograničeni Tauri shell. .NET i frontend provjere prolaze; native desktop build i Windows dokaz još nisu završeni. Faza 01 ima nula završenih priča i nije završena.

[BMAD opis proizvoda](docs/bmad/product-brief.md), [PRD](docs/bmad/prd.md), [arhitektura](docs/bmad/architecture.md) i [UX specifikacija](docs/bmad/ux-design.md) izrađeni su za cijeli definisani MVP iz faza 01–09. Početni korisnik je samo vlasnik; cilj su ušteda vremena i pravovremeno završavanje obaveza, sa fokusom na važne mejlove, prijedloge odgovora i podsjetnike. Rok i brojčani ciljevi još nisu potvrđeni. UX definiše 16 ekrana, osam glavnih tokova, stanja greške/odobrenja, pristupačnost i tri lokalizacije. Na zahtjev vlasnika pripremljen je [uslovni plan sprinta 1](docs/bmad/sprint-plan.md), bez kalendarskog roka.

PRD sadrži 28 funkcionalnih i 12 nefunkcionalnih zahtjeva, 9 epika, 23 kandidata za priče i mapiranje na planirane scenarije provjere. Potvrđeni su vlasnikova pravila i oznake uz AI preporuku važnosti, kao i podesiv dnevni pregled nepročitanih mejlova starijih od 3 dana i nezavršenih zadataka koji kasne ili dospijevaju u naredna 24 sata. Vrijeme bira vlasnik; poslije otključavanja dobija jedan naknadni pregled. Kandidati za priče nisu aktivan sprint, a planirani scenariji nisu izvršeni aplikacioni testovi.

Planirani stack je Windows 11 x64, Tauri 2, React/TypeScript strict, ASP.NET Core/.NET 10 i SQLite uz SQLCipher. Backend je isključivo lokalni, na `127.0.0.1` sa nasumičnim portom i zaštitom sesijskim tokenom i Origin/CORS politikom.

Arhitektura bira modularni .NET backend i ograničeni Rust IPC most; renderer ne prima sačuvane tajne ili sesijski token. Razrađeni su odobrenje vezano za konačne argumente, cloud politika, šifrovani model podataka, jedan naknadni pregled i DST pravila. [Provjera spremnosti](docs/bmad/gate-check.md) daje `CONDITIONAL PASS`: svih 28 FR i 12 NFR imaju projektantsko mapiranje, uz dva djelimično razrađena NFR. Native SQLCipher distribucija/licenca, konačni KDF izbor i recovery/crash dokaz ostaju kritičan uslov prije implementacije vaulta; nema plaintext fallback-a. To je provjera dizajna, bez aplikacionog builda ili testova.

Trajne odluke: [ADR-002 — lokalni transport](docs/architecture/ADR-002_LOCAL_TRANSPORT.md), [ADR-003 — vault i izvršavanje](docs/architecture/ADR-003_VAULT_EXECUTION.md), [ADR-004 — raspored i izvori](docs/architecture/ADR-004_SCHEDULER_SOURCES.md) i [ADR-005 — pamćenje teme](docs/architecture/ADR-005_THEME_PERSISTENCE.md). Postojeći ADR-001 ostaje mjerodavan za jezike. BMAD evidencija razlikuje završenu provjeru od bezuslovnog prolaza; STORY-024 je započeta, a sprint ostaje uslovan bez potvrđenog kapaciteta. Nova gate provjera dopunjene bootstrap allowlist-e potrebna je prije pogođenih priča.

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

Osnova je razložena na [sedam priča STORY-024–030](docs/bmad/sprint-plan.md): ukupno 33 relativna boda, od čega su prve tri kandidat sprinta 1 sa 15 bodova, a četiri preostale imaju 18 bodova bez dodijeljenog budućeg sprinta. Procjene i scenario kapaciteta od 16 bodova služe planiranju i nisu obećanje isporuke. STORY-024 je `in_progress`; ostale su `not_started`. Postojeći PRD kandidati STORY-001–023 nisu prepisani. Struktura prati PersonalAI.* i apps/web + apps/desktop iz mjerodavne Faze 01.

Dokumentacioni plan se provjerava komandom `python docs/bmad/validate-sprint-plan.py --self-test`; potrebno je Python okruženje sa PyYAML 6.0.3 iz [zaključane zavisnosti validatora](docs/bmad/requirements-validation.txt). Izolovane komande i stvarni rezultati su u [planu sprinta](docs/bmad/sprint-plan.md). Provjera pokriva YAML, priče, zbir, zavisnosti, acceptance mapiranje, lokalne veze i usklađenu autorizaciju Faze 01. Pozitivne i negativne kontrole pokrivaju plan prije aktivacije i aktivaciju bez početka sprinta. Ovo nije aplikacioni build ili dokaz završene faze.

Prije implementacije pročitati [AGENTS.md](AGENTS.md), [početni plan](docs/codex-plan/00_START_HERE.md), [odluke](docs/codex-plan/DECISIONS.md), [specifikaciju lokalizacije](docs/codex-plan/I18N_SPEC.md) i samo aktivni fajl faze. Pravila zahtijevaju sva tri prevoda, paritet kataloga i testove čuvanja jezika, formatiranja i kompatibilnosti.

Zahtjev vlasnika „Sljedeći korak: izričito aktiviranje Faze 01” i odgovor „Aktivacija i početak implementacije” odobravaju tekući rad na Fazi 01. Sljedeći razvojni korak je završetak desktop dokaza za [STORY-024](docs/stories/STORY-024.md), pa zaštićeni transport kroz STORY-025. Kapacitet i rok ostaju nepotvrđeni. Plan prati otvorene uslove arhitekture; G-001 za vault ostaje otvoren. Sve tri lokalizacije važe od prvog builda; trajni jezik, tema i Windows dokaz dio su preostalog obima iste faze. Faza 01 završava tek sa svih šest acceptance dokaza i potvrđenom dopunom teme. Ne prelaziti na narednu fazu bez zahtjeva vlasnika.

## Razvoj osnove

Potrebni alati: .NET SDK **10.0.401**, Node.js **24.19.0** i Rust **1.99.0**. Verzije paketa su zaključane u NuGet, npm i Cargo lockfile fajlovima; NuGet verzije su centralizovane u `Directory.Packages.props`. Licence, rezultati i svih šest kriterijuma nalaze se u [izvještaju početka implementacije](docs/architecture/FOUNDATION_STATUS.md).

U pripremljenom Linux cloud okruženju:

```bash
cd /workspace/PersonalAiAgent
source /workspace/.personal-ai-agent-setup/activate.sh
bash scripts/verify-foundation.sh
npm --prefix apps/web run dev
```

Frontend razvojni server radi samo na loopback adresi. Prikazuje prazne module i zaključano stanje; trenutno nema sidecar veze ili otključavanja. Izbor jezika važi tokom tekućeg pokretanja; trajni bootstrap i tema još nisu implementirani.

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
```

Za Windows 11 x64 dodatno su potrebni Microsoft C++ Build Tools sa Desktop development with C++, MSVC Rust toolchain i WebView2, prema službenim Tauri preduslovima. Iz korijena projekta `npm --prefix apps/desktop run dev` pokreće shell, a `npm --prefix apps/desktop run build -- --no-bundle` priprema executable. Te Windows komande još nisu izvršene. Installer i ugrađeni .NET sidecar dolaze kroz STORY-026/030; trenutno ne postoji paket za krajnju instalaciju.

U Linux cloud okruženju prošli su .NET build, 4 .NET testa, frontend build/typecheck/lint, paritet 3 kataloga sa 28 semantičkih ključeva i 10 UI testova. Native `cargo check` staje na nedostajućem GLib/GTK/WebKit razvojnom okruženju. GitHub Actions konfiguracija je dodana, ali CI i Windows smoke provjere još nisu izvršeni.
