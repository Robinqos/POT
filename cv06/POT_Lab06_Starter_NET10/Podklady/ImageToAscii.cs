using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;

namespace Communication.HelperLib.Ascii;

public static class ImageToAscii
{
    private const string AsciiChars = "@%#*+=-:. ";

    public static string Convert(byte[] imageBytes, int asciiWidth = 80, int asciiHeight = 40)
    {
        using MemoryStream imageStream = new MemoryStream(imageBytes);
        using (Image<Rgba32> image = Image.Load<Rgba32>(imageStream))
        {
            image.Mutate(x => x.Resize(asciiWidth, asciiHeight));

            StringBuilder result = new StringBuilder();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var rgbPixel = image[x, y];
                    var grayscalePixel = (0.2126f * rgbPixel.R + 0.7152f * rgbPixel.G + 0.0722f * rgbPixel.B) / 255f;
                    var index = (int)(grayscalePixel * (AsciiChars.Length - 1));
                    result.Append(AsciiChars[index]);
                }
                result.AppendLine();
            }

            return result.ToString();
        }
    }
}
