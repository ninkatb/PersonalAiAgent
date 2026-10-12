# STORY-026 — Životni ciklus i izolacija sidecar procesa

- **Epik:** EPIC-001; roditelj STORY-001.
- **Prioritet:** MUST.
- **Procjena:** 5 bodova, uslovna; Windows prekidi, session lifecycle i utrkivanja povećavaju neizvjesnost.
- **Status:** in_progress; implementirana 12.10.2026. uz automatizovane Windows dokaze. Stvarni sleep/resume i vizuelna native provjera ostaju uslov završetka; Faza 01 je aktivna.
- **Sljedivost:** FR-001 djelimično; NFR-001/003/007/012; TC-001/101/103/107/112; F01-AC-03/05; G-002.

## Korisnička priča

Kao vlasnik želim da zatvaranje, prekid i ponovno pokretanje aplikacije ostave urednu privatnu instancu, da bih izbjegao skrivene procese i upotrebu stare sesije.

## Kriterijumi prihvatanja

- [x] AC-01: Normalni izlaz, pad desktop-a i prekinut startup uklanjaju njegov sidecar i otvoreni kanal; cleanup ne prekida nepovezane procese niti briše korisnički direktorijum.
- [x] AC-02: Restart generiše novi token; stari token i kanal ne prihvataju pozive u novoj sesiji. Neuspješan handshake ili pad sidecar-a daje lokalizovanu nedostupnost, bez prelaska na nepoznat backend.
- [x] AC-03: Per-profile instance lock dozvoljava najviše jedan desktop/backend par; istovremeni pokušaji ne stvaraju dva pisca. Različiti test profili imaju odvojene kanale i bootstrap putanje, bez međusobnog pristupa.
- [ ] AC-04: Sleep/prekid kanala opoziva aktivne transportne operacije; nastavak ostaje u zaključanom stanju i zahtijeva provjerenu sesiju. Ne uvodi se stvarni vault unlock ili tajni posao.
- [x] AC-05: Sintetički tok u test hostu potvrđuje autentifikaciju prije otvaranja toka i prekid na opozivu/sleep-u. Produkcioni paket ne dobija AI streaming ili debug endpoint zbog ovog testa.
- [ ] AC-06: Windows desktop smoke i negativni integration scenariji dokazuju cleanup, restart, dvije instance, izolaciju i sleep; G-002 dobija pojedinačne rezultate, a nezatvoreni dio je eksplicitno naveden kao uslov predaje.

## Implementacija i granice

Moduli: Rust process ownership/lifecycle, startup/session ugovor, per-profile lock, API cancellation i desktop test harness. Koristiti provjeren Windows mehanizam nad potomkom; dokumentovati stvarni način cleanup-a i privatnog pipe-a. Nema generičkog process-kill alata u rendereru.

Nema baze, migracije, pravih tajni ili scheduler-a. Opoziv transporta je priprema za budući vault; testni `Unlocked` ne dokazuje buduće brisanje vault ključa iz memorije. Ne obećavati odbranu od kompromitovanog OS-a.

## Zavisnosti i rubni slučajevi

Zavisi od STORY-025; blokira konačnu STORY-030 predaju. Pad između kreiranja lock-a i handshaka, zaostali lock, prekid toka i istovremeni restart moraju bezbjedno odbiti nedokazanu sesiju. Pri zaostalom lock-u provjeriti stvarnog vlasnika; ne brisati lock/fajlove drugog procesa naslijepo. Neuspješan Windows dokaz blokira pogođeni dio G-002.

## Provjere i završetak

- Unit: lifecycle tranzicije, opoziv i instance ownership sa kontrolisanim vremenom.
- Integration/security: stari token, konkurentni startup, odvojeni profili i cancellation sintetičkog stream-a.
- Desktop/ručno: gašenje i pad oba procesa, sleep/resume i ponovno pokretanje na Windows-u; dokumentovati PID i ishod bez tokena.

Kriterijum završetka je dokaz svih kriterijuma, pregled koda i ažurirana transportna dokumentacija. [Windows izvještaj](../architecture/STORY-026_WINDOWS_VERIFICATION.md) bilježi implementaciju, rezultate i pojedinačne G-002 dokaze. AC-01/02/03/05 imaju automatizovane dokaze; AC-04/06 ostaju otvoreni za stvarni sleep/resume i vizuelnu native provjeru. Sintetičke power poruke nisu fizički sleep. Priča ostaje `in_progress`, bez prelaska na narednu priču ili fazu.
