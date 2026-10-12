param([switch]$Desktop)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-Check {
    param([string]$Command, [string[]]$Arguments)
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Command failed with exit code $LASTEXITCODE" }
}

Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    Invoke-Check dotnet @('restore', 'PersonalAI.sln', '--locked-mode')
    Invoke-Check dotnet @('build', 'PersonalAI.sln', '--no-restore')
    Invoke-Check dotnet @('test', 'PersonalAI.sln', '--no-build', '--no-restore')
    Invoke-Check npm.cmd @('--prefix', 'apps/web', 'run', 'build')
    Invoke-Check npm.cmd @('--prefix', 'apps/web', 'run', 'lint')
    Invoke-Check npm.cmd @('--prefix', 'apps/web', 'run', 'test:catalogs')
    Invoke-Check npm.cmd @('--prefix', 'apps/web', 'run', 'test:coverage')
    Invoke-Check cargo @('fmt', '--manifest-path', 'apps/desktop/src-tauri/Cargo.toml', '--check')
    if ($Desktop) {
        Invoke-Check npm.cmd @('--prefix', 'apps/desktop', 'run', 'build', '--', '--debug', '--no-bundle', '--', '--locked')
        Invoke-Check cargo @('test', '--locked', '--manifest-path', 'apps/desktop/src-tauri/Cargo.toml')
        Invoke-Check cargo @('clippy', '--locked', '--manifest-path', 'apps/desktop/src-tauri/Cargo.toml', '--all-targets', '--', '-D', 'warnings')
    }
} finally {
    Pop-Location
}
