using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Text;

namespace BitmapToZpl;

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

        using var bitmap = new Bitmap(filePath);
        bitmap.Save("test.png", ImageFormat.Png);
        return await ConvertToZplAsync(bitmap, useZ64);
    }

    private static async Task<string> ConvertToZplAsync(Bitmap bitmap, bool useZ64)
    {
        // Convert the bitmap to a monochrome image
        var monochromeBitmap = ToMonochrome(bitmap);

        // Encode the monochrome bitmap to Z64 format
        var z64Data = await EncodeAsync(monochromeBitmap, useZ64);

        var crcEncoder = new Crc16Ccitt();
        var crc = crcEncoder.ComputeChecksum(Encoding.ASCII.GetBytes(z64Data.Data));
        var encoding = useZ64 ? "Z64" : "B64";

        // Construct the ZPL command
        var zplCommand = new StringBuilder();

        // Start ZPL
        zplCommand.AppendLine("^XA");

        // Set field origin
        zplCommand.AppendLine($"^FO{0},{0}");
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
    private static async Task<Z64EncodingResult> EncodeAsync(Bitmap bitmap, bool useZ64)
    {
        var bmpData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format1bppIndexed
        );

        var bytesPerRow = bmpData.Stride;
        var totalBytes = bytesPerRow * bitmap.Height;
        var rawData = new byte[totalBytes];

        System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rawData, 0, totalBytes);
        bitmap.UnlockBits(bmpData);

        // Invert the colors (0 becomes 1 and 1 becomes 0)
        // Monochrome conversion in this project results in inverted colors that need to be flipped back
        for (var i = 0; i < rawData.Length; i++)
        {
            rawData[i] = (byte)~rawData[i];
        }

        var outputData = rawData;
        if (useZ64)
        {
            outputData = await CompressDataAsync(outputData);
        }

        var encodedOutputData = Convert.ToBase64String(outputData);

        return new Z64EncodingResult(totalBytes, bytesPerRow, encodedOutputData);
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

    private static Bitmap ToMonochrome(Bitmap original)
    {
        return original.Clone(new Rectangle(0, 0, original.Width, original.Height), PixelFormat.Format1bppIndexed);
    }
}