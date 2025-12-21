#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Uploads CodeSoupCafe.Maui package to NuGet.org

.DESCRIPTION
    This script uploads the CodeSoupCafe.Maui NuGet package to NuGet.org.
    Requires a valid NuGet API key.

.PARAMETER ApiKey
    NuGet API key for authentication. If not provided, will use NUGET_API_KEY environment variable.

.EXAMPLE
    .\upload-to-nuget.ps1 -ApiKey "your-api-key-here"

.EXAMPLE
    $env:NUGET_API_KEY = "your-api-key-here"
    .\upload-to-nuget.ps1

.NOTES
    Get your API key from: https://www.nuget.org/account/apikeys
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey
)

# Package configuration
$PackagePath = Join-Path $PSScriptRoot "..\LocalNuGetPackages\CodeSoupCafe.Maui.1.0.0.nupkg"
$NuGetSource = "https://api.nuget.org/v3/index.json"

# Get API key from parameter or environment variable
if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    $ApiKey = $env:NUGET_API_KEY
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    Write-Host "ERROR: NuGet API key not provided" -ForegroundColor Red
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\upload-to-nuget.ps1 -ApiKey YOUR_API_KEY"
    Write-Host ""
    Write-Host "Or set environment variable:" -ForegroundColor Yellow
    Write-Host "  `$env:NUGET_API_KEY = 'YOUR_API_KEY'"
    Write-Host "  .\upload-to-nuget.ps1"
    Write-Host ""
    Write-Host "Get your API key from: https://www.nuget.org/account/apikeys" -ForegroundColor Cyan
    exit 1
}

# Verify package exists
if (-not (Test-Path $PackagePath)) {
    Write-Host "ERROR: Package not found at $PackagePath" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Uploading CodeSoupCafe.Maui to NuGet.org" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Package: $PackagePath" -ForegroundColor White
Write-Host "Source: $NuGetSource" -ForegroundColor White
Write-Host ""

# Upload to NuGet
try {
    dotnet nuget push $PackagePath --api-key $ApiKey --source $NuGetSource

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "SUCCESS: Package uploaded to NuGet.org" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "View your package at:" -ForegroundColor Cyan
        Write-Host "https://www.nuget.org/packages/CodeSoupCafe.Maui/" -ForegroundColor Yellow
    } else {
        throw "dotnet nuget push failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "ERROR: Failed to upload package" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
