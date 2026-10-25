<#
.SYNOPSIS
    Downloads and extracts the Pinta source tree used as the compile-time
    reference for building this add-in.

.DESCRIPTION
    The add-in is compiled against Pinta's source (like the official
    PintaDemoExtension / FarbfeldFormatAddin samples). This script fetches the
    release tarball from GitHub and extracts it next to the repository, where
    the project's default PintaSourceDir points to.

.PARAMETER Version
    Pinta release tag to fetch, 3.1.2 by default.

.PARAMETER Destination
    Extraction directory. Defaults to ..\Pinta-<Version>-src relative to this
    script.

.EXAMPLE
    pwsh ./scripts/get-pinta-source.ps1
    pwsh ./scripts/get-pinta-source.ps1 -Version 3.2
#>
param(
    [string]$Version = '3.1.2',
    [string]$Destination = (Join-Path $PSScriptRoot "..\Pinta-$Version-src")
)

$ErrorActionPreference = 'Stop'

$project = Join-Path $Destination 'Pinta.Core\Pinta.Core.csproj'
if (Test-Path $project) {
    Write-Host "Pinta $Version source already present: $Destination"
    exit 0
}

$url = "https://codeload.github.com/PintaProject/Pinta/tar.gz/refs/tags/$Version"
$archive = Join-Path ([System.IO.Path]::GetTempPath()) "pinta-$Version.tar.gz"

Write-Host "Downloading Pinta $Version from $url ..."
Invoke-WebRequest -Uri $url -OutFile $archive -UseBasicParsing

New-Item -ItemType Directory -Force -Path $Destination | Out-Null
Write-Host "Extracting to $Destination ..."
tar -xzf $archive -C $Destination --strip-components=1

if (-not (Test-Path $project)) {
    throw "Extraction finished but $project was not found."
}

Remove-Item $archive -ErrorAction SilentlyContinue
Write-Host "Done: $Destination"
