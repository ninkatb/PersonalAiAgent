# Personal AI Agent

Privatni AI agent za jednog vlasnika po instalaciji, sa sopstvenim API ključevima (BYOK), razgovorima, mejlovima, kalendarom, dokumentima, obavezama, memorijom i automatizacijama.

Repozitorijum: [ninkatb/PersonalAiAgent](https://github.com/ninkatb/PersonalAiAgent).

## Trenutno stanje

Repozitorijum sadrži plan razvoja i pravila rada. Implementacija aplikacije još nije započeta; trenutno nema koda aplikacije, build komandi niti testova.

Planirani stack je React + TypeScript + Tauri 2, lokalni ASP.NET Core / .NET 10 backend i SQLite uz SQLCipher. Interfejs će podržavati SRB (Ekavica), SRB (Ijekavica) i ENG, uz trajno pamćenje korisnikovog izbora.

## Dokumentacija

- [Pravila rada za Codex](AGENTS.md)
- [Početak i redoslijed razvoja](docs/codex-plan/00_START_HERE.md)
- [Odluke i pretpostavke](docs/codex-plan/DECISIONS.md)
- [Specifikacija lokalizacije](docs/codex-plan/I18N_SPEC.md)

Razvoj svake faze započinje na izričit zahtjev vlasnika. Git povezivanje ne započinje Fazu 01.

## Lokalni rad

```powershell
git clone https://github.com/ninkatb/PersonalAiAgent.git
cd PersonalAiAgent
git status
```

Glavna grana je `main`, a GitHub remote je `origin`.

Stvarni API ključevi, tokeni, lozinke, privatni dokumenti, baze, backup-i i lokalna konfiguracija ne smiju se dodavati u Git. `.gitignore` isključuje uobičajene lokalne datoteke i rezultate builda; prije svakog commita treba pregledati sadržaj pripremljenih izmjena.
