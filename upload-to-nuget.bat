@echo off
REM Batch file to upload CodeSoupCafe.Maui to NuGet.org
REM Usage: upload-to-nuget.bat [API_KEY]
REM   Or set NUGET_API_KEY environment variable before running

setlocal

REM Package configuration
set PACKAGE_PATH=..\LocalNuGetPackages\CodeSoupCafe.Maui.1.0.4.nupkg
set NUGET_SOURCE=https://api.nuget.org/v3/index.json

REM Get API key from parameter or environment variable
if not "%~1"=="" (
    set API_KEY=%~1
) else if not "%NUGET_API_KEY%"=="" (
    set API_KEY=%NUGET_API_KEY%
) else (
    echo ERROR: NuGet API key not provided
    echo.
    echo Usage:
    echo   upload-to-nuget.bat YOUR_API_KEY
    echo.
    echo Or set environment variable:
    echo   set NUGET_API_KEY=YOUR_API_KEY
    echo   upload-to-nuget.bat
    echo.
    echo Get your API key from: https://www.nuget.org/account/apikeys
    exit /b 1
)

REM Verify package exists
if not exist "%PACKAGE_PATH%" (
    echo ERROR: Package not found at %PACKAGE_PATH%
    exit /b 1
)

echo ========================================
echo Uploading CodeSoupCafe.Maui to NuGet.org
echo ========================================
echo Package: %PACKAGE_PATH%
echo Source: %NUGET_SOURCE%
echo.

REM Upload to NuGet
dotnet nuget push "%PACKAGE_PATH%" --api-key %API_KEY% --source %NUGET_SOURCE%

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================
    echo SUCCESS: Package uploaded to NuGet.org
    echo ========================================
    echo.
    echo View your package at:
    echo https://www.nuget.org/packages/CodeSoupCafe.Maui/
) else (
    echo.
    echo ========================================
    echo ERROR: Failed to upload package
    echo ========================================
    exit /b %ERRORLEVEL%
)

endlocal
