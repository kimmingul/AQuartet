using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace IconGen;

class Program
{
    static void Main(string[] args)
    {
        var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../AQuartet.App/Assets/Logos"));
        if (!Directory.Exists(baseDir))
        {
            Console.WriteLine($"Directory not found: {baseDir}");
            return;
        }

        Console.WriteLine($"Processing icons in: {baseDir}");
        var files = Directory.GetFiles(baseDir, "*.png");

        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            if (filename.Count(c => c == '.') > 1)
                continue;

            Console.WriteLine($"Processing {filename}...");

            using var original = new Bitmap(file);
            var avgBrightness = GetAverageBrightness(original);
            Console.WriteLine($"  Brightness: {avgBrightness:F2}");

            var lightPath = Path.Combine(baseDir, Path.GetFileNameWithoutExtension(file) + ".light.png");
            if (avgBrightness > 0.8f)
            {
                using var lightBmp = Invert(original);
                lightBmp.Save(lightPath, ImageFormat.Png);
            }
            else
            {
                original.Save(lightPath, ImageFormat.Png);
            }

            var darkPath = Path.Combine(baseDir, Path.GetFileNameWithoutExtension(file) + ".dark.png");
            if (avgBrightness < 0.2f)
            {
                using var darkBmp = Invert(original);
                darkBmp.Save(darkPath, ImageFormat.Png);
            }
            else
            {
                original.Save(darkPath, ImageFormat.Png);
            }

            var grayPath = Path.Combine(baseDir, Path.GetFileNameWithoutExtension(file) + ".gray.png");
            using var grayBmp = ToGrayscale(original);
            grayBmp.Save(grayPath, ImageFormat.Png);
        }
    }

    static float GetAverageBrightness(Bitmap bmp)
    {
        long total = 0;
        int count = 0;
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                var p = bmp.GetPixel(x, y);
                if (p.A < 10) continue;
                total += (long)(p.R * 0.299 + p.G * 0.587 + p.B * 0.114);
                count++;
            }
        }
        if (count == 0) return 0;
        return (total / (float)count) / 255.0f;
    }

    static Bitmap Invert(Bitmap original)
    {
        var newBmp = new Bitmap(original.Width, original.Height);
        using (var g = Graphics.FromImage(newBmp))
        {
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {1, 1, 1, 0, 1}
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        }
        return newBmp;
    }

    static Bitmap ToGrayscale(Bitmap original)
    {
        var newBmp = new Bitmap(original.Width, original.Height);
        using (var g = Graphics.FromImage(newBmp))
        {
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        }
        return newBmp;
    }
}
