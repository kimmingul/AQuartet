# download-logos.ps1
# Downloads AI service favicons for sidebar logos

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$logosDir = Join-Path $scriptDir "Logos"

if (-not (Test-Path $logosDir)) { New-Item -ItemType Directory -Path $logosDir | Out-Null }

$services = @(
    @{ Name = "grok";       Url = "https://www.google.com/s2/favicons?domain=grok.com&sz=64" },
    @{ Name = "claude";     Url = "https://www.google.com/s2/favicons?domain=claude.ai&sz=64" },
    @{ Name = "chatgpt";    Url = "https://www.google.com/s2/favicons?domain=chatgpt.com&sz=64" },
    @{ Name = "gemini";     Url = "https://www.google.com/s2/favicons?domain=gemini.google.com&sz=64" },
    @{ Name = "perplexity"; Url = "https://www.google.com/s2/favicons?domain=perplexity.ai&sz=64" },
    @{ Name = "copilot";    Url = "https://www.google.com/s2/favicons?domain=copilot.microsoft.com&sz=64" }
)

foreach ($svc in $services) {
    $outPath = Join-Path $logosDir "$($svc.Name).png"
    Write-Host "Downloading $($svc.Name) logo..."
    try {
        Invoke-WebRequest -Uri $svc.Url -OutFile $outPath -UseBasicParsing
        Write-Host "  Saved: $outPath"
    } catch {
        Write-Host "  FAILED: $_"
    }
}

Write-Host "`nAll logos downloaded."
