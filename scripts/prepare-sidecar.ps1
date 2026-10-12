param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskProfile = $Configuration.ToLowerInvariant()
$taskOutput = Join-Path $taskRoot "apps/desktop/src-tauri/target/$taskProfile/sidecar"
dotnet publish (Join-Path $taskRoot 'src/PersonalAI.Api/PersonalAI.Api.csproj') --configuration $Configuration --no-self-contained --output $taskOutput
if ($LASTEXITCODE -ne 0) { throw 'SIDECAR_BUILD_FAILED' }
