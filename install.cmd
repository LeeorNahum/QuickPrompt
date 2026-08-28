@echo off
setlocal
rem Build qp.exe and install it to %USERPROFILE%\.local\bin, on the user PATH.

call "%~dp0build.cmd" || exit /b 1

set "QP_INSTALL_BIN=%USERPROFILE%\.local\bin"
if not exist "%QP_INSTALL_BIN%" mkdir "%QP_INSTALL_BIN%"
copy /y "%~dp0dist\qp.exe" "%QP_INSTALL_BIN%\qp.exe" >nul || exit /b 1

powershell -NoProfile -Command "$ErrorActionPreference = 'Stop'; $bin = $env:QP_INSTALL_BIN; $norm = { param($p) ($p.Trim().Trim([char]34)).TrimEnd([char]92, [char]47) }; $parts = @([string][Environment]::GetEnvironmentVariable('Path', 'User') -split ';' | Where-Object { $_.Trim() -ne '' }); if (@($parts | Where-Object { (& $norm $_) -ieq (& $norm $bin) }).Count -eq 0) { [Environment]::SetEnvironmentVariable('Path', (($parts + $bin) -join ';'), 'User'); Write-Output ('Added ' + $bin + ' to your PATH. Open a new window before using qp.') }" || exit /b 1
echo Installed %QP_INSTALL_BIN%\qp.exe
