<#
.SYNOPSIS
  Serialised Unity batch runner for Bulwark (imported from Thread; MASTER_PROMPT 3.2 #1, #2, #10).
  Every Unity batch call (lead, builders, testers) goes through this script.
  It takes a machine-wide named mutex (one Unity at a time for THIS project; it is a different mutex from gpu-guard's
  Global\MegastructuresLongGpuJob), refuses to start while another Unity process has this project open, runs exactly
  one Unity batch call, writes Logs\<Name>.log, and releases the lock.

.PARAMETER StallMin
  Silent-log watchdog in minutes. Default 15 (imports and builds). It is forced to 0 (off) for test runs (-runTests in
  -ExtraArgs), because EditMode/PlayMode tests log nothing while they run (lesson 3.2 #2).

.PARAMETER BuildDir
  Output folder for BatchTools.BuildWindows (sets BW_BUILD_DIR for the Unity process). One build folder per owner
  (lesson 3.2 #10). Relative paths resolve against the project root.

.PARAMETER ScanLog
  Runs only the log digest below on an existing batch log (no lock, no Unity): prints the error lines and the number of
  [BW-FAIL] lines (D-06: BwLog.Problem / BwLog.Fail and the "[BW-FAIL] problem:" summary of every batch method).

  Digest after every run: compiler errors, exceptions and every [BW-FAIL] line (first 25), the count of [BW-FAIL] lines,
  and the last 40 [BW] lines.

.EXAMPLE
  powershell -NoProfile -File Tools\unity-queue.ps1 -Name compile -Method Megastructure.EditorTools.BatchTools.CompileCheck
  powershell -NoProfile -File Tools\unity-queue.ps1 -Name rebuild -Method Megastructure.EditorTools.BatchTools.RebuildAll -TimeoutMin 30
  powershell -NoProfile -File Tools\unity-queue.ps1 -Name build-b3 -Method Megastructure.EditorTools.BatchTools.BuildWindows -BuildDir Builds\b3
  powershell -NoProfile -File Tools\unity-queue.ps1 -Name tests -NoQuit -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,W:\abs\QA\tests\editmode.xml"
#>
param(
  [Parameter(Mandatory = $true)][string]$Name,
  [string]$Method = "",
  [string[]]$ExtraArgs = @(),
  [switch]$NoQuit,
  [switch]$NoGraphics,
  [int]$TimeoutMin = 60,
  [int]$StallMin = 15,
  [string]$BuildDir = "",
  [string]$ScanLog = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

# Digest of a batch log: errors (compiler, exceptions, [BW-FAIL]) and the [BW-FAIL] count (D-06: BwLog.Problem logs the tag).
function Write-Digest([string]$path) {
  # Burst pipe errors at exit are harmless (lesson 3.2 #10)
  $errs = @(Select-String -Path $path -Pattern "error CS\d+|Exception:|\[BW-FAIL\]|Scripts have compiler errors" -SimpleMatch:$false | Where-Object { $_.Line -notmatch "Burst|burst" })
  foreach ($e in ($errs | Select-Object -First 25)) { Write-Output ("  " + $e.Line.Trim()) }
  $fails = @(Select-String -Path $path -Pattern "[BW-FAIL]" -SimpleMatch)
  Write-Output ("  [BW-FAIL] lines: {0}" -f $fails.Count)
  $tags = Select-String -Path $path -Pattern "\[BW\]" | Select-Object -Last 40
  foreach ($t in $tags) { Write-Output ("  " + $t.Line.Trim()) }
  $script:FailCount = $fails.Count
}
$script:FailCount = 0
if ($ScanLog -ne "") {
  if (-not (Test-Path $ScanLog)) { Write-Output "log not found: $ScanLog"; exit 2 }
  Write-Output "digest of $ScanLog"
  Write-Digest $ScanLog
  exit 0
}

$unity = "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe"
if (-not (Test-Path $unity)) { Write-Output "Unity not found: $unity"; exit 96 }
$logDir = Join-Path $root "Logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force $logDir | Out-Null }
$log = Join-Path $logDir "$Name.log"
$queueLog = Join-Path $logDir "unity-queue.log"

function Write-Q([string]$msg) {
  $line = "[{0}] [{1}] {2}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Name, $msg
  Add-Content -Path $queueLog -Value $line
  Write-Output $line
}

# A Unity process has THIS project open when its command line carries -projectPath "<root>" followed by a quote, a
# space or the end. A plain substring test would also match "...\4 Megastructures\4 Megastructures2".
function Get-ProjectUnityProcesses {
  $rootNorm = [Regex]::Escape($root.ToLowerInvariant().TrimEnd('\'))
  $pattern = '-projectpath\s+"?' + $rootNorm + '\\?("|\s|$)'
  Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {
    $_.CommandLine -and ($_.CommandLine.ToLowerInvariant() -match $pattern)
  }
}

# a list passed through 'powershell -File' arrives as one comma-joined string: split it (lesson 3.2 #1)
if ($ExtraArgs.Count -eq 1 -and $ExtraArgs[0] -match ',') { $ExtraArgs = $ExtraArgs[0].Split(',') }
if ($ExtraArgs -contains "-runTests") {
  if ($StallMin -ne 0) { Write-Output "test run: stall watchdog off (tests log nothing while they run)" }
  $StallMin = 0
  if (-not $NoQuit) { Write-Output "test run: -quit is not passed with -runTests"; $NoQuit = $true }
}
if ($BuildDir -ne "") {
  if (-not [System.IO.Path]::IsPathRooted($BuildDir)) { $BuildDir = Join-Path $root $BuildDir }
  $env:BW_BUILD_DIR = $BuildDir
}

# one queue per project folder (the main project and each worktree under .worktrees\ are separate Unity projects)
$queueName = "Global\BulwarkUnityQueue_" + (($root.ToLowerInvariant().TrimEnd('\')) -replace '[^a-z0-9]', '_')
if ($queueName.Length -gt 250) { $queueName = $queueName.Substring(0, 250) }
$mutex = New-Object System.Threading.Mutex($false, $queueName)
$acquired = $false
try {
  try { $acquired = $mutex.WaitOne([TimeSpan]::FromMinutes(90)) }
  catch [System.Threading.AbandonedMutexException] { $acquired = $true }
  if (-not $acquired) { Write-Q "could not acquire the Unity queue lock within 90 min"; exit 97 }

  # Single-instance rule: an interactive Editor with this project open is closed gracefully.
  $procs = @(Get-ProjectUnityProcesses)
  foreach ($p in $procs) {
    $isBatch = $p.CommandLine -match "-batchmode"
    Write-Q ("found Unity pid {0} with this project open (batch={1})" -f $p.ProcessId, $isBatch)
    $gp = Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue
    if ($gp) {
      if (-not $isBatch) { [void]$gp.CloseMainWindow() }
      if (-not $gp.WaitForExit(30000)) { Write-Q "terminating pid $($p.ProcessId)"; Stop-Process -Id $p.ProcessId -Force }
    }
  }
  $lockfile = Join-Path $root "Temp\UnityLockfile"
  if ((Test-Path $lockfile) -and (@(Get-ProjectUnityProcesses).Count -eq 0)) {
    Write-Q "removing stale Temp\UnityLockfile"
    Remove-Item $lockfile -Force -ErrorAction SilentlyContinue
  }

  $argList = @("-batchmode", "-projectPath", "`"$root`"", "-logFile", "`"$log`"")
  if (-not $NoQuit) { $argList += "-quit" }
  if ($NoGraphics) { $argList += "-nographics" }
  if ($Method -ne "") { $argList += @("-executeMethod", $Method) }
  foreach ($a in $ExtraArgs) { if ($a -match '\s') { $argList += "`"$a`"" } else { $argList += $a } }

  if (Test-Path $log) { Remove-Item $log -Force -ErrorAction SilentlyContinue }
  $bd = if ($env:BW_BUILD_DIR) { " (BW_BUILD_DIR=$($env:BW_BUILD_DIR))" } else { "" }
  Write-Q ("start: Unity.exe " + ($argList -join " ") + $bd + " stall=" + $StallMin + " min")
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  $proc = Start-Process -FilePath $unity -ArgumentList $argList -PassThru -WindowStyle Hidden
  $null = $proc.Handle   # cache the handle so ExitCode is readable after exit (PowerShell 5.1)
  $lastSize = -1; $lastChange = Get-Date
  while (-not $proc.HasExited) {
    Start-Sleep -Milliseconds 1000
    $size = if (Test-Path $log) { (Get-Item $log).Length } else { 0 }
    if ($size -ne $lastSize) { $lastSize = $size; $lastChange = Get-Date }
    if ($StallMin -gt 0 -and ((Get-Date) - $lastChange).TotalMinutes -gt $StallMin) {
      Write-Q "log unchanged for $StallMin min: killing pid $($proc.Id)"; Stop-Process -Id $proc.Id -Force; break
    }
    if ($sw.Elapsed.TotalMinutes -gt $TimeoutMin) {
      Write-Q "timeout $TimeoutMin min: killing pid $($proc.Id)"; Stop-Process -Id $proc.Id -Force; break
    }
  }
  $proc.WaitForExit()
  $code = $proc.ExitCode
  if ($null -eq $code) { $code = 99 }
  Write-Q ("exit code {0} after {1:N1} s; log {2}" -f $code, $sw.Elapsed.TotalSeconds, $log)
  if (Test-Path $log) {
    Write-Digest $log
    if ($script:FailCount -gt 0) { Write-Q ("{0} [BW-FAIL] line(s) in {1}" -f $script:FailCount, $log) }
  }
  exit $code
}
finally {
  if ($acquired) { $mutex.ReleaseMutex() }
  $mutex.Dispose()
}
