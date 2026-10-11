# Faza 03 — Multi-provider AI, BYOK i izbor modela

## Cilj

Korisnik dodaje jedan ili više AI provajdera i **svoje** API ključeve. Ne postoji developerski, zajednički ili tajno ugrađeni API ključ. Provider-specific HTTP, SDK, message/streaming i tool-call formati izolovani su u adapterima, dok ostatak aplikacije vidi neutralan ugovor. Lokalni Ollama radi bez ključa.

## Prvo definisati ugovore

```csharp
public interface IAIProvider
{
    string ProviderId { get; }
    Task<ProviderCapabilities> GetCapabilitiesAsync(string modelId, CancellationToken ct);
    Task<ConnectionTestResult> TestConnectionAsync(ProviderConnection connection, CancellationToken ct);
    IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, CancellationToken ct);
}

public interface IProviderRegistry
{
    IReadOnlyList<ProviderDescriptor> GetInstalledProviders();
    IAIProvider Resolve(string providerId);
}
```

Ugovori treba da pokriju tekst, streaming, alatne pozive, token usage, greške, prekid, identifikatore modela i dokumentovane capabilities (`SupportsTools`, `SupportsImages`, `SupportsStructuredOutput`, veličina konteksta ako je pouzdano poznata). Koristiti DTO bez pretpostavki o internim formatima konkretnih SDK-ova.

## Implementacija po redoslijedu

1. Kreiraj `ProviderConnection` i `AIModelPreference` modele: `ProviderId`, `ConnectionId`, `ModelId`, `DisplayName`, `BaseUrl`, `SecretRef`, `Role` (`primary`, `fallback`, `document`, `fast`), `Capabilities`, `IsEnabled`, `CreatedAtUtc`.
2. Dodaj adaptere: `OpenAIProvider` (aktuelni Responses API gdje je primjenjivo), `AnthropicProvider` (Messages API), `GeminiProvider` (Gemini API), `OllamaProvider` (lokalni endpoint) i `OpenAICompatibleProvider` samo kao **posebnu kompatibilnu opciju**, ne kao univerzalnu implementaciju svih provajdera.
3. Zahtijevaj pravilno dostupno polje API ključa po provajderu. Prilikom snimanja direktno upiši vrijednost u `ISecretVault`; poslije toga UI dobija **samo secret reference i masked status**. Nikada ne vraćati vrijednost kroz GET.
4. Omogući test konekcije sa razumljivom porukom (invalid key, revoked, quota, rate limit, mreža, unsupported model), bez prosljeđivanja podataka ključa klijentu ili logovima. Test ne treba trošiti znatan broj tokena.
5. Lista modela: dinamički fetch kada provajder podržava tu funkciju; inače korisnik može ručno unijeti tačan `ModelId`. Ne hardkodirati zastarjele nazive ili cijene. Cache uz rok i ručni refresh.
6. UI: dodavanje/brisanje konekcije, izbor primary/fallback modela, model za specifični zadatak, opcioni zasebni limit tokena/zahtjeva, pauziranje provajdera, historija grešaka bez tajni.
7. Implementiraj rutiranje po namjeni i sposobnostima. Ako model nema tool calling, aplikacija ne smije simulirati alat običnim nekontrolisanim tekstom; jasno prikazuje ograničenje ili traži prikladniji model.
8. **Automatski fallback je podrazumijevano isključen**, jer prelazak drugom provajderu može poslati isti privatni sadržaj trećoj strani. Uključuje se eksplicitno uz izbor dozvoljenih rezervnih provajdera i disclosure.
9. Bez pretpostavke da se kompletan korisnikov chat automatski čuva kod provajdera: aplikacija vodi lokalni kanonski format istorije i prevodi ga za svaki adapter.
10. Loguj metrike potrošnje i model ID kad ih provajder vrati; procjena novčanog troška samo uz provjerljive, konfigurisane cjenovnike, jasno označiti kao procjenu.

## Posebne sigurnosne kontrole

- HTTPS i provjeren TLS za sve udaljene provajdere. `http://` dopustiti samo eksplicitno potvrđenom loopback endpointu za lokalni Ollama ili drugu lokalnu uslugu.
- Za custom `BaseUrl` validirati šemu, port, redirect i DNS rebinding/SSRF: remote endpointi ne smiju bez odobrenja biti privatni/LAN/link-local/metadata servisi; HTTP redirect ne smije odvesti na nedozvoljenu destinaciju.
- Nijedan adapter nema pristup čitavoj bazi ili proizvoljnim tajnama; dobija samo tajnu veze potrebnu za jedan poziv.
- Network retry s exponential backoff i pravilima za rate limit; izbjegavati dvostruko izvršavanje alatnih akcija pri ponavljanju zahtjeva.
- Korisnički API ključevi nikad se ne emituju u request body-u za telemetriju ili logovanje. Nema ključeva u query stringu.

## Testovi

- Mock server test za svaki adapter: chat, streaming, usage, invalid key, 401/403/429/500, timeout, cancellation.
- Adapteri pravilno mapiraju alatne pozive i rezultate u kanonski model bez gubitka korelacionog ID-ja.
- Fallback van korisnikovog allowlista je zabranjen.
- Kada je vault zaključan, nijedan provajder poziv sa tajnom ne može krenuti.
- E2E: dodaj dva provajdera različitim ključevima; promijeni aktivni model; ukloni jedan ključ; drugi nastavlja da radi.

## Kriterijumi prihvatanja

- [ ] Najmanje OpenAI, Anthropic, Gemini i Ollama imaju zasebne funkcionalne adaptere.
- [ ] Ključevi svakog korisnika su u šifrovanom SQLite vaultu, nikad u JS localStorage/configu.
- [ ] Moguće je promijeniti primarni model bez mijenjanja Agent Core-a.
- [ ] Korisnik može vidjeti/toggle capabilities i zna kada zadatak nije podržan.
- [ ] Provjerene mrežne zaštite za custom endpoint.
- [ ] Uputstva za generisanje API ključa vode na zvanične portale provajdera.

## Zvanične reference

- OpenAI: https://developers.openai.com/api/reference/responses/overview
- Anthropic: https://platform.claude.com/docs/en/api/messages/create
- Gemini: https://ai.google.dev/gemini-api/docs/function-calling
- Ollama: https://docs.ollama.com/api/introduction

## Lokalizacija (obavezno)

Prikaz provajdera, statusa povezivanja, opis greške (`invalid key`, `quota`, `timeout`) i objašnjenja o troškovima moraju imati prevode za SRB (Ekavica), SRB (Ijekavica) i ENG u tri zasebna kataloga. API/model identifikatori se **ne prevode**. Kompletna pravila: `I18N_SPEC.md`.
