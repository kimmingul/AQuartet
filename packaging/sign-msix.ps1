param(
    [string]$Thumbprint,
    [string]$PfxFile,
    [string]$PfxPassword,
    [string]$TimestampUrl = "http://timestamp.digicert.com",
    [string]$Csp,
    [string]$KeyContainer
)

$ErrorActionPreference = "Stop"

if (-not $Thumbprint -and -not $PfxFile) {
    throw "Either -Thumbprint or -PfxFile must be specified."
}

$root = Split-Path -Parent $PSScriptRoot
$msixPath = Join-Path $root "artifacts\AQuartet.msix"

if (-not (Test-Path $msixPath)) {
    throw "MSIX not found: $msixPath"
}

# Find signtool.exe
$signtool = $null
$candidateRoots = @()
if ($env:ProgramFiles) { $candidateRoots += Join-Path $env:ProgramFiles "Windows Kits\10\bin" }
if (${env:ProgramFiles(x86)}) { $candidateRoots += Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin" }

foreach ($rootPath in $candidateRoots | Select-Object -Unique) {
    if (-not (Test-Path $rootPath)) { continue }
    $x64 = Get-ChildItem -Path $rootPath -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\signtool\.exe$' } |
        Select-Object -First 1
    if ($x64) {
        $signtool = $x64.FullName
        break
    }
}

if (-not $signtool) {
    $command = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($command) {
        $signtool = $command.Source
    }
}

if (-not $signtool) {
    throw "signtool.exe not found. Install Windows SDK (10.0.17763+)."
}

Write-Host "Using signtool: $signtool"

# Build sign arguments
$signArgs = @("sign", "/fd", "SHA256")

if ($PfxFile) {
    if (-not (Test-Path $PfxFile)) { throw "PFX file not found: $PfxFile" }
    Write-Host "Signing MSIX with PFX: $PfxFile"
    $signArgs += @("/f", $PfxFile)
    if ($PfxPassword) { $signArgs += @("/p", $PfxPassword) }
} else {
    Write-Host "Signing MSIX with thumbprint: $Thumbprint"
    $signArgs += @("/sha1", $Thumbprint)
    $signArgs += @("/tr", $TimestampUrl, "/td", "SHA256")
    if ($Csp) { $signArgs += @("/csp", $Csp) }
    if ($KeyContainer) { $signArgs += @("/kc", $KeyContainer) }
}

$signArgs += $msixPath

& $signtool @signArgs | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "signtool.exe sign failed with exit code $LASTEXITCODE."
}

Write-Host "Verifying signature..."
& $signtool verify /pa /v $msixPath | Out-Host
if ($LASTEXITCODE -ne 0) {
    Write-Warning "signtool verify returned non-zero (self-signed certs may not pass chain validation)."
}

Write-Host "Signed MSIX: $msixPath"
