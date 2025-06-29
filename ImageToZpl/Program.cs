﻿using CommandLine;

namespace ImageToZpl;

public static class Program
{
    public class CommandLineOptions
    {
        [Option('z', "z64", Required = false, HelpText = "Compress the image data using Z64 encoding.", Default = true)]
        public bool UseZ64 { get; init; } = true;

        [Option(
            'i',
            "input",
            Required = true,
            HelpText =
                "Input file path. Supports formats compatible with ImageSharp (https://docs.sixlabors.com/articles/imagesharp/imageformats.html)."
        )]
        public required string InputFilePath { get; init; }

        [Option('o', "output", Required = false, HelpText = "Output file path.", Default = "output.zpl")]
        public string OutputFilePath { get; init; } = "output.zpl";

        [Option('w', "width", Required = false, HelpText = "Resize image to this width (pixels).", Default = null)]
        public int? Width { get; init; }

        [Option('h', "height", Required = false, HelpText = "Resize image to this height (pixels).", Default = null)]
        public int? Height { get; init; }
    }

    public static async Task Main(string[] args)
    {
        var parseResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
        if (parseResult == null || parseResult.Tag == ParserResultType.NotParsed)
        {
            Console.WriteLine("Invalid arguments.");
            return;
        }

        try
        {
            var options = parseResult.Value;
            var resultZpl = await BitmapToZplConverter.ConvertToZplAsync(
                options.InputFilePath,
                new BitmapToZplConverter.ConvertToZplArguments
                {
                    UseZ64 = options.UseZ64,
                    Width = options.Width,
                    Height = options.Height
                }
            );

            await File.WriteAllTextAsync(options.OutputFilePath, resultZpl);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}