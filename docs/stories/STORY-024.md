# STORY-024 — Pokretna osnova i lokalizovana navigacija

- **Epik:** EPIC-001; roditelji STORY-001 i STORY-002 iz PRD-a.
- **Prioritet:** MUST.
- **Procjena:** 5 bodova, uslovna; početni toolchain i više slojeva povećavaju neizvjesnost.
- **Status:** completed; završena 12.10.2026. uz Windows desktop dokaz. Kandidat sprinta 1; Faza 01 ostaje aktivna i nezavršena.
- **Sljedivost:** FR-001/FR-024 djelimično; NFR-003/007/012; TC-001/024/103/107/112; F01-AC-01/04/05/06.

## Korisnička priča

Kao vlasnik želim da pokrenem praznu privatnu desktop aplikaciju i navigaciju na svom jeziku, da bih provjerio osnovu bez stvarnih naloga i podataka.

## Kriterijumi prihvatanja

- [x] AC-01: Čist checkout podržava .NET 10 solution, React/Vite sa TypeScript strict i Tauri 2 shell; postoje unit, integration i security projekti. Verzije su zaključane, NuGet centralizovan, a podrška/licence izabranih paketa dokumentovane nakon provjere primarnih izvora.
- [x] AC-02: Struktura prati Fazu 01: `apps/web`, `apps/desktop`, `src/PersonalAI.*` i test projekti. Domain/Application nemaju reference na konkretne AI SDK-ove, SQLite ili Tauri; renderer nema tajne, storage ni SDK pristup.
- [x] AC-03: Postoje ugovori `IAIProvider`, `IProviderRegistry`, `ISecretVault`, `IConversationStore`, `IToolRegistry`, `IApprovalService`, `IIntegrationConnector`, `IMemoryService`, `IJobScheduler`, `ISettingsStore`, plus odvojeni autentifikacioni/transportni port gdje je potreban. Nema implementacije budućih funkcija.
- [x] AC-04: Početna stranica i sedam navigabilnih praznih modula rade tastaturom. Startup prikazuje `Locked`; `Unlocked` postoji samo kao ugovorno/testno stanje, bez lažnog produkcionog otključavanja ili sadržaja.
- [x] AC-05: Svaki uvedeni tekst, prazno stanje, greška i accessibility natpis dolazi iz stvarnih zasebnih kataloga `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk`, `en-US`. Selektor odmah mijenja sve tekstove i čuva navigaciju; osnovni paritet ključeva/placeholdera već prolazi.
- [x] AC-06: Tauri capabilities ograničavaju shell/file/remote navigation pristup, CSP nema unsafe-eval, a README navodi provjereno razvojno okruženje i stvarne komande sa rezultatima build/typecheck/lint provjera.

## Implementacija i granice

Planirani moduli: `apps/web/src`, `apps/desktop/src-tauri`, `src/PersonalAI.Domain`, `src/PersonalAI.Application`, `src/PersonalAI.Infrastructure`, `src/PersonalAI.Api`, test projekti, lock fajlovi, build skripte i README. Koristiti i18next/react-i18next prema I18N_SPEC-u. Nacrt `docs/bmad/ux-copy.json` je izvor tekstova za trenutne ekrane, ne produkcioni katalog koji se učitava cijeli bez provjere.

Nema baze, migracije ni javnog endpointa. Minimalni DI skeleton priprema narednu priču; ne dodaje privremeni neautorizovani health. AI jezik ima odvojen ugovor sa četiri dozvoljene vrijednosti, ali se ne čuva u bootstrap-u i ne glumi potvrđenu trajnu postavku prije vaulta.

## Zavisnosti i rubni slučajevi

Nema prethodnih priča. Preduslovi su aktiviranje Faze 01 i provjeren Windows toolchain. Blokira STORY-025/027/028. Nepostojeći alat, nedostupan paket ili neprovjerena licenca daju prijavljenu blokadu; ne dodavati slabiju biblioteku. Nedostajući prevod je greška development/CI provjere. Konkretne verzije se biraju pri implementaciji, ne ovim dokumentom.

## Provjere i završetak

- Unit/contract: obavezni portovi i projektne reference, tri kataloga i placeholderi.
- Integration/desktop: pokretanje praznog shell-a, sve navigacije i selektori tastaturom, bez produkcionog unlock-a.
- Build: dotnet build/test, npm build/typecheck/lint i Tauri razvojni build; tačne putanje i komande dopisati nakon scaffold-a.

Završetak zahtijeva sve AC dokaze, pregled koda, README i stvarni zapis komandi. Nema aplikacionog testa izvršenog pri pisanju priče. Puna pravila završetka su u [planu sprinta](../bmad/sprint-plan.md).

## Početak implementacije — 12.10.2026.

Dodani su solution i slojevi, traženi interfejsi, test projekti, React/Vite UI sa tri kataloga i Tauri shell sa ograničenim capabilities/CSP. U Linux cloud okruženju prošli su .NET build i 4 testa, frontend build/typecheck/lint, paritet 3 kataloga i 10 UI testova. Native `cargo check` staje na nedostajućoj GLib sistemskoj biblioteci; Windows shell još nije pokrenut. CI je konfigurisan, nije izvršen.

AC-02/03 imaju početni implementacioni dokaz; AC-01/04/05/06 imaju lokalne provjere, uz preostalu desktop provjeru. Nijedna checkbox stavka nije označena završenom bez cijelog traženog dokaza. Komande, licence i ograničenja su u izvještaju `docs/architecture/FOUNDATION_STATUS.md`. Priča ostaje `in_progress`.

## Završetak i Windows dokaz — 12.10.2026.

Prethodni odjeljak je istorijski zapis početka. Sada je svih šest AC stavki potvrđeno; [Windows izvještaj](../architecture/STORY-024_WINDOWS_VERIFICATION.md) sadrži mapiranje svakog kriterijuma, pregled koda, tačne komande i ograničenja.

Windows 11 x64: zaključani restore/npm ci, .NET build bez upozorenja/grešaka, 5/5 .NET testova, frontend build/typecheck/lint, 3 kataloga i 10/10 UI testova uz 90,9% frontend line coverage. Tauri debug executable sa ugrađenim UI-jem je izgrađen i pokrenut; Tab/Enter otvaraju svih osam stranica, a jezički selektor radi tastaturom i čuva navigaciju. Native naslov prati aktivni katalog; UI ostaje zaključan. Rust navigaciona zaštita ima 2/2 testa; fmt i clippy prolaze. BMAD lint/pre-commit/coverage provjere prolaze. CI nije izvršen.

Ispravljene su Windows putanje, ESLint obuhvat i native naslov; dodan je zaseban navigation guard bez širih capabilities, PowerShell provjera i dokazima usklađen BMAD validator. Nema skladišta, migracija, stvarnog unlock-a ili naredne implementacione faze. Preostali G-002/health/lifecycle, bootstrap/tema i installer pripadaju STORY-025–030. Precizan sljedeći korak je `bmad:dev-story` za STORY-025, u istoj Fazi 01; Faza 02 nije aktivirana.
