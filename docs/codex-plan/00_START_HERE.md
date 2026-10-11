# Personal AI Agent — plan razvoja za Codex

**Status:** specifikacija za implementaciju | **Verzija:** 1.2 | **Datum:** 11.10.2026.

## Vizija proizvoda

Komercijalna aplikacija za **jednog vlasnika po instalaciji/instanci**, ne zajednički chatbot za više ljudi. Vlasnik na svom računaru dobija privatnog AI agenta za razgovore, mejlove, kalendar, dokumente, obaveze, memoriju i automatizacije. Svaki korisnik sam povezuje naloge i unosi **svoje API ključeve (BYOK)**. Interfejs od prve verzije podržava **SRB (Ekavica), SRB (Ijekavica) i ENG**: `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk` i `en-US`. Obje srpske varijante su latinicom; svaki vlasnik bira podrazumijevani jezik i varijantu koji ostaju sačuvani pri svakom narednom pokretanju. Sistem od početka podržava više AI provajdera, a kasnije se može bezbjedno proširiti na pristup istom agentu sa telefona i drugih računara.

Prvi podržani operativni sistem: **Windows 11 x64**. UI: React + TypeScript + Tauri 2; backend: ASP.NET Core / .NET 10 (lokalni sidecar); trajni podaci: **SQLite uz SQLCipher**; dodatna enkripcija tajni u kolonama. Nikakav Microsoft Credential Manager/DPAPI **nije obavezan** za čuvanje ključeva. Microsoft 365 je prva produktivna integracija; ostale se kasnije dodaju kroz konektore.

## Glavna pravila, koja važe za sve faze

1. **Model-agnostic** — nijedan poslovni servis ne poziva direktno OpenAI/Anthropic/Google SDK; koristi apstrakcije. Prvi provajder ne smije postati privilegovan ili ugrađen u srž.
2. **BYOK** — svaki korisnik sam obezbjeđuje svoj API ključ; aplikacija nikad ne koristi zajednički ključ izdavača. Za lokalni Ollama nije potreban API ključ.
3. **Privatno po instanci** — odvojeni direktorijumi, baze, ključevi i integracije; centralna komercijalna infrastruktura ne čita privatne podatke.
4. **Security by default** — zaključan vault na pokretanju; potvrda prije vanjskih promjena; pristup principom najmanjih privilegija; nikad plaintext ključevi u SQLite, logovima, backupu ili frontendu.
5. **Korisničke postavke i i18n** — model, provajderi, integracije, dozvole, memorija, obavještenja, vremenska zona, trajanje podataka i **podrazumijevani jezik (SRB ekavica / SRB ijekavica / ENG)** konfigurišu se kroz UI i trajno pamte; obavezne sigurnosne zaštite se ne mogu isključiti. Obavezno pratiti `I18N_SPEC.md` od Faze 01.
6. **Lokalno prvo** — ne otvarati backend na LAN-u ili internetu; mobile/cloud su posebna kasnija faza. Ne projektovati lokalni bypass koji bi se kasnije pretvorio u javni endpoint.
7. **Bez samovoljnog proširivanja** — Codex implementira **samo aktivnu fazu**; ne radi refaktor nevezanog koda i ne mijenja postojeću logiku bez potrebe.
8. **Kvalitet kao uslov završetka** — build, testovi, migracije, demonstracija rezultata, sigurnosni negativni testovi, ažuriran README i evidencija preostalih rizika.

## Redoslijed izvršavanja

| Red | Fajl | Rezultat | Opseg |
|---|---|---|---|
| 1 | `01_FOUNDATION.md` | Pokretna desktop aplikacija, lokalni API, struktura repozitorijuma | MVP |
| 2 | `02_ENCRYPTED_SQLITE_VAULT.md` | Enkriptovana baza + sigurni korisnički ključevi | MVP / kritično |
| 3 | `03_AI_PROVIDERS_BYOK.md` | OpenAI, Anthropic, Gemini, Ollama, drugi kompatibilni API-ji | MVP |
| 4 | `04_AGENT_CORE_PERMISSIONS.md` | Chat, istorija, alati i odobravanje akcija | MVP |
| 5 | `05_MICROSOFT_365.md` | Outlook mail, kalendar, To Do, OneDrive | MVP |
| 6 | `06_DOCUMENTS_MEMORY.md` | Učitavanje/pretraga dokumenata i kontrolisana memorija | MVP |
| 7 | `07_TASKS_AUTOMATIONS.md` | Obaveze, podsjetnici, zakazani poslovi | MVP |
| 8 | `08_SETTINGS_DESKTOP_UX.md` | Potpuni korisnički UI i podešavanja | MVP |
| 9 | `09_HARDENING_RELEASE.md` | Sigurnosna provjera, testovi, Windows instalacija | MVP izlaz |
| 10 | `10_REMOTE_COMMERCIAL_FUTURE.md` | Plan za udaljeni pristup i prodaju — **ne implementirati u MVP-u** | Kasnije |

`AGENTS.md` sadrži trajna pravila rada za Codex; `DECISIONS.md` dokumentuje odluke i pretpostavke, a **`I18N_SPEC.md` je obavezna specifikacija sve tri lokalizacije (SRB ekavica / SRB ijekavica / ENG) za sve faze**.

## Kako raditi sa Codexom

Kopirati cijeli ovaj direktorijum u `docs/codex-plan/` projekta, **osim** `AGENTS.md` koji treba kopirati ili spojiti u korijenski `AGENTS.md` repozitorijuma. Počevši od Faze 1, Codexu slati po jedan zadatak:

> Pročitaj `AGENTS.md`, `docs/codex-plan/00_START_HERE.md`, `docs/codex-plan/DECISIONS.md`, `docs/codex-plan/I18N_SPEC.md` i `docs/codex-plan/01_FOUNDATION.md`. Implementiraj isključivo Fazu 1. Prije promjena pregledaj postojeći kod i napiši kratku listu potrebnih izmjena. Primijeni izmjene, izvrši build i testove, dokumentuj dokaz za svaku stavku kriterijuma prihvatanja i navedi blokere. Ne prelazi na sljedeću fazu bez moje potvrde.

Za kasnije faze mijenja se samo naziv aktivnog Markdown fajla. Koristiti zaseban branch/PR ili commit za svaku fazu.

## Minimalni kriterijum gotovog proizvoda

Vlasnik instalira aplikaciju, uspostavi master lozinku, doda sopstveni API ključ ili lokalni model, testira konekciju, razgovara s agentom, poveže Microsoft 365, pronađe mejl i dokument, napravi nacrt odgovora, pregleda kalendar, kreira obavezu, odobri osjetljivu akciju, podesi memoriju i podsjetnike, izabere SRB (Ekavica), SRB (Ijekavica) ili ENG kao podrazumijevani jezik koji se pamti kroz restart, zaključa aplikaciju, te obnovi šifrovani backup. Nijedan ključ nije čitljiv iz datoteka na disku, niti se vanjske akcije izvršavaju bez propisane autorizacije.

## Granice v1

**Nije MVP:** javno dostupan server, više istovremenih korisnika jedne instance, mobilni klijent, cloud sinhronizacija, automatsko izvršavanje uz zaključan vault, govor, autonoman browser/desktop control, samovoljno izvršavanje komandi i direktni SQL pristup korisničkim bazama, obračun i naplata pretplata. Sve to dobija posebnu projektantsku i sigurnosnu procjenu.

## Referentna dokumentacija

- OWASP: https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html
- OWASP Argon2id: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
- OWASP Key Management: https://cheatsheetseries.owasp.org/cheatsheets/Key_Management_Cheat_Sheet.html
- Microsoft AesGcm: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm
- SQLCipher: https://github.com/sqlcipher/sqlcipher
- Tauri 2 sidecar: https://v2.tauri.app/develop/sidecar/
- Microsoft Graph: https://learn.microsoft.com/en-us/graph/overview
