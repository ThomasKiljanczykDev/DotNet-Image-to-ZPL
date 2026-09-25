# DotNet Image to ZPL Converter

.NET console app that converts images to ZPL (Zebra Programming Language) as a `^GFA` graphic field with Z64 (zlib-compressed) encoding.

- Input: PNG, JPEG, GIF, BMP, WebP, ICO, WBMP.
- Output: black and white, 50% luminance threshold.
- Optional resize; aspect ratio preserved when only width or height is given.

## Requirements

- .NET 10 SDK.
- Windows, macOS, or Linux (glibc or musl, x64 or ARM64). SkiaSharp native binaries are bundled; no system packages needed.

## Usage

```bash
git clone https://github.com/ThomasKiljanczykDev/DotNet-Image-to-ZPL
cd DotNet-Image-to-ZPL
dotnet run --project ImageToZpl -- --input test.png --width 300
```

Relative paths resolve against the current working directory.

## Options

| Option | Default | Description |
|---|---|---|
| `-i`, `--input <file>` | required | Input image. |
| `-o`, `--output <file>` | `output.zpl` | Output ZPL file. |
| `-w`, `--width <px>` | | Target width. |
| `-h`, `--height <px>` | | Target height. |
| `-z`, `--z64` | on | Z64 encoding. Cannot currently be disabled. |
| `--help` | | Show help. |

## Project Structure

- `ImageToZpl/ImageToZplConverter.cs`: image to ZPL conversion.
- `ImageToZpl/Crc16Ccitt.cs`: CRC16 checksum.
- `ImageToZpl/Program.cs`: entry point, argument parsing.
- `global.json`: pins .NET SDK 10.0 (rolls forward to latest minor).

## License

MIT. See `LICENSE`.

Issues and pull requests welcome.
