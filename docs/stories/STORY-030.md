# STORY-030 — CI, Windows paket i dokazi Faze 01

- **Epik:** EPIC-001; roditelj STORY-001. Ovo nije STORY-023 za završno MVP izdanje.
- **Prioritet:** MUST.
- **Procjena:** 5 bodova, uslovna; CI i čisto Windows okruženje zahtijevaju provjeru native pakovanja.
- **Status:** not_started; preostali obim Faze 01, bez dodijeljenog sprinta. Razvoj nije odobren.
- **Sljedivost:** FR-001/FR-024 i tema FR-025 djelimično; NFR-001/003/007/008/012; TC-001/024/025/101/103/107/108/112; F01-AC-01–06; G-002 konačni dokaz.

## Korisnička priča

Kao vlasnik želim provjerenu Windows instalaciju osnove i jasan zapis rezultata, da bih mogao ponoviti pokretanje i procijeniti spremnost za sljedeću fazu.

## Kriterijumi prihvatanja

- [ ] AC-01: CI iz čistog checkout-a koristi zaključane restore postupke i izvršava .NET build/unit/integration/security testove, frontend build/typecheck/lint, paritet kataloga i odgovarajuće Rust provjere. Namjerno pokvaren prevod ili negativni sigurnosni test ruši pipeline.
- [ ] AC-02: Windows paket sadrži .NET sidecar sa provjerenim načinom distribucije/runtime-a. Instalacija na čistoj Windows 11 x64 instanci pokreće UI i zaštićeni health bez razvojnih servera ili slučajno instaliranog SDK-a.
- [ ] AC-03: Instalirani paket prolazi startup/quit/restart/crash, token/Origin/Host i LAN izolaciju, instance lock i sleep; nema zaostalog sidecar procesa. G-002 dobija konkretne dokaze svih stavki ili prijavljenu blokadu predaje.
- [ ] AC-04: Paket prolazi tri locale cold start putanje, legacy bootstrap i svih šest locale/tema kombinacija. Kontrolisani update zadržava postojeći izbor; neuspjeh ne briše profil. Installer ne uvodi neautorizovani pristup ili generičke capabilities.
- [ ] AC-05: README navodi verzije/licence, tačne restore/build/test/run/installer komande i rezultate; docs/architecture opisuje dokazani transport, process lifecycle, bootstrap i preostale granice. Evidencija razlikuje automatizovano, ručno, blokirano i neizvršeno.
- [ ] AC-06: Svih šest acceptance kriterijuma Faze 01 ima pojedinačan dokaz, uz temu kao potvrđenu dopunu. Ako kriterijum nema dokaz, faza nije završena. Vlasniku se predaju rizici i sljedeći korak bez aktiviranja Faze 02.

## Implementacija i granice

Moduli: `.github/workflows`, build/publish/package skripte, Tauri bundle konfiguracija, Windows smoke harness, README i docs/architecture. Pipeline se dodaje ranije po potrebi svake priče; ova priča objedinjuje reprodukciju i dokaz instaliranog rezultata. Nema obećanja javnog potpisa/distribucije cijelog MVP-a niti release FR-028 završenosti.

Nema baze ili destruktivne migracije. Upgrade fixture je sintetički bootstrap profil; stvarni korisnički podaci se ne brišu radi testa ili deinstalacije. Ne objavljivati tajne/privatni sadržaj u CI artefaktima.

## Zavisnosti i rubni slučajevi

Zavisi od STORY-024–STORY-029, aktiviranja Faze 01 i dostupnog čistog Windows test okruženja. Nedostajući runtime/native DLL, start na zauzetom portu, prekid instalacije i update sa starim locale-om moraju imati jasan ishod. Bez čistog Windows smoke dokaza paket se ne označava provjerenim.

## Provjere i završetak

CI i lokalne komande, Windows installer/sandbox smoke i kontrolisani update sa izvještajem izlaznih kodova. Ponoviti test samo poslije novih izmjena ili neuspjeha. Završetak zahtijeva sve dokaze prethodnih priča i F01-AC redova, pregled koda i ažuriranu BMAD evidenciju. Build/test i Windows instalacija nisu izvršeni pri pisanju priče.
