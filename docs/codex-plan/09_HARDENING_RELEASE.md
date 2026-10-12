# Faza 09 — Security hardening, verifikacija i Windows release

## Cilj

Završiti MVP kao stabilnu lokalnu desktop aplikaciju spremnu za ograničeni test sa stvarnim korisnicima (ne automatski sertifikovanu za produkcijsku prodaju). Security testovi su **release gate**, a ne opciona naknadna optimizacija.

## Sigurnosna revizija

1. Izvrši threat-model pregled lokalnog attack surface-a: Tauri renderer, IPC/loopback API, sidecar lifecycle, SQLCipher DB i bootstrap, master passphrase, AES-GCM, token cache, Graph/OAuth, agent tools, custom URL/SSRF, backup/restore, dokument parsiranje.
2. Provjeri CORS, session token, CSRF, CSP i Tauri permissions; nijedan API na `0.0.0.0`, javnom interfejsu ili bez sesijske autentifikacije. Server-side authorization mora biti nezavisan od UI state-a.
3. Audituj crypto implementaciju: Argon2id parametre, HKDF domen separator, nonce uniqueness, AAD, recovery wrap, SQLCipher native distribuciju, promjenu lozinke i memorijski životni vijek ključeva. Koristi provjerene biblioteke, a ne izmišljene kriptografske primitive.
4. Red-team skup testova: prompt injection preko mejla/PDF-a, lažni tool call, odobrenje izmijenjenog payload-a, dupliranje slanja, malicious provider URL, dokument path traversal, zamjena ciphertexta u SQLite, ukradeni šifrovani backup.
5. Tajne u svim logovima/exceptionima/crash reportovima: automatska redakcija i provjere da telemetry ne radi implicitno. Dokumentuj politiku log retention i pravo na brisanje.
6. SCA/SBOM i dependency review: NuGet, npm, Rust crates i native SQLCipher; licence provjerene za komercijalnu distribuciju. Lockfile, reproducibilni build gdje je moguće.
7. Backup/restore vježba na čistoj Windows instalaciji; kvar fajla, neispravna lozinka i izgubljeni recovery kod daju bezbjedno ponašanje.
8. Accessibility i funkcionalni E2E scenariji: registracija/otključavanje, BYOK, model switch, Outlook OAuth, analiza mejla, dokument, zadatak, approve, lock, backup. Obavezno cijeli ključni tok proći **u sve tri lokalizacije (`sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk`, `en-US`)** i dokazati trajno pamćenje izbora jezika i srpske varijante.

## Distribucija i update

- Tauri 2 Windows x64 installer uključuje pravilan .NET sidecar, potrebne native biblioteke za SQLCipher i startup lifecycle.
- Signed installer i verified update artefakti; ako nema code-signing certifikata, **ne tvrditi da je release potpisan**. Za komercijalnu isporuku definisati postupak pribavljanja certifikata i Tauri updater signing.
- Migracije izvodi backend sa provjerom integriteta i prethodnom šifrovanom rezervnom kopijom; nema downgrade-a koji bi obrisao korisničke podatke.
- Odvojiti debug i release: debug console, EF sensitive logging, dev origin i test API ključevi nisu dostupni u releaseu.
- Uninstaller ne briše privatne podatke automatski bez jasne potvrde; omogućiti eksplicitno sigurno uklanjanje korisničke instance uz pojašnjenje ograničenja fizičkog brisanja na SSD-u.

## Test matrica

| Skup | Minimalni slučajevi |
|---|---|
| Unit | Vault, permissions, model mapper, timezone, memory |
| Integration | SQLCipher, OAuth token cache, Graph mock, local API auth |
| Security | Wrong passphrase, tampering, prompt injection, SSRF, privilege bypass |
| E2E | Setup -> provider -> chat -> email -> draft/approve -> lock/unlock; cijeli tok za SRB (Ekavica), SRB (Ijekavica) i ENG |
| i18n | Paritet sva tri kataloga, placeholderi, pluralna pravila jezika, srpska latinica i varijanta, datumi/brojevi, per-user default, restart, locked ekran, nezavisni AI jezik, migracija starog sačuvanog koda u ijekavicu i backup/restore |
| Recovery | Backup/restore, izgubljena lozinka + recovery key, interrupted update |
| Release | Installer Windows 11, cold start, app restart, update/rollback procedure |

## MVP release kriterijumi

- [ ] Svaka faza 01–08 ima uspješne testove i dokaz acceptance kriterijuma.
- [ ] Nijedan plaintext API ključ ne može se pronaći u DB datoteci, WAL, backupu ili logovima.
- [ ] Ne postoji prečica kojom LLM zaobilazi odobravanje ili izvršava nerazrađen alat.
- [ ] Prepoznata ograničenja su vidljiva korisniku: aktivna aplikacija i otključan vault za zakazane poslove, eksterni AI cloud prenosi podatke.
- [ ] Installer i zavisnosti rade bez developerskog okruženja na čistom Windowsu.
- [ ] Postoje recovery priručnik, pravila privatnosti, lista third-party komponenti/licenci i lista poznatih rizika.
- [ ] Korisnik bira SRB (Ekavica), SRB (Ijekavica) ili ENG kao podrazumijevani jezik; sve tri lokalizacije su kompletne i izbor jezika/varijante preživljava cold start, update i vraćanje backup-a, uključujući migraciju starog sačuvanog `sr-Latn-RS` u ijekavicu.
- [ ] `I18N_SPEC.md` Definition of Done je u potpunosti ispunjen i potvrđen automatizovanim + ručnim testovima.
- [ ] Prije šire komercijalne distribucije dogovorena nezavisna security revizija i kontrolisani pilot.

## Zahtijevani završni izvještaj Codexa

Napisati `docs/release/MVP_READINESS.md` sa tabelom: zahtjev, implementacija, test ID, dokaz, status, otvoreni rizik, prioritet. Ne označavati neprovjerene ili ručno neizvršene testove kao `Passed`.
