#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet restore PersonalAI.sln --locked-mode
dotnet build PersonalAI.sln --no-restore -m:4
dotnet test PersonalAI.sln --no-build --no-restore
npm --prefix apps/web run build
npm --prefix apps/web run lint
npm --prefix apps/web run test:catalogs
npm --prefix apps/web test
cargo fmt --manifest-path apps/desktop/src-tauri/Cargo.toml --check
