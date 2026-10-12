# STORY-025 — Zaštićeni lokalni health kroz Rust most

- **Epik:** EPIC-001; roditelj STORY-001.
- **Prioritet:** MUST.
- **Procjena:** 5 bodova, uslovna; privatni handshake i tri transportne granice zahtijevaju negativne scenarije.
- **Status:** not_started; kandidat sprinta 1. Faza 01 je aktivirana 12.10.2026; rad na priči još nije počeo.
- **Sljedivost:** FR-001 djelimično; NFR-001/003/007/012; TC-001/101/103/107/112; F01-AC-02/03/05; G-002.

## Korisnička priča

Kao vlasnik želim da desktop provjeri stanje svog lokalnog backend-a kroz zaštićenu operaciju, da bih znao da je instanca dostupna i da drugi klijent nema pristup.

## Kriterijumi prihvatanja

- [ ] AC-01: ASP.NET binduje isključivo `127.0.0.1:0`; desktop dobija stvarni dodijeljeni port privatnim startup handshake-om. Greška handshaka zaustavlja sesiju bez rezervnog javnog listenera.
- [ ] AC-02: Rust generiše kriptografski nasumičan token za svaku sesiju i predaje ga sidecar-u privatnim naslijeđenim kanalom prema ADR-002. Token nije u rendereru, argv/env, URL-u, bootstrap-u, logu ili trajnom fajlu.
- [ ] AC-03: Svaka stvarna ruta, uključujući `/health`, zahtijeva važeći token, tačan session-bound Origin i Host. Nedostajući/pogrešan token i Origin/Host se odbijaju; ograničeni CORS preflight ne otkriva podatke i ne odobrava nedozvoljen origin.
- [ ] AC-04: Renderer koristi samo poznatu, tipizovanu Rust IPC operaciju za health; ne može zadati URL, metod, header, putanju ili komandu. Rezultat sadrži samo bezopasan status i UI ga prikazuje na sve tri lokalizacije.
- [ ] AC-05: DTO validacija i globalni handler vraćaju stabilan errorCode i bezbjedan ProblemDetails bez stack trace-a; strukturisani logovi rediguju sintetičke tajne. Neočekivana greška ne vraća sadržaj zahtjeva UI-ju.
- [ ] AC-06: Pozitivan desktop → Rust → API scenario prolazi; direktni neautorizovani poziv, lokalni browser sa zabranjenim origin-om i pristup sa drugog računara ne mogu dobiti zaštićeni odgovor. Normalno zatvaranje desktop-a uklanja njegov sidecar.

## Implementacija i granice

Moduli: Rust transport/sidecar launcher, PersonalAI.Api session middleware/health/ProblemDetails, autentifikacioni ugovor, frontend health prikaz i testovi. [ADR-002](../architecture/ADR-002_LOCAL_TRANSPORT.md) je mjerodavan za token; nije dozvoljen direktan renderer fetch sa credential-om. Origin/Host se određuju iz provjerene startup konfiguracije i testiraju u development i instaliranom okruženju, bez wildcard-a.

Nema baze ili migracije. Nema unlock, AI ili poslovnih ruta. Test-validacioni endpoint postoji samo u test hostu ako je potreban za DTO negativne scenarije; ne objavljivati generičku debug rutu u paketu.

## Zavisnosti i rubni slučajevi

Zavisi od STORY-024 i aktiviranja Faze 01; blokira STORY-026. Nepotpuni kanal, lažni port, nevažeći Host, nedozvoljen preflight i prekid startup-a završavaju zatvorenom sesijom. Token iz prethodne sesije testira STORY-026. Dokaz Windows privatnog kanala je dio G-002; neuspjeh blokira transport. Odgovornost: razvoj.

## Provjere i završetak

- Unit: token/Origin/Host odluke, DTO kodovi i redakcija kontrolisanih sintetičkih markera.
- Integration/security: HTTP negativna matrica, preflight i bind; provjera da port nije LAN listener; Windows mrežni scenario sa drugim uređajem ili izolovanom VM.
- Desktop: stvarni IPC health, lokalizovane greške i normalno gašenje procesa.

Svaki AC ima rezultat i dokaz; sigurnosno kritične putanje imaju unit i integration provjere. Ažurirati README i dokaz transporta u docs/architecture. Svi navedeni testovi sada su planirani.
