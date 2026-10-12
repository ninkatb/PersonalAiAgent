# Faza 01 — početak implementacije

**Datum:** 12.10.2026.

**Status:** STORY-024 je u toku. Faza 01 nije završena.

Vlasnik je izričito aktivirao Fazu 01 i potvrdio početak implementacije. Uslovni sprint ostaje bez potvrđenog kapaciteta ili roka. Evidencije bilježe jednu priču u toku i nula završenih bodova.

## Implementirane komponente

- `PersonalAI.sln`: .NET 10 Domain, Application, Infrastructure i Api, uz unit, integration i security projekte. `Directory.Packages.props` centralizuje NuGet verzije; svaki projekat ima zaključane zavisnosti. `global.json` zahtijeva SDK 10.0.401.
- Domain sadrži deset traženih portova i odvojeni `IRequestAuthenticator`, bez konkretnih AI, SQLite ili desktop adaptera. Application registruje samo zaključano početno stanje. Infrastructure je pripremljen za buduće adaptere.
- API executable izlazi kodom 78 i `API_TRANSPORT_NOT_CONFIGURED`. Dok STORY-025 ne implementira privatni kanal i autorizaciju, nijedan HTTP listener ili health ruta se ne otvaraju.
- `apps/web`: React/Vite, TypeScript strict, početna stranica i sedam praznih modula, navigacija tastaturom i selektor sa tri stvarna zasebna kataloga. Promjena jezika čuva trenutnu navigaciju. UI ostaje zaključan; nema produkcionog unlock-a.
- Jezik se trenutno mijenja samo za tekuće pokretanje. Trajni bootstrap, legacy migracija i tema pripadaju STORY-028/029; UI ne tvrdi da je izbor sačuvan.
- `apps/desktop`: Tauri 2 shell sa praznim permissions skupom, bez shell/fs plugin-a ili generičkih IPC komandi, i CSP bez `unsafe-eval`. Shell još ne pokreće sidecar niti pravi installer; to dolazi kroz STORY-025/026/030.
- GitHub Actions provjere su konfiguracije za Linux/Windows .NET/frontend i Windows desktop shell. CI nije izvršen u ovom zadatku.

## Zavisnosti i provjera izvora

Verzije i licence provjerene su na službenom .NET release metadata endpointu, npm registru, NuGet paketima (`.nuspec`) i crates.io metapodacima 12.10.2026. Npm i Cargo lockfile fajlovi čuvaju kontrolne zbirove artefakata; NuGet lockfile fajlovi čuvaju content hash. SDK je preuzet uz provjeru službenog SHA-512, a Rust distribucija uz službeni SHA-256. TLS provjera nije isključivana.

| Alat/paket | Verzija | Licenca / izvor |
|---|---|---|
| .NET SDK | 10.0.401 | MIT; .NET 10 LTS, podrška do 14.11.2028; [release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json) |
| Rust | 1.99.0 | MIT/Apache-2.0; [službeni kanal](https://static.rust-lang.org/dist/channel-rust-stable.toml) |
| React / React DOM | 19.3.0 | MIT; [npm](https://registry.npmjs.org/react/19.3.0) |
| Vite / React plugin | 8.3.4 / 6.1.2 | MIT; [npm](https://registry.npmjs.org/vite/8.3.4); Node 24.19.0 zadovoljava engine uslov |
| TypeScript | 6.0.3 | Apache-2.0; [npm](https://registry.npmjs.org/typescript/6.0.3); ESLint parser podržava <6.1 |
| i18next / react-i18next | 26.4.3 / 17.0.16 | MIT; npm |
| Tauri / tauri-build / CLI | 2.12.3 / 2.7.2 / 2.12.2 | Apache-2.0 ili MIT; [crates.io](https://crates.io/crates/tauri/2.12.3), npm; Rust minimum 1.90 |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT; NuGet |
| xunit / runner | 2.9.3 / 3.1.4 | Apache-2.0; NuGet |
| Vitest / Testing Library / jsdom | 5.0.3 / 16.3.3 + 14.6.7 / 30.1.2 | MIT; npm; Node 24.19.0 zadovoljava engine uslove |
| ESLint / typescript-eslint | 10.12.0 / 8.71.1 | MIT; npm |

Licenca samog projekta još nije određena. Ovaj početak implementacije ne dodjeljuje licencu projektu niti predstavlja kompletnu reviziju licenci svih tranzitivnih paketa prije distribucije.

## Provjere

| Komanda | Stvarni rezultat u Linux cloud okruženju |
|---|---|
| `dotnet build PersonalAI.sln --no-restore -m:4` | Prošlo, 0 upozorenja / 0 grešaka |
| `dotnet test PersonalAI.sln --no-build --no-restore` | 4/4 prošlo: unit 2, integration 1, security 1; nijedan preskočen |
| `npm --prefix apps/web run build` | Prošlo, uključuje TypeScript provjeru |
| `npm --prefix apps/web run lint` | Prošlo, uključuje granice renderer pristupa |
| `npm --prefix apps/web run test:catalogs` | Prošlo: 3 kataloga, 28 semantičkih ključeva, placeholderi i CLDR |
| `npm --prefix apps/web test` | 10/10 prošlo; početni lock, sve navigacije, tastatura, promjena varijante i jezika, OS prijedlog i regionalna mapa |
| `cargo fmt --manifest-path apps/desktop/src-tauri/Cargo.toml --check` | Prošlo |
| Pokretanje API executable-a uz `ASPNETCORE_URLS=http://0.0.0.0:5000` | Izlaz 78, `API_TRANSPORT_NOT_CONFIGURED`; listener nije pokrenut |
| `cargo check --locked --manifest-path apps/desktop/src-tauri/Cargo.toml -j 4` | Nije prošlo: nedostaje `glib-2.0.pc`; GTK/WebKit sistemske zavisnosti nisu instalirane |
| Razvojni server i lokalni HTTP zahtjevi | Prošlo: index, React ulazni modul i ijekavski katalog dostupni su preko loopback-a |
| Dodatni Chromium smoke | Nije izvršen: browser nije instaliran; preuzimanje sa `cdn.playwright.dev` odbijeno je sa HTTP 403 `Domain forbidden` |

Ove provjere nisu Windows desktop smoke test, installer dokaz ili dokaz zaštićenog health poziva.

Chromium smoke je dodatna provjera; za njega je potrebno dozvoliti `cdn.playwright.dev` u mrežnim podešavanjima okruženja ili koristiti već instaliran browser. Postojećih 10 UI testova izvršeno je u jsdom-u; nisu browser E2E dokaz.

## Kriterijumi Faze 01

| Kriterijum | Trenutni dokaz / preostali rad |
|---|---|
| F01-AC-01 | README i razvojne komande dodani; Windows instalacija i sidecar bundle čekaju STORY-030 |
| F01-AC-02 | Planirani Rust → sidecar transport još nije implementiran; STORY-025 |
| F01-AC-03 | Trenutno nema HTTP listenera; token/Origin/Host/loopback dokazi čekaju STORY-025/026 |
| F01-AC-04 | Portovi, slojevi i DI testovi prolaze; nema konkretnih provajdera u core-u |
| F01-AC-05 | .NET/frontend provjere pokrenute; native build blokiran sistemskim bibliotekama, Windows/CI provjere još nisu izvršene |
| F01-AC-06 | Stvarni katalozi, selektor, OS prijedlog i regionalna mapa rade; trajni default i legacy migracija čekaju STORY-027/028 |
| Dopuna teme | STORY-029; bootstrap gate ostaje pending |

Sljedeće: obezbijediti desktop razvojne preduslove iz [Tauri uputstva](https://v2.tauri.app/start/prerequisites/) i provjeriti shell na Windows 11 x64. STORY-024 se završava tek sa svojim AC dokazima; zatim STORY-025 implementira zaštićeni lokalni health. G-001 za budući vault, G-002 i bootstrap gate ostaju otvoreni. Naredna faza nije aktivirana.
