# Faza 07 — Obaveze, podsjetnici i kontrolisane automatizacije

## Cilj

Jednostavno upravljanje ličnim obavezama, izvršavanje zakazanih poslova u dozvoljenom periodu i jasan audit. U MVP-u zadaci rade samo dok je lokalni backend pokrenut i vault **otključan**. To je namjerno sigurnosno ograničenje: bez čuvanja dekripcionog ključa mimo master lozinke ne može se bezbjedno obećati rad sa tajnim API-jevima u potpunosti zaključanom stanju.

## Model podataka

- `Tasks(Id, OwnerId, Title, Notes, DueAtUtc, TimeZoneId, Priority, Status, Source, ExternalId, CreatedAtUtc, UpdatedAtUtc)`.
- `JobDefinitions(Id, OwnerId, Name, ToolOrWorkflowId, ScheduleType, ScheduleJson, TimeZoneId, IsEnabled, RequiresSecrets, MisfirePolicy, LastRunAtUtc, NextRunAtUtc)`.
- `JobRuns(Id, JobId, RunKey, StartedAtUtc, CompletedAtUtc, Status, AttemptCount, ErrorCode, ResultSummary)`.
- `Notifications(Id, OwnerId, Title, BodySummary, ReadAtUtc, SourceJobId)` — bez nepotrebnih povjerljivih detalja na zaključanom ekranu.

## Koraci za Codex

1. Kreiraj klasični Tasks ekran sa filterima (`Svi`, `Danas`, `Predstojeći`, `Završeni`), prioritetima, rokom, podsjetnikom, uređivanjem i označavanjem završenim.
2. `ITaskService`: CRUD uz autorizaciju, validaciju rokova i audit. Integracija sa Microsoft To Do kroz adapter: definisati local/external source i spriječiti duple zapise.
3. Planer poslova: trajno čuva definicije u šifrovanom SQLite; lokalni `BackgroundService` je dovoljan za MVP. Ako se izabere drugi scheduler, obrazložiti zašto i provjeriti SQLCipher kompatibilnost.
4. Podržati jednokratno u lokalnom vremenu i ponavljanja dnevno, određenim danima sedmice i mjesečno; za složeniji RRULE prikaz samo ako je provjeren i testiran. Korisnik bira IANA vremensku zonu; testirati Windows mapiranje, DST forward/backward i promjenu zone.
5. `MisfirePolicy`: `Skip`, `RunOnceAfterUnlock`, `AskUser` — korisnik bira za svaki posao. Ne izvršavati „propuštene” radnje opasnog tipa masovno nakon starta.
6. Svako pokretanje ima jedinstveni `RunKey`; idempotency i zabranu dvostrukog izvršavanja. Ako jedan job pokrene slanje mejla, korisničko odobrenje iz Faze 4 važi i dalje.
7. Obavještenja: lokalni UI/toast, dnevni pregled, status izvršavanja, razlog preskakanja. Ne prikazivati osjetljivu punu sadržinu na OS lock screen-u.
8. Dodati kontrolu `Pause all`, `Disable`, `Run now` (sa odobrenjem), `Job history`, `Delete`; maksimalan broj automatskih pokušaja i ograničenje potrošnje AI tokena.

## Posebna sigurnosna pravila

- Zaključavanje vaulta obustavlja poslove koji traže tajne ili pristup šifrovanoj DB; čekanje se dosljedno evidentira nakon narednog unlocka. Bez automatske dekripcije master lozinke u datoteci.
- Ako korisnik zatvori aplikaciju ili isključi računar, **nema garancije izvršavanja u tom trenutku**. Ovo jasno označiti u UI-ju.
- Eventualni režim unattended poslova zahtijeva posebno korisnički odobreno rješenje (npr. namenski host i čuvar ključeva) i **nije MVP**.
- Nema automatizacija koje samostalno mijenjaju dozvole, eksportuju credentiale ili šalju masovnu poštu.

## Testovi

- Kreiranje i izvršenje jednokratnog podsjetnika i dnevnog posla.
- Restart procesa, propušteno izvršenje, promjena DST-a, promjena vremenske zone, više paralelnih job tickova.
- Vault lock prije, u toku i poslije job-a; ne curi ključ i ne nastaje duplo izvršenje.
- Slanje mejla iz automatizacije ostaje `PendingApproval` bez obzira na tekst AI prijedloga.

## Kriterijumi prihvatanja

- [ ] Korisnik sam kreira/uređuje/otkazuje zadatke i rasporede.
- [ ] Lokalno zakazani podsjetnici se izvršavaju dok je aplikacija aktivna i otključana.
- [ ] Propuštena izvršenja imaju dokumentovanu i konfigurisanu politiku.
- [ ] Integracija Microsoft To Do ne pravi duplikate.
- [ ] Audit pokazuje šta je zaista izvršeno, a šta je samo predloženo ili preskočeno.

## Lokalizacija (obavezno)

Nazivi statusa, pravila ponavljanja, poruke o zakazivanju, podsjetnici i aplikaciona obavještenja imaju sve tri lokalizacije: SRB (Ekavica), SRB (Ijekavica) i ENG, u zasebnim katalozima. Spremiti vremenske oznake u UTC-u i eksplicitnu vremensku zonu, a format prikaza određuje mapa za `ui.locale` iz `I18N_SPEC.md`. Korisnički napisane naslove zadataka ne prevoditi pri promjeni jezika ili srpske varijante. Primijeni `I18N_SPEC.md`.
