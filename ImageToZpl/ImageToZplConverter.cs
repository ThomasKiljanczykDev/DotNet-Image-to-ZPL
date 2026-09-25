using System.IO.Compression;
using System.Text;
using SkiaSharp;

namespace ImageToZpl;

public static class BitmapToZplConverter
{
    public class ConvertToZplArguments
    {
        public bool UseZ64 { get; init; } = true;
        public int? Width { get; init; }
        public int? Height { get; init; }
    }

    private record Z64EncodingResult(int TotalBytes, int BytesPerRow, string Data);

    public static async Task<string> ConvertToZplAsync(string filePath, ConvertToZplArguments args)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Input file not found.", filePath);
        }

        var fileData = await File.ReadAllBytesAsync(filePath);
        using var image =
            SKBitmap.Decode(fileData)
            ?? throw new InvalidDataException($"Unsupported or corrupt image file: {filePath}");
        using var grayscaleImage =
            image.Copy(SKColorType.Gray8)
            ?? throw new InvalidOperationException("Failed to convert image to grayscale.");

        return await ConvertToZplAsync(grayscaleImage, args);
    }

    private static async Task<string> ConvertToZplAsync(
        SKBitmap grayscaleImage,
        ConvertToZplArguments args
    )
    {
        // Convert the image to a monochrome image
        ApplyBinaryThreshold(grayscaleImage);

        using var resizedImage = Resize(grayscaleImage, args);
        var image = resizedImage ?? grayscaleImage;

        // Encode the monochrome image to Z64 format
        var z64Data = await EncodeAsync(image, args.UseZ64);

        var crcEncoder = new Crc16Ccitt();
        var crc = crcEncoder.ComputeChecksum(Encoding.ASCII.GetBytes(z64Data.Data));
        var encoding = args.UseZ64 ? "Z64" : "B64";

        // Construct the ZPL command
        var zplCommand = new StringBuilder();

        // Start ZPL
        zplCommand.AppendLine("^XA");

        // Set field origin
        zplCommand.AppendLine("^FO0,0");
        zplCommand.AppendLine(
            $"^GFA,{z64Data.TotalBytes},{z64Data.TotalBytes},{z64Data.BytesPerRow},:{encoding}:{z64Data.Data}:{crc:X}"
        );

        // End ZPL
        zplCommand.AppendLine("^XZ");

        return zplCommand.ToString();
    }

    private static void ApplyBinaryThreshold(SKBitmap image)
    {
        var pixels = image.GetPixelSpan();
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i] < 128 ? byte.MinValue : byte.MaxValue;
        }
    }

    private static SKBitmap? Resize(SKBitmap image, ConvertToZplArguments args)
    {
        if (!args.Width.HasValue && !args.Height.HasValue)
        {
            return null;
        }

        var width = args.Width;
        var height = args.Height;

        if (!width.HasValue && height.HasValue)
        {
            var scaleFactor = height.Value / (double)image.Height;
            width = (int)Math.Floor(image.Width * scaleFactor);
        }
        else if (!height.HasValue && width.HasValue)
        {
            var scaleFactor = width.Value / (double)image.Width;
            height = (int)Math.Floor(image.Height * scaleFactor);
        }

        var resizedInfo = new SKImageInfo(
            width!.Value,
            height!.Value,
            SKColorType.Gray8,
            SKAlphaType.Opaque
        );
        return image.Resize(resizedInfo, new SKSamplingOptions(SKCubicResampler.CatmullRom))
            ?? throw new InvalidOperationException("Failed to resize image.");
    }

    /// <summary>
    /// Based on guidelines from
    /// <see href="https://stackoverflow.com/questions/59319970/zpl-binary-b64-and-compressed-z64-encoding" />
    /// </summary>
    private static async Task<Z64EncodingResult> EncodeAsync(SKBitmap image, bool useZ64)
    {
        var bytesPerRow = (image.Width + 7) / 8; // Each row is padded to the nearest byte
        var totalBytes = bytesPerRow * image.Height;

        var outputData = Get1BppBytes(image, bytesPerRow, totalBytes);

        if (useZ64)
        {
            outputData = await CompressDataAsync(outputData);
        }

        var encodedOutputData = Convert.ToBase64String(outputData);

        return new Z64EncodingResult(totalBytes, bytesPerRow, encodedOutputData);
    }

    private static byte[] Get1BppBytes(SKBitmap image, int bytesPerRow, int totalBytes)
    {
        var outputData = new byte[totalBytes];

        var pixelData = image.GetPixelSpan();
        var sourceRowBytes = image.RowBytes;

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var byteIndex = y * bytesPerRow + x / 8;

                // Pack bits from MSB to LSB
                var bitIndex = 7 - x % 8;

                // Threshold for black
                if (pixelData[y * sourceRowBytes + x] < 128)
                {
                    outputData[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
        }

        return outputData;
    }

    private static async Task<byte[]> CompressDataAsync(byte[] data)
    {
        await using var memoryStream = new MemoryStream();
        await using (var zlibStream = new ZLibStream(memoryStream, CompressionLevel.SmallestSize))
        {
            await zlibStream.WriteAsync(data);
        }

        return memoryStream.ToArray();
    }
}
