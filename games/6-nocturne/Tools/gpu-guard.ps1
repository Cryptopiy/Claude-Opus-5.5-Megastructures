<#
.SYNOPSIS
  Machine-wide guard for heavy GPU jobs (protects the display driver from overload).

.DESCRIPTION
  One RTX 5070 with 12 GB of video memory is shared by several agents and by the user. A display driver
  reset (TDR) is most likely when several GPU-heavy processes run at once or video memory runs out.
  This script makes heavy jobs take turns across ALL projects on the machine:
    1. waits for the machine-wide mutex  Global\MegastructuresLongGpuJob  (one heavy job at a time),
    2. waits until the GPU is cool enough and has enough free video memory (nvidia-smi),
    3. runs the job (a player, a capture, a soak, an encode test) with a time limit,
    4. always releases the mutex, even when the job fails or is killed.

  Use it for every job that renders continuously for more than about 2 minutes (soak, 4K performance runs,
  long frame sequences, encode-test captures, uncapped autotests). Short jobs (compile, build, a few shots)
  do not need it. It does not change any system setting.

.EXAMPLES
  powershell -NoProfile -ExecutionPolicy Bypass -File Tools\gpu-guard.ps1 -Status
  powershell -NoProfile -ExecutionPolicy Bypass -File Tools\gpu-guard.ps1 -Label soak -TimeoutMin 40 `
      -FilePath "C:\path\Builds\Game\Game.exe" -ArgumentString "-soak 1800 -logFile C:\path\Logs\soak.log"

  Pass ALL player arguments as ONE string in -ArgumentString (never as a PowerShell array: -File turns
  arrays into one comma-joined string).

.EXIT CODES
  the job's own exit code; 124 = job killed by the time limit; 75 = GPU busy (too hot or not enough free
  video memory after the wait: retry later or lower the resolution); 76 = could not get the machine-wide
  turn within -MaxTurnWaitMin.
#>
param(
  [switch]$Status,
  [string]$Label = "gpujob",
  [string]$FilePath,
  [string]$ArgumentString = "",
  [string]$WorkingDirectory = "",
  [int]$TimeoutMin = 45,
  [int]$MinFreeVramMiB = 5000,
  [int]$MaxTempC = 83,
  [int]$MaxCoolWaitMin = 15,
  [int]$MaxTurnWaitMin = 120
)

$MutexName = "Global\MegastructuresLongGpuJob"

function Get-GpuState {
  $smi = Get-Command nvidia-smi -ErrorAction SilentlyContinue
  if (-not $smi) { return $null }
  $line = & nvidia-smi --query-gpu=temperature.gpu,utilization.gpu,memory.used,memory.total,power.draw --format=csv,noheader,nounits 2>$null | Select-Object -First 1
  if (-not $line) { return $null }
  $p = $line.Split(",") | ForEach-Object { $_.Trim() }
  [pscustomobject]@{
    TempC = [double]$p[0]; UtilPct = [double]$p[1]; UsedMiB = [double]$p[2]; TotalMiB = [double]$p[3]
    FreeMiB = [double]$p[3] - [double]$p[2]; PowerW = $p[4]
  }
}

function Write-Log([string]$m) { Write-Host ("[gpu-guard {0:HH:mm:ss}] {1}" -f (Get-Date), $m) }

if ($Status) {
  $s = Get-GpuState
  if ($null -eq $s) { Write-Log "nvidia-smi not available"; exit 0 }
  Write-Log ("GPU {0} C, util {1} %, VRAM used {2} of {3} MiB (free {4}), power {5} W" -f $s.TempC, $s.UtilPct, $s.UsedMiB, $s.TotalMiB, $s.FreeMiB, $s.PowerW)
  exit 0
}

if (-not $FilePath) { Write-Log "no -FilePath given"; exit 2 }

$mutex = New-Object System.Threading.Mutex($false, $MutexName)
$owned = $false
try {
  # 1. wait for the machine-wide turn
  $deadline = (Get-Date).AddMinutes($MaxTurnWaitMin)
  while (-not $owned) {
    try { $owned = $mutex.WaitOne(15000) }
    catch [System.Threading.AbandonedMutexException] { $owned = $true; Write-Log "previous holder died; took over the turn" }
    if (-not $owned) {
      if ((Get-Date) -gt $deadline) { Write-Log "no turn within $MaxTurnWaitMin min"; exit 76 }
      Write-Log "[$Label] waiting for another heavy GPU job to finish..."
    }
  }
  Write-Log "[$Label] got the machine-wide turn"

  # 2. wait for a cool GPU with enough free video memory
  $coolDeadline = (Get-Date).AddMinutes($MaxCoolWaitMin)
  while ($true) {
    $s = Get-GpuState
    if ($null -eq $s) { Write-Log "nvidia-smi not available: no thermal or memory check"; break }
    if ($s.TempC -lt $MaxTempC -and $s.FreeMiB -ge $MinFreeVramMiB) {
      Write-Log ("GPU ok: {0} C, free VRAM {1} MiB" -f $s.TempC, $s.FreeMiB); break
    }
    if ((Get-Date) -gt $coolDeadline) {
      Write-Log ("GPU busy: {0} C, free VRAM {1} MiB (need < {2} C and >= {3} MiB); giving up" -f $s.TempC, $s.FreeMiB, $MaxTempC, $MinFreeVramMiB)
      exit 75
    }
    Write-Log ("waiting for the GPU: {0} C, free VRAM {1} MiB" -f $s.TempC, $s.FreeMiB)
    Start-Sleep -Seconds 30
  }

  # 3. run the job with a time limit
  $sp = @{ FilePath = $FilePath; PassThru = $true }
  if ($ArgumentString) { $sp.ArgumentList = $ArgumentString }
  if ($WorkingDirectory) { $sp.WorkingDirectory = $WorkingDirectory }
  Write-Log "[$Label] starting: $FilePath $ArgumentString"
  $p = Start-Process @sp
  if (-not $p.WaitForExit($TimeoutMin * 60 * 1000)) {
    Write-Log "[$Label] time limit of $TimeoutMin min reached: killing the process tree"
    & taskkill /PID $p.Id /T /F | Out-Null
    exit 124
  }
  Write-Log "[$Label] finished with exit code $($p.ExitCode)"
  exit $p.ExitCode
}
finally {
  if ($owned) { try { $mutex.ReleaseMutex() } catch {} }
  $mutex.Dispose()
}
