@echo off
rem Nocturne - double-click this file to start the game on Windows.
if exist "%~dp0Game\Windows\Nocturne.exe" (
  start "" "%~dp0Game\Windows\Nocturne.exe" %*
) else if exist "%~dp0Builds\Nocturne\Nocturne.exe" (
  start "" "%~dp0Builds\Nocturne\Nocturne.exe" %*
) else (
  echo Nocturne.exe not found. Download the whole folder, including Game\Windows.
  pause
)
