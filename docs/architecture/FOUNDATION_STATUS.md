# Faza 01 — stanje implementacije

**Datum:** 12.10.2026.

**Status:** STORY-024 i STORY-025 su završene. STORY-026 je `in_progress` uz implementiran lifecycle i automatizovane Windows dokaze; stvarni sleep/resume i vizuelni native pregled ostaju otvoreni. Faza 01 nije završena.

Vlasnik je izričito aktivirao Fazu 01 i potvrdio početak implementacije. Uslovni sprint ostaje bez potvrđenog kapaciteta ili roka. Evidencije bilježe 2/7 završenih priča, 10/33 boda; STORY-025 je završena na osnovu lokalnih transportnih dokaza i vlasnikove potvrde uspješne manuelne provjere AC-06.

[Windows provjera STORY-024](STORY-024_WINDOWS_VERIFICATION.md) bilježi nastavak u Windows 11 x64 okruženju: .NET/frontend provjere, ispravke putanje/ESLint-a, strožu Tauri navigaciju, coverage i razvojni toolchain. Tabela Linux rezultata ispod ostaje istorijski zapis početka rada.

## Implementirane komponente

- `PersonalAI.sln`: .NET 10 Domain, Application, Infrastructure i Api, uz unit, integration i security projekte. `Directory.Packages.props` centralizuje NuGet verzije; svaki projekat ima zaključane zavisnosti. `global.json` zahtijeva SDK 10.0.401.
- Domain sadrži deset traženih portova i odvojeni `IRequestAuthenticator`, bez konkretnih AI, SQLite ili desktop adaptera. Application registruje samo zaključano početno stanje. Infrastructure je pripremljen za buduće adaptere.
- API iz STORY-025 otvara samo 127.0.0.1:0 poslije privatnog handshaka. Health i sve stvarne rute traže token i tačan session Origin/Host; neispravan ili tihi kanal izlazi kodom 78 bez listenera. [Windows dokazi](STORY-025_WINDOWS_VERIFICATION.md) pokrivaju HTTP/DTO/redakciju i startup procese.
- `apps/web`: React/Vite, TypeScript strict, početna stranica i sedam praznih modula, navigacija tastaturom i selektor sa tri stvarna zasebna kataloga. Promjena jezika čuva trenutnu navigaciju. UI ostaje zaključan; nema produkcionog unlock-a.
- Jezik se trenutno mijenja samo za tekuće pokretanje. Trajni bootstrap, legacy migracija i tema pripadaju STORY-028/029; UI ne tvrdi da je izbor sačuvan.
- `apps/desktop`: Tauri 2 shell sa capability samo za backend_health, bez shell/fs plugin-a ili generičkih IPC komandi, i CSP bez `unsafe-eval`. Shell pokreće fiksni sidecar preko privatnih stdin/stdout cijevi i normalno ga uklanja pri zatvaranju. Installer ostaje STORY-030; puni lifecycle STORY-026.
- Windows native debug build i pokretanje su provjereni: svih osam stranica Tab/Enter tastaturom, selektor tri jezika i lokalizovan naslov. Poseban Rust navigation guard odbija vanjske/file URL-ove; 2 testa i clippy prolaze. .NET rezultati STORY-024 su 5 testova, frontend 10 i 90,9% line coverage; STORY-025 ima zaseban noviji izvještaj. [Windows izvještaj](STORY-024_WINDOWS_VERIFICATION.md) mapira svih šest AC priče.
- GitHub Actions provjere su konfiguracije za Linux/Windows .NET/frontend i Windows desktop shell. CI nije izvršen u ovom zadatku.

STORY-026 dodaje Windows Job Object, per-profile kernel lease, terminalni opoziv transporta, native power handler i cancellation HTTP odgovora. Renderer dobija samo read-only događaj nedostupnosti uz health. [Novi Windows izvještaj](STORY-026_WINDOWS_VERIFICATION.md) sadrži komande, pojedinačne kriterijume i granice dokaza. F01-AC-03/05 dobijaju lifecycle/test dokaze; konačni installer i trajne UI postavke ostaju otvoreni u istoj fazi. Nema promjene broja završenih priča ili bootstrap implementacije.

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
| F01-AC-02 | Stvarni Windows UI → Rust → API health prolazi; STORY-025 izvještaj |
| F01-AC-03 | Token/Origin/Host/preflight/loopback lokalni testovi prolaze; vlasnik je potvrdio manuelnu provjeru AC-06 STORY-025. STORY-026 lifecycle ostaje otvoren |
| F01-AC-04 | Portovi, slojevi i DI testovi prolaze; nema konkretnih provajdera u core-u |
| F01-AC-05 | .NET/frontend/native debug build, testovi, fmt/clippy i Windows shell smoke prošli; installer, puni phase scenariji i CI izvršavanje čekaju naredne priče |
| F01-AC-06 | Stvarni katalozi, selektor, OS prijedlog i regionalna mapa rade; trajni default i legacy migracija čekaju STORY-027/028 |
| Dopuna teme | STORY-029; bootstrap gate ostaje pending |

Windows razvojni preduslovi iz [Tauri uputstva](https://v2.tauri.app/start/prerequisites/) sada su pripremljeni i shell je provjeren. STORY-024 i STORY-025 su završene sa zasebnim AC dokazima. STORY-026 je implementirana uz automatizovane dokaze; sljedeći korak je stvarni sleep/resume i vizuelna native provjera u istoj priči. G-001, preostali G-002 i bootstrap gate ostaju otvoreni. Faza 02 nije aktivirana.
