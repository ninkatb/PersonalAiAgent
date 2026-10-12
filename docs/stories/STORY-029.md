# STORY-029 — Svijetla i tamna tema sa trajnim izborom

- **Epik:** EPIC-001; roditelj STORY-022, samo početni dio FR-025 za temu.
- **Prioritet:** MUST.
- **Procjena:** 3 boda, uslovna; koristi gotove tokene i zajednički bootstrap servis.
- **Status:** not_started; preostali obim Faze 01, bez dodijeljenog sprinta. Razvoj nije odobren.
- **Sljedivost:** FR-025 djelimično, potvrđena dopuna 12.10.2026; NFR-007/008/011; TC-025/107/108/111; UX-THEME-01/02/05/06 u početnom dijelu; ADR-005.

## Korisnička priča

Kao vlasnik želim svijetlu ili tamnu temu koja ostaje ista pri sljedećem pokretanju, da bih zadržao čitljiv i prijatan izgled bez ponovnog podešavanja.

## Kriterijumi prihvatanja

- [ ] AC-01: Sve postojeće stranice koriste provjerene light/dark tokene i lokalizovanu radio grupu za izbor. Obje teme imaju čitljiv fokus, prazne/error prikaze i kontrast u stvarnom webview-u.
- [ ] AC-02: ui.theme prihvata isključivo light ili dark. Nova instanca bez izbora dobija light kao projektantski prijedlog; OS tema ne prepisuje izbor. Save preko zajedničkog bootstrap servisa prethodi potvrdi i primjeni trajnog izbora.
- [ ] AC-03: Oba izbora opstaju kroz cold start i kontrolisani update bez vaulta; zaključani shell čita isti bootstrap prije bilo kakve baze. Upis teme ne gubi locale, a upis locale-a ne gubi temu.
- [ ] AC-04: Greška upisa čuva prethodnu sačuvanu temu i daje lokalizovan neuspjeh. Stari zapis bez teme i nevažeća tema imaju ponašanje iz ADR-005, uz očuvanje izvornog fajla i validnog jezika.
- [ ] AC-05: Svih šest UI locale/tema kombinacija čuva nezavisne izbore i navigaciju. Tema ne mijenja ugovorni AI jezik ili dozvole; provjera sa četiri AI vrijednosti je sintetička, bez tvrdnje o trajnom AI izboru.
- [ ] AC-06: Dokumentovan je kasniji privremeni izbor na zaključanom vaultu i izričito čuvanje nakon unlock-a, sa kanonskim SQLite prioritetom. Pravi unlock, SQLite konflikt i restore nisu implementirani ovim tokom.

## Implementacija i granice

Moduli: frontend theme provider, pregledani `docs/bmad/ux-tokens.json` kao izvor paleta, selector i shared bootstrap komanda iz STORY-028. [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md) ostaje mjerodavan. Nema baze, nove migracije ili generičkog settings skladišta u rendereru.

## Zavisnosti i rubni slučajevi

Zavisi od STORY-027/028 i njihove gate provjere; blokira STORY-030. Testirati prekid upisa, restart neposredno poslije save-a, nedostajuću/nepoznatu temu i paralelnu promjenu jezika. Vizuelni audit provjerava stvarni CSS/fokus; raniji 44 kontrastna para iz UX dokumenta nisu automatski dokaz webview-a.

## Provjere i završetak

Unit/contract: dozvoljene vrijednosti i nezavisnost. Integration: shared writer i greška upisa. Desktop/ručno: šest kombinacija, cold start/update, fokus i kontrast. UX-THEME-03/04 i stvarni lock/unlock dio UX-THEME-02 ostaju buduća vault provjera. Svi kriterijumi, pregled i dokumentacija potrebni su za završetak; testovi sada nisu izvršeni.
