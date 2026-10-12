# STORY-024 — Windows provjera

**Datum:** 12.10.2026. **Status:** PASS za svih šest kriterijuma STORY-024; priča je završena. Faza 01 nije završena.

## Okruženje i preduslovi

Windows 11 Pro x64, build 10.0.26200; .NET SDK 10.0.401. Node 24.19.0 (npm 11.17.0) i Rust 1.99.0 MSVC (cargo 1.99.0, rustup 1.29.1) pripremljeni su u `%LOCALAPPDATA%/PersonalAI/dev-tools`, bez promjene globalnog Node-a ili trajnog PATH-a. Preuzimanja sa nodejs.org/static.rust-lang.org provjerena su službenim SHA-256 kontrolnim zbirovima. Rust ima rustfmt i clippy.

Postojeći Visual Studio Community 2022 dopunjen je, na izričit zahtjev vlasnika „instaliraj sam”, workload-om `Microsoft.VisualStudio.Workload.NativeDesktop` i preporučenim komponentama. Pokrenut je postojeći Microsoft installer sa provjerenim Authenticode potpisom, `modify --quiet --norestart`, uz UAC podizanje privilegija. Installer je vratio **0**, bez restarta; `vswhere` sada potvrđuje C++ komponente. MSVC linker 14.44.35207 i Windows SDK 10.0.26100.0 su prisutni. WebView2 155.0.4283.45 je već instaliran. Preduslovi i parametri slijede [Tauri uputstvo](https://v2.tauri.app/start/prerequisites/) i [Microsoft CLI uputstvo](https://learn.microsoft.com/en-us/visualstudio/install/use-command-line-parameters-to-install-visual-studio?view=vs-2022).

## Izmjene i pregled koda

- `check-boundaries.mjs` pretvara file URL pomoću `fileURLToPath`; prethodno je `root.pathname` pravio nevažeću Windows putanju. Renderer provjera sada radi na oba OS-a.
- ESLint izuzima generisane `dist`/`coverage` fajlove i prepoznaje Node globale u razvojnim skriptama. Produkcioni renderer zadržava browser pravila.
- Test navigacije sada koristi stvarni Tab redoslijed, umjesto programskog postavljanja fokusa. Prolazi za sve tri lokalizacije.
- Security projekat dodatno provjerava da Application referencira samo Domain, platformu i DI apstrakcije; NuGet lock ažuriran je za projektnu referencu.
- Tauri `on_navigation` zasebno zabranjuje napuštanje lokalnog UI origin-a. Capabilities ostaju prazne; ne dodaju se shell/fs prava ili IPC komande. Dozvoljeni su `http://tauri.localhost` i `tauri://localhost`, a u debug buildu i tačno `http://127.0.0.1:5173`. Negativni Rust testovi pokrivaju vanjski host, sličan host, pogrešan port/protokol, credentials, file/data/javascript/about URL-ove. Windows WebView2 podrazumijevano odbija nove prozore kada handler nije registrovan, što je provjereno u zaključanom Wry izvoru.
- Glavni native prozor kreira se iz iste konfiguracije uz Rust callback za `document.title`, koji već dolazi iz aktivnog kataloga. Nema hardkodovanog početnog naslova ili novih renderer dozvola za promjenu prozora. Neuspješna sinhronizacija naslova daje samo stabilni kod `DESKTOP_TITLE_UPDATE_FAILED`.
- `@vitest/coverage-v8` 5.0.3 dodat je isključivo kao razvojna zavisnost; [npm metapodaci](https://registry.npmjs.org/@vitest%2fcoverage-v8/5.0.3) potvrđuju MIT licencu. Verzija odgovara Vitest-u i zaključana je u package-lock-u. Pragovi 80% za linije/grane/funkcije/izraze automatski ruše neuspješnu provjeru.
- `verify-foundation.ps1` pruža Windows provjere sa prekidom na prvom neuspješnom native izlaznom kodu; `-Desktop` dodaje Tauri razvojni build i Rust testove.
- BMAD validator prihvata završetak uz vrijeme i dokaz, provjerava metrike/zavisnosti i potvrđene AC stavke. Autorizacija ostaje samo za Fazu 01.
- CI konfiguracija uključuje frontend coverage i zaključani desktop build/test/clippy; CI još nije izvršen. Tauri generisane ACL šeme iz `src-tauri/gen` izuzete su iz Git-a.

Arhitektonske granice ostaju iste: nema skladišta, migracije, tajni, stvarnog unlock-a, AI poziva, sidecara ili HTTP listenera u ovoj priči. Storage durability i webhook/idempotency zahtjevi nisu primjenjivi na prazni shell bez upisa. API se i dalje bezbjedno zatvara dok zaštićeni transport nije implementiran.

## Komande i stvarni rezultati

Iz korijena repozitorijuma, u PowerShell-u, za ovu izolovanu pripremu:

```powershell
$taskTools = Join-Path $env:LOCALAPPDATA 'PersonalAI/dev-tools'
$env:CARGO_HOME = Join-Path $taskTools 'cargo'
$env:RUSTUP_HOME = Join-Path $taskTools 'rustup'
$env:Path = (Join-Path $taskTools 'node-v24.19.0-win-x64') + ';' + (Join-Path $taskTools 'cargo/bin') + ';' + $env:Path
dotnet restore PersonalAI.sln --locked-mode
npm ci --prefix apps/web
npm ci --prefix apps/desktop
./scripts/verify-foundation.ps1
./scripts/verify-foundation.ps1 -Desktop
npm --prefix apps/desktop run build -- --debug --no-bundle -- --locked
cargo test --locked --manifest-path apps/desktop/src-tauri/Cargo.toml
```

| Izvršena provjera | Rezultat |
|---|---|
| Zaključani .NET restore, npm ci oba projekta | Kod 0; frontend/desktop audit pri instalaciji: 0 ranjivosti |
| `dotnet build PersonalAI.sln --no-restore` | Kod 0; 0 upozorenja, 0 grešaka |
| `dotnet test PersonalAI.sln --no-build --no-restore` | Kod 0; 5/5, unit 2 / integration 1 / security 2, bez preskakanja |
| `npm --prefix apps/web run build` | Kod 0; TypeScript strict i Vite build |
| `npm --prefix apps/web run lint` | Kod 0; ESLint i renderer granice |
| `npm --prefix apps/web run test:catalogs` | Kod 0; 3 stvarna kataloga, 28 semantičkih ključeva, placeholderi i CLDR |
| `npm --prefix apps/web run test:coverage` | Kod 0; 10/10 UI testova, linije 90,9%, izrazi 91,66%, grane/funkcije 100%; prag 80% prošao |
| `cargo fmt --manifest-path apps/desktop/src-tauri/Cargo.toml --check` | Kod 0 |
| API executable uz `ASPNETCORE_URLS=http://0.0.0.0:5000` | Izlaz 78, `API_TRANSPORT_NOT_CONFIGURED`; ne pokreće listener |
| `./scripts/verify-foundation.ps1` | Kod 0; sve provjere bez opcije Desktop |
| `./scripts/verify-foundation.ps1 -Desktop` | Kod 0; konačni cjeloviti Windows restore/build/test/lint/catalog/coverage/fmt/native build/test/clippy niz |
| BMAD `lint-check.sh` kroz `pre-commit-check.sh` u apps/web | Kod 0; ESLint svih izvornih fajlova i typecheck |
| BMAD `pre-commit-check.sh` u apps/web | Kod 0; lint i 10 testova. Staged skup je prazan; dodatno je pregledan stvarni radni diff |
| BMAD `check-coverage.sh` u apps/web | Kod 0; isti izvršni pragovi i izmjerena pokrivenost |
| `python docs/bmad/validate-sprint-plan.py --self-test` | Kod 0; 3 istorijska pozitivna stanja + 1 završetak, 13/13 negativnih kontrola odbijeno |
| `node docs/bmad/validate-ux.cjs` | Kod 0; dokumentaciona provjera 3 nacrta kataloga, 200 ključeva, 44 kontrastna para; nije desktop dokaz |
| `npm --prefix apps/desktop run build -- --debug --no-bundle -- --locked` | Kod 0; native executable sa ugrađenim frontendom |
| `cargo test --locked --manifest-path apps/desktop/src-tauri/Cargo.toml` | Kod 0; 2/2, bez preskakanja |
| `cargo clippy --locked --manifest-path apps/desktop/src-tauri/Cargo.toml --all-targets -- -D warnings` | Kod 0; bez upozorenja |
| Native Windows prozor | Prošlo: locked startup, svih osam navigacija tastaturom, tri lokalizacije i native naslov; detalji ispod |

Početni cargo check je stao na `link.exe not found`; ponovljen je poslije instalacije preduslova. Tokom dodavanja Application security provjere zabilježen je privremeni build neuspjeh zbog nedostajuće projektne reference; referenca/lock su dodani i cijeli build/test ponovljen uspješno. Generički lint je prvo obrađivao build artefakte, a paralelno coverage pokretanje je odbijeno zaštitom Vitest-a; oba slučaja riješena su konfiguracijom i sekvencijalnim ponavljanjem.

Native kompajler je otkrio neodređen generički tip prvog nacrta navigacionog plugin-a; konačno rješenje koristi WebviewWindowBuilder sa direktnim callback-ovima za navigaciju/naslov. Dodatni dijagnostički rebuild odbijen je Windows zaključavanjem pokrenutog executable-a (`Access is denied`); testni proces je zatvoren, privremena dijagnostika uklonjena, pa konačni build, 2 testa, clippy i fmt ponovljeni sa kodom 0.

Pokrivenost iz tabele odnosi se na frontend, uključujući nepokriveni ulazni `main.tsx`. Dodatni .NET `Code Coverage;Format=Cobertura` pilot je izvršen prije proširenja security testa: 4/4 testa, zbir linija 36,26% (33/91), uz većinom nepozvane DTO/port ugovore budućih priča. To nije dokaz 80% pokrivenosti backend-a; testovi backend-a u ovoj priči provjeravaju ugovore, početni lock, DI i granice slojeva. Native pokrivenost nije mjerena.

## Desktop scenariji i AC dokazi

Provjera je izvršena na native executable-u `apps/desktop/src-tauri/target/debug/personal-ai-desktop.exe`, sa ugrađenim frontend assetima preko `http://tauri.localhost/`; Vite server nije bio potreban. Windows Computer Use vještina korišćena je za snimke prozora, accessibility tekst i unos u sam test prozor. Početni screenshot bio je prazan tokom pokretanja; naredno očitavanje pokazalo je UI bez promjene koda ili sigurnosnog fallback-a. To nije mjerenje startup performansi. Ručni unos vlasnika privremeno je prekidao automatizaciju; nastavak tastature izvršen je nakon njegovog odgovora „prozor je slobodan”.

| Izvršeni desktop scenario | Dokaz / rezultat |
|---|---|
| Startup | Ugrađeni UI prikazan u WebView2; „The application is locked.”, prazni moduli i 0 stavki; nema unlock dugmeta ili stvarnog sadržaja |
| Fokus | Tab iz web dokumenta prikazuje skip link, pa vidljiv okvir fokusa na Overview i svakom narednom dugmetu; provjeren stvarni screenshot, jer UIA fokus u webview-u često ostaje prijavljen na RootWebArea |
| Sve navigacije tastaturom | Tab + Enter otvorili Overview, Chats, Mail, Calendar, Documents, Tasks, Memory, Settings; poslije svakog Enter-a pročitan odgovarajući naslov i prazno stanje |
| Jezici tastaturom | Tab do selektora; Home bira ekavicu („Obe”), Down ijekavicu („Obje”); engleski je prethodno potvrđen. Sve promjene čuvaju aktivnu stranicu Settings i zaključano stanje |
| Native naslov | English: „Personal AI Agent”; obje srpske varijante: „Lični AI agent”; promjena je potvrđena u vraćenom Windows naslovu, bez renderer IPC-a |
| Stvarni prevodi | English overview/prazno stanje; ijekavski overview „Sve na jednom mjestu” i „Ovdje još nema…”; obje srpske varijante Settings; nema sirovih translation ključeva u posmatranim prikazima |
| Završetak provjere | Testni proces zaustavljen prije ponovnog builda; ne postoje korisnički podaci za upis u ovoj priči |

Desktop provjera je smoke test izgrađenog shell-a. Svih osam navigacija u svakoj od tri lokalizacije, zadržavanje Documents stranice pri promjeni varijante, OS prijedlog i regionalna mapa dodatno su provjereni u 10 automatizovanih UI testova. Nisu izvršeni installer, restart pamćenja jezika, vault ili zaštićeni health scenariji drugih priča.

| Kriterijum STORY-024 | Dokaz |
|---|---|
| AC-01 | .NET 10 locked restore/build, npm ci oba projekta i Tauri native debug build; centralni NuGet/lock fajlovi; provjera licenci u FOUNDATION_STATUS i ovom izvještaju |
| AC-02 | Tražena struktura; Domain i Application security testovi; renderer boundary lint; native shell bez shell/fs plugin-a ili privilegovanih renderer komandi |
| AC-03 | 11 portova sa zasebnim IRequestAuthenticator; unit contract test i DI test; budući adapteri nisu registrovani |
| AC-04 | Desktop Tab/Enter svih 8 stranica, početni lock i prazni prikazi; unit/DI/UI testovi potvrđuju lock bez produkcionog unlock-a; Unlocked ostaje samo tipizovan ugovor |
| AC-05 | 3 zasebna stvarna kataloga, paritet 28 ključeva/placeholdera/CLDR; UI testovi svih jezika; native jezički selektor, zadržana navigacija i lokalizovan naslov |
| AC-06 | Prazne capabilities, CSP bez unsafe-eval, zaseban native navigation guard sa 2 pozitivna/negativna Rust testa; Windows build, fmt/clippy i README komande |

Pregled koda u opsegu STORY-024 provjeren je prema BMAD Developer Reference/clean-code/testing pravilima. Pronađene Windows putanje, ESLint obuhvat, nepokrivena remote navigacija i native naslov popravljeni su. Nema preostalog kritičnog nalaza u opsegu ove priče; prethodne arhitektonske granice nisu promijenjene.

## Preostali opseg

Zaštićeni health i token/Origin/Host dokazi pripadaju STORY-025; lifecycle/sleep/instance lock STORY-026; dodatno formatiranje STORY-027; trajni locale/legacy STORY-028; tema STORY-029; installer/čisto instalaciono okruženje/CI dokaz STORY-030. G-002, bootstrap gate i G-001 za budući vault ostaju otvoreni. Faza 02 nije aktivirana.
