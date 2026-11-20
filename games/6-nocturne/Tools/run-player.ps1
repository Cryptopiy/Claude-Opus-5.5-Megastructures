<#
.SYNOPSIS
  Runs the built Windows player (Builds/Nocturne/Nocturne.exe) with QA flags, waits for it, and reports.
  Windowed 1920x1080 by default; captures render through RenderTextures at any size (Part 11.2).
  Long GPU jobs (-Long, or any of -soak, -perf-log, -autotest, -autotest-until, -wallbot, -sequence, the heavy capture
  sets and every 4K render) run through Tools/gpu-guard.ps1 (Part 2.5): one heavy GPU job at a time machine-wide, GPU
  temperature and free video memory checked first (6500 MiB for any 4K render: a 4K capture set, -capture-4k,
  -res / -perf-rt / -seq-res at 3840 or more, -Width 3840). Never start two players of this project at once.
  $heavySets / $fourKSets mirror ShotPlan.HeavySets / ShotPlan.FourKSets (an EditMode test keeps them in sync).
.EXAMPLE
  powershell -NoProfile -File Tools\run-player.ps1 -Name autotest -PlayerArgs "-autotest"
  powershell -NoProfile -File Tools\run-player.ps1 -Name keyshots -PlayerArgs "-capture,QA\captures\run01,-capture-set,keyshots"
  (PowerShell -File joins an array into one comma string; this script splits it again. Pass one value per
  option for lists such as -capture-only.)
#>
param(
  [Parameter(Mandatory = $true)][string]$Name,
  [string[]]$PlayerArgs = @(),
  [int]$Width = 1920,
  [int]$Height = 1080,
  [int]$TimeoutMin = 60,
  [int]$StallMin = 6,          # 0 disables the silent-log watchdog
  [switch]$Long,               # force the gpu-guard route
  [int]$MinFreeVramMiB = 5000,
  [string]$Exe = "Builds\Nocturne\Nocturne.exe"   # relative to the project root (lookdev slots: Builds\lookdev_<Slot>\Nocturne.exe)
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$exe = if ([System.IO.Path]::IsPathRooted($Exe)) { $Exe } else { Join-Path $root $Exe }
if (-not (Test-Path $exe)) { Write-Output "player not found: $exe"; exit 98 }
$logDir = Join-Path $root "Logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force $logDir | Out-Null }
$log = Join-Path $logDir "player-$Name.log"
if (Test-Path $log) { Remove-Item $log -Force }

# never two GPU-rendering processes of this project at once (Part 2.5 item 2). A machine-wide named mutex taken before the
# process check closes the race between the check and the start (two calls 10 s apart both passed the check, G1 2026-11-14
# 05:38); it is held until this script exits (a waiting gpu-guard turn counts as the player's turn).
$playerMutex = New-Object System.Threading.Mutex($false, "Global\NocturnePlayer_6Megastructures")
$haveTurn = $false
try { $haveTurn = $playerMutex.WaitOne(0) } catch [System.Threading.AbandonedMutexException] { $haveTurn = $true }
if (-not $haveTurn) { Write-Output "another run-player call holds the Nocturne player turn; refusing to start a second player"; exit 97 }
$running = @(Get-Process -Name "Nocturne" -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) { Write-Output ("a Nocturne player is already running (pid {0}); refusing to start a second one" -f $running[0].Id); exit 97 }

# a list passed through 'powershell -File' arrives as one comma-joined string: split it
if ($PlayerArgs.Count -eq 1 -and $PlayerArgs[0] -match ',') { $PlayerArgs = $PlayerArgs[0].Split(',') }
# resolve relative paths in the argument list against the project root (the player's cwd differs)
$resolved = @()
for ($i = 0; $i -lt $PlayerArgs.Count; $i++) {
  $a = $PlayerArgs[$i]
  if ($i -gt 0 -and @("-capture", "-perf-log", "-audio-dump", "-render-audio", "-report-dir", "-soak-log") -contains $PlayerArgs[$i - 1] -and -not [System.IO.Path]::IsPathRooted($a)) {
    $a = Join-Path $root $a
  }
  if ($a -match '\s') { $resolved += "`"$a`"" } else { $resolved += $a }
}
$argList = @("-screen-fullscreen", "0", "-screen-width", "$Width", "-screen-height", "$Height", "-logFile", "`"$log`"", "-report-dir", "`"$(Join-Path $root 'QA\reports')`"") + $resolved
$joined = $argList -join " "

# long GPU jobs (Part 2.5): soak, perf, autotests, wall bot, frame sequences, and the heavy capture sets of the harness
# (keep in sync with ShotPlan.HeavySets / ShotPlan.FourKSets; QaTests.RunPlayer_RoutingListsMatchShotPlan checks it)
$heavySets = @("keyshots", "stills", "parity", "all", "everything", "sequences", "boundaries", "transitions", "statics", "pans", "edges", "encode", "signs", "rain", "koi", "traffic", "texpans", "dolly")
$fourKSets = @("keyshots", "stills", "all", "everything")
$heavy = $Long -or @($PlayerArgs | Where-Object { @("-soak", "-perf-log", "-autotest", "-autotest-until", "-wallbot", "-sequence") -contains $_ }).Count -gt 0 -or
         ($joined -match '-capture-4k') -or ($Width -ge 3840)
$is4k = ($joined -match '-capture-4k') -or ($Width -ge 3840)
$hasCapture = @($PlayerArgs | Where-Object { $_ -eq "-capture" }).Count -gt 0
$hasSet = $false
for ($i = 0; $i -lt $PlayerArgs.Count - 1; $i++) {
  $opt = $PlayerArgs[$i]; $val = $PlayerArgs[$i + 1].ToLowerInvariant()
  if ($opt -eq "-capture-set" -or $opt -eq "-sequence") {
    $hasSet = $true
    if ($heavySets -contains $val) { $heavy = $true }
    if ($fourKSets -contains $val) { $is4k = $true }
  }
  if (@("-res", "-perf-rt", "-seq-res") -contains $opt -and $val -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -ge 3840) { $heavy = $true; $is4k = $true }
  # -capture-ss N >= 2 renders N x per axis (3840 x 2160 per 1080p frame): a 4K job (TQ's REQUEST LEAD, D-032 9, carried by FX)
  if ($opt -eq "-capture-ss" -and $val -match '^\d+$' -and [int]$val -ge 2) { $heavy = $true; $is4k = $true }
}
# -capture without -capture-set renders the default set "keyshots" (CliArgs.CaptureSet): heavy and 4K
if ($hasCapture -and -not $hasSet) { $heavy = $true; $is4k = $true }
if ($heavy) {
  $guard = Join-Path $root "Tools\gpu-guard.ps1"
  $vram = if ($is4k) { [Math]::Max($MinFreeVramMiB, 6500) } else { $MinFreeVramMiB }
  Write-Output ("start (gpu-guard, min free VRAM {0} MiB): Nocturne.exe {1}" -f $vram, $joined)
  # In-process call (not a child powershell.exe): Windows PowerShell 5.1 re-quotes native arguments and breaks an
  # -ArgumentString that itself contains quoted paths with spaces (the project root has spaces; seen 2026-11-13).
  $guardArgs = @{ Label = "nocturne-$Name"; TimeoutMin = $TimeoutMin; MinFreeVramMiB = $vram; FilePath = $exe;
                  WorkingDirectory = (Split-Path $exe); ArgumentString = $joined }
  $global:LASTEXITCODE = 0
  & $guard @guardArgs
  $code = $LASTEXITCODE
} else {
  Write-Output ("start: Nocturne.exe " + $joined)
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  $p = Start-Process -FilePath $exe -ArgumentList $argList -PassThru -WorkingDirectory (Split-Path $exe)
  $lastSize = -1; $lastChange = Get-Date
  while (-not $p.HasExited) {
    Start-Sleep -Milliseconds 1000
    $size = if (Test-Path $log) { (Get-Item $log).Length } else { 0 }
    if ($size -ne $lastSize) { $lastSize = $size; $lastChange = Get-Date }
    if ($StallMin -gt 0 -and ((Get-Date) - $lastChange).TotalMinutes -gt $StallMin) { Write-Output "log stalled $StallMin min: killing"; Stop-Process -Id $p.Id -Force; break }
    if ($sw.Elapsed.TotalMinutes -gt $TimeoutMin) { Write-Output "timeout: killing"; Stop-Process -Id $p.Id -Force; break }
  }
  $p.WaitForExit()
  $code = $p.ExitCode
  Write-Output ("exit code {0} after {1:N1} s" -f $code, $sw.Elapsed.TotalSeconds)
}
Write-Output ("exit code {0}; log {1}" -f $code, $log)
if (Test-Path $log) {
  $errs = Select-String -Path $log -Pattern "Exception|\[NOC-FAIL\]|^.*Error.*$" | Where-Object { $_.Line -notmatch "d3d11: failed to create staging|Burst" } | Select-Object -First 30
  Write-Output ("error-like lines: {0}" -f @($errs).Count)
  foreach ($e in $errs) { Write-Output ("  " + $e.Line.Trim()) }
  $tags = Select-String -Path $log -Pattern "\[NOC\]" | Select-Object -Last 30
  foreach ($t in $tags) { Write-Output ("  " + $t.Line.Trim()) }
}
try { $playerMutex.ReleaseMutex() } catch { }
exit $code
