# STORY-028 — Trajni UI jezik i bezbjedan bootstrap

- **Epik:** EPIC-001; roditelj STORY-002.
- **Prioritet:** MUST.
- **Procjena:** 5 bodova, uslovna; atomarni upis, kompatibilnost i odbijanje proizvoljnih putanja traže fault injection.
- **Status:** not_started; preostali obim Faze 01, bez dodijeljenog sprinta. Faza 01 je aktivirana 12.10.2026; rad na priči još nije počeo.
- **Sljedivost:** FR-024 djelimično; NFR-003/006/007/008/012; TC-024/103/106/107/108/112; F01-AC-05/06; UX-I18N-02; ADR-005 bootstrap dopuna.

## Korisnička priča

Kao vlasnik želim da aplikacija prije budućeg unlock-a ponovo prikaže moj sačuvani jezik, da bih izbjegao ponovni izbor i gubitak srpske varijante.

## Kriterijumi prihvatanja

- [ ] AC-01: Jedan privilegovani bootstrap servis čita/piše fiksni per-instance bootstrap.json izvan repozitorijuma. Allowlist sadrži samo ui.locale i ui.theme; renderer ne zadaje putanju ili proizvoljno JSON polje. Tajne, agent.responseLanguage i privatni sadržaj se odbijaju.
- [ ] AC-02: Nova instanca predlaže locale prema OS-u: eksplicitna podržana varijanta se čuva, srpski bez varijante daje ekavicu, engleski i nepodržani jezici en-US. Sačuvani validan izbor uvijek ima prednost; bosanski/hrvatski se ne pretvaraju u srpski.
- [ ] AC-03: Izbor bilo kog punog koda ostaje poslije zatvaranja/cold start-a i prikazuje odgovarajući zaključani shell prije pristupa bazi. Promjena ui.locale čuva postojeći ui.theme; sačuvana vrijednost ne zavisi od novog OS prijedloga.
- [ ] AC-04: Stari sačuvani sr-Latn-RS se idempotentno normalizuje u sr-Latn-RS-ijekavsk. Isti kod iz OS-a na novoj instalaciji predlaže ekavicu. Novi upisi koriste pune kodove, a nepodržani povratak na stari format ne konvertuje ekavicu tiho.
- [ ] AC-05: Nevažeći locale i oštećen JSON daju bezbjedan en-US prikaz i lokalizovanu grešku bez rušenja/startup privilegije; izvorni fajl se čuva. Nepoznata tema u strukturno validnom fajlu daje light prikaz uz očuvanje validnog locale-a i izvornog zapisa.
- [ ] AC-06: Upis je atomaran i serijalizovan; neuspjeh ili prekid zadržava prethodni validan izbor i ne prikazuje čuvanje kao uspješno. Putanje, reparse izlazak iz profila, proizvoljna polja i konkurentni upisi jezika/teme imaju negativne testove.
- [ ] AC-07: Dokumentovan je budući bootstrap → SQLite ugovor: uvoz samo bez kanonske vrijednosti, kanonski prioritet, idempotentna legacy normalizacija i sinhronizacija poslije uspješne transakcije. Implementacija baze/restore-a je van ove priče.

## Implementacija i granice

Moduli: privilegovani bootstrap store, ograničene Rust operacije, locale validator/normalizer, selector i testovi. Odgovornost pisca i putanja se konkretizuju pri implementaciji prema [ADR-001](../architecture/ADR-001_LOCALIZATION.md) i [ADR-005](../architecture/ADR-005_THEME_PERSISTENCE.md); ne uvoditi dva nezavisna pisca. U Fazi 01 izbor se čuva u bootstrap-u jer vault još nije implementiran.

Jedina postojeća konfiguraciona transformacija je stari UI locale, bez brisanja podataka. Nema SQLite šeme, agent jezika ili backup/restore implementacije. Test povratka koristi kopiju sintetičkog zapisa, nikada vlasnikov stvarni profil.

## Zavisnosti i rubni slučajevi

Zavisi od STORY-024, aktiviranja Faze 01 i nove gate provjere dopunjene bootstrap šeme prije implementacije. Blokira STORY-029/030. Provjeriti nepostojeći fajl, stari fajl bez teme, dozvole direktorijuma, prekid zamjene i međusobno očuvanje polja. Potvrđena allowlist dopuna ne zatvara G-001 ili istorijski gate.

## Provjere i završetak

Unit: OS naspram saved normalizacije i validacija. Integration/security: atomarni upis/fault injection, traversal/reparse i konkurencija. Desktop: tri locale cold start putanje, promjena OS-a i očuvanje teme. SQLite, restore i pravi lock/unlock testovi ostaju u Fazi 02. Završetak traži sve AC dokaze, pregled, README i arhitektonski zapis ugovora; testovi sada nisu izvršeni.
