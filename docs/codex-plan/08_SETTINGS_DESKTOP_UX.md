# Faza 08 — Kompletan UI, korisnička podešavanja i upotrebljivost

## Cilj

Korisnik može bez programera instalirati aplikaciju, podesiti sve integracije i modele, otkriti gdje se čuvaju podaci, odrediti pravila rada agenta i pregledati aktivne dozvole. **Engleski i srpski moraju biti potpuno funkcionalni od Faze 01**, a ne samo ovdje. **Sva funkcionalna podešavanja moraju biti vidljiva i izmjenjiva kroz UI**; sigurnosni minimumi ostaju obavezni.

## Struktura navigacije

```text
Dashboard
Chat / Conversations
Email / Mail Drafts
Calendar
Documents / Index
Tasks / Automations
Memory
Settings
  Profile & Language
  AI Providers & Models
  Connections & Accounts
  Agent Instructions & Permissions
  Privacy, Retention & Sharing
  Vault, Master Passphrase & Recovery
  Notifications & Scheduler
  Backups & Restore
  Network / Local Model Endpoints
  Diagnostics & Audit
  Updates & About
```

## Konkretni ekrani

1. **First-run wizard:** dobrodošlica, informacija local-first/BYOK, definisanje master lozinke, generisanje recovery koda i potvrda da ga korisnik sačuva, izbor **English / Srpski (latinica, ijekavica)** i zone, dodavanje prvog AI modela; opciono povezivanje Microsoft 365. Na kraju prikaz šta se šalje AI provajderu, a šta ostaje lokalno.
2. **Unlock/Lock:** prikaz zaključanog vaulta prije privatnog sadržaja; korisnik podešava auto-lock interval (ne smije biti neograničen bez sigurnosne procjene). Lock now dostupan iz svih ekrana.
3. **AI Providers:** kartice provajdera, secret status (nikad vrijednost), zamjena/uklanjanje ključa, test konekcije, izbor modela po namjeni, dozvole za fallback, konfigurisani limiti potrošnje i linkovi do zvaničnih API konzola.
4. **Connections:** listanje Microsoft naloga, OAuth connect/disconnect, koje dozvole su date, kada je posljednji put sinhronizovano, koji izvori/folderi su obuhvaćeni.
5. **Agent permissions:** matrica alata sa nivoima pristupa, čitljivi zahtjevi odobrenja, istorija izvršenja, jednostavan pregled „šta agent smije”; podrazumijevani deny za nove alate.
6. **Privacy & Memory:** aktivni AI provajder, podaci koji napuštaju uređaj, pamćenje, brisanje/eksport razgovora, izvora i memorije, retention i ograničenja čuvanja.
7. **Documents:** izaberi foldere/fajlove, pregled indeksiranih izvora, reindex, revoke pristupa, objašnjenje citata i sadržaja.
8. **Automation:** rasporedi, vremenske zone, stanje aktivan/pauziran, propušteni poslovi, istorija pokretanja i jasna informacija da posao ne radi kad je aplikacija ugašena/zaključana.
9. **Security/Backup:** master passphrase change, recovery regen uz potvrdu, bezbjedni export/import šifrovanog backup-a, audit pregled, bez opcije prikazivanja sirovih ključeva.
10. **Diagnostics:** provjera verzija, konekcija, grešaka sa automatskom redakcijom. Eksport dijagnostike tek nakon korisničkog pregleda sadržaja.

## Obavezni tokovi za izbor jezika

1. U `Settings > Profile & Language` implementiraj dvije **odvojene** postavke: `Application language (ui.locale)` i `AI response language (agent.responseLanguage)`. UI opcije: SRB (Ekavica) — `sr-Latn-RS-ekavsk`, SRB (Ijekavica) — `sr-Latn-RS-ijekavsk`, ENG — `en-US`; AI jezik: iste tri lokalizacije i `follow-user`.
2. Svaki korisnik bira **podrazumijevani UI jezik**, koji se trajno čuva; ne smije ga ponovo birati nakon restarta, updatea, lock/unlock ili promjene modela. Promjena odmah prevodi UI bez restarta.
3. Locked/unlock stranica prikazuje posljednji jezik prije dekripcije baze čitanjem samo `ui.locale` iz nesenzitivnog bootstrap fajla. Poslije unlock-a kanonska SQLite vrijednost ima prednost, uz sinhronizaciju prema bootstrap fajlu. Implementirati tok i testirati kao u `I18N_SPEC.md`.
4. Svi Settings ekrani, dijalozi, obavještenja, validacione greške, onboarding, approval i accessibility tekstovi moraju postojati u **sve tri** lokalizacije. Koristiti standardni `Intl` i mapu za formatiranje iz `I18N_SPEC.md` za datume, vrijeme, brojeve i pluralizaciju, posebno srpske množine i tekst odgovarajuće varijante.
5. Pokrenuti automatsku provjeru sva tri kataloga i E2E za sve promjene između tri lokalizacije, čuvanje i ponovno učitavanje jezika i srpske varijante. Provjeriti nezavisnost AI jezika i migraciju starog sačuvanog `sr-Latn-RS` u ijekavicu. Ne dozvoliti izdavanje nepotpuno prevedene verzije.

## UX standardi

- Interfejs SRB (Ekavica), SRB (Ijekavica) i ENG (`sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk`, `en-US`), kroz tri zasebna i18n kataloga sa provjerom pariteta; nema hardkodovanih tekstova.
- Navigacija tastaturom, fokus, kontrast, statusi učitavanja, potvrde, greške i stanje offline; WCAG 2.2 AA kao cilj za ključne tokove.
- Prazna stanja objašnjavaju naredni korak; stvarni podaci se nikad ne zamjenjuju generisanim prividnim mejlovima.
- Mobilno-responzivan React layout od početka, **ali bez stvarnog remote pristupa u MVP-u**.
- Confirmacije opasnih radnji ne smiju biti samo generičko „Da/Ne”, nego prikazuju stvarni payload i posljedicu.

## Testovi i kriterijumi prihvatanja

- [ ] Potpuno nov korisnik može završiti prvi setup, dodati svoj ključ, razgovarati i zaključati vault bez uređivanja fajlova.
- [ ] Svaki funkcionalni parametar naveden u Settings ima trajnu pohranu, provjeru validnosti i objašnjenje posljedica.
- [ ] Nijedan ekran ne prikazuje plaintext tajnu, OAuth token ili skriveni developer ključ.
- [ ] Approve ekran jasno pokazuje šta će agent uraditi, sa stvarnim primaocima ili događajem.
- [ ] Keyboard navigation, error handling, loading i prazna stanja su provjereni.
- [ ] Upute za početnike uključuju objašnjenje da AI API naplatu plaćaju svom provajderu.
- [ ] Svi MVP ekrani imaju SRB (Ekavica), SRB (Ijekavica) i ENG lokalizacije, bez neprevedenih tekstova, sa ispravnom varijantom, dijakriticima i pluralizacijom.
- [ ] Podrazumijevani jezik i srpska varijanta iz Settings ostaju sačuvani nakon restartovanja, ažuriranja i lock/unlock; korisnik ih ne bira ponovo.
- [ ] UI locale i agentov jezik odgovora su nezavisna podešavanja; oba se mogu promijeniti i čuvaju se trajno.
