# STORY-026 — Windows provjera životnog ciklusa i izolacije

**Datum:** 12.10.2026. **Status:** `in_progress`; implementacija i automatizovani dokazi postoje, stvarni sleep/resume nije izvršen. Faza 01 ostaje aktivna i nezavršena. Faza 02 nije aktivirana.

## Implementacija i odluke

`owned_process.rs` kreira neimenovani Windows Job Object sa `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`. `CreateProcessW` koristi `STARTUPINFOEXW` i `PROC_THREAD_ATTRIBUTE_JOB_LIST`, tako da dijete pripada job-u prije prvog izvršavanja; nema razmaka između starta i naknadnog dodjeljivanja job-a. Neuspješna konfiguracija ili kreiranje procesa odbija startup. Ne postoji slabiji launcher, breakaway ili generička kill komanda. Normalni cleanup i opoziv koriste samo taj job i sačuvani process handle, uz čekanje do pet sekundi. Pad roditelja zatvara kernel handle-ove i uklanja njegov job.

`PROC_THREAD_ATTRIBUTE_HANDLE_LIST` ograničava nasljeđivanje na djetetov stdin, stdout, null stderr i lease profila. Roditeljski krajevi cijevi i job nisu nasljedivi. Samo verzionisani startup okvir nosi token; token nije u argumentima, environment-u, URL-u, rendereru ili fajlu. Rust kreiranje procesa i testni launcher-i koriste zajednički `SPAWN_GATE`, da privremeno nasljedivi handle-ovi ne pređu u drugi istovremeno kreirani test proces. Desktop kreira sidecar prije webview-a; nema kasnijeg automatskog pokretanja drugih sidecar-a.

`profile.rs` otvara `instance.lock` bez dijeljenja na Windows-u. Kernel otvoreni handle je dokaz vlasništva; sadržaj fajla i PID se ne smatraju dokazom. Postojeći marker se nikad ne briše niti skraćuje. Kopija lease-a nasljeđuje se u sidecar, pa naredni pokušaj ostaje odbijen dok prethodno dijete ne zatvori handle. Poslije pada, postojeći marker može ponovo biti otvoren tek kada kernel oslobodi stvarno vlasništvo. Kratak prelazni neuspjeh restarta ostaje bezbjedno odbijanje, bez uklanjanja tuđeg lock-a.

Produkcioni profil je fiksni `profiles/default` ispod Tauri `app_local_data_dir()` za identifikator `com.personalai.agent`. Renderer ne može zadati profil, putanju, PID ili komandu. Desktop zadržava lease i ako handshake ne uspije; ne prikazuje se drugi desktop vlasnik istog profila. Testovi koriste zasebne sintetičke direktorijume u sistemskom temp prostoru. Svaki profil ima svoju `bootstrap.json` putanju; u ovoj priči taj fajl se ne čita, ne kreira i ne mijenja. Trajni bootstrap ostaje STORY-028.

`Revocation` je terminalna tranzicija aktivne transportne sesije: opoziv označi sesiju nevažećom i prekine njen job bez čekanja na mutex aktivnog health poziva. Health odbija završeni proces i opozvanu sesiju i ponovo provjerava opoziv prije vraćanja rezultata. Monitor izlaza sidecar-a šalje rendereru samo nesenzitivni događaj `backend-unavailable`. `power.rs` na owning UI thread-u instalira Windows subclass za `WM_POWERBROADCAST`: suspend i sve tri resume varijante opozivaju istu sesiju. Resume ne pravi novu sesiju; korisnik restartom dobija novi provjeren handshake.

Renderer ima samo `backend_health` i dozvole za slušanje/uklanjanje pretplate na događaje. Nema emit, shell, filesystem, process-kill ili HTTP proxy komande. `BackendHealth` prati lokalnu generaciju zahtjeva, uklanja pretplatu pri unmount-u i odbacuje kasni uspjeh poslije opoziva. Postojeća poruka `health.failed` u sva tri kataloga već traži restart; novi korisnički tekst nije uveden. Produkcioni UI cijelo vrijeme ostaje `Locked`.

Backend `SessionConfiguration.Revoke()` atomarno odbija naredne autentifikacije, briše kontrolisani credential buffer i otkazuje session cancellation token. Middleware povezuje taj token sa `RequestAborted` i prekida HTTP odgovor pri opozivu. Watcher privatnog roditeljskog kanala postavljen je prije listener startup-a; EOF, neočekivani bajt ili prekid opozivaju sesiju i zaustavljaju host. Ovo je transportni mehanizam, bez stvarnog vault unlock-a, ključa baze, AI poziva ili scheduler-a. Ne predstavlja dokaz budućeg brisanja vault ključeva ili zaštite od kompromitovanog OS-a.

## Kriterijumi i G-002

| Kriterijum | Izvršeni dokaz | Stanje |
|---|---|---|
| AC-01 | Rust testovi normalnog cleanup-a, EOF-a/nepoznatog bajta, nasilnog pada roditelja prije handshaka i poslije readiness-a; nepovezani test proces ostaje živ, sintetički korisnički fajl očuvan. Native prozor normalno zatvoren, desktop nasilno prekinut, njegov sidecar uklonjen. | Ispunjeno automatizovano |
| AC-02 | Stvarni restart dobija novi token/Origin; stari token vraća 401 u novoj sesiji, stari Origin 403; stari kanal je zatvoren. Missing executable i nevažeći readiness nemaju alternativni launcher. .NET procesni negativni handshake testovi i UI testovi tri prevoda; native pad sidecar-a ne zatvara desktop. | Ispunjeno automatizovano; vizuelna manuelna dopuna navedena ispod |
| AC-03 | Dva konkurentna starta istog profila imaju tačno jednog vlasnika; neuspješan startup zadržava desktop lease; različiti profili imaju različite portove, lease-ove i bootstrap putanje i odbijaju uzajamne kredencijale. Native druga instanca odbijena, prvobitni par ostaje živ. | Ispunjeno |
| AC-04 | Terminalni opoziv, EOF/nepoznati bajt, aktivni health i sintetički HTTP stream imaju negativne testove. Native `WM_POWERBROADCAST` suspend/resume prekida sidecar i ne kreira drugi; UI testovi potvrđuju Locked i odbacivanje stale rezultata. | Implementirano; stvarni fizički sleep/resume ostaje otvoren |
| AC-05 | `SessionLifecycleTests` registruje `/test/stream` samo u test hostu: missing/wrong token ne otvara tok, autorizovani poziv dobija sintetički okvir, opoziv prekida aktivni odgovor. Poseban test produkcionog hosta potvrđuje 404 za tu rutu. | Ispunjeno; nema produkcionog stream-a ili debug endpointa |
| AC-06 | Native smoke obuhvata normalni close, desktop crash, restart, drugu instancu, sintetičke power poruke i sidecar crash. Odvojeni profili i prekinuti startup pokriveni su procesnim integration testovima. | Djelimično; stvarni Windows sleep/resume i vizuelni pregled nedostupnosti ostaju uslov predaje |

G-002 ostaje `open`. Startup kanal/token/Origin/Host imaju završene STORY-025 dokaze. STORY-026 dodaje pojedinačne rezultate za cleanup, prekinuti startup, crash, restart/stale token, konkurentnost, izolaciju, HTTP cancellation i sintetički native power događaj. Stvarni sleep/resume i vizuelni native pregled su nezatvoreni dio ove priče; installer/čista instalacija/objedinjeni capability dokaz ostaju STORY-030. Nijedan sintetički suspend nije predstavljen kao fizički sleep.

## Komande i stvarni rezultati

Koristiti izolovani Node/Rust PATH iz README-a. Iz korijena projekta:

```powershell
dotnet build PersonalAI.sln --no-restore
dotnet test PersonalAI.sln --no-build --no-restore
dotnet test tests/PersonalAI.SecurityTests/PersonalAI.SecurityTests.csproj --no-restore --collect 'Code Coverage;Format=Cobertura' --results-directory TestResults/story-026
npm --prefix apps/web run build
npm --prefix apps/web run lint
npm --prefix apps/web run test:catalogs
npm --prefix apps/web run test:coverage
npm --prefix apps/desktop run build -- --debug --no-bundle -- --locked
cargo fmt --manifest-path apps/desktop/src-tauri/Cargo.toml --check
cargo test --locked --manifest-path apps/desktop/src-tauri/Cargo.toml -- --nocapture
cargo clippy --locked --manifest-path apps/desktop/src-tauri/Cargo.toml --all-targets -- -D warnings
./scripts/verify-lifecycle-windows.ps1
python docs/bmad/validate-sprint-plan.py --self-test
git diff --check
```

| Završna provjera | Stvarni rezultat |
|---|---|
| Locked .NET restore, build i `dotnet test PersonalAI.sln --no-build --no-restore` | Prošlo; 0 grešaka/upozorenja, 48/48 testova: 2 unit, 1 integration, 45 security; nijedan preskočen |
| Security testovi sa `Code Coverage;Format=Cobertura`, završni `TestResults/story-026-final` | Kod 0, 45/45; API 96,60% linija i 88,24% grana; prag 80% provjeren. `SessionConfiguration`: 100% linija, 94,44% grana |
| Frontend build/typecheck/lint/katalozi | Prošlo, lint bez upozorenja; 3 kataloga, 34 semantička ključa, placeholderi i CLDR |
| Frontend `test:coverage` | 25/25 testova; 95,83% linija, 96,49% izraza, 96,55% grana, 100% funkcija; svi pragovi prolaze |
| Native debug build sa `--no-bundle -- --locked` | Kod 0; executable sa ugrađenim UI-jem i pripremljenim sidecar-om; installer nije kreiran |
| `cargo fmt --check`, `cargo test --locked`, `cargo clippy --all-targets -- -D warnings` | Kod 0; 17/17 Rust testova, nijedan preskočen; clippy bez upozorenja. Rust coverage nije izmjeren |
| `./scripts/verify-lifecycle-windows.ps1` poslije završnog build-a | Kod 0; duplicate/normalni close/desktop crash/restart/sintetički suspend-resume/sidecar crash prolaze |
| `python docs/bmad/validate-sprint-plan.py --self-test` | Kod 0 uz postojeći izolovani PyYAML 6.0.3; 3 početne + 1 završna pozitivna kontrola, 13/13 negativnih, 77 lokalnih veza; 2 završene priče i 1 u toku |
| `node docs/bmad/validate-ux.cjs` | Kod 0, PASS; provjera istorijske UX specifikacije, bez tvrdnje da je cijeli UX implementiran |
| `git diff --check` | Kod 0; Git prijavljuje samo postojeću LF/CRLF politiku |

`verify-foundation.ps1 -Desktop` u završnom prolazu izvršio je .NET i frontend provjere, pa stao na neformatiranoj liniji novog Rust testa. Poslije `cargo fmt` posebno su uspješno izvršene preostale fmt/native build/test/clippy provjere i native smoke. Ne tvrdi se da je taj prethodno zaustavljeni poziv skripte imao kod 0. Prvi poziv dokumentacionog validatora bez `PYTHONPATH` nije imao PyYAML; ponovljeni poziv koristi postojeći `%TEMP%/personal-agent-sprint-validation-packages`, bez nove instalacije ili globalne promjene Python-a.

Native smoke od 12.10.2026. bilježi parove desktop/sidecar PID: 9112/27076 (normalni close i duplicate), 16848/37076 (desktop crash/restart), 30496/16228 (sintetički suspend/resume), 33292/17140 (sidecar crash). PID nisu trajni identiteti i nisu korišćeni za otkrivanje ili uklanjanje tuđeg vlasnika.

Poslije završnog build-a isti smoke ponovo prolazi sa parovima 36344/2200, 34076/20464, 30764/41672 i 34288/40000. Test nasilnog prekida roditelja bilježi 29492/38924 poslije readiness-a i 33144/22580 prije handshaka. Testni harness je u `#[cfg(test)]`; produkcioni executable ne čita njegove environment promjenljive.

Prva smoke skripta nije čekala završetak konstrukcije prozora i koristila je keširan `MainWindowHandle`, pa je normalni close istekao. Harness sada čeka lokalizovani document title, radi sinhroni WM_NULL roundtrip i osvježava process handle prije close-a. Ponovljeni normalni close prolazi. Paralelni Rust test otkrio je privremeno nasljeđivanje handle-a kroz konkurentni test launcher; zajednički spawn guard i čekanje kernel oslobađanja otklanjaju taj slučaj. Početni neuspješni rezultati nisu prećutani.

BMAD generičke bash lint/pre-commit/coverage skripte ne postoje u projektu. Njihove provjere obavljaju navedene projektne komande i `git diff --check`. CI i installer nisu izvršeni; native smoke provjerava lokalni debug executable.

## Licenca i podrška

Jedina nova direktna zavisnost je postojeći transitive `windows-sys`, sada eksplicitno zaključan na `0.61.2` u Cargo.toml/Cargo.lock. Lokalni registry paket potvrđuje `rust-version = 1.71` i `MIT OR Apache-2.0`; postojeći Rust 1.99 zadovoljava minimum. Microsoftov [windows-rs](https://github.com/microsoft/windows-rs) održava Windows API binding-e. Mehanizmi su provjereni prema [Windows Job Objects](https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects), [UpdateProcThreadAttribute](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-updateprocthreadattribute) i [resume događajima](https://learn.microsoft.com/en-us/windows/win32/power/pbt-apmresumesuspend). Windows 11 x64 je podržani desktop cilj; ovo nije obećanje Linux/macOS lifecycle podrške.

## Pregled koda, rizici i sljedeći korak

Pregled je obuhvatio vlasništvo/RAII handle-ova, atomarni job assignment, allowlist nasljeđivanja, per-profile lease, greške startup-a, opoziv bez mutex čekanja, cancellation middleware, renderer stale rezultat i granicu testnog hosta. Nema novog storage/webhook/idempotency opsega, destruktivne migracije ili privatnog fixture-a. Postojeće nezabilježene izmjene STORY-024/025 očuvane su.

Izmijenjeni fajlovi po odgovornosti:

- Desktop: novi `owned_process.rs`, `profile.rs`, `power.rs`, `lifecycle_tests.rs`; dopunjeni `main.rs`, `transport.rs`, capability manifest i zaključana `windows-sys` zavisnost.
- API/UI: `SessionConfiguration.cs`, `ApiHost.cs`, `SidecarEntry.cs`, novi `SessionLifecycleTests.cs`, te `BackendHealth.tsx` i njegovi testovi. Postojeći prevodi se ponovo koriste.
- Provjera/evidencija: nova `scripts/verify-lifecycle-windows.ps1` i ovaj izvještaj; ažurirani STORY-026, README, ADR-002, FOUNDATION_STATUS, BMAD arhitektura/plan/statusi i DECISIONS.

Naredni korak je manuelni dokaz u istoj STORY-026: pokrenuti native executable, provjeriti zaštićeni health; stvarno uspavati Windows, zatim nastaviti; potvrditi `Locked`, lokalizovanu nedostupnost i da se sidecar nije automatski vratio. Restart treba da omogući novi health uz novi sidecar. Ponoviti vidljivu nedostupnost poslije pada sidecar-a na sva tri jezika. Zabilježiti PID-ove/ishode bez tokena ili privatnog sadržaja. U ovoj sesiji računar nije prisilno uspavan; vlasnik još nije dostavio manuelnu potvrdu STORY-026.

Do tih dokaza AC-04/06 ostaju otvoreni i priča je `in_progress`. Ne započinjati sljedeću priču ili Fazu 02 ovim izvještajem. Bootstrap gate, G-001, kapacitet i rok sprinta ostaju nepromijenjeni.
