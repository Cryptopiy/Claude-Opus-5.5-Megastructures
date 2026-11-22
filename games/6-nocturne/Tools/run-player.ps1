<#
.SYNOPSIS
  Runs a built Windows player (Builds/Nocturne/Nocturne.exe, or a role's own build folder) with QA flags, waits for it,
  and reports. Windowed 1920x1080 by default; captures render through RenderTextures at any size (Part 11.2).

  2026-11-20, user's order ("full access, no queues that slow you"): no gpu-guard machine turn and no GPU hold. Only our
  own renderer cap applies: at most 3 Nocturne players of this project (main tree and every worktree) at once, at most 2
  when one of them is a 4K job. The check and the start run under a short machine-wide mutex, so two calls cannot both
  pass the check. A call that finds the cap full waits up to -BusyWaitMin, then exits 95.
  A 4K job: -Width 3840 or more, -capture-4k, -res / -perf-rt / -seq-res 3840 or wider, -capture-ss N >= 2, the capture
  sets keyshots / stills / all / everything, or -capture without -capture-set (= keyshots).
.EXAMPLE
  powershell -NoProfile -File Tools\run-player.ps1 -Name autotest -PlayerArgs "-autotest"
  powershell -NoProfile -File Tools\run-player.ps1 -Name bv_cap -BuildDir Builds\BV -PlayerArgs "-capture,QA\captures\BV_r05,-capture-set,beats"
  powershell -NoProfile -File Tools\run-player.ps1 -Name ks -ArgString "-capture QA\captures\ks -capture-only KS04,KS05"
  (PowerShell -File joins an array into one comma string; this script splits -PlayerArgs on commas again. Use -ArgString
  for values that contain commas.)
#>
param(
  [Parameter(Mandatory = $true)][string]$Name,
  [string[]]$PlayerArgs = @(),
  [string]$ArgString = "",
  [int]$Width = 1920,
  [int]$Height = 1080,
  [int]$TimeoutMin = 60,
  [int]$StallMin = -1,         # -1: 6 min for normal runs, off for silent runs (autotest, wallbot, soak, perf-log); 0 = off
  [switch]$Long,               # kept for old callers; no effect (no gpu-guard route any more)
  [int]$MinFreeVramMiB = 1500, # wait while the card has less free video memory than this (other projects render too)
  [int]$BusyWaitMin = 30,
  [string]$BuildDir = "",      # e.g. Builds\BV -> Builds\BV\Nocturne.exe (relative to this root)
  [string]$Exe = "",           # explicit exe; relative to the project root
  [double]$MinFreeGB = 5.0,    # refuse to start under this much free disk (exit 94)
  [double]$AbortFreeGB = 1.5   # kill the player when the disk falls under this while it runs (exit 94)
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if ($Exe -eq "") {
  $dir = $BuildDir
  if ($dir -eq "" -and $env:NOC_BUILD_DIR) { $dir = $env:NOC_BUILD_DIR }
  if ($dir -eq "") { $dir = "Builds\Nocturne" }
  if (-not [System.IO.Path]::IsPathRooted($dir)) { $dir = Join-Path $root $dir }
  $exe = Join-Path $dir "Nocturne.exe"
} else {
  $exe = if ([System.IO.Path]::IsPathRooted($Exe)) { $Exe } else { Join-Path $root $Exe }
}
if (-not (Test-Path $exe)) { Write-Output "player not found: $exe"; exit 98 }
$logDir = Join-Path $root "Logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force $logDir | Out-Null }
$log = Join-Path $logDir "player-$Name.log"
if (Test-Path $log) { Remove-Item $log -Force }

# ---- arguments: -PlayerArgs (split on commas) + -ArgString (split on spaces, quotes group)
if ($PlayerArgs.Count -eq 1 -and $PlayerArgs[0] -match ',') { $PlayerArgs = $PlayerArgs[0].Split(',') }
$tokens = @()
foreach ($a in $PlayerArgs) { if ($a -ne "") { $tokens += $a } }
if ($ArgString -ne "") { foreach ($m in [regex]::Matches($ArgString, '"[^"]*"|\S+')) { $tokens += $m.Value.Trim('"') } }
# resolve relative paths in the argument list against the project root (the player's cwd differs)
$pathFlags = @("-capture", "-perf-log", "-audio-dump", "-render-audio", "-report-dir", "-soak-log")
$resolved = @()
for ($i = 0; $i -lt $tokens.Count; $i++) {
  $a = $tokens[$i]
  if ($i -gt 0 -and $pathFlags -contains $tokens[$i - 1] -and -not [System.IO.Path]::IsPathRooted($a)) { $a = Join-Path $root $a }
  if ($a -match '\s') { $resolved += "`"$a`"" } else { $resolved += $a }
}
# QA fix wave C20: the facadesheet set measures the sheet towers, which are built only with -facade-sheet (p4_01fs ran without it:
# "0 triangles", every pose showed fog); add the flag instead of letting the harness refuse the run
for ($i = 0; $i -lt $tokens.Count - 1; $i++) {
  if ($tokens[$i] -eq "-capture-set" -and @($tokens[$i + 1].ToLowerInvariant().Split(',') | Where-Object { $_ -eq "facadesheet" }).Count -gt 0 -and -not ($tokens -contains "-facade-sheet")) {
    $resolved += "-facade-sheet"; Write-Output "note: -capture-set facadesheet: -facade-sheet added"; break
  }
}
$argList = @("-screen-fullscreen", "0", "-screen-width", "$Width", "-screen-height", "$Height", "-logFile", "`"$log`"")
if (-not ($tokens -contains "-report-dir")) { $argList += @("-report-dir", "`"$(Join-Path $root 'QA\reports')`"") }
$argList += $resolved
$joined = $argList -join " "

# ---- 4K detection (also applied to the command lines of players already running)
function Test-4K([string]$cmd) {
  $c = $cmd.ToLowerInvariant()
  if ($c -match '-screen-width\s+"?(\d+)' -and [int]$Matches[1] -ge 3840) { return $true }
  if ($c -match '-capture-4k') { return $true }
  if ($c -match '-(res|perf-rt|seq-res)\s+(\d+)x\d+' -and [int]$Matches[2] -ge 3840) { return $true }
  if ($c -match '-capture-ss\s+(\d+)' -and [int]$Matches[1] -ge 2) { return $true }
  if ($c -match '-capture-set\s+"?([a-z0-9_,:\-]+)') {
    foreach ($part in $Matches[1].Split(',')) { if (@("keyshots", "stills", "all", "everything") -contains $part.Trim()) { return $true } }
  } elseif ($c -match '\s-capture\s') { return $true }   # -capture without a set renders keyshots (4K)
  return $false
}

# Classification of the job (kept from the gpu-guard era: QaTests parses these lists against ShotPlan.HeavySets / FourKSets /
# RunPlayerRoute). $heavy is reported only; $is4k feeds the renderer cap. (The old gpu-guard route asked 6500 MiB for 4K.)
$PlayerArgs = $tokens
$heavySets = @("keyshots", "stills", "parity", "all", "everything", "sequences", "boundaries", "transitions", "statics", "pans", "edges", "encode", "signs", "rain", "koi", "traffic", "texpans", "dolly", "dread", "cinematic", "cinematic60", "ark")
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
    # HT's request (QA fix wave): a comma list of sets ('statics,boundaries') is classified per set, as ShotPlan.GuardNote does
    if (@($val.Split(',') | Where-Object { $heavySets -contains $_ }).Count -gt 0) { $heavy = $true }
    if (@($val.Split(',') | Where-Object { $fourKSets -contains $_ }).Count -gt 0) { $is4k = $true }
  }
  if (@("-res", "-perf-rt", "-seq-res") -contains $opt -and $val -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -ge 3840) { $heavy = $true; $is4k = $true }
  if ($opt -eq "-capture-ss" -and $val -match '^\d+$' -and [int]$val -ge 2) { $heavy = $true; $is4k = $true }
}
if ($hasCapture -and -not $hasSet) { $heavy = $true; $is4k = $true }
$is4k = $is4k -or (Test-4K $joined)
$silent = @($tokens | Where-Object { @("-autotest", "-wallbot", "-soak", "-perf-log") -contains $_ }).Count -gt 0
if ($StallMin -lt 0) { $StallMin = if ($silent) { 0 } else { 6 } }

function Get-OurPlayers {
  @(Get-CimInstance Win32_Process -Filter "Name='Nocturne.exe'" -ErrorAction SilentlyContinue)
}
function Get-FreeVram {
  try {
    $l = & nvidia-smi --query-gpu=memory.free --format=csv,noheader,nounits 2>$null | Select-Object -First 1
    if ($l) { return [int]$l.Trim() }
  } catch { }
  return 99999
}

# ---- disk guard (QA fix wave C24): the p4_01 gate filled the disk mid-run (Win32 IO 112, half-written sets, players exiting at
# "Direct3D:"). Refuse to start under -MinFreeGB free on the project drive, and kill a running player under -AbortFreeGB.
function Get-FreeGB {
  try { $d = New-Object System.IO.DriveInfo ([System.IO.Path]::GetPathRoot($root)); return [double]$d.AvailableFreeSpace / 1GB } catch { return 999.0 }
}
$freeGB = Get-FreeGB
if ($freeGB -lt $MinFreeGB) {
  Write-Output ("[NOC-FAIL] refused: {0:N1} GB free on {1} (< {2} GB): delete analysed frame sets first (run-player exit 94)" -f $freeGB, [System.IO.Path]::GetPathRoot($root), $MinFreeGB)
  exit 94
}

# ---- renderer cap: check and start under a short machine-wide mutex
$startMutex = New-Object System.Threading.Mutex($false, "Global\NocturnePlayerStart_6Megastructures")
$deadline = (Get-Date).AddMinutes($BusyWaitMin)
$p = $null
while ($true) {
  $have = $false
  try { $have = $startMutex.WaitOne([TimeSpan]::FromMinutes(2)) } catch [System.Threading.AbandonedMutexException] { $have = $true }
  if ($have) {
    try {
      $players = Get-OurPlayers
      $any4k = @($players | Where-Object { $_.CommandLine -and (Test-4K $_.CommandLine) }).Count -gt 0
      $cap = if ($is4k -or $any4k) { 2 } else { 3 }
      $free = Get-FreeVram
      $need = if ($is4k) { [Math]::Max($MinFreeVramMiB, 2500) } else { $MinFreeVramMiB }
      if ($players.Count -lt $cap -and $free -ge $need) {
        Write-Output ("start ({0} of ours running, cap {1}, 4K {2}, free VRAM {3} MiB): {4} {5}" -f $players.Count, $cap, $is4k, $free, $exe, $joined)
        $p = Start-Process -FilePath $exe -ArgumentList $joined -PassThru -WorkingDirectory (Split-Path $exe)
        $null = $p.Handle   # cache the handle so ExitCode is readable after exit (PowerShell 5.1)
        Start-Sleep -Milliseconds 1500   # let the process appear before the next caller counts
      } else {
        $why = "players {0}/{1}, free VRAM {2}/{3} MiB" -f $players.Count, $cap, $free, $need
      }
    } finally { $startMutex.ReleaseMutex() }
  }
  if ($p) { break }
  if ((Get-Date) -gt $deadline) { Write-Output "refused after $BusyWaitMin min: $why"; $startMutex.Dispose(); exit 95 }
  Write-Output ("waiting: " + $why)
  Start-Sleep -Seconds 10
}
$startMutex.Dispose()

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$diskKill = $false
$lastSize = -1; $lastChange = Get-Date
while (-not $p.HasExited) {
  Start-Sleep -Milliseconds 1000
  $size = if (Test-Path $log) { (Get-Item $log).Length } else { 0 }
  if ($size -ne $lastSize) { $lastSize = $size; $lastChange = Get-Date }
  if ($StallMin -gt 0 -and ((Get-Date) - $lastChange).TotalMinutes -gt $StallMin) { Write-Output "log stalled $StallMin min: killing"; Stop-Process -Id $p.Id -Force; break }
  if ($sw.Elapsed.TotalMinutes -gt $TimeoutMin) { Write-Output "timeout: killing"; Stop-Process -Id $p.Id -Force; break }
  if (($sw.Elapsed.TotalSeconds % 10) -lt 1.05) {
    $fg = Get-FreeGB
    if ($fg -lt $AbortFreeGB) { Write-Output ("[NOC-FAIL] disk {0:N2} GB free (< {1} GB): killing the player, the set is incomplete" -f $fg, $AbortFreeGB); Stop-Process -Id $p.Id -Force; $diskKill = $true; break }
  }
}
$p.WaitForExit()
$code = $p.ExitCode
if ($null -eq $code) { $code = 99 }
if ($diskKill) { $code = 94 }
Write-Output ("exit code {0} after {1:N1} s; log {2}" -f $code, $sw.Elapsed.TotalSeconds, $log)
if (Test-Path $log) {
  $errs = Select-String -Path $log -Pattern "Exception|\[NOC-FAIL\]|^.*Error.*$" | Where-Object { $_.Line -notmatch "d3d11: failed to create staging|Burst" } | Select-Object -First 30
  Write-Output ("error-like lines: {0}" -f @($errs).Count)
  foreach ($e in $errs) { Write-Output ("  " + $e.Line.Trim()) }
  $tags = Select-String -Path $log -Pattern "\[NOC\]" | Select-Object -Last 30
  foreach ($t in $tags) { Write-Output ("  " + $t.Line.Trim()) }
  # AU's request (C52, QA): the autotest / perf report of a silent run carries "audio overruns after the intro = 0" (reported, the
  # exit code stays the player's)
  if ($silent) {
    try {
      $rep = & python -I (Join-Path $PSScriptRoot "qa\player_report.py") $log --run $Name 2>&1
      foreach ($l in $rep) { Write-Output ("  player_report: " + $l) }
    } catch { Write-Output ("  player_report: not run (" + $_.Exception.Message + ")") }
  }
}
exit $code
