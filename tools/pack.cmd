@echo off
setlocal
for %%I in ("%~dp0..") do set ROOT=%%~fI
set FEED=%ROOT%\artifacts\packages
if not exist "%FEED%" mkdir "%FEED%"
dotnet pack "%ROOT%\src\Arlecchino.Core\Arlecchino.Core.csproj" -c Release || exit /b 1
dotnet pack "%ROOT%\src\Arlecchino\Arlecchino.csproj" -c Release || exit /b 1
dotnet pack "%ROOT%\src\Arlecchino.Testing\Arlecchino.Testing.csproj" -c Release || exit /b 1
echo Packed into %FEED%
