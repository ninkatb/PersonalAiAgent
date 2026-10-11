# Faza 06 — Dokumenti, pretraga i dugoročna memorija

## Cilj

Korisnik sam kontroliše koje lokalne dokumente ili povezane cloud datoteke agent smije pročitati i indeksirati. Memorija je lokalno trajna, pregledna, izmjenjiva i brisiva; nijedan AI provajder nije obavezan skladišni sistem memorije.

## Obrada dokumenata

1. Izvori: drag/drop, file-picker za lokalne PDF/DOCX/TXT datoteke, te autorizovano OneDrive otvaranje iz Faze 5. Početni limit npr. 50 MB/fajl uz postavku korisnika i zaštitni gornji maksimum.
2. `DocumentService`: hash sadržaja, čuvanje naziva, MIME, veličine, izvornog URI/file ID-ja, vremena indeksiranja, pravila dostupnosti, statusa i dozvole. **Ne čuvati automatski kopiju kompletnog cloud fajla** osim kada korisnik odobri offline indeksiranje.
3. Ekstrakcija teksta pouzdanim bibliotekama; zaštita od zip bomb, neispravnih dokumenata, malicioznih fajlova i path traversala. Ne izvršavati makroe/skripte. OCR nije obavezan za MVP; nedostupnost teksta jasno prijaviti.
4. Chunkovanje lokalno uz izvornu stranicu/sekciju i stabilni `DocumentChunk.Id`; osnovni retrieval: SQLite FTS5 unutar **SQLCipher** baze. Vektorski embedding/semantički indeks je opciona odvojena komponenta, nikad uslov za rad dokumentnog MVP-a.
5. AI odgovori iz dokumenata sadrže citat/pokazivač na datoteku, stranicu i odlomak kada su dostupni. Ako nema dokaza, agent treba da kaže da podatak nije pronađen.
6. Kontrole: `IndexNow`, `Reindex`, `Pause`, `RemoveFromIndex`, `DeleteLocalCopy`, `OpenSource`, odabir foldera, data retention i prikaz „šta je indeksirano”.
7. Brisanje izvedenih podataka mora obuhvatiti FTS, chunkove, embeddinge i vezane cacheve. Brisanje lokalnog indeksa ne smije slučajno obrisati originalni dokument na OneDrive-u.

## Memorija agenta

- `MemoryEntries(Id, OwnerId, Category, Content, Source, CreatedAtUtc, UpdatedAtUtc, ExpiresAtUtc, IsPinned, Status)` i `MemoryLinks` prema razgovorima/dokumentima.
- Kategorije: preferencije, činjenice koje je korisnik dao, projekti i stalne instrukcije. **Ne upisivati automatski osjetljive informacije** iz tuđih mejlova ili dokumenata u globalnu memoriju.
- MVP: izričita naredba „zapamti” i ekran koji daje prijedlog memorije za ručno odobrenje; kasnija automatika tek uz posebno podešavanje/transparentnost.
- `List`, `Search`, `Edit`, `Pin`, `Forget`, `Export`; svaka izmjena beleži samo neosjetljiv audit događaj.
- „Zaboravi” uklanja relevantan zapis i izvedene indekse (uz pojašnjenje da originalni mejl, dokument i zakonski backup mogu ostati na svojim izvornim lokacijama dok ih korisnik posebno ne ukloni).
- Memory context builder vraća samo najmanji relevantan skup uz jasnu granicu privatnosti, ne šalje sve memorije svakom provajderu.

## Bezbednosni zahtjevi

- Svi tekstovi, nazivi fajlova, HTML i citati iz dokumenata su nepouzdani, ne smiju postati instrukcije modelu ili direktni HTML u rendereru.
- Privremene lokalne datoteke moraju biti obrisane; ako se koristi disk cache, mora biti šifrovan. Zajednički temp folder bez kontrole pristupa ne sadrži čitav privatni dokument.
- Dozvole na folderima, simbolički linkovi i reparse points: ne skenirati van odobrenog korijena; zaštita od symlink escape.
- Indeks i memorija čuvaju se u SQLCipher bazi; prije udaljenog AI odgovora prikazati aktivnog provajdera i kontrolu dijeljenja sadržaja.

## Testovi i prihvatanje

- [ ] PDF/DOCX/TXT se indeksiraju, pretražuju i pravilno citiraju bez gubljenja source ID-ja.
- [ ] Nevažeći/ogromni fajlovi odbijeni; nema izvršavanja skripti ili čitanja izvan autorizovanog foldera.
- [ ] Indeks se čisti pri `Remove`; cache/backup politika je dokumentovana.
- [ ] Korisnik može zapamtiti, izmijeniti, izvesti i zaboraviti informaciju bez vanjskog API provajdera.
- [ ] Prompt injection u dokumentu ne može izvršiti alat.
- [ ] Zaključan vault ne dozvoljava pristup dokumentnom indeksu ili memoriji.

## Lokalizacija (obavezno)

Kontrole dokumenata, oznake indeksiranja, izvori, prazna stanja, greške i objašnjenja memorije imaju sve tri lokalizacije: SRB (Ekavica), SRB (Ijekavica) i ENG, u zasebnim katalozima. Korisničke dokumente i sačuvane memorije ne mijenjati i ne prevoditi automatski pri promjeni UI jezika ili srpske varijante; ponašanje agentovih odgovora reguliše odvojeni `agent.responseLanguage`. Primijeni `I18N_SPEC.md`.
