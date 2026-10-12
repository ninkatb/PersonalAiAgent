# ADR-003 — Vault, granice prenosa i jednokratno izvršavanje

**Datum:** 11.10.2026.

**Status:** Projektantska odluka; native distribucija i kriptografski format su uslov G-001, bez završenog sigurnosnog dokaza.

## Kontekst

Zahtjevi FR-002–009, FR-014 i NFR-002/004/005/006 traže SQLCipher bazu, dodatnu AES-GCM zaštitu tajni, Argon2id master lozinku, recovery, default deny i tačno odobrenje promjene. Poslovi ne smiju koristiti tajne poslije lock-a. Timeout nije potvrđen neuspjeh vanjskog efekta.

## Odluka

Ključevi pripadaju instanci i postoje dekriptovani samo u otključanom backendu. Nasumični root ima odvojene izvedene ključeve; password i recovery ga omotavaju nezavisno. Kriptografski descriptor je odvojen od UI bootstrap-a. Konkretni KDF parametri, paketi, native build i crash-consistency format moraju se uskladiti sa aktivnom fazom 02 i dokazati prije upotrebe. Ne postoji plaintext SQLite fallback.

Lock opoziva epoch prije novih tajnih poziva, otkazuje rad, zatvara DB i briše kontrolisane buffere/privatno UI stanje. Lease i dozvole se ponovo provjeravaju pred prenos, uz guard koji određuje redoslijed lock-a i početka poziva. Već poslat poziv može imati efekat i poslije lock-a; bez potvrde rezultat je `unknown`.

Odobrenje je jednokratno i vezano za kanonske argumente, verziju alata, nalog, cilj, izvorni version/ETag, primaoce, sadržaj i priloge. Izmjena, opoziv ili lock ga poništavaju. Potrošnja odobrenja i zauzimanje jedinstvenog executionKey-a su transakcioni. Model, vanjski tekst i generisani prijedlog nikad ne odobravaju izvršavanje.

Slanje i brisanje uvijek imaju novo izričito odobrenje. Nema slijepog retry-a mutacije poslije timeout-a ili restarta. Lokalna deduplikacija ne garantuje tačno jedan vanjski efekat; koristi se dokumentovana idempotentnost servisa gdje postoji, a inače provjera/nepoznat ishod.

Cloud politika zasebno vezuje odredište, resurse, vrste podataka i svrhu, uključujući embeddings i rezervni model. Credential i scope ne prenose se drugom provajderu.

## Alternative i posljedice

Samo zaštita kolona ostavlja privatni tekst/indekse izvan pune enkripcije. Samo sistemski prompt ne sprovodi prava. Opšte odobrenje za promjene ne vezuje pregled za konačni efekat. Izabrana zaštita povećava cijenu upravljanja ključevima, stanjima i prekidima; portovi, transakcije i negativni testovi tu cijenu čine provjerljivom.

Recovery rotira aktuelnu generaciju i izdaje novi kod. Stari backup ostaje zaštićen starim pristupnim materijalom; potrošnja koda ne opoziva istorijske kopije. Staged generacije čuvaju kompletan prethodni skup pri prekidu. Destruktivna zamjena traži backup i potvrdu.

## Provjera

TC-002–004/008/009/014/102/104/105/106: pogrešan ključ, tamper/nonce/AAD, cipher identity, recovery ponovna upotreba, crash na commit granicama, replay, izmjena primalaca, opoziv/lock trka i zabranjen prenos. Svi scenariji su planirani; nema sigurnosne potvrde proizvoda.

Detalji i G-001 nalaze se u [arhitekturi §3–4 i §11](../bmad/architecture.md). Ova odluka ne aktivira faze 02–04.
