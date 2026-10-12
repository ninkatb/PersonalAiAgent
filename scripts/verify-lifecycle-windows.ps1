param([string]$Executable = (Join-Path (Split-Path $PSScriptRoot -Parent) 'apps/desktop/src-tauri/target/debug/personal-ai-desktop.exe'))
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Only processes created by this run are stopped; never kill by process name.
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PersonalAILifecycleMessages {
    [DllImport("user32.dll")] public static extern IntPtr SendMessageW(IntPtr window, uint message, UIntPtr value, IntPtr data);
}
'@

function Wait-Condition([scriptblock]$Condition, [string]$Description) {
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    while (-not (& $Condition)) {
        if ([DateTime]::UtcNow -gt $deadline) { throw "TIMEOUT: $Description" }
        Start-Sleep -Milliseconds 100
    }
}

function Start-Pair {
    $desktop = Start-Process -FilePath $Executable -WindowStyle Hidden -PassThru
    try {
        Wait-Condition { $desktop.Refresh(); $desktop.MainWindowHandle -ne [IntPtr]::Zero } 'desktop window'
        $script:sidecar = $null
        Wait-Condition {
            $child = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $($desktop.Id) AND Name = 'PersonalAI.Api.exe'")
            if ($child.Count -eq 1) { $script:sidecar = Get-Process -Id $child[0].ProcessId; return $true }
            return $false
        } 'owned sidecar'
        # Window construction precedes Tauri's message loop. A synchronous
        # WM_NULL roundtrip prevents closing a window still being initialized.
        [PersonalAILifecycleMessages]::SendMessageW($desktop.MainWindowHandle, 0, [UIntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        Wait-Condition { $desktop.Refresh(); -not [string]::IsNullOrWhiteSpace($desktop.MainWindowTitle) } 'localized document title'
        Write-Host "PAIR: desktop PID $($desktop.Id), sidecar PID $($script:sidecar.Id)"
        return @{ Desktop = $desktop; Sidecar = $script:sidecar }
    } catch {
        if (-not $desktop.HasExited) { $desktop.Kill(); $desktop.WaitForExit(10000) | Out-Null }
        throw
    }
}

function Stop-OwnedPair($Pair) {
    if (-not $Pair.Desktop.HasExited) { $Pair.Desktop.Kill(); $Pair.Desktop.WaitForExit(10000) | Out-Null }
    Wait-Condition { $Pair.Sidecar.HasExited } 'sidecar cleanup'
}

if (-not (Test-Path -LiteralPath $Executable -PathType Leaf)) { throw 'Build the desktop first.' }
$pair = $null
try {
    $pair = Start-Pair
    $duplicate = Start-Process -FilePath $Executable -WindowStyle Hidden -PassThru
    if (-not $duplicate.WaitForExit(15000)) { $duplicate.Kill(); throw 'Duplicate instance did not fail closed.' }
    if ($duplicate.ExitCode -eq 0) { throw 'Duplicate instance reported success.' }
    if ($pair.Sidecar.HasExited) { throw 'Duplicate affected the original sidecar.' }
    Write-Host 'PASS: duplicate instance rejected; original pair unaffected'
    $pair.Desktop.Refresh()
    if (-not $pair.Desktop.CloseMainWindow()) { throw 'Normal close could not be delivered.' }
    if (-not $pair.Desktop.WaitForExit(15000)) { throw 'Normal close timed out.' }
    Wait-Condition { $pair.Sidecar.HasExited } 'normal sidecar exit'
    Write-Host 'PASS: normal desktop close removes sidecar'

    $pair = Start-Pair
    Stop-OwnedPair $pair
    Write-Host 'PASS: abrupt desktop termination removes sidecar; restart succeeded'

    $pair = Start-Pair
    $window = $pair.Desktop.MainWindowHandle
    [PersonalAILifecycleMessages]::SendMessageW($window, 0x218, [UIntPtr]4, [IntPtr]::Zero) | Out-Null
    Wait-Condition { $pair.Sidecar.HasExited } 'synthetic suspend revocation'
    [PersonalAILifecycleMessages]::SendMessageW($window, 0x218, [UIntPtr]18, [IntPtr]::Zero) | Out-Null
    Start-Sleep -Milliseconds 500
    if ($pair.Desktop.HasExited) { throw 'Resume closed the desktop unexpectedly.' }
    $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $($pair.Desktop.Id) AND Name = 'PersonalAI.Api.exe'")
    if ($children.Count -ne 0) { throw 'Resume automatically restarted a session.' }
    Write-Host 'PASS: native synthetic suspend/resume revokes session and does not restart backend'
    Stop-OwnedPair $pair

    $pair = Start-Pair
    $pair.Sidecar.Kill()
    $pair.Sidecar.WaitForExit(10000) | Out-Null
    if ($pair.Desktop.HasExited) { throw 'Sidecar death closed desktop unexpectedly.' }
    Write-Host 'PASS: sidecar crash keeps desktop available for localized failure'
    Stop-OwnedPair $pair
    Write-Host 'LIMIT: actual Windows sleep/resume and visual localized failure require separate evidence'
} finally {
    if ($null -ne $pair) { Stop-OwnedPair $pair }
}
