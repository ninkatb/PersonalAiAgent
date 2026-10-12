# ADR-001 — Tri lokalizacije i očuvanje srpske varijante

**Datum:** 11.10.2026.  
**Status:** Prihvaćena projektantska odluka; implementacija tek slijedi.

## Kontekst

Vlasnik je proširio prethodni zahtjev za ENG i srpsku ijekavicu na tri izbora: SRB (Ekavica), SRB (Ijekavica) i ENG. Prethodna specifikacija v1.1 koristila je `sr-Latn-RS` za ijekavicu. Jedan zajednički srpski kod više ne može jednoznačno predstavljati korisnikov izbor.

## Odluka

SRB (Ijekavica) koristi srpsku ijekavicu u upotrebi u Republici Srpskoj, prema potvrđenom govornom području vlasnika. Katalog, dokumentacija i uputstvo modelu koriste „nisu”, „nisam” i „sutra”, uz ijekavske oblike „vrijeme”, „sljedeći” i „izvještaj”. Ovo preciziranje ne uvodi novi kod lokalizacije niti mijenja regionalno formatiranje ili migracije. Konkretni jezički zahtjevi i provjere definisani su u [I18N_SPEC.md](../codex-plan/I18N_SPEC.md).

Koristiti `sr-Latn-RS-ekavsk`, `sr-Latn-RS-ijekavsk` i `en-US` kao pune kodove u katalozima, API ugovorima, `ui.locale` i `agent.responseLanguage`. Varijante su registrovane u [IANA registru](https://www.iana.org/assignments/lang-subtags-templates). Srpski katalozi su latinicom, sa dijakriticima i jezički provjerenim tekstovima; nema automatske konverzije ekavice u ijekavicu ili obratno.

`ui.locale` određuje katalog UI-ja. `agent.responseLanguage` je nezavisna postavka sa iste tri lokalizacije i dodatnim izborom `follow-user`; srpska preferencija modelu eksplicitno navodi pismo i varijantu. Eksplicitna korisnička instrukcija ima prednost nad podrazumijevanim jezikom AI odgovora.

Obje srpske varijante za regionalno formatiranje i pluralna pravila koriste `sr-Latn-RS`, engleski `en-US`. Puni kod varijante ostaje u postavkama i izboru kataloga. Tekst koji runtime generiše, poput relativnog vremena, mora odgovarati izabranoj varijanti; ako ne odgovara, koristiti provjerene kataloške oblike uz standardno numeričko formatiranje i pluralna pravila.

Prije otključavanja jezik UI-ja čita se iz nesenzitivne bootstrap kopije `ui.locale`. Od Faze 02 kanonska postavka je u šifrovanoj SQLite bazi; uspješan upis prethodi atomarnoj sinhronizaciji bootstrap kopije. AI jezik i privatni podaci ne ulaze u taj fajl. UI bootstrap je odvojen od kriptografskih vault metapodataka. [ADR-005](ADR-005_THEME_PERSISTENCE.md) od 12.10.2026. izričito dodaje samo nesenzitivni `ui.theme` u dozvoljena polja radi pamćenja izgleda zaključanog ekrana; jezička pravila ostaju ista.

## Kompatibilnost

Stari **sačuvani** `sr-Latn-RS` migrira se u `sr-Latn-RS-ijekavsk`, zasebno za UI i AI postavke, bez promjene prethodnog jezičkog izbora. Migracija je transakciona i idempotentna; kanonska SQLite vrijednost ima prednost nad bootstrap kopijom. Ista pravila važe za restore. Prelaz iz Faze 01 u Fazu 02 ne prepisuje postojeću kanonsku postavku.

Srpski OS bez varijante na **novoj instalaciji bez sačuvane postavke** predlaže ekavicu. Ovo je samo onboarding prijedlog i ne primjenjuje se na migraciju starih postavki. Nepodržani OS jezici i nevažeći bootstrap kod koriste `en-US` prema [I18N_SPEC.md](../codex-plan/I18N_SPEC.md).

Povratak na stariju verziju koja poznaje samo jedan srpski kod ne može očuvati ekavski izbor. Ne konvertovati ga tiho u ijekavicu; sačuvati backup i prijaviti nekompatibilnost prema specifikaciji.

## Posljedice i provjera

Sva tri kataloga imaju potpunu pokrivenost, iste semantičke ključeve i imenovane placeholdere, uz pluralne oblike prema pravilima jezika. Testovi moraju pokriti svaku lokalizaciju, promjene između srpskih varijanti, restart i locked ekran, nezavisnost AI jezika, formatiranje, migraciju i backup/restore.

Rizici za implementaciju su svođenje oba srpska kataloga na isti jezik u i18n biblioteci, tiho mijenjanje ijekavice pri migraciji i tekst druge varijante iz runtime formatiranja. Detaljni zahtjevi i kriterijumi prihvatanja ostaju u [I18N_SPEC.md](../codex-plan/I18N_SPEC.md); ova odluka ne predstavlja dokaz implementacije ili prolaska testova.
