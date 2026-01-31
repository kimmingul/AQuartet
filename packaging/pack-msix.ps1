param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $root "src\AQuartet.App\AQuartet.App.csproj"
$publishDir = Join-Path $root "artifacts\publish"
$stagingDir = Join-Path $PSScriptRoot "Appx"
$outFile = Join-Path $root "artifacts\AQuartet.msix"

Write-Host "Publishing app..."
dotnet publish $appProject -c $Configuration -f net10.0-windows -o $publishDir | Out-Host

Write-Host "Preparing staging directory..."
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $stagingDir | Out-Null

Copy-Item -Path (Join-Path $PSScriptRoot "AppxManifest.xml") -Destination $stagingDir
Copy-Item -Path (Join-Path $PSScriptRoot "Assets") -Destination (Join-Path $stagingDir "Assets") -Recurse
Copy-Item -Path (Join-Path $publishDir "*") -Destination $stagingDir -Recurse

$makeappx = $null
$command = Get-Command makeappx.exe -ErrorAction SilentlyContinue
if ($command) {
    $makeappx = $command.Source
}

if (-not $makeappx) {
    $candidateRoots = @()
    if ($env:ProgramFiles) { $candidateRoots += Join-Path $env:ProgramFiles "Windows Kits\\10\\bin" }
    if ($env:ProgramFiles -and (Test-Path $env:ProgramFiles)) { $candidateRoots += Join-Path $env:ProgramFiles "Windows Kits\\10\\bin" }
    if (${env:ProgramFiles(x86)}) { $candidateRoots += Join-Path ${env:ProgramFiles(x86)} "Windows Kits\\10\\bin" }

    foreach ($root in $candidateRoots | Select-Object -Unique) {
        if (-not (Test-Path $root)) { continue }
        $x64 = Get-ChildItem -Path $root -Recurse -Filter makeappx.exe -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\makeappx\.exe$' } |
            Select-Object -First 1
        if ($x64) {
            $makeappx = $x64.FullName
            break
        }
    }

    if (-not $makeappx) {
        foreach ($root in $candidateRoots | Select-Object -Unique) {
            if (-not (Test-Path $root)) { continue }
            $found = Get-ChildItem -Path $root -Recurse -Filter makeappx.exe -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) {
                $makeappx = $found.FullName
                break
            }
        }
    }
}
if (-not $makeappx) {
    throw "makeappx.exe not found. Install Windows SDK (10.0.17763+)."
}

Write-Host "Packing MSIX..."
& $makeappx pack /d $stagingDir /p $outFile /o | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "makeappx.exe failed with exit code $LASTEXITCODE."
}

Write-Host "MSIX created: $outFile"
Write-Host "Note: Sign the package with a trusted certificate (signtool.exe)."
