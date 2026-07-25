@echo off
setlocal
set FEED=%~dp0artifacts\packages
if not exist "%FEED%" mkdir "%FEED%"
dotnet pack "%~dp0src\Arlecchino.Core\Arlecchino.Core.csproj" -c Release || exit /b 1
dotnet pack "%~dp0src\Arlecchino\Arlecchino.csproj" -c Release || exit /b 1
dotnet pack "%~dp0src\Arlecchino.Testing\Arlecchino.Testing.csproj" -c Release || exit /b 1
echo Packed into %FEED%
