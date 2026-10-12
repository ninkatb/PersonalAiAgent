# ADR-002 — Modularni backend i privatni desktop transport

**Datum:** 11.10.2026.

**Status:** Projektantska odluka; dokaz na Windows-u slijedi u izričito aktiviranoj fazi 01.

## Kontekst

[PRD](../bmad/prd.md) zahtijeva jedan privatni vlasnički proces, backend na `127.0.0.1` sa nasumičnim portom, token i Origin/CORS politiku, bez privilegovanog pristupa iz renderera. .NET 10, Tauri 2 i React/TypeScript strict su preuzeta ograničenja. BMAD nivo 3 ne propisuje mikroservise.

## Odluka

Backend je modularni monolit sa portovima u aplikacionom sloju i adapterima u infrastrukturi. Modeli, konektori, storage, vault, autentifikacija i alati imaju odvojene ugovore. Rust shell pokreće ugrađeni sidecar i predstavlja ograničeni IPC most prema poznatim REST operacijama. Nema generičkog proxy-ja, proizvoljnog URL-a ili shell komande iz UI-ja.

Rust generiše token po procesu i prenosi ga privatnim naslijeđenim kanalom. Token nije u rendereru, argv, env, bootstrap-u, URL-u ili logovima. Backend binduje `127.0.0.1:0` i prijavljuje dodijeljeni port. Svi stvarni API zahtjevi, uključujući health/unlock/stream, provjeravaju token, tačan session-bound Origin i Host. CORS preflight je samo ograničen transportni odgovor, bez podataka. Direktan HTTP poziv nema izuzetak.

OAuth login koristi zaseban privremeni callback sa state/PKCE zaštitom, bez opštih API funkcija. To ne mijenja zaštitu glavnog API-ja. Capability/CSP politika dozvoljava samo ugrađeni UI; udaljeni tekst/stranice ne dobijaju privilegovani prozor.

## Alternative i posljedice

Mikroservisi uvode nepotrebne procese, mrežne granice i distribuirane transakcije za jednu instalaciju. Direktan renderer fetch pojednostavljuje transport, ali izlaže session credential webview-u. Rust most ima dodatnu cijenu DTO i streaming ugovora; prihvata se radi manje privilegovane površine.

Odluka ne tvrdi da je renderer izolovan od svakog napada ili da token štiti kompromitovan OS. Backend i dalje provjerava prava, argumente i unlock. Neuspješan dokaz transporta blokira pogođeni dio; nema javnog listenera ili slabijeg auth fallback-a.

## Provjera i ponovna procjena

Faza 01 treba da dokaže bind, startup handshake, cleanup, sleep, ograničeni IPC i stream. TC-101/103/112 pokrivaju stale/missing token, Origin/Host, preflight, dva procesa/instance i zabranjene reference. Testovi još nisu izvršeni.

Detaljni ugovori i obrazloženje su u [arhitekturi §2–3 i §5](../bmad/architecture.md). Udaljeni pristup zahtijeva novu odluku u posebnoj fazi.
