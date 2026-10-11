# Faza 04 — Agent Core, razgovori, alati, odobrenja i audit

## Cilj

Agent vodi pouzdan razgovor, koristi isključivo registrovane alate sa provjerenim argumentima i ne obavlja vanjske promjene bez odgovarajuće saglasnosti. Glavna petlja ne zna da li radi sa OpenAI, Claude, Gemini ili Ollama modelom.

## Kanonski proces

```text
User request -> Authenticated local API -> Conversation/context builder
  -> IAIProvider (selected model)
  -> response or requested tool call
  -> server-side schema validation + policy check
  -> READ: allowed tool izvršavanje / WRITE: pending approval
  -> tool result -> IAIProvider -> final answer
  -> audit + persist conversation
```

## Predložene tabele

- `Conversations(Id, OwnerId, Title, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc)`.
- `Messages(Id, ConversationId, Role, ContentJson, ProviderId, ModelId, CreatedAtUtc, ToolCallId, Status)`.
- `ToolExecutions(Id, ConversationId, ToolName, ToolCallId, IdempotencyKey, ValidationStatus, ExecutionStatus, StartedAtUtc, FinishedAtUtc, ErrorCode)`.
- `Approvals(Id, OwnerId, ConversationId, ToolCallId, ToolName, RedactedInputJson, Status, ExpiresAtUtc, RequestedAtUtc, DecidedAtUtc)`.
- `AgentPolicies(Id, OwnerId, PolicyName, ToolCategory, ApprovalMode, IsEnabled)`.
- `AuditEvents` (iz Faze 2): akcije i metadata, nikad tajni sadržaj po defaultu.

## Implementacioni zadaci

1. Implementiraj razgovore: kreiranje, uređivanje naslova, arhiva, istorija, pretraga i eksport u kontrolisanom formatu. Streaming sa `CancellationToken` i prekidom bez dupliranja poruka.
2. Kanonski format poruka i tool-callova: sistem/developer/user/assistant/tool, ID, status, provider provenance. Ne slati cijelu istoriju bez limita; konfigurisani prozor, osnovni context budget i kompresija bez izmišljanja činjenica. **Agentov jezik odgovora** odvojen je od `ui.locale`: primijeni `agent.responseLanguage` (`follow-user`, `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk`, `en-US`) kao neobaveznu preferenciju nižeg prioriteta od eksplicitnog jezika ili varijante u korisnikovom upitu. Modelu eksplicitno navedi srpsku latinicu i izabranu ekavicu/ijekavicu. Ne mijenjati postojeći sadržaj dokumenata/mejlova zbog promjene UI jezika.
3. `IToolRegistry` registruje samo eksplicitno dozvoljene alate sa JSON Schema i policy kategorijom (`Read`, `Write`, `SensitiveWrite`, `Destructive`). **Nema** proizvoljnog `ExecuteShell`, browser-control, otvaranja lokalnih datoteka bez dozvole, direktnog SQL izvršavanja ili generičkog HTTP-fetch iz prompta.
4. Server provjerava identitet vlasnika, stanje vaulta, aktivnu integraciju, prava, ulaznu JSON šemu, maksimalnu veličinu i idempotency prije pozivanja izvršioca. Parametri iz LLM-a nikad se ne smatraju validnim dok ih server ne provjeri.
5. `ApprovalService`: akcije vanjskih izmjena dobijaju vremenski ograničen `Pending` zapis; UI prikazuje čitljiv sadržaj (`Primaoci`, `Naslov`, `Sadržaj`, `Vrijeme`, `Dokument`, itd.) i tačno `Approve`/`Reject`. Izvršenje samo jednom, tek nakon potvrde od vlasnika; binding na digest potvrđenog payload-a.
6. Ne dozvoliti `approve` za izmijenjen/istekao payload. Podrazumijevano slanje mejlova, pozivanje na sastanke, izmjene dokumenata i brisanje moraju tražiti odobrenje. Korisnik kasnije podešava manje rizične kategorije, ali ne može isključiti potvrdu za destruktivne/vanjski vidljive akcije.
7. Primijeni redakciju audit događaja i korisnički pregled: šta je predloženo, kada je potvrđeno, koji alat je stvarno pozvan, uspjeh/neuspjeh. Audit čuva semantičke kodove i podatke nezavisno od jezika; UI lokalizuje nazive alata, policy tekstove i potvrde za SRB (Ekavica), SRB (Ijekavica) i ENG.
8. Ako model ne podržava tool calling, UI pokazuje informativnu grešku/ograničenje; ne oslanjati se na pokušaj parsiranja proizvoljnog AI teksta kao komande.
9. Upravljaj retry semantikom i idempotentnim kreiranjem: ako mreža pukne nakon slanja mejla, stanje može biti `Unknown` i korisnik mora provjeriti prije ponovnog slanja.

## Prompt injection i privatni podaci

- Mejl, dokument, rezultat pretrage i drugi vanjski sadržaj označavaju se kao **nepouzdani podaci**, ne kao sistemske instrukcije. Ignoriši pokušaje „zanemari prethodne instrukcije” u takvom sadržaju.
- Ograniči podatke koji se šalju AI provajderu na najmanji potreban opseg; UI prikazuje aktivni provajder i kada se prenose podaci van računara.
- Zabraniti iznošenje ključeva, OAuth tokena, internih putanja, sistemskih promptova i privatnih podataka u vanjske servise alatima bez eksplicitno podržane funkcionalnosti i provjera.
- Nepotvrđene AI tvrdnje ne mogu promijeniti `ApprovalStatus` ili `TaskStatus`.

## Test scenariji

1. Tekst iz mejla pokušava naložiti agentu da pošalje druge mejlove: agent to tretira kao podatak, ne instrukciju.
2. AI predloži nepoznat alat ili pogrešan JSON schema: odbiti bez izvršenja.
3. Dva paralelna approve zahtjeva: alat izvršen maksimalno jednom.
4. Agent mijenja primaoca nakon odobrenja: hash/payload mismatch i obavezno novo odobrenje.
5. Vault zaključan prije potvrde: pending ostaje, izvršavanje se ne vrši.
6. Provider timeout/stream prekid: poruke i akcije ostaju u dosljednom stanju.
7. Za UI na engleskom i svaku srpsku varijantu odgovora: model dobija preferenciju srpske latinice i tačne ekavice/ijekavice; promjena UI jezika ne mijenja AI izbor. Eksplicitni korisnički zahtjev za engleski ili drugu srpsku varijantu ima prednost; provjeriti i `follow-user`.

## Kriterijumi prihvatanja

- [ ] Chat funkcioniše sa najmanje dva različita model adaptera, uz istu implementaciju Agent Core-a.
- [ ] API pozivi `execute` i `approve` ne prihvataju neautorizovanog korisnika, neutvrđen alat ili isteklo odobrenje.
- [ ] Audit precizno razlikuje prijedlog od izvršene akcije.
- [ ] Testovi pokrivaju prompt injection, race conditions i duplo izvršenje.
- [ ] Nema ugrađenog načina da AI samostalno mijenja politike dozvola.
