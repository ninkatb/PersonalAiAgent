# STORY-027 — Jezičko formatiranje i potpuna provjera kataloga

- **Epik:** EPIC-001; roditelj STORY-002.
- **Prioritet:** MUST.
- **Procjena:** 5 bodova, uslovna; CLDR, regionalni tekst i jezički pregled zahtijevaju više scenarija.
- **Status:** not_started; preostali obim Faze 01, bez dodijeljenog sprinta. Razvoj nije odobren.
- **Sljedivost:** FR-024 djelimično; NFR-007/008/011/012; TC-024/107/108/111/112; F01-AC-05/06; UX-I18N-01.

## Korisnička priča

Kao vlasnik želim ispravan tekst, datume i brojeve u izabranoj srpskoj varijanti ili na engleskom, da bih razumio prikaz i zadržao isti tok rada pri promjeni jezika.

## Kriterijumi prihvatanja

- [ ] AC-01: Katalozi trenutnih ekrana imaju paritet semantičkih ključeva i imenovanih placeholdera; pluralni sufiksi odgovaraju CLDR pravilima svakog jezika. Negativne fixture varijante nedostajućeg ključa, placeholdera i množine ruše provjeru.
- [ ] AC-02: Obje srpske varijante mapiraju formatiranje na sr-Latn-RS, a en-US ostaje en-US. Puni kod kataloga i postavke se ne skraćuje. Datum, broj i relativno vrijeme koriste provjerene Intl/CLDR forme; UTC zapis ostaje isti pri promjeni prikaza i testne zone.
- [ ] AC-03: Jezički pregled potvrđuje zasebne stvarne srpske prevode, latinicu/dijakritike i ijekavicu Republike Srpske sa oblicima nisu, nisam i sutra. Relativno vrijeme i nazivi mjeseci/dana koji ne odgovaraju varijanti koriste jezički provjerene kataloške oblike.
- [ ] AC-04: Promjene između sva tri izbora, uključujući srpske varijante, čuvaju navigaciju i stanje kontrola. Runtime fallback je en-US, dok development/CI nedostajući prevod tretira kao grešku; sirovi ključ nije uspješan rezultat.
- [ ] AC-05: Escape tekstova i interpolacije sprečava izvršavanje HTML/JavaScript sadržaja. Accessibility natpisi, fokus navigacije i sva prazna/error stanja prolaze kroz kataloge i provjeravaju se tastaturom u sve tri lokalizacije.
- [ ] AC-06: Odvojeni ugovor za agent.responseLanguage prihvata tri puna koda i follow-user; promjena UI locale-a ga ne mijenja u 12 sintetičkih kombinacija. Ovo nije dokaz trajnog AI izbora ili stvarnog odgovora modela.

## Implementacija i granice

Moduli: `apps/web/src/locales`, i18n konfiguracija, formatters, frontend/unit/desktop testovi i provjera hardkodovanog teksta. STORY-024 već uvodi tri kataloga; ova priča dodaje kompletnu regresionu matricu i regionalne rubne slučajeve. Izvor pravila je [I18N_SPEC](../codex-plan/I18N_SPEC.md); UX-copy se koristi samo kao pregledan izvor odgovarajućih tekstova.

Nema baze, API migracije, AI poziva ili trajnog AI jezika u bootstrap-u. Zone i datumi u testu su sintetički; čuvanje poslovne vremenske zone nije dio ove priče.

## Zavisnosti i rubni slučajevi

Zavisi od STORY-024; blokira STORY-029/030. Razlike engleskih i srpskih pluralnih kategorija su očekivane, ne zahtijevati identične sufikse. Unsupported Intl varijanta ne smije promijeniti katalog. Testirati 1/2/5/11/21 i decimalne vrijednosti, named placeholder i kontrolisan zlonamjeran tekst. Neprihvaćen jezički pregled blokira predaju lokalizacije.

## Provjere i završetak

Unit: mapa formatiranja, CLDR i 12 UI/AI ugovornih kombinacija. Frontend/desktop: sve trenutne stranice, promjene varijanti, fokus i escape. CI provjera se uvodi čim postoji skripta; STORY-030 objedinjuje njen Windows pipeline dokaz. Svi AC dokazi, pregled i ažuriran opis lokalizacije potrebni su za završetak; testovi sada nisu izvršeni.
