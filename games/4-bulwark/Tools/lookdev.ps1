# Look-development loop (imported from Abyss): build the player into its own folder, capture shots with it, compute
# metrics and a contact sheet. Safe for parallel agents: Unity builds are serialised by unity-queue.ps1's mutex, the
# capture goes through run-player.ps1 (which waits for any other Bulwark.exe and sends long or 4K jobs through
# gpu-guard.ps1), and each agent uses its own -Slot (its own build folder, lesson 3.2 #10).
#
#   powershell -NoProfile -File Tools\lookdev.ps1 -Slot crest -Run r01 -Shots keyshots             (build + capture + metrics)
#   powershell -NoProfile -File Tools\lookdev.ps1 -Slot crest -Run r02 -Shots KS02,KS03 -NoBuild   (reuse the last build)
#   extra player args (one string, never split on commas): -Extra "-dither off -tier-override 1"
#
# -Shots: a capture set name (keyshots, beats, parity, stills, sequences, transitions, statics, pans, edges, encode, all,
#         everything; becomes -capture-set) or a comma list of shot ids (becomes -capture-only, passed as ONE argument).
# Outputs: Builds/lookdev_<Slot>/Bulwark.exe, QA/captures/<Slot>_<Run>/*.png + report.json,
#          QA/reports/image_metrics_<Slot>_<Run>.json, QA/captures/<Slot>_<Run>/_contact/*.jpg.
# Exit code: 0 when the capture ran, 2 when the build failed, else the run-player.ps1 exit code.
param(
    [Parameter(Mandatory)][string]$Slot,
    [Parameter(Mandatory)][string]$Run,
    [string]$Shots = "keyshots",
    [string]$Extra = "",
    [switch]$NoBuild,
    [switch]$Long,
    [int]$TimeoutMin = 30
)
$ErrorActionPreference = 'Continue'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$buildDir = "Builds\lookdev_$Slot"
$exe = Join-Path $root "$buildDir\Bulwark.exe"
if (-not $NoBuild) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'unity-queue.ps1') -Name "lookdev_$Slot" -Method Megastructure.EditorTools.BatchTools.BuildWindows -BuildDir $buildDir | Select-Object -Last 3
    if ($LASTEXITCODE -ne 0) { Write-Output "BUILD FAILED (see Logs\lookdev_$Slot.log)"; exit 2 }
}
if (-not (Test-Path $exe)) { Write-Output "no exe at $exe"; exit 2 }

$name = "${Slot}_$Run"
$out = Join-Path $root "QA\captures\$name"
$sets = @("keyshots", "beats", "parity", "stills", "sequences", "transitions", "statics", "pans", "edges", "encode", "all", "everything")
if ($sets -contains $Shots.ToLowerInvariant()) { $shotArgs = "-capture-set $Shots" } else { $shotArgs = "-capture-only $Shots" }
$argString = "-capture `"$out`" $shotArgs"
if ($Extra -ne "") { $argString += " " + $Extra }
$rp = @{ Name = "lookdev_$name"; ArgString = $argString; BuildDir = $buildDir; TimeoutMin = $TimeoutMin }
if ($Long) { $rp.Long = $true }
& (Join-Path $PSScriptRoot 'run-player.ps1') @rp | Select-Object -Last 12
$code = $LASTEXITCODE
Write-Output "capture exit $code -> $out"
if (-not (Test-Path $out)) { exit $code }
& python -I (Join-Path $root 'Tools\qa\image_metrics.py') $out --run $name | Select-String -Pattern 'PASS|FAIL|image_metrics:' | ForEach-Object { $_.Line }
& python -I (Join-Path $root 'Tools\qa\contact_sheet.py') $out --run $name | Select-Object -Last 3
exit $code
