using Avalonia.Media.Imaging;
using SkiaSharp;
using StarfieldWeaponIconFileBuilder.Extensions;
using Svg.Skia;
using System;
using System.IO;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static class SvgRenderer
{
    #region Methods

    /// <summary>
    /// Renders the passed SVG file path to a Bitmap with the passed dimensions, maintaining aspect ratio.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    public static Bitmap? Render(string path, int targetWidth, int targetHeight)
    {
        if (!path.PathExists().Exist)
            return null;

        try
        {
            var svg = new SKSvg();
            svg.Load(path);

            var picture = svg.Picture;
            if (picture.IsNullOrEmpty())
                return null;

            var rect = picture.CullRect;

            if (rect.Width <= 0 || rect.Height <= 0)
                rect = new SKRect(0, 0, 256, 256);

            float scale = System.Math.Min(targetWidth / rect.Width, targetHeight / rect.Height);
            int width = (int)(rect.Width * scale);
            int height = (int)(rect.Height * scale);

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.Transparent);
            canvas.Scale(scale);
            canvas.DrawPicture(picture);
            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new MemoryStream(data.ToArray());

            return new Bitmap(stream);
        }
        catch (Exception Ex)
        {
            Logging.Exception(new Exception($"Exception attempting to load SVG: {path}", Ex));
        }

        return null;
    }

    #endregion
}
