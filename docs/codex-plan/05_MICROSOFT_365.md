# Faza 05 — Microsoft 365: Outlook, kalendar, To Do i OneDrive

## Cilj

Prvi produktivni konektor sa podacima korisnika preko Microsoft Graph API-ja. Jedan vlasnik može imati jedan ili više svojih Microsoft naloga ako provider dozvoli; agent koristi samo odabrane naloge i dozvoljene funkcije. Dizajn takođe omogućava kasnije `GmailConnector`, `GoogleCalendarConnector` i drugačije izvore bez promjene Agent Core-a.

## OAuth i nalog

1. Registracija aplikacije: javni desktop klijent, MSAL, authorization code + PKCE, sistemski browser (ne ugrađivati lozinku korisnika u aplikaciju). Razlikovati `AI provider API key` (korisnik donosi) od `Microsoft OAuth client ID` (javni ID aplikacije izdavača). **Ne zahtijevati od svakog kupca da registruje sopstvenu Microsoft aplikaciju.**
2. `Connect`, `Disconnect`, `Reauthorize`, `ListAccounts`, `SelectAccount`, `TestPermissions`. Delegirana saglasnost i **inkrementalni scope-ovi** samo kada funkcija zatraži pristup. Poslovni tenant može tražiti admin consent — objasniti to UI-jem.
3. MSAL token cache i refresh token podatke čuvati unutar `IntegrationCredentials` kroz `ISecretVault`; nikada koristiti podrazumijevani plaintext cache. Potvrditi integraciju MSAL token cache serialize/deserialize i ponašanje pri vault lock.
4. Pozivi isključivo server-side kroz konektor i korisničku authorized sesiju; bez izlaganja access tokena rendereru.

## Interfejsi konektora

```text
IEmailConnector: Search, GetThread, GetMessage, CreateDraft, SendApprovedDraft
ICalendarConnector: ListEvents, FindFreeSlots, CreateApprovedEvent
ITaskConnector: ListTasks, CreateApprovedTask, UpdateTask
IDocumentSource: SearchFiles, GetFileMetadata, OpenAuthorizedFile
```

Svaki rezultat mora nositi identifikator izvora (`AccountId`, `ProviderItemId`, vrijeme, URL/deep-link ako je dozvoljen) radi ponovnog pronalaska i provjeravanja originala.

## Funkcionalni opseg

### Mejl
- Pretraga po pošiljaocu, periodu, folderu, naslovu i ključnim riječima, paginacija i ograničenje broja poruka; pregled niti i rezime.
- Kreiranje nacrta odgovora; **slanje samo nakon prikaza konačnog sadržaja, primalaca i odobrenja**. Podrazumijevano ne brisati, ne premještati i ne označavati pročitano bez akcije korisnika.
- Razjasniti razliku `message ID`/`conversation ID` i način rada s nacrtima; voditi idempotency kako ponovni pokušaj ne bi poslao dva mejla.

### Kalendar
- Sastanci za dan/sedmicu, slobodni termini prema korisnikovoj vremenskoj zoni i radnom vremenu; time-zone/DST testovi.
- Kreiranje/dopuna događaja, učesnika i podsjetnika nakon eksplicitne potvrde prije slanja poziva. Ne mijenjati već postojeći događaj bez dozvole.

### Microsoft To Do
- Lista zadataka, kreiranje, rokovi, označavanje završenim uz prava korisnika. Lokalna integracija sa `Tasks` iz Faze 7 kroz mapiranje external IDs.

### OneDrive
- Metapodaci/pretraga odabranih foldera i fajlova; korisnik može ograničiti opseg indeksiranja. Preuzimanje i obrada kroz dokument pipeline Faze 6, po potrebi i uz odobrene dozvole.

## Scope i policy

- Dati korisniku eksplicitnu tabelu prava i razloga: `Mail.Read`, dodatna `Mail.ReadWrite` samo gdje je stvarno potrebna za nacrte/izmjene, `Mail.Send` tek za slanje; `Calendars.Read` / `Calendars.ReadWrite`; `Tasks.Read` / `Tasks.ReadWrite`; najminimalniji `Files.*` scope koji realni API zahtijeva. To je plan, **tačan skup prava provjeriti na Graph dokumentaciji pri implementaciji**.
- `User.Read` samo ako funkcija treba identitet; izbjegavati application permissions i čitanje čitavog tenant-a u ličnoj aplikaciji.
- Rate limit / Graph paging / delta token refresh / `Retry-After`; postupati prema stvarnom API odgovoru.
- Nema globalnog mirrorovanja cijele pošte ili kompletnog OneDrive-a u SQLite; čuvati minimum metapodataka i korisnički odabrani dokumentni indeks.

## Testovi

- Mock Graph: paginacija, 429, token refresh, revoke, admin consent denial, disconnect i ponovni connect.
- Approval provjera prije `Mail.Send` i kreiranja događaja sa učesnicima; pokušaj podmetanja drugog primaoca nakon approval-a mora pasti.
- OAuth token čuvanje samo šifrovano u vaultu.
- Bez duplih mejlova i duplih događaja pri retryju; ako status nije siguran, označiti `Unknown` i ponuditi provjeru.
- UI test za odvojene privatne i poslovne naloge gdje provajder to podržava.

## Kriterijumi prihvatanja

- [ ] Korisnik povezuje Outlook bez upisivanja lozinke u Personal AI Agent aplikaciju.
- [ ] Može sažeti mejlove, pretražiti OneDrive i pogledati kalendar; izvori su klikabilni.
- [ ] Agent pravi nacrt i šalje ga **tek nakon eksplicitnog potvrđivanja**.
- [ ] Korisnik može ukinuti/obnoviti konekciju i vidjeti opseg prava.
- [ ] Nema Microsoft tokena u HTML localStorage, URL-u, logovima ili običnom SQLite plaintextu.

Reference: https://learn.microsoft.com/en-us/entra/identity-platform/msal-authentication-flows ; https://learn.microsoft.com/en-us/graph/overview

## Lokalizacija (obavezno)

Sve poruke o povezivanju naloga, OAuth greškama, pristupnim dozvolama, statusima mejlova, kalendara i To Do zadataka moraju imati prevode za SRB (Ekavica), SRB (Ijekavica) i ENG u tri zasebna kataloga. Naslove i tijela stvarnih mejlova ili događaja **ne prevoditi automatski**; prikazati originalni sadržaj. Format datuma i vremena određuju mapa UI locale-a iz `I18N_SPEC.md` i korisnička vremenska zona. Primijeni `I18N_SPEC.md`.
