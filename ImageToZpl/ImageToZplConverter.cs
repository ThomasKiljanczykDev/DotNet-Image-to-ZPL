using System.IO.Compression;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageToZpl;

public static class BitmapToZplConverter
{
    private record Z64EncodingResult(
        int TotalBytes,
        int BytesPerRow,
        string Data
    );

    public static async Task<string> ConvertToZplAsync(string filePath, bool useZ64)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Input file not found.", filePath);
        }

        using var image = await Image.LoadAsync(filePath);
        using var grayscaleImage = image.CloneAs<L8>();

        return await ConvertToZplAsync(grayscaleImage, useZ64);
    }

    private static async Task<string> ConvertToZplAsync(Image<L8> image, bool useZ64)
    {
        // Convert the image to a monochrome image
        image.Mutate(ctx => ctx.BinaryThreshold(0.5f));

        // Encode the monochrome image to Z64 format
        var z64Data = await EncodeAsync(image, useZ64);

        var crcEncoder = new Crc16Ccitt();
        var crc = crcEncoder.ComputeChecksum(Encoding.ASCII.GetBytes(z64Data.Data));
        var encoding = useZ64 ? "Z64" : "B64";

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

    /// <summary>
    /// Based on guidelines from
    /// <see href="https://stackoverflow.com/questions/59319970/zpl-binary-b64-and-compressed-z64-encoding" />
    /// </summary>
    private static async Task<Z64EncodingResult> EncodeAsync(Image<L8> image, bool useZ64)
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

    private static byte[] Get1BppBytes(Image<L8> image, int bytesPerRow, int totalBytes)
    {
        var outputData = new byte[totalBytes];

        var pixelData = new byte[image.Width * image.Height];
        image.CopyPixelDataTo(pixelData);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var byteIndex = y * bytesPerRow + x / 8;

                // Pack bits from MSB to LSB
                var bitIndex = 7 - x % 8;

                // Threshold for black
                if (pixelData[y * image.Width + x] < 128)
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