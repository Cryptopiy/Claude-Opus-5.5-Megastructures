<#
.SYNOPSIS
  Samples the video memory of the Nocturne player while it runs (MASTER_PROMPT 2.5.4: under about 6 GB at 4K and 4 GB at
  1080p, measured with nvidia-smi during the runs and logged in QA/). Read-only: it never starts, stops or changes anything.
  Every -IntervalSec: nvidia-smi total memory.used / temperature / utilisation, and the player's own dedicated GPU memory
  from the Windows counter "\GPU Process Memory(pid_<pid>*)\Dedicated Usage" (nvidia-smi reports N/A per process under WDDM).
  Waits up to -WaitStartSec for a process named -Process to appear, samples until it exits (or -TimeoutMin), then writes
  <OutCsv> and <OutCsv>.summary.json (baseline total before the player, peaks, the player's peak dedicated MiB).
.EXAMPLE
  Start this first (background), then the player through Tools/run-player.ps1:
  powershell -NoProfile -ExecutionPolicy Bypass -File Tools\vram-sample.ps1 -OutCsv QA\reports\vram_perf_1080.csv
#>
param(
  [Parameter(Mandatory = $true)][string]$OutCsv,
  [string]$Process = "Nocturne",
  [double]$IntervalSec = 1.0,
  [int]$WaitStartSec = 1800,
  [int]$TimeoutMin = 30
)
$ErrorActionPreference = "Continue"
$root = Split-Path -Parent $PSScriptRoot
if (-not [System.IO.Path]::IsPathRooted($OutCsv)) { $OutCsv = Join-Path $root $OutCsv }
$dir = Split-Path $OutCsv
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
$inv = [System.Globalization.CultureInfo]::InvariantCulture

function Read-Smi {
  $line = & nvidia-smi --query-gpu=memory.used,memory.total,temperature.gpu,utilization.gpu --format=csv,noheader,nounits 2>$null | Select-Object -First 1
  if (-not $line) { return $null }
  $p = $line.Split(',') | ForEach-Object { $_.Trim() }
  return [pscustomobject]@{ used = [double]$p[0]; total = [double]$p[1]; temp = [double]$p[2]; util = [double]$p[3] }
}

function Read-ProcMiB([int]$procId) {
  try {
    $c = Get-Counter -Counter ("\GPU Process Memory(pid_{0}_*)\Dedicated Usage" -f $procId) -ErrorAction Stop
    $sum = 0.0
    foreach ($s in $c.CounterSamples) { $sum += $s.CookedValue }
    return $sum / 1MB
  } catch { return -1.0 }
}

$base = Read-Smi
$t0 = Get-Date
$proc = $null
while (-not $proc -and ((Get-Date) - $t0).TotalSeconds -lt $WaitStartSec) {
  $proc = Get-Process -Name $Process -ErrorAction SilentlyContinue | Select-Object -First 1
  if (-not $proc) { Start-Sleep -Milliseconds 500; $base = Read-Smi }
}
if (-not $proc) { Write-Output "vram-sample: no $Process process within $WaitStartSec s"; exit 2 }
$rows = New-Object System.Collections.Generic.List[string]
$rows.Add("t_s,total_used_mib,player_dedicated_mib,temp_c,util_pct")
$peakTotal = 0.0; $peakProc = 0.0; $n = 0
$start = Get-Date
while (-not $proc.HasExited -and ((Get-Date) - $start).TotalMinutes -lt $TimeoutMin) {
  $smi = Read-Smi
  $pm = Read-ProcMiB $proc.Id
  $t = ((Get-Date) - $start).TotalSeconds
  if ($smi) {
    if ($smi.used -gt $peakTotal) { $peakTotal = $smi.used }
    $rows.Add(([string]::Format($inv, "{0:F1},{1:F0},{2:F1},{3:F0},{4:F0}", $t, $smi.used, $pm, $smi.temp, $smi.util)))
  }
  if ($pm -gt $peakProc) { $peakProc = $pm }
  $n++
  Start-Sleep -Milliseconds ([int]($IntervalSec * 1000))
  $proc.Refresh()
}
[System.IO.File]::WriteAllLines($OutCsv, $rows)
$baseUsed = if ($base) { $base.used } else { -1 }
$summary = [string]::Format($inv, '{{"process": "{0}", "pid": {1}, "samples": {2}, "baselineTotalUsedMiB": {3:F0}, "peakTotalUsedMiB": {4:F0}, "peakTotalMinusBaselineMiB": {5:F0}, "peakPlayerDedicatedMiB": {6:F1}, "csv": "{7}"}}',
  $Process, $proc.Id, $n, $baseUsed, $peakTotal, ($peakTotal - $baseUsed), $peakProc, ($OutCsv -replace '\\', '/'))
[System.IO.File]::WriteAllText($OutCsv + ".summary.json", $summary)
Write-Output ("vram-sample: " + $summary)
