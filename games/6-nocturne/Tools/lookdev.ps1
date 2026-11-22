# Look-development loop (from Abyss Tools/lookdev.ps1, adapted for Nocturne, IMPORT_PLAN 4.1 item 1.9):
# build the player into its own folder, capture shots with it through Tools/run-player.ps1 (one player at a time,
# gpu-guard for heavy jobs; never start the exe directly), then compute image metrics and a contact sheet.
# Safe for parallel agents: Unity builds are serialised by unity-queue.ps1's mutex; each agent uses its own -Slot,
# so builds never overwrite each other (NOC_BUILD_DIR, lesson 10).
#
#   powershell -NoProfile -File Tools\lookdev.ps1 -Slot facade -Run r01 -CaptureSet keyshots
#   powershell -NoProfile -File Tools\lookdev.ps1 -Slot facade -Run r02 -CaptureSet beats -NoBuild
#   one value per call for -capture-only (lesson 1):  -CaptureOnly KS04
#   extra player args (comma separated, split by run-player): -Extra "-rain,0,-freeze-clock"
#
# Outputs: Builds/lookdev_<Slot>/Nocturne.exe, QA/captures/<Slot>_<Run>/*.png + report.json, metrics and a contact sheet.
param(
    [Parameter(Mandatory)][string]$Slot,
    [Parameter(Mandatory)][string]$Run,
    [string]$CaptureSet = "keyshots",
    [string]$CaptureOnly = "",
    [string]$Extra = "",
    [switch]$NoBuild,
    [string]$BuildDir = "",   # e.g. Builds\BV (default Builds\lookdev_<Slot>)
    [int]$TimeoutMin = 20
)
$ErrorActionPreference = 'Continue'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$buildDir = if ($BuildDir -ne "") { $BuildDir } else { "Builds\lookdev_$Slot" }
$exe = Join-Path $root "$buildDir\Nocturne.exe"
if (-not $NoBuild) {
    $env:NOC_BUILD_DIR = $buildDir
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'unity-queue.ps1') -Name "lookdev_$Slot" -Method Megastructure.EditorTools.BatchTools.BuildWindows | Select-Object -Last 5
    $code = $LASTEXITCODE
    Remove-Item Env:\NOC_BUILD_DIR -ErrorAction SilentlyContinue
    if ($code -ne 0) { Write-Output "[NOC-FAIL] lookdev build failed (see Logs\lookdev_$Slot.log)"; exit 2 }
}
if (-not (Test-Path $exe)) { Write-Output "[NOC-FAIL] no exe at $exe"; exit 2 }
$name = "${Slot}_$Run"
$out = "QA\captures\$name"
$pargs = "-capture,$out,-capture-set,$CaptureSet"
if ($CaptureOnly -ne "") { $pargs += ",-capture-only,$CaptureOnly" }
if ($Extra -ne "") { $pargs += ",$Extra" }
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-player.ps1') -Name "lookdev_$name" -Exe "$buildDir\Nocturne.exe" -TimeoutMin $TimeoutMin -PlayerArgs $pargs
Write-Output "capture exit $LASTEXITCODE -> $out"
& python -I (Join-Path $root 'Tools\qa\image_metrics.py') (Join-Path $root $out) | Select-String -Pattern 'PASS|FAIL|image_metrics:' | ForEach-Object { $_.Line }
& python -I (Join-Path $root 'Tools\qa\contact_sheet.py') (Join-Path $root $out) | Select-Object -Last 3
exit 0
