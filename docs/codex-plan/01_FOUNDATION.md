# Faza 01 — Foundation: projekat, desktop shell i lokalni backend

## Cilj

Reproducibilno pokretanje desktop aplikacije sa React UI-jem i lokalnim .NET API-jem, **bez** stvarnih tajni i vanjskih integracija. Implementacija mora biti pripremljena za izmjenu transporta pri kasnijem udaljenom pristupu, ali danas nema javno izloženih servisa.

## Predložena struktura repozitorijuma

```text
personal-ai/
  AGENTS.md
  docs/codex-plan/                      # ovi Markdown fajlovi
  docs/architecture/                   # ADR, dijagrami, threat model
  apps/desktop/                         # Tauri 2
  apps/web/                             # React + TypeScript (Vite)
  src/PersonalAI.Api/                   # ASP.NET Core lokalni API sidecar
  src/PersonalAI.Application/           # use cases, orchestration
  src/PersonalAI.Domain/                # modeli i interfejsi
  src/PersonalAI.Infrastructure/        # SQLite, mreža, kriptografija, konektori
  tests/PersonalAI.UnitTests/
  tests/PersonalAI.IntegrationTests/
  tests/PersonalAI.SecurityTests/
  scripts/                              # provjera/build/pakovanje
```

## Codex zadaci — raditi ovim redom

1. Provjeri stanje repozitorijuma; ako je prazan kreiraj .NET solution, React/Vite aplikaciju, Tauri 2 desktop projekat i minimalne test projekte. Podesi centralno upravljanje NuGet paketima i zaključavanje npm zavisnosti.
2. U domenskom sloju definiši **interfejse**, bez implementiranja narednih faza: `IAIProvider`, `IProviderRegistry`, `ISecretVault`, `IConversationStore`, `IToolRegistry`, `IApprovalService`, `IIntegrationConnector`, `IMemoryService`, `IJobScheduler`, `ISettingsStore`.
3. ASP.NET Core API radi na `127.0.0.1` i nasumičnom slobodnom portu. Endpoint `/health` vraća samo bezopasan status. Nema `0.0.0.0`, LAN bindinga ili javnog reverse proxyja.
4. Desktop Tauri pokreće **.NET sidecar** i sigurno prekida potomak pri gašenju. Dokumentovati bundle način i provjeriti da se sidecar stvarno uključuje u Windows instalaciju.
5. Zaštićena komunikacija: generiši kriptografski nasumični sesijski token pri svakom pokretanju, predaj ga samo pouzdanim kanalom sidecar-u/rendereru, zahtijevaj ga na svim funkcionalnim endpointima; definisati konkretan način prenošenja bez izlaganja argumentima procesa, URL-u, logovima ili dugotrajnoj pohrani. Provjeri `Origin` i CORS, zaštitu od lokalnog CSRF-a.
6. Minimalni UI: početna stranica, meni (`Chat`, `Email`, `Kalendar`, `Dokumenti`, `Zadaci`, `Memorija`, `Postavke`) i stanje `Locked/Unlocked`. Svi moduli su prazne navigabilne stranice, bez lažnog stvarnog sadržaja. **Odmah implementiraj tri lokalizacije prema `I18N_SPEC.md`**: SRB (Ekavica) — `sr-Latn-RS-ekavsk`, SRB (Ijekavica) — `sr-Latn-RS-ijekavsk`, ENG — `en-US`, uz `i18next`/`react-i18next`, tri zasebna kataloga za sve tekstove trenutnih ekrana i selektor jezika koji osvježava UI bez restarta. U ovoj fazi ne čekati Fazu 08 za višejezičnost.
7. Uspostavi DTO validaciju, `ProblemDetails` bez stack trace-a prema UI-ju, strukturirane redigovane logove, globalni exception handler i DI konvencije. API greške imaju neutralne stabilne `errorCode` identifikatore, a korisnički tekst prevodi frontend.
8. Dodaj minimalni per-instance `bootstrap.json` za **isključivo nesenzitivnu postavku `ui.locale`** koju UI čita i prije otključavanja vaulta. Otkrij OS jezik pri prvom startu prema `I18N_SPEC.md` (srpski bez varijante predlaže ekavicu, `en-US` fallback), dozvoli korisnikov izbor i pamti puni kod pri ponovnom startu. Stari sačuvani `sr-Latn-RS` normalizuj u `sr-Latn-RS-ijekavsk`. Predvidi migraciju u SQLite u Fazi 02 (kanonska postavka poslije otključavanja). Ne koristiti `localStorage` za kredencijale ili druge privatne podatke.
9. Napiši ADR: granice desktop/UI/API, lokalni transport i buduća apstrakcija za remote API. Postavi CI za unit testove, TypeScript provjeru i paritet prevodilačkih ključeva i placeholdera sva tri kataloga, uz pluralne varijante prema pravilima svakog jezika.

## Sigurnosne napomene

- `health` bez privatnih detalja; nijedna privilegovana operacija prije uspješne autorizacije.
- Restriktivne Tauri capabilities; bez univerzalnog shell execution, file system access i remote navigation prava.
- CSP bez `unsafe-eval`; eksterni HTML i tekst bezbjedno renderovati, bez izvršavanja JavaScript-a iz poruka.
- Jedan pokrenuti backend i jedna instanca baze po profilu; instance lock, bez utrkivanja različitih procesa.

## Testovi

- API odbija poziv bez/ispravnog tokena; ne veže se za mrežni interfejs osim loopbacka.
- Browser sa nedozvoljenog origin-a ne može izvršavati privilegovane zahtjeve.
- Sidecar se startuje/gasi uz desktop; restart pravi novi sesijski token.
- UI u sve tri lokalizacije pokriva sve postojeće komponente, uključujući locked ekran. Izbor SRB (Ekavica), SRB (Ijekavica) ili ENG se pamti kroz restart; svaka promjena, uključujući srpsku varijantu, odmah osvježava UI. Testirati OS prijedlog i stari sačuvani kod prema `I18N_SPEC.md`. Nepodržan/pokvaren locale se bezbjedno vraća na `en-US`.
- `dotnet build`, `dotnet test`, `npm run build`, `npm run typecheck` prolaze; lokalni installer/sandbox smoke test.

## Kriterijumi prihvatanja

- [ ] `README` opisuje pokretanje i zahtjeve (Node, Rust/Tauri, .NET) i instalaciju na Windowsu.
- [ ] Frontend može pozvati zaštićeni lokalni `health` endpoint kroz predviđeni transport.
- [ ] Lokalni API nije dostupan sa drugog računara i odbija neautorizovan pristup.
- [ ] Dependency injection / projekti nemaju direktne zavisnosti core-a od konkretnih provajdera.
- [ ] Testovi i lint/typecheck su pokrenuti sa navedenim rezultatima.
- [ ] SRB (Ekavica), SRB (Ijekavica) i ENG prevodi su stvarni, ne placeholderi, u tri zasebna kataloga; selektor, formatiranje, fallback, trajni default i kompatibilnost starog sačuvanog koda rade već u Fazi 01.

**Ne implementirati:** SQL tajne, OAuth, AI pozive i pozadinske automatske zadatke.
