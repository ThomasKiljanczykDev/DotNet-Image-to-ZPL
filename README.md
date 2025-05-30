# DotNet Image to ZPL Converter

This is a simple .NET console application that converts bitmap images
(from e.g., JPG, PNG) into ZPL (Zebra Programming Language) format.
It includes an option for Z64 encoding.

## Features

- Converts PNG, JPG, and other image formats to ZPL.
- Supports Z64 encoding for compressed ZPL output.
- Resizes images to specified dimensions.
- Easy-to-use console interface.

## Prerequisites

- .NET 8.0 SDK or later (not tested with earlier versions).

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/ThomasKiljanczykDev/DotNet-Bitmap-to-ZPL
cd DotNet-Bitmap-to-ZPL
```

### Build the Project

```bash
dotnet build
```

### Run the Application

```bash
dotnet run --project BitmapToZpl
```

### Example Usage

Place your image file (e.g., `test.png`) in the same directory as the executable and run the application.
The output ZPL will be saved as `output.zpl` in the same directory.

## Command Line Arguments

The application supports the following command-line arguments:

- `--input <file>`: Specifies the input image file to convert. Example:
  ```bash
  dotnet run --project BitmapToZpl -- --input test.png
  ```
- `--output <file>`: Specifies the output ZPL file. If not provided, the default is `output.zpl`. Example:
  ```bash
  dotnet run --project BitmapToZpl -- --input test.png --output custom_output.zpl
  ```
- `--z64`: Enables Z64 encoding for the output ZPL. Example:
  ```bash
  dotnet run --project BitmapToZpl -- --input test.png --z64
  ```
- `--width <pixels>`: Resize the image to this width (in pixels). Optional. Example:
  ```bash
  dotnet run --project BitmapToZpl -- --input test.png --width 300
  ```
- `--height <pixels>`: Resize the image to this height (in pixels). Optional. Example:
  ```bash
  dotnet run --project BitmapToZpl -- --input test.png --height 200
  ```
- `--help`: Displays help information about the command-line options. Example:
  ```bash
  dotnet run --project BitmapToZpl -- --help
  ```

You can combine these arguments to customize the behavior of the application.

## Project Structure

- **BitmapToZpl/**: Contains the main application code.
    - `BitmapToZplConverter.cs`: Core logic for converting images to ZPL.
    - `Crc16Ccitt.cs`: Utility for CRC16 checksum calculation.
    - `Program.cs`: Entry point of the application.
- **bin/**: Compiled binaries.
- **obj/**: Build artifacts.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## Contact

For any questions or issues, please open an issue in the repository.

