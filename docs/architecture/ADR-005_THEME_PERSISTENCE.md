# ADR-005 — Izbor teme i pamćenje prije otključavanja

**Datum:** 12.10.2026.  
**Status:** Zahtjev vlasnika za dvije teme i trajno pamćenje je potvrđen; način čuvanja je projektantska razrada. Implementacija slijedi tek u izričito aktiviranoj fazi.

## Kontekst

Tokom `bmad:ux-design` vlasnik je zatražio izbor svijetle ili tamne teme koji ostaje sačuvan za buduća prijavljivanja. Zaključani ekran se prikazuje prije pristupa šifrovanim postavkama, pa isključivo kanonsko čuvanje ne bi omogućilo isti izgled pri pokretanju.

## Odluka

`ui.theme` ima isključivo mašinske vrijednosti `light | dark`. Tema je nezavisna od `ui.locale`, `agent.responseLanguage`, modela i dozvola. Svijetla tema je početni UX prijedlog samo za novu instancu bez sačuvanog izbora; OS tema ne prepisuje korisnikov izbor. Automatsko praćenje OS teme nije dio ove razrade.

Minimalni bootstrap dobija dozvoljeno nesenzitivno polje `ui.theme` uz postojeći `ui.locale`. To je izričita dopuna prethodne allowlist-e od jednog polja. Ne dodaju se AI jezik, korisnički profil, raspored, privatni sadržaj ili tajne. Parser i dalje odbija proizvoljna polja i putanje; fajl ostaje na fiksnoj aplikacionoj putanji, odvojen od vault descriptora.

Prije faze 02 bootstrap može biti jedini trajni zapis ove dvije UI postavke. Od faze 02 kanonska vrijednost je u šifrovanoj SQLite bazi. UI potvrđuje čuvanje i primjenjuje novu temu tek nakon uspješne kanonske transakcije, zatim atomarno ažurira bootstrap uz očuvanje locale-a. Neuspješan kanonski upis čuva prethodnu temu; greška bootstrap kopije daje zasebno upozorenje i ponovnu sinhronizaciju. Pri unlock-u kanonski izbor ima prednost.

Na zaključanom ekranu promjena teme je privremena; poslije unlock-a korisnik može izričito sačuvati taj izbor. Bez te odluke kanonska tema se vraća. Privremeni izbor ne postaje trajni zapis kroz neautorizovan upis i ne mijenja sačuvani jezik.

## Kompatibilnost i obnova

Stari bootstrap bez teme dobija svijetli prikaz dok kanonska vrijednost ne postane dostupna. Nepoznata vrijednost teme ne otključava podatke: koristi se svijetli prikaz uz očuvanje originalnog fajla i važećeg locale-a kada je zapis strukturno validan. Nedostajuća kanonska tema može uvesti validan sačuvani bootstrap izbor; postojeća kanonska vrijednost se ne prepisuje.

Restart, update, lock/unlock i restore čuvaju `ui.theme`. Restore regeneriše bootstrap iz provjerenih kanonskih postavki prije prikaza obnovljene zaključane instance. Kompatibilnost formata sa starijim binarnim izdanjem mora biti eksplicitno provjerena; nema tihog gubitka korisnikovog izbora.

## Posljedice i provjera

Ovo je proširenje nesenzitivne bootstrap allowlist-e i postavki, bez proširenja pristupa vaultu. Pri implementaciji su potrebne provjere oba izbora kroz cold start/update/lock/restore, prekida upisa, sukoba bootstrap/SQLite, starih zapisa bez teme i svih šest kombinacija UI jezika/teme. Neuspjeh kopiranja je vidljiv; ne obećava se ista tema na narednom startu dok kopija nije popravljena.

Detaljan UX i planirane provjere: [ux-design.md](../bmad/ux-design.md), `UX-THEME-01–06`. Ova odluka ne zatvara G-001, ne mijenja odluku gate provjere i ne aktivira implementacionu fazu.
