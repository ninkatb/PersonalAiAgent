# STORY-025 — Windows provjera zaštićenog transporta

**Datum:** 12.10.2026. **Status:** STORY-025 je završena. Postojeći lokalni dokazi dopunjeni su vlasnikovom potvrdom uspješne manuelne provjere AC-06 od istog dana. Faza 01 ostaje aktivna i nezavršena, Faza 02 nije aktivirana.

## Implementacija

Rust pokreće isključivo `sidecar/PersonalAI.Api.exe` uz svoj executable. Razvojna skripta objavljuje framework-dependent .NET sidecar u taj fiksni direktorijum; nema putanje iz renderera, shell plugin-a ili generičkog launcher-a. Installer i samostalna distribucija ostaju STORY-030. Postojeće izmjene iz STORY-024 sačuvane su.

`getrandom` generiše 32 nasumična bajta za token i 16 za identitet sesije. Privatne anonimne naslijeđene stdin/stdout cijevi nose verzionisane JSON okvire, ograničene na 1024 bajta. Rust šalje `protocol/token/sessionId/origin` samo u stdin; backend vraća isključivo `protocol/pid/port` u stdout. Provjeravaju se verzija, PID, port i autorizovani health odgovor. Pogrešan/nepotpun okvir, timeout ili neuspješna provjera zatvaraju sesiju i uklanjaju potomka. Token se ne predaje preko argv/env/fajla/URL-a, niti se vraća rendereru.

Origin transporta je tačno `http://session-{sessionId}.localhost`, izveden iz provjerenog startup ugovora. To je interna session-bound vrijednost koju šalje Rust; browser origin i developer server nisu automatski ovlašćeni. Backend provjerava tačan `127.0.0.1:{dodijeljeniPort}` Host i bearer token pomoću `FixedTimeEquals`. Svaka stvarna ruta prolazi middleware, uključujući nepoznate rute. OPTIONS izuzetak daje samo 204 za `/health`, GET i Authorization, uz tačan Origin/Host; ostali preflight zahtjevi ne dobijaju CORS dozvolu.

Kestrel koristi jedino `Listen(IPAddress.Loopback, 0)`. Konfiguracioni izvori se uklanjaju prije pokretanja servera, pa ni `ASPNETCORE_URLS` ni `Kestrel__Endpoints__Public__Url` ne dodaju LAN listener. API ne startuje bez preusmjerenog privatnog kanala. Backend startup ima nezavisan rok od 10 sekundi; Rust readiness rok je 12 sekundi, a health HTTP rok 3 sekunde. Windows console pipe može ignorisati cancellation, pa startup čitanje radi u pozadinskom zadatku sa zasebnim `WaitAsync` rokom. Negativan test tihog kanala provjerava stvarni proces.

Renderer koristi javni `@tauri-apps/api/core` API za samo `backend_health` sa praznim objektom. Tauri manifest i capability dozvoljavaju samo tu komandu glavnom lokalnom prozoru. Rust dodatno provjerava prozor, navigacioni origin i prazne argumente; URL/metod/header/putanja/komanda i raw payload se odbijaju. HTTP klijent isključuje proxy i redirect, te ograničava rezultat na 1024 bajta i `{status:"ok"}`. Backend status se prevodi u sva tri kataloga, sa pristupačnim live regionom; greška ne prikazuje raw exception ili IPC sadržaj. Bočna navigacija ostaje u visini prozora kada health kartica produži sadržaj.

`RequestValidation` odbija nepoznata DTO polja, nevažeći JSON i vrijednosti izvan dozvoljene šeme. Globalni handler vraća bezbjedan `application/problem+json` sa stabilnim `errorCode`, praznim `parameters`, nasumičnim `correlationId` i `retryable` oznakom, bez putanje, body-ja i exception teksta. `SafeLogProvider` zadržava samo nivo, numerički event ID i `[REDACTED]`; poruke, scopes, kategorije, event imena i exception tekst se ne zapisuju. Ovo namjerno daje minimalnu dijagnostiku. Test rute se registruju isključivo u testnom hostu; produkcioni executable ima samo `/health`.

Nema baze, migracije, vault/unlock, AI, OAuth ili poslovnih operacija. Autentifikacija ostaje iza domenskog `IRequestAuthenticator` ugovora; core ne zavisi od konkretnih adaptera.

## Provjere i kriterijumi

| Kriterijum | Dokaz / rezultat |
|---|---|
| AC-01 | Kestrel stvarno binduje IPv4 loopback sa portom 0; readiness vraća dodijeljeni port. Procesni test potvrđuje ignorisanje javnih env endpointa i odbijanje nepotpunog/velikog/nevažećeg/tihog kanala. Rust test odbija lažni PID, port 0/van raspona, verziju i dodatna polja. |
| AC-02 | CSPRNG token nastaje u Rust-u; stdin/stdout su privatni `Stdio::piped`, token ostaje u Rust/.NET memoriji. Test fresh tokena i pregled ugovora/izlaza procesa. Windows provjera CommandLine pokazala je samo fiksni executable, bez tokena. |
| AC-03 | Stvarni Kestrel HTTP testovi: missing/wrong token, nedozvoljeni/null/missing Origin, pogrešan Host, duplicate headers, nepoznate rute i preflight matrica. Testovi koriste isti middleware kao produkcioni proces. |
| AC-04 | Stvarni Windows UI → Rust → API poziv prikazao je dostupan servis. IPC testovi odbijaju proizvoljne transportne argumente; UI testovi provjeravaju sigurne rezultate i prevode na sva tri jezika. Paritet: 3 kataloga, 34 semantička ključa. |
| AC-05 | DTO negativni slučajevi, nepoznata polja, neočekivana greška i redakcija sintetičkih markera imaju unit/integration dokaze; nijedan marker ne izlazi u ProblemDetails ili strukturisani log. |
| AC-06 | Lokalni direktni HTTP i browser-origin matrica se odbijaju. Windows listener pregled pokazao je samo `127.0.0.1` (u jednoj smoke sesiji port 47315). Povezivanje na mrežne IPv4 adrese sopstvenog računara se odbija. Stvarno zatvaranje desktop prozora uklonilo je sidecar; Rust i .NET procesni testovi provjeravaju cleanup/EOF. Preostala manuelna provjera AC-06 potvrđena je izjavom vlasnika od 12.10.2026; kriterijum je završen. |

Browser-origin matrica je stvarni HTTP test Kestrel-a sa browser zaglavljima. Dodatni privremeni Chrome test sa loopback stranice prikazao je `BROWSER_CORS_REJECTED` bez zaštićenog sadržaja; direktnu browser navigaciju na health blokirao je klijent (`net::ERR_BLOCKED_BY_CLIENT`). Zato se taj browser pokušaj ne koristi kao samostalan dokaz API CORS odluke. Privremeni server/tab su uklonjeni. Installer na čistom računaru čeka STORY-030.

## Ponovljive komande

Koristiti lokalni Node/Rust PATH iz README-a. Komande se izvršavaju iz korijena projekta:

```powershell
dotnet restore PersonalAI.sln --locked-mode
dotnet build PersonalAI.sln --no-restore
dotnet test PersonalAI.sln --no-build --no-restore
dotnet test tests/PersonalAI.SecurityTests/PersonalAI.SecurityTests.csproj --no-restore --collect 'Code Coverage;Format=Cobertura' --results-directory TestResults/STORY-025-Verified
npm --prefix apps/web run build
npm --prefix apps/web run lint
npm --prefix apps/web run test:catalogs
npm --prefix apps/web run test:coverage
npm --prefix apps/desktop run build -- --debug --no-bundle -- --locked
cargo fmt --manifest-path apps/desktop/src-tauri/Cargo.toml --check
cargo test --locked --manifest-path apps/desktop/src-tauri/Cargo.toml
cargo clippy --locked --manifest-path apps/desktop/src-tauri/Cargo.toml --all-targets -- -D warnings
```

BMAD vještina navodi generičke `lint-check.sh`, `pre-commit-check.sh` i `check-coverage.sh`; ovaj repozitorijum ih nema. Iste provjere sprovode projektne npm/.NET/Cargo komande i `git diff --check`, bez izmišljanja rezultata nepostojećih skripti. Pokrivenost se mjeri za implementirani API i frontend; domenski ugovori i generisani kod nisu poslovna implementacija.

## Licence i zaključane zavisnosti

Završna lokalna provjera obuhvata 44 .NET testa (2 unit, 1 integration, 41 security), 21 UI test i 8 Rust testova. API Cobertura izvještaj mjeri 96,10% linija i 87,76% grana; frontend V8 izvještaj 94,44% linija, 95% izraza, 100% grana i funkcija. Prag od 80% linija/grana API-ja je provjeren, a frontend pragove sprovodi Vitest. Raw coverage rezultati ostaju izvan Git-a u TestResults/coverage direktorijumima.

Verzije su zaključane; podrška i licence provjerene su 12.10.2026. u službenim package metapodacima i dokumentaciji. Sve uvedene biblioteke imaju MIT ili Apache-2.0 licencu: `getrandom 0.4.3` (Rust ≥1.85), `serde 1.0.229` (≥1.56), `serde_json 1.0.154` (≥1.71), `reqwest 0.13.5` (≥1.85) i `@tauri-apps/api 2.12.3`. Rust 1.99 i postojeći Tauri 2 zadovoljavaju zahtjeve. Nema novog NuGet paketa; coverage koristi postojeći Microsoft test collector.

Izvori: [Kestrel endpointi](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0), [Tauri capabilities](https://v2.tauri.app/security/capabilities/), [getrandom](https://docs.rs/getrandom/0.4.3/getrandom/), [reqwest proxy i redirect](https://docs.rs/reqwest/0.13.5/reqwest/), [serde](https://docs.rs/serde/1.0.229/serde/), [Tauri npm metapodaci](https://registry.npmjs.org/@tauri-apps/api/2.12.3). Lokalni Cargo.toml metapodaci potvrđuju i licence/kompatibilnost serde_json verzije.

## Vlasnikova manuelna potvrda AC-06

Vlasnik je 12.10.2026, nakon preporuke da dopuni preostalu mrežnu provjeru AC-06 sa drugog uređaja ili izolovane VM, u ovom razgovoru potvrdio: „AC-06 sam testirao manuelno i uredu je. zabiljezi to gdje treba”. Ova izjava prihvaćena je kao vlasnikov manuelni dokaz preostalog dijela AC-06. Uz ranije dokumentovane lokalne rezultate, AC-06 se označava završenim, a STORY-025 dobija status `completed`.

Izvor dokaza je vlasnikova potvrda. Nisu dostavljeni tip uređaja/VM, tačna komanda, port, vrijeme izvršavanja testa ili sirovi izlaz, pa se ti detalji ne navode kao utvrđeni. Codex nije ponovio ovaj manuelni test. Vrijeme `completed_at` u BMAD evidenciji označava evidentiranje završetka na osnovu potvrde, a ne vrijeme izvršavanja manuelnog testa.

## Preostali dokazi i sljedeći korak

Sljedeći korak je `bmad:dev-story STORY-026`; zavisnost od STORY-025 je ispunjena. STORY-026 ovim evidentiranjem nije započeta.

G-002 ostaje otvoren: restart/stale token, instance lock, crash/sleep i stream dokazi pripadaju STORY-026/030. Paket/installer nije isporučen. Naredne priče i Faza 02 nisu započete. G-001 za budući vault i bootstrap gate ostaju nepromijenjeni.

## Stvarni završni rezultati

| Komanda / provjera | Rezultat |
|---|---|
| `./scripts/verify-foundation.ps1 -Desktop` | Kod 0: locked restore, .NET build/test, frontend build/typecheck/lint/katalozi/coverage, Tauri debug build, fmt, 8 Rust testova i clippy |
| Security testovi sa `Code Coverage;Format=Cobertura`, poslije dopune ProblemDetails ugovora | Kod 0, 41/41; 96,10% API linija i 87,76% grana |
| `python docs/bmad/validate-sprint-plan.py --self-test` | Kod 0, 3 početne pozitivne + 1 završna kontrola, 13/13 negativnih; 74 lokalne veze |
| `node docs/bmad/validate-ux.cjs` | Kod 0, PASS; istorijska UX specifikacija i sljedivost |
| Ugrađeni Windows UI `http://tauri.localhost/` | Stvarni health; ENG → ekavica → ijekavica prevode sačuvan rezultat. Selektor ostaje vidljiv. Nakon prekida testnog sidecar-a, ijekavska greška je bezbjedna i lokalizovana. |
| `npm --prefix apps/desktop run dev -- --no-watch` | Native dokument na `http://127.0.0.1:5173/`; stvarni health prolazi kroz isti Rust/token ugovor, API listener samo 127.0.0.1 (smoke port 60122). Normalno zatvaranje uklanja sidecar. |
| Normalno zatvaranje ugrađenog desktop prozora | Nestali desktop i njegov API proces; Rust test dodatno provjerava zatvoren port |

Validator istorijskih kontrola je dopunjen da poništi napredak svih priča u svojim kopijama, umjesto da početne scenarije zagađuje tekućim statusom STORY-025. Stvarna YAML evidencija i završetak STORY-024 nisu poništeni.

Izmijenjene cjeline: `src/PersonalAI.Api/{Program,ApiHost,SessionConfiguration,SidecarEntry,RequestValidation,SafeLogProvider}.cs`; Rust `main.rs`, `transport.rs`, manifest/capability i zaključane zavisnosti; `apps/web` health komponenta/transport/prevodi/stilovi/testovi; razvojni `prepare-sidecar.ps1`; README, ADR-002, status osnove, BMAD evidencije/validator i ova priča. Nema nove arhitektonske alternative ili migracije.

## Ponovljena provjera STORY-025 — 12.10.2026.

Ponovljeni zahtjev `bmad:dev-story STORY-025` zatekao je priču sa statusom `completed` i postojećim izmjenama u radnom direktorijumu. Pregled implementacije i testova prema AC-01–06 nije zahtijevao dodatne promjene koda. ADR-002 je usklađen sa već evidentiranom vlasnikovom potvrdom AC-06; arhitektonska odluka ostaje ista.

| Izvršena komanda / provjera | Stvarni rezultat ponovljene provjere |
|---|---|
| `./scripts/verify-foundation.ps1 -Desktop` uz lokalni Node/Rust PATH iz README-a | Kod 0. Locked restore i .NET build: 0 grešaka i 0 upozorenja; 44/44 testa (2 unit, 1 integration, 41 security), bez preskočenih. Frontend build/typecheck/lint i granice renderera prolaze; 3 kataloga / 34 ključa; 21/21 UI test. Tauri debug executable izgrađen; fmt, 8/8 Rust testova i clippy sa `-D warnings` prolaze. Rust test uključuje stvarni sidecar health i zatvoren port poslije cleanup-a. |
| `dotnet test tests/PersonalAI.SecurityTests/PersonalAI.SecurityTests.csproj --no-build --no-restore --collect 'Code Coverage;Format=Cobertura' --results-directory TestResults/STORY-025-Revalidation` | Kod 0, 41/41, bez preskočenih; API 96,10% linija i 87,76% grana. XML provjera oba praga ≥80% prolazi. |
| Frontend V8 coverage u osnovnoj skripti | 94,44% linija, 95% izraza, 100% grana i funkcija; sva četiri praga ≥80% prolaze. |
| `python docs/bmad/validate-sprint-plan.py --self-test` uz `PYTHONPATH` postojećeg TEMP direktorijuma iz plana sprinta | Kod 0; 3/3 početne pozitivne, 1/1 završna i 13/13 negativnih kontrola; 74 lokalne veze. Globalni Python bez tog direktorijuma prvo je prijavio nedostajući PyYAML; postojeća izolovana zavisnost omogućila je uspješan završetak. |
| `node docs/bmad/validate-ux.cjs` | Kod 0, PASS: 3 nacrtna kataloga / 200 ključeva, 44 kontrastna para, 28 FR i 12 NFR. Dokumentaciona provjera. |
| `git diff --check` | Kod 0; nema whitespace grešaka. |

AC-01–05 ponovo su provjereni pregledom i navedenim automatizovanim testovima. Za AC-06 ponovljeni su lokalni HTTP/Origin/loopback i procesni cleanup testovi; raniji Windows UI dokazi i vlasnikova manuelna mrežna potvrda ostaju zasebni izvori. U ovom ponavljanju nisu izvršeni novi desktop UI smoke, drugi uređaj/VM, installer ili GitHub Actions run. Status i prvobitno vrijeme završetka priče nisu promijenjeni; STORY-026 ostaje sljedeći korak, a Faza 01 nezavršena.
