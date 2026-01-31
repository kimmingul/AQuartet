# Converts logo PNGs to themed variants using ImageMagick
# Requires: ImageMagick installed and 'magick' in PATH
# Install: winget install ImageMagick.ImageMagick

$logosDir = $PSScriptRoot
$services = @("chatgpt", "claude", "copilot", "gemini", "grok", "perplexity")

foreach ($svc in $services) {
    $src = Join-Path $logosDir "$svc.png"
    if (-not (Test-Path $src)) {
        Write-Warning "Source not found: $src"
        continue
    }

    # Light theme: slightly increase brightness for light backgrounds
    $light = Join-Path $logosDir "$svc.light.png"
    magick $src -modulate 110,100,100 $light
    Write-Host "Created: $light"

    # Dark theme: increase brightness/contrast for dark backgrounds
    $dark = Join-Path $logosDir "$svc.dark.png"
    magick $src -modulate 120,100,100 -brightness-contrast 10x5 $dark
    Write-Host "Created: $dark"

    # Gray theme: desaturate to grayscale
    $gray = Join-Path $logosDir "$svc.gray.png"
    magick $src -colorspace Gray $gray
    Write-Host "Created: $gray"
}

Write-Host "`nDone. Created themed logo variants."
