# Faza 10 — Kasniji udaljeni pristup i komercijalizacija (POSLIJE MVP-a)

> **Ovaj fajl je projektni backlog, ne zadatak za trenutnu implementaciju.** Aktivira se tek nakon Faze 09, kada korisnik potvrdi model hostinga, cijenu i threat model.

## Poslovni cilj

Svaki kupac ima sopstvenog privatnog agenta i odvojene podatke. Isti vlasnik pristupa **svom** agentu sa desktopa, drugih računara i telefona. Prodavac može upravljati licencama i verzijama, ali **nema pristup privatnim API ključevima, OAuth tokenima, razgovorima ili dokumentima** bez zasebno odobrenog support mehanizma.

## Opcije hostinga — odlučiti prije implementacije

1. **Self-hosted personal instance:** backend radi na vlasnikovom računaru ili mini-serveru; pristup sa drugih uređaja samo preko sigurnog tunela/VPN-a ili zero-trust gateway-a, nikada jednostavno otvaranje lokalnog porta. Ako je vlasnikov računar ugašen, agent nije dostupan.
2. **Managed isolated instance:** posebna instanca ili stroga logička/kriptografska izolacija po kupcu; bezbjedna autentifikacija, zasebni encryption domains, backup politika, monitoring bez privatnog sadržaja. Troškove hosting-a i sigurnosne obaveze uračunati u pretplatu.

## Zahtjevi koje treba dokazati prije udaljenog pristupa

- OIDC/OAuth2 autentifikacija korisnika i uređaja, MFA/passkeys, revoke sesija i pregled prijavljenih uređaja; TLS, kratkotrajni tokeni, rotacija refresh tokena i rate limits.
- End-to-end šifrovanje ili korisnički kontrolisan model dekripcije **izbor je otvoren**: posebno razriješiti gdje je DB ključ dok backend obrađuje zadatke i ko može pristupiti plaintextu.
- Ako jedan kupac pristupa sa tri uređaja, svi su vezani za isti owner identity i izolovani od ostalih kupaca. Integracioni test protiv cross-tenant IDOR i BOLA ranjivosti.
- Bez oslanjanja na local API session token za internet autorizaciju — kreira se poseban remote security model.
- Ključeve nije dozvoljeno kopirati na telefon samo radi prikazivanja UI-ja; telefon podrazumijevano koristi siguran remote API vlasnikove instance.
- EU/RS privatnost i regulatorne obaveze, obrada podataka, retention, delete/export, ugovori sa podobrađivačima, transparentnost eksternog AI prenosa — obavezna pravna procjena po ciljnom tržištu.
- Komercijalni control plane: licence, aktivacije, korisnički plan, istek i nadogradnje **bez centralnog skladištenja privatnih sadržaja**.
- BYOK je i dalje pravilo: svaki kupac koristi sopstvene AI ključeve; cijena aplikacije/hostinga odvojena je od AI API potrošnje.
- Softverska nadogradnja sa potpisanim releaseom i sigurna migracija SQLite vaulta ili izričit export/import pri preseljenju instance.

## Preporučene dodatne faze nakon MVP-a

1. `11_REMOTE_AUTH_THREAT_MODEL` — dizajn autentifikacije/hostinga i security review.
2. `12_REMOTE_GATEWAY_AND_DEVICE_MANAGEMENT` — siguran pristup jednom agentu sa više uređaja.
3. `13_COMMERCIAL_CONTROL_PLANE` — licence i pretplate, bez privatnih korisničkih podataka.
4. `14_OPERATIONAL_SECURITY` — incident response, security patching, abuse controls, politika privatnosti i komercijalni pilot.

## Kriterijumi izlaska iz dizajna (prije pisanja remote koda)

- [ ] Korisnik je izabrao self-hosted ili managed-hosted model.
- [ ] Dokumentovano je gdje su master/DB/secret ključevi kada pristup dolazi sa telefona i kada je agent zaključan.
- [ ] Nacrtan je threat model za krađu uređaja, kompromitovan hosting, support pristup, cross-customer izolaciju i remote API.
- [ ] Korisnik je odobrio model troškova i prijave/licenciranja.
- [ ] Bezbjednosne granice imaju nezavisnu reviziju prije javne dostupnosti.

## Višejezičnost u budućem udaljenom pristupu

Mobilni/web klijenti istog vlasnika po defaultu koriste njegov puni `ui.locale`: SRB (Ekavica) — `sr-Latn-RS-ekavsk`, SRB (Ijekavica) — `sr-Latn-RS-ijekavsk` ili ENG — `en-US`. Ne gubiti srpsku varijantu pri sinhronizaciji. Ako se kasnije podrži per-device override, mora biti eksplicitan i odvojen od podrazumijevane korisničke postavke i od `agent.responseLanguage`. Sinhronizovati samo dozvoljene postavke uz autentifikaciju; ne slati bootstrap fajl ni tajne. Nove lokalizacije prate `I18N_SPEC.md`; udaljeni pristup se i dalje ne implementira u MVP-u.
