<#
.SYNOPSIS
  Serialised Unity batch runner for the Nocturne project (MASTER_PROMPT Part 5.2).
  Every Unity batch call (lead, builders, testers) goes through this script.
  It takes a machine-wide named mutex, refuses to start while another Unity process has this
  project open, runs exactly one Unity batch call, writes Logs\<Name>.log, and releases the lock.

.EXAMPLE
  powershell -NoProfile -File Tools\unity-queue.ps1 -Name compile -Method Megastructure.EditorTools.BatchTools.CompileCheck
  powershell -NoProfile -File Tools\unity-queue.ps1 -Name rebuild -Method Megastructure.EditorTools.BatchTools.RebuildAll -TimeoutMin 30
#>
param(
  [Parameter(Mandatory = $true)][string]$Name,
  [string]$Method = "",
  [string[]]$ExtraArgs = @(),
  [switch]$NoQuit,
  [switch]$NoGraphics,
  [int]$TimeoutMin = 60,
  [int]$StallMin = 6,  # 0 disables the silent-log watchdog (test runs log nothing while tests execute)
  [string]$BuildDir = ""   # BuildWindows output folder (sets NOC_BUILD_DIR), relative to this root, e.g. Builds\BV
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$unity = "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe"
$logDir = Join-Path $root "Logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force $logDir | Out-Null }
$log = Join-Path $logDir "$Name.log"
$queueLog = Join-Path $logDir "unity-queue.log"

function Write-Q([string]$msg) {
  $line = "[{0}] [{1}] {2}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Name, $msg
  Add-Content -Path $queueLog -Value $line
  Write-Output $line
}

# A Unity process has THIS project open when its command line carries -projectPath "<root>" followed by a quote, a space
# or the end. A plain substring test would also match the worktrees under <root>\.worktrees\ (2026-11-20, game 4 lesson).
function Get-ProjectUnityProcesses {
  $rootNorm = [Regex]::Escape($root.ToLowerInvariant().TrimEnd('\'))
  $pattern = '-projectpath\s+"?' + $rootNorm + '\\?("|\s|$)'
  Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {
    $_.CommandLine -and ($_.CommandLine.ToLowerInvariant() -match $pattern)
  }
}

# One queue per project folder (2026-11-20, user's order): the main project and each worktree under .worktrees\ are
# separate Unity projects with their own Library, so each gets its own mutex derived from its path.
$queueName = "Global\NocturneUnityQueue_" + (($root.ToLowerInvariant().TrimEnd('\')) -replace '[^a-z0-9]', '_')
if ($queueName.Length -gt 250) { $queueName = $queueName.Substring(0, 250) }
if ($BuildDir -ne "") {
  if (-not [System.IO.Path]::IsPathRooted($BuildDir)) { $BuildDir = Join-Path $root $BuildDir }
  $env:NOC_BUILD_DIR = $BuildDir
}
$mutex = New-Object System.Threading.Mutex($false, $queueName)
$acquired = $false
try {
  try { $acquired = $mutex.WaitOne([TimeSpan]::FromMinutes(90)) }
  catch [System.Threading.AbandonedMutexException] { $acquired = $true }
  if (-not $acquired) { Write-Q "could not acquire the Unity queue lock within 90 min"; exit 97 }

  # Single-instance rule (Part 3): an interactive Editor with this project open is closed gracefully.
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

  # a list passed through 'powershell -File' arrives as one comma-joined string: split it
  if ($ExtraArgs.Count -eq 1 -and $ExtraArgs[0] -match ',') { $ExtraArgs = $ExtraArgs[0].Split(',') }
  if ($ExtraArgs -contains "-runTests") { $StallMin = 0; $NoQuit = $true }   # tests log nothing while they run; -quit breaks -runTests
  $argList = @("-batchmode", "-projectPath", "`"$root`"", "-logFile", "`"$log`"")
  if (-not $NoQuit) { $argList += "-quit" }
  if ($NoGraphics) { $argList += "-nographics" }
  if ($Method -ne "") { $argList += @("-executeMethod", $Method) }
  foreach ($a in $ExtraArgs) { if ($a -match '\s') { $argList += "`"$a`"" } else { $argList += $a } }

  if (Test-Path $log) { Remove-Item $log -Force -ErrorAction SilentlyContinue }
  $bd = if ($env:NOC_BUILD_DIR) { " (NOC_BUILD_DIR=$($env:NOC_BUILD_DIR))" } else { "" }
  Write-Q ("start: Unity.exe " + ($argList -join " ") + $bd)
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  $proc = Start-Process -FilePath $unity -ArgumentList $argList -PassThru -WindowStyle Hidden
  $null = $proc.Handle   # cache the handle so ExitCode is readable after exit (PowerShell 5.1)
  $lastSize = -1; $lastChange = Get-Date; $ilppCheck = Get-Date
  while (-not $proc.HasExited) {
    Start-Sleep -Milliseconds 1000
    $size = if (Test-Path $log) { (Get-Item $log).Length } else { 0 }
    if ($size -ne $lastSize) { $lastSize = $size; $lastChange = Get-Date }
    # ILPP stall guard (2026-11-20): when .NET processes start very slowly on this machine, the IL Post Processor
    # runner never opens its pipe and Unity retries every ~30 s forever. Fail fast (exit 96) instead of hanging.
    if (((Get-Date) - $ilppCheck).TotalSeconds -ge 15 -and (Test-Path $log)) {
      $ilppCheck = Get-Date
      $ilpp = 0
      try {
        $fs = New-Object IO.FileStream($log, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
        $sr = New-Object IO.StreamReader($fs); $txt = $sr.ReadToEnd(); $sr.Close()
        $ilpp = ([regex]::Matches($txt, "IL Post Processor runner cannot be established")).Count
      } catch { }
      if ($ilpp -ge 12) {
        Write-Q "ILPP stall: $ilpp retries (the machine starts .NET processes too slowly); killing the tree of pid $($proc.Id), exit 96"
        & taskkill.exe /PID $proc.Id /T /F | Out-Null; $proc.WaitForExit(); exit 96
      }
    }
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
    $errs = Select-String -Path $log -Pattern "error CS\d+|Exception:|\[NOC-FAIL\]|Scripts have compiler errors" -SimpleMatch:$false | Where-Object { $_.Line -notmatch "Burst|burst" } | Select-Object -First 25
    foreach ($e in $errs) { Write-Output ("  " + $e.Line.Trim()) }
    $tags = Select-String -Path $log -Pattern "\[NOC\]" | Select-Object -Last 40
    foreach ($t in $tags) { Write-Output ("  " + $t.Line.Trim()) }
  }
  exit $code
}
finally {
  if ($acquired) { $mutex.ReleaseMutex() }
  $mutex.Dispose()
}
