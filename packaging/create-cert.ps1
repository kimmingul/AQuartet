# create-cert.ps1
# Creates a self-signed certificate for MSIX signing (development only)
# Must be run as Administrator

param(
    [string]$Subject = "CN=AQuartet",
    [string]$FriendlyName = "A-Quartet Dev Certificate",
    [string]$PfxPassword = "GrokDev123!",
    [string]$PfxOutputPath
)

$ErrorActionPreference = "Stop"

if (-not $PfxOutputPath) {
    $PfxOutputPath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "AQuartet-dev.pfx"
}

# Check admin privileges
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Warning "This script should be run as Administrator to install the certificate to TrustedPeople store."
    Write-Warning "Without admin rights, the certificate will be exported as PFX only."
}

# Check if certificate already exists
$existing = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $Subject }
if ($existing) {
    Write-Host "Certificate already exists: $($existing.Thumbprint)"
    $cert = $existing | Select-Object -First 1
} else {
    Write-Host "Creating self-signed certificate..."
    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $Subject `
        -KeyUsage DigitalSignature `
        -FriendlyName $FriendlyName `
        -CertStoreLocation Cert:\CurrentUser\My `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

    Write-Host "Certificate created: $($cert.Thumbprint)"
}

# Export PFX
$securePassword = ConvertTo-SecureString -String $PfxPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $PfxOutputPath -Password $securePassword | Out-Null
Write-Host "PFX exported: $PfxOutputPath"

# Install to TrustedPeople (requires admin)
if ($isAdmin) {
    $trustedPeople = Get-ChildItem -Path Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Thumbprint -eq $cert.Thumbprint }
    if (-not $trustedPeople) {
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("TrustedPeople", "LocalMachine")
        $store.Open("ReadWrite")
        $store.Add($cert)
        $store.Close()
        Write-Host "Certificate installed to LocalMachine\TrustedPeople (MSIX sideload trust)"
    } else {
        Write-Host "Certificate already in TrustedPeople store."
    }
}

Write-Host ""
Write-Host "=== Next Steps ==="
Write-Host "1. Sign the MSIX:"
Write-Host "   .\sign-msix.ps1 -Thumbprint $($cert.Thumbprint)"
Write-Host ""
Write-Host "2. Or sign with PFX directly:"
Write-Host "   signtool sign /fd SHA256 /a /f `"$PfxOutputPath`" /p `"$PfxPassword`" artifacts\AQuartet.msix"
Write-Host ""
Write-Host "Thumbprint: $($cert.Thumbprint)"
