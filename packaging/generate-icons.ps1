# generate-icons.ps1
# A-Quartet icon generation script
# Dark background + white "G" text style

Add-Type -AssemblyName System.Drawing

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$assetsDir = Join-Path $scriptDir "Assets"
$appDir = Join-Path $scriptDir "..\src\AQuartet.App"

function New-Icon {
    param(
        [int]$Width,
        [int]$Height,
        [string]$OutputPath,
        [string]$Text = "G",
        [float]$FontSizeRatio = 0.55
    )

    $bmp = New-Object System.Drawing.Bitmap($Width, $Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

    $g.Clear([System.Drawing.Color]::FromArgb(24, 24, 24))

    $fontSize = [float]([Math]::Max(8, [int]($Height * $FontSizeRatio)))
    $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)
    $brush = [System.Drawing.Brushes]::White
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center

    $rect = New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)
    $g.DrawString($Text, $font, $brush, $rect, $sf)

    $bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    $font.Dispose()
    $sf.Dispose()
    $g.Dispose()
    $bmp.Dispose()

    Write-Host "Created: $OutputPath ($Width x $Height)"
}

function New-IcoFile {
    param(
        [string]$OutputPath,
        [int[]]$Sizes = @(16, 32, 48, 256)
    )

    # Generate PNG byte arrays for each size
    $pngDataList = [System.Collections.ArrayList]::new()

    foreach ($size in $Sizes) {
        $bmp = New-Object System.Drawing.Bitmap($size, $size)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $g.Clear([System.Drawing.Color]::FromArgb(24, 24, 24))

        $fontSize = [float]([Math]::Max(8, [int]($size * 0.55)))
        $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)
        $brush = [System.Drawing.Brushes]::White
        $sf = New-Object System.Drawing.StringFormat
        $sf.Alignment = [System.Drawing.StringAlignment]::Center
        $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
        $rect = New-Object System.Drawing.RectangleF(0, 0, $size, $size)
        $g.DrawString("G", $font, $brush, $rect, $sf)

        $pngMs = New-Object System.IO.MemoryStream
        $bmp.Save($pngMs, [System.Drawing.Imaging.ImageFormat]::Png)
        $null = $pngDataList.Add($pngMs.ToArray())

        $font.Dispose()
        $sf.Dispose()
        $g.Dispose()
        $bmp.Dispose()
        $pngMs.Dispose()
    }

    # Build ICO file manually
    $ms = New-Object System.IO.MemoryStream
    $writer = New-Object System.IO.BinaryWriter($ms)

    # ICO header
    $writer.Write([uint16]0)                # Reserved
    $writer.Write([uint16]1)                # Type: ICO
    $writer.Write([uint16]$Sizes.Count)     # Image count

    # Calculate data offset
    $headerSize = 6
    $entrySize = 16
    $dataOffset = $headerSize + ($entrySize * $Sizes.Count)

    # Write directory entries
    for ($i = 0; $i -lt $Sizes.Count; $i++) {
        $size = $Sizes[$i]
        $pngBytes = $pngDataList[$i]

        $w = if ($size -ge 256) { [byte]0 } else { [byte]$size }
        $h = if ($size -ge 256) { [byte]0 } else { [byte]$size }

        $writer.Write($w)                         # Width
        $writer.Write($h)                         # Height
        $writer.Write([byte]0)                    # Color palette
        $writer.Write([byte]0)                    # Reserved
        $writer.Write([uint16]1)                  # Color planes
        $writer.Write([uint16]32)                 # Bits per pixel
        $writer.Write([uint32]$pngBytes.Length)   # Data size
        $writer.Write([uint32]$dataOffset)        # Data offset

        $dataOffset += $pngBytes.Length
    }

    # Write PNG data
    for ($i = 0; $i -lt $pngDataList.Count; $i++) {
        $writer.Write([byte[]]$pngDataList[$i])
    }

    [System.IO.File]::WriteAllBytes($OutputPath, $ms.ToArray())

    $writer.Dispose()
    $ms.Dispose()

    Write-Host "Created: $OutputPath (ICO, $($Sizes.Count) sizes)"
}

# Ensure directories exist
if (-not (Test-Path $assetsDir)) { New-Item -ItemType Directory -Path $assetsDir | Out-Null }

# Generate MSIX PNGs
New-Icon -Width 44 -Height 44 -OutputPath (Join-Path $assetsDir "Square44x44Logo.png")
New-Icon -Width 150 -Height 150 -OutputPath (Join-Path $assetsDir "Square150x150Logo.png")
New-Icon -Width 310 -Height 310 -OutputPath (Join-Path $assetsDir "Square310x310Logo.png")
New-Icon -Width 310 -Height 150 -OutputPath (Join-Path $assetsDir "Wide310x150Logo.png")
New-Icon -Width 50 -Height 50 -OutputPath (Join-Path $assetsDir "StoreLogo.png")

# Generate app icon (.ico)
New-IcoFile -OutputPath (Join-Path $appDir "app.ico")

Write-Host "`nAll icons generated successfully."
