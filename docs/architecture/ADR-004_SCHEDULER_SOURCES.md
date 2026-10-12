# ADR-004 — Status izvora, dnevni termini i naknadni pregled

**Datum:** 11.10.2026.

**Status:** Projektantska razrada potvrđenog dnevnog profila; implementacija i UX provjera slijede.

## Kontekst

Vlasnik je potvrdio objedinjeni dnevni pregled nepročitanih mejlova starijih od tri dana i nezavršenih zadataka koji kasne ili dospijevaju u naredna 24 sata. FR-022/023 i NFR-009 traže jedan naknadni pregled, provjeren status, deduplikaciju i DST pravila. Backend radi samo uz otključan vault.

## Odluka

Podobnost se određuje u UTC-u: pošta `receivedAtUtc < nowUtc - 72h`, zadaci sa potvrđenim nezavršenim statusom i `dueAtUtc <= nowUtc + 24h`, uz razlikovanje prošlog/tekućeg roka. Bez roka nema izmišljenog datuma. Date-only lokalni rok ima prikazanu konvenciju kraja dana, na granici početka sljedećeg dana u korisničkoj zoni.

Vrijeme/zonu bira korisnik; nema automatskog proizvoljnog termina. Dnevni termin je jedinstven za raspored i lokalni dan. DST preskok koristi prvi važeći trenutak poslije nepostojećeg vremena; ponovljeno vrijeme koristi raniji UTC trenutak. Promjena postavki ne izvršava ponovo već obrađen dnevni termin.

Poslije unlock/start-a svi propušteni termini objedine se u jedan catchUp sa novim statusima i watermark-om. Dnevni i naknadni posao su serijalizovani za isti raspored. Ista izvorna stavka ulazi najviše jednom u digest, ali smije u naredni dnevni digest ako je i dalje podobna.

Izvorni status/version i vrijeme osvježavanja su dio ugovora konektora. Nedostupna provjera znači nepotpun pregled; stale kopija se prikazuje odvojeno i nije potvrđen novi podsjetnik. AI važnost ne skriva ostalu podobnu poštu niti potvrđuje završenost zadatka.

Transakcioni digest/outbox i stabilan notification ID ograničavaju duplikate. Privatno obavještenje se ne prikazuje poslije lock-a. CatchUp ne uključuje vanjske promjene; nema ključevskog materijala ili privatnog rasporeda u bootstrap-u.

## Alternative i posljedice

Replay svakog propuštenog dana proizvodi zastarjele i duple preglede. Oslanjanje na cache kao potvrđen izvor daje pogrešne podsjetnike. Izabrani postupak zahtijeva osvježavanje, jedinstvene termine i vidljivo stanje nepotpunosti; korisnik može zakasniti dok je računar ugašen/zaključan, što lokalni sistem ne skriva.

## Provjera

TC-021–023/109 provjeravaju tačno 72h i ±24h granice, date-only, DST preskok/ponavljanje, promjenu zone i sata, više propuštenih dana, lock/restart tokom posla, nedostupan izvor i retry outbox-a. Testovi nisu izvršeni. Detalji su u [arhitekturi §3.4–3.6](../bmad/architecture.md); aktivni fajl faze 07 treba učitati tek na zahtjev vlasnika.
