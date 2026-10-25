<#
.SYNOPSIS
    Builds the PintaNumberBadges add-in and optionally packs it into a .mpack.

.DESCRIPTION
    Requires the .NET 8 SDK and a Pinta 3.1.x source checkout (used as the
    compile-time reference, exactly like the official Pinta add-in samples).

.PARAMETER PintaSourceDir
    Path to a Pinta source tree. Defaults to ..\Pinta-3.1.2-src next to this script.

.PARAMETER Configuration
    Build configuration, Release by default.

.PARAMETER Pack
    Also create package\PintaNumberBadges.<version>.mpack via Mono.Addins' mautil.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -PintaSourceDir C:\src\Pinta-3.1.2-src -Pack
#>
param(
    [string]$PintaSourceDir = (Join-Path $PSScriptRoot '..\Pinta-3.1.2-src'),
    [string]$Configuration = 'Release',
    [switch]$Pack
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'PintaNumberBadges.csproj'

if (-not (Test-Path (Join-Path $PintaSourceDir 'Pinta.Core\Pinta.Core.csproj'))) {
    throw "找不到 Pinta 源码：$PintaSourceDir。请先运行 .\scripts\get-pinta-source.ps1 下载 Pinta 3.1.2 源码，或用 -PintaSourceDir 指定路径。"
}

Write-Host "Pinta source: $PintaSourceDir" -ForegroundColor Cyan
dotnet build $project -c $Configuration "-p:PintaSourceDir=$PintaSourceDir"
if ($LASTEXITCODE -ne 0) { throw "dotnet build 失败" }

$dll = Join-Path $PSScriptRoot "bin\$Configuration\net8.0\PintaNumberBadges.dll"
Write-Host "生成: $dll" -ForegroundColor Green

if ($Pack) {
    if (-not (Get-Command mautil -ErrorAction SilentlyContinue)) {
        Write-Host "安装 Mono.Addins.UtilTool ..." -ForegroundColor Cyan
        dotnet tool install --global Mono.Addins.UtilTool
    }

    $stage = Join-Path $PSScriptRoot 'package'
    New-Item -ItemType Directory -Force -Path $stage | Out-Null
    Get-ChildItem $stage -Filter '*.mpack' | Remove-Item -Force
    Copy-Item $dll $stage -Force

    Push-Location $stage
    try {
        mautil pack (Split-Path $dll -Leaf)
        if ($LASTEXITCODE -ne 0) { throw "mautil pack 失败" }
    } finally {
        Remove-Item (Join-Path $stage (Split-Path $dll -Leaf)) -ErrorAction SilentlyContinue
        Pop-Location
    }

    Get-ChildItem $stage -Filter '*.mpack' | ForEach-Object {
        Write-Host "安装包: $($_.FullName)" -ForegroundColor Green
    }
}

