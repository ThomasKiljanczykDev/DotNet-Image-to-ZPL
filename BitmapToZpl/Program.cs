using CommandLine;

namespace BitmapToZpl;

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
                "Input file path. Supports formats from System.Drawing.Imaging.ImageFormat (https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.imageformat?view=windowsdesktop-9.0)."
        )]
        public required string InputFilePath { get; init; }

        [Option('o', "output", Required = false, HelpText = "Output file path.", Default = "output.zpl")]
        public string OutputFilePath { get; init; } = "output.zpl";
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
            var resultZpl = await BitmapToZplConverter.ConvertToZplAsync(options.InputFilePath, options.UseZ64);

            await File.WriteAllTextAsync(options.OutputFilePath, resultZpl);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}