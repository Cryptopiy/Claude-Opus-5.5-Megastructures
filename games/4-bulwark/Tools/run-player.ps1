<#
.SYNOPSIS
  Runs the built Windows player (Builds\Bulwark\Bulwark.exe) with QA flags, waits for it and reports
  (imported from Thread; MASTER_PROMPT 2.5, 3.2 #1, #2, #9, #10, 11.2).
  Windowed 1920x1080 by default; captures render through RenderTextures at any size.

.DESCRIPTION
  Arguments
    -PlayerArgs  array form, one value per element. Under 'powershell -File' an array arrives as ONE comma-joined string
                 and is split on commas (lesson 3.2 #1), so never put a comma list (-capture-only a,b) here.
    -ArgString   one string, tokenised on spaces (double quotes group), never split on commas: use it for
                 -capture-only a,b and anything else with commas.
  Relative paths after -capture, -perf-log, -audio-dump and -report-dir resolve against the project root.

  Long GPU jobs (2.5.1) go through Tools\gpu-guard.ps1 with the arguments as ONE -ArgumentString:
    -Long, and implied by -soak, -autotest, -wallbot, -perf-log, -perf-rt, -capture-4k, any -capture-set list (split on
    commas) that names encode, sequences, transitions, statics, pans, edges, all, everything, keyshots or stills (keyshots
    and stills always render the 3840 x 2160 variant of each key shot, so they are 4K jobs) or a QA rig's own set (any name
    ShotPlan does not know, e.g. kraken: treated as 4K), -capture without -capture-set (= keyshots), or any capture at
    3840 px or wider. 4K jobs ask for 6500 MiB of free VRAM, others 5000.
    Perf runs stay capped at 60 fps unless they also pass -uncapped (QualityBootstrap, Part 2.5.3 / 11.7).
  -DryRun prints how the arguments were parsed (tokens, capture sets, 4K / long / silent, the gpu-guard route and the final
    argument string) and exits 0 without starting anything; with -ScanLog <file> it also runs the log scan below on that
    file. The scan reports real errors only (case-sensitive): [BW-FAIL] lines, exception lines, Debug.LogError /
    LogException / LogAssertion stack lines, lines starting with Error, compiler and shader errors, crashes and device
    removal; never a summary such as "errors=0".
    gpu-guard exit codes: 75 = GPU busy (retry later or lower the resolution, never force), 76 = no machine-wide turn,
    124 = its time limit killed the job.
  Stall watchdog: off (0) for silent runs (-autotest, -wallbot, -soak, -perf-log) and for every gpu-guard job (its time
  limit applies instead); 20 min for other direct runs. -StallMin overrides it for direct runs.
  Before starting, waits up to -BusyWaitMin for any other Bulwark.exe or an interactive Editor of this project to close
  (2.5.2: never two GPU-rendering processes of our own), then refuses with exit 95.
  The result is the player's exit code AND the 'pass' field of every QA JSON written during the run (PowerShell 5.1 can
  lose ExitCode): any pass=false gives exit 1.
  Video memory (2.5.4): nvidia-smi is sampled every 5 s into QA\vram\vram-<Name>.csv; the baseline and peak go to
  QA\vram\vram_summary.log.

.EXAMPLE
  powershell -NoProfile -File Tools\run-player.ps1 -Name autotest -PlayerArgs "-autotest"
  powershell -NoProfile -File Tools\run-player.ps1 -Name autotest-cin -PlayerArgs "-autotest,-cinematic"
  powershell -NoProfile -File Tools\run-player.ps1 -Name keyshots -PlayerArgs "-capture,QA\captures\run1,-capture-set,keyshots"
  powershell -NoProfile -File Tools\run-player.ps1 -Name ks2 -ArgString "-capture QA\captures\ks2 -capture-only KS01,KS04"
  powershell -NoProfile -File Tools\run-player.ps1 -Name soak -PlayerArgs "-soak,1800" -TimeoutMin 45
  powershell -NoProfile -File Tools\run-player.ps1 -Name b3 -BuildDir Builds\b3 -PlayerArgs "-capture,QA\captures\b3,-capture-set,beats"
#>
param(
  [Parameter(Mandatory = $true)][string]$Name,
  [string[]]$PlayerArgs = @(),
  [string]$ArgString = "",
  [string]$Exe = "",
  [string]$BuildDir = "",
  [int]$Width = 1920,
  [int]$Height = 1080,
  [int]$TimeoutMin = 60,
  [int]$StallMin = -1,
  [switch]$Long,
  [int]$BusyWaitMin = 30,
  [switch]$DryRun,
  [string]$ScanLog = ""
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

# ---------------------------------------------------------------- log scan: real errors only (case-sensitive; never "errors=0")
# device loss (Part 2.5.7, top-priority blocker) case-insensitive: 'Failed to present D3D11 swapchain due to device reset',
# 'Device removed', 'device hung' (the rest of the pattern stays case-sensitive: 'errors=0' summaries are not errors)
$ErrPattern = '\[BW-FAIL\]|^\s*[\w.]*Exception(:|\s*$)|UnityEngine\.Debug:Log(Error|Exception|Assertion)\b|^\s*(Error|ERROR)\b|\berror CS\d{4}\b|Shader error|Crash!!!|DXGI_ERROR_DEVICE_(REMOVED|HUNG|RESET)|(?i:\bdevice (removed|hung|reset)\b)|(?i:failed to present d3d11 swapchain)|(?i:swapchain.*(device|fail))|Assertion failed'
$ErrIgnore = 'd3d11: failed to create staging|Burst'
function Scan-Log([string]$path) {
  return @(Select-String -Path $path -Pattern $ErrPattern -CaseSensitive | Where-Object { $_.Line -cnotmatch $ErrIgnore })
}
# the reports this player run wrote: the QA runners end their PASS / FAIL line with "report=<path>"
function Get-OwnReports([string]$path) {
  $list = @()
  foreach ($m in @(Select-String -Path $path -Pattern 'report=(.+\.json)\s*$' -CaseSensitive)) {
    $p = $m.Matches[0].Groups[1].Value.Trim()
    if ((Test-Path $p) -and -not ($list -contains $p)) { $list += $p }
  }
  return $list
}

# ---------------------------------------------------------------- the player exe (one build folder per owner, 3.2 #10)
if ($Exe -eq "") {
  $dir = $BuildDir
  if ($dir -eq "" -and $env:BW_BUILD_DIR) { $dir = $env:BW_BUILD_DIR }
  if ($dir -eq "") { $dir = "Builds\Bulwark" }
  if (-not [System.IO.Path]::IsPathRooted($dir)) { $dir = Join-Path $root $dir }
  $Exe = Join-Path $dir "Bulwark.exe"
}
elseif (-not [System.IO.Path]::IsPathRooted($Exe)) { $Exe = Join-Path $root $Exe }
if (-not (Test-Path $Exe) -and -not $DryRun) { Write-Output "player not found: $Exe"; exit 98 }

$logDir = Join-Path $root "Logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force $logDir | Out-Null }
$log = Join-Path $logDir "player-$Name.log"
if ((Test-Path $log) -and -not $DryRun) { Remove-Item $log -Force }
$vramDir = Join-Path $root "QA\vram"
if (-not (Test-Path $vramDir)) { New-Item -ItemType Directory -Force $vramDir | Out-Null }

# ---------------------------------------------------------------- arguments
# a list passed through 'powershell -File' arrives as one comma-joined string: split it (lesson 3.2 #1)
if ($PlayerArgs.Count -eq 1 -and $PlayerArgs[0] -match ',') { $PlayerArgs = $PlayerArgs[0].Split(',') }
$tokens = @()
foreach ($a in $PlayerArgs) { if ($a -ne "") { $tokens += $a } }
if ($ArgString -ne "") {
  foreach ($m in [regex]::Matches($ArgString, '"[^"]*"|\S+')) { $tokens += $m.Value.Trim('"') }
}
$pathFlags = @("-capture", "-perf-log", "-audio-dump", "-report-dir")
$resolved = @()
$reportDir = Join-Path $root "QA\reports"
$hasReportDir = $false
for ($i = 0; $i -lt $tokens.Count; $i++) {
  $a = $tokens[$i]
  if ($i -gt 0 -and $pathFlags -contains $tokens[$i - 1] -and -not [System.IO.Path]::IsPathRooted($a)) { $a = Join-Path $root $a }
  if ($i -gt 0 -and $tokens[$i - 1] -eq "-report-dir") { $reportDir = $a; $hasReportDir = $true }
  $resolved += $a
}
if (-not (Test-Path $reportDir)) { New-Item -ItemType Directory -Force $reportDir | Out-Null }

function Has-Flag([string]$f) { return $tokens -contains $f }
function Flag-Value([string]$f) {
  for ($i = 0; $i -lt $tokens.Count - 1; $i++) { if ($tokens[$i] -eq $f) { return $tokens[$i + 1] } }
  return $null
}

# ---------------------------------------------------------------- long-job and 4K detection (2.5.1)
$is4k = $Width -ge 3840 -or (Has-Flag "-capture-4k")
$perfRt = Flag-Value "-perf-rt"
if ($perfRt -and $perfRt -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -ge 3840) { $is4k = $true }
$res = Flag-Value "-res"
if ($res -and $res -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -ge 3840) { $is4k = $true }
$seqRes = Flag-Value "-seq-res"
if ($seqRes -and $seqRes -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -ge 3840) { $is4k = $true }
$set = Flag-Value "-capture-set"
# -capture-set is a comma list (ShotPlan.Build): split it. Every key shot is also captured at 3840x2160
# (ShotPlan.KeyShotStills), and -capture without -capture-set means "keyshots" (CliArgs.CaptureSet): such runs are 4K jobs
# (gpu-guard, 6500 MiB free VRAM; 2.5.1).
foreach ($listFlag in @("-capture-set", "-capture-only")) {
  for ($i = 0; $i -lt $tokens.Count - 2; $i++) {
    if ($tokens[$i] -eq $listFlag -and -not $tokens[$i + 2].StartsWith("-")) {
      Write-Output ("warning: '{0} {1}' is followed by the bare token '{2}': a comma list passed through -PlayerArgs was split (lesson 3.2 #1); pass it in -ArgString" -f $listFlag, $tokens[$i + 1], $tokens[$i + 2])
    }
  }
}
$setParts = @()
if (Has-Flag "-capture") {
  $setList = if ($set) { $set } else { "keyshots" }
  $setParts = @($setList.ToLowerInvariant().Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" })
}
$fourKSets = @("keyshots", "stills", "all", "everything")
# long GPU jobs (2.5.1): every sequence set (frames for minutes), the encode clips, and the sets that contain the 4K key shots
$heavySets = @("encode", "sequences", "transitions", "statics", "pans", "edges", "all", "everything", "keyshots", "stills", "ride", "kseq", "tprec")   # ride / kseq / tprec: 1080p sequences (fix wave X2), not 4K
# a set ShotPlan does not know itself is a QA rig's set (QaRigs, e.g. "kraken" from QA_KrakenRig): it may hold 4K stills
# and long motion sequences, so it is treated as a heavy 4K job (gpu-guard, 6500 MiB free); pose: tokens are single stills
$lightSets = @("beats", "parity", "materials", "matcontext")
$heavySet = $false
foreach ($part in $setParts) {
  if ($fourKSets -contains $part) { $is4k = $true }
  if ($heavySets -contains $part) { $heavySet = $true }
  if (-not ($fourKSets -contains $part) -and -not ($heavySets -contains $part) -and -not ($lightSets -contains $part) -and -not $part.StartsWith("pose:")) { $is4k = $true; $heavySet = $true }
}
$silent = (Has-Flag "-autotest") -or (Has-Flag "-wallbot") -or (Has-Flag "-soak") -or (Has-Flag "-perf-log")
$isLong = ($Long.IsPresent -or $silent -or (Has-Flag "-perf-rt") -or (Has-Flag "-capture-4k") -or $heavySet -or ($is4k -and (Has-Flag "-capture")))
$soakS = Flag-Value "-soak"
if ($soakS) { $need = [int][Math]::Ceiling([double]$soakS / 60.0) + 15; if ($TimeoutMin -lt $need) { $TimeoutMin = $need } }
if ($StallMin -lt 0) { if ($silent) { $StallMin = 0 } else { $StallMin = 20 } }

$argList = @("-screen-fullscreen", "0", "-screen-width", "$Width", "-screen-height", "$Height", "-logFile", $log)
if (-not $hasReportDir) { $argList += @("-report-dir", $reportDir) }
$argList += $resolved
$quoted = @()
foreach ($a in $argList) { if ($a -match '\s') { $quoted += "`"$a`"" } else { $quoted += $a } }
$argString1 = $quoted -join " "

# ---------------------------------------------------------------- dry check: show the parse, start nothing
if ($DryRun) {
  $minFreeDry = if ($is4k) { 6500 } else { 5000 }
  Write-Output ("dry-run {0}: exe {1} (exists {2})" -f $Name, $Exe, (Test-Path $Exe))
  Write-Output ("  tokens ({0}): {1}" -f $tokens.Count, ($tokens -join " | "))
  Write-Output ("  capture sets: [{0}]  4k={1} heavySet={2} silent={3} long={4} stallMin={5} timeoutMin={6}" -f ($setParts -join ", "), $is4k, $heavySet, $silent, $isLong, $StallMin, $TimeoutMin)
  if ($isLong) { Write-Output ("  route: gpu-guard (VRAM >= {0} MiB, limit {1} min)" -f $minFreeDry, $TimeoutMin) } else { Write-Output "  route: direct (stall watchdog)" }
  Write-Output ("  args: " + $argString1)
  if ($ScanLog -ne "") {
    if (-not (Test-Path $ScanLog)) { Write-Output "  scan: $ScanLog not found"; exit 2 }
    $hits = Scan-Log $ScanLog
    Write-Output ("  scan {0}: error lines {1}" -f $ScanLog, $hits.Count)
    foreach ($h in $hits) { Write-Output ("    {0}: {1}" -f $h.LineNumber, $h.Line.Trim()) }
    $own = @(Get-OwnReports $ScanLog)
    Write-Output ("  reports named by the log: {0}" -f $(if ($own.Count -gt 0) { $own -join ", " } else { "none (fallback: every report written since the start)" }))
  }
  exit 0
}

# ---------------------------------------------------------------- (2026-11-20) the user gave the whole machine to this
# project and ordered "no queues": no lead GPU hold, no machine-wide gpu-guard turn; only our own renderer cap below.

# ---------------------------------------------------------------- never two renderers of our own (2.5.2)
function Get-Busy {
  $list = @()
  # the user gave the whole machine to this project (2026-11-20): up to 2 short 1080p renderers of ours may run at once;
  $players = @(Get-Process -Name "Bulwark" -ErrorAction SilentlyContinue)
  $allowed = if ($is4k) { 1 } else { 2 }   # at most 3 renderers of ours at once (2 when one of them is 4K): VRAM safety
  if ($players.Count -gt $allowed) { foreach ($p in $players) { $list += "Bulwark.exe pid $($p.Id)" } }
  $rootNorm = [Regex]::Escape($root.ToLowerInvariant().TrimEnd('\'))
  $pattern = '-projectpath\s+"?' + $rootNorm + '\\?("|\s|$)'
  foreach ($u in @(Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" -ErrorAction SilentlyContinue)) {
    if ($u.CommandLine -and ($u.CommandLine.ToLowerInvariant() -match $pattern) -and ($u.CommandLine -notmatch "-batchmode")) {
      $list += "interactive Unity Editor of this project pid $($u.ProcessId)"
    }
  }
  return $list
}
$busyDeadline = (Get-Date).AddMinutes($BusyWaitMin)
while ($true) {
  $busy = @(Get-Busy)
  if ($busy.Count -eq 0) { break }
  if ((Get-Date) -gt $busyDeadline) { Write-Output ("refused: another renderer of ours is running: " + ($busy -join "; ")); exit 95 }
  Write-Output ("waiting: " + ($busy -join "; "))
  Start-Sleep -Seconds 10
}

# ---------------------------------------------------------------- VRAM sampler (2.5.4)
$vramCsv = Join-Path $vramDir "vram-$Name.csv"
$sampler = $null
if (Get-Command nvidia-smi -ErrorAction SilentlyContinue) {
  $stopFile = Join-Path $vramDir "vram-$Name.stop"
  if (Test-Path $stopFile) { Remove-Item $stopFile -Force }
  $sampler = Start-Job -ArgumentList $vramCsv, $stopFile -ScriptBlock {
    param($csv, $stop)
    Set-Content -Path $csv -Value "time,used_mib,total_mib,temp_c,util_pct"
    while (-not (Test-Path $stop)) {
      $l = & nvidia-smi --query-gpu=memory.used,memory.total,temperature.gpu,utilization.gpu --format=csv,noheader,nounits 2>$null | Select-Object -First 1
      if ($l) { Add-Content -Path $csv -Value ((Get-Date -Format "HH:mm:ss") + "," + (($l.Split(",") | ForEach-Object { $_.Trim() }) -join ",")) }
      Start-Sleep -Seconds 5
    }
  }
}

$start = Get-Date
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$code = $null
if ($isLong) { $StallMin = 0 }   # silent and long runs log little: no stall killer, only the time limit
if ($false) {   # gpu-guard route disabled by the user's order (2026-11-20); long jobs launch directly
  $minFree = 5000
  if ($is4k) { $minFree = 6500 }
  Write-Output ("start (gpu-guard, VRAM >= {0} MiB, limit {1} min): Bulwark.exe {2}" -f $minFree, $TimeoutMin, $argString1)
  & (Join-Path $PSScriptRoot "gpu-guard.ps1") -Label $Name -TimeoutMin $TimeoutMin -MinFreeVramMiB $minFree -FilePath $Exe -WorkingDirectory (Split-Path $Exe) -ArgumentString $argString1
  $code = $LASTEXITCODE
  if ($code -eq 75) { Write-Output "gpu-guard: GPU busy (too hot or not enough free VRAM): retry later or lower the resolution" }
  elseif ($code -eq 76) { Write-Output "gpu-guard: no machine-wide turn within its wait limit" }
  elseif ($code -eq 124) { Write-Output "gpu-guard: the time limit of $TimeoutMin min killed the job" }
}
else {
  Write-Output ("start: Bulwark.exe " + $argString1 + " (stall " + $StallMin + " min)")
  $p = Start-Process -FilePath $Exe -ArgumentList $argString1 -PassThru -WorkingDirectory (Split-Path $Exe)
  $null = $p.Handle   # cache the handle so ExitCode survives (PowerShell 5.1)
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
}

# ---------------------------------------------------------------- VRAM summary
if ($sampler) {
  Set-Content -Path $stopFile -Value "stop"
  Wait-Job $sampler -Timeout 15 | Out-Null
  Remove-Job $sampler -Force -ErrorAction SilentlyContinue
  Remove-Item $stopFile -Force -ErrorAction SilentlyContinue
  if (Test-Path $vramCsv) {
    $rows = @(Import-Csv $vramCsv)
    if ($rows.Count -gt 0) {
      $base = [double]$rows[0].used_mib
      $peak = ($rows | ForEach-Object { [double]$_.used_mib } | Measure-Object -Maximum).Maximum
      $tmax = ($rows | ForEach-Object { [double]$_.temp_c } | Measure-Object -Maximum).Maximum
      $line = "{0} {1} res={2}x{3} 4k={4} baseline={5} MiB peak={6} MiB delta={7} MiB maxTemp={8} C samples={9}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Name, $Width, $Height, $is4k, $base, $peak, ($peak - $base), $tmax, $rows.Count
      Add-Content -Path (Join-Path $vramDir "vram_summary.log") -Value $line
      Write-Output ("vram: " + $line)
    }
  }
}

# ---------------------------------------------------------------- result: exit code AND the QA JSON pass fields
# The QA runners name their report at the end of the PASS / FAIL line ("... report=<path>"): read exactly those, so a report
# that another owner's run wrote into the shared QA\reports while this run waited for its GPU turn is never counted. Runs
# without such a line (captures, perf) fall back to every report written since this script started.
$fails = 0; $passes = 0
$ownReports = if (Test-Path $log) { @(Get-OwnReports $log) } else { @() }
$reportFiles = if ($ownReports.Count -gt 0) { @($ownReports | ForEach-Object { Get-Item $_ }) } else { @(Get-ChildItem -Path $reportDir -Filter "*.json" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $start }) }
foreach ($f in $reportFiles) {
  try {
    $j = Get-Content $f.FullName -Raw | ConvertFrom-Json
    if ($null -ne $j.pass) {
      if ($j.pass -eq $true) { $passes++ } else { $fails++ }
      Write-Output ("  report {0}: pass={1}" -f $f.Name, $j.pass)
    }
  }
  catch { Write-Output ("  report {0}: unreadable ({1})" -f $f.Name, $_.Exception.Message); $fails++ }
}
Write-Output ("exit code {0} after {1:N1} s; reports pass={2} fail={3}; log {4}" -f $code, $sw.Elapsed.TotalSeconds, $passes, $fails, $log)
if (Test-Path $log) {
  $errs = Scan-Log $log
  Write-Output ("error lines: {0}" -f $errs.Count)
  foreach ($e in ($errs | Select-Object -First 30)) { Write-Output ("  {0}: {1}" -f $e.LineNumber, $e.Line.Trim()) }
  $tags = Select-String -Path $log -Pattern "\[BW\]" | Select-Object -Last 30
  foreach ($t in $tags) { Write-Output ("  " + $t.Line.Trim()) }
}
$leftover = @(Get-Process -Name "Bulwark" -ErrorAction SilentlyContinue)
if ($leftover.Count -gt 0) { Write-Output ("warning: Bulwark.exe still running: " + (($leftover | ForEach-Object { $_.Id }) -join ", ")) }
if ($fails -gt 0) { exit 1 }
if ($null -eq $code) { if ($passes -gt 0) { exit 0 } else { exit 99 } }
exit $code
