@echo off
rem Bulwark - double-click this file to start the game on Windows.
if exist "%~dp0Game\Windows\Bulwark.exe" (
  start "" "%~dp0Game\Windows\Bulwark.exe" %*
) else if exist "%~dp0Builds\Bulwark\Bulwark.exe" (
  start "" "%~dp0Builds\Bulwark\Bulwark.exe" %*
) else (
  echo Bulwark.exe not found. Download the whole folder, including Game\Windows.
  pause
)
