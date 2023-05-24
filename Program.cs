using ConverterProject;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Parsing;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var rootCommand = CreateRootCommand();
        return await rootCommand.InvokeAsync(args);
    }

    private static RootCommand CreateRootCommand()
    {
        IConfiguration configuration = new ConfigurationBuilder()
             .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
             .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
             .Build();

        var rootCommand = new RootCommand();

        var inputCmlNameOption = new Option<string>
            (
            name: "--input",
            description: "File with input data",
            getDefaultValue: () => Defaults.DefaultInputFileName
            );
        inputCmlNameOption.AddAlias("-i");
        rootCommand.Add(inputCmlNameOption);

        var outputXmlNameOption = new Option<string>
            (
            name: "--output",
            description: "Output file name",
            getDefaultValue: () => Defaults.DefaultOutputFileName
            );
        outputXmlNameOption.AddAlias("-o");
        rootCommand.Add(outputXmlNameOption);

        var parametersOption = new Option<string>
            (
            name: "--parameters",
            description: "File with import parameters",
            getDefaultValue: () => "params.json"
            );
        parametersOption.AddAlias("-p");
        rootCommand.Add(parametersOption);

        var convertOnlyOption = new Option<bool>
            (
            name: "--convert-only",
            description: "Convert file only. Do not upload",
            getDefaultValue: () => false
            );
        convertOnlyOption.AddAlias("-co");
        rootCommand.Add(convertOnlyOption);

        var uploadCommand = new Command("upload", "Upload previously prepared file.");
        rootCommand.AddCommand(uploadCommand);

        var uploadCommandArgument = new Argument<string>(name: "filename", getDefaultValue: () => "import_offers.xml", description: "File to upload");
        uploadCommand.AddArgument(uploadCommandArgument);

        // -i "..\..\..\Data\instock.cml"
        rootCommand.SetHandler(async (context) =>
                {
                    var inputFileName = context.ParseResult.GetValueForOption(inputCmlNameOption) ?? Defaults.DefaultInputFileName;
                    var outputFileName = context.ParseResult.GetValueForOption(outputXmlNameOption) ?? Defaults.DefaultOutputFileName;
                    var parameters = context.ParseResult.GetValueForOption(parametersOption);
                    var convertOnly = context.ParseResult.GetValueForOption(convertOnlyOption);

                    if (File.Exists(inputFileName))
                    {
                        var outputFileWithPath = await Converter.Convert(inputFileName, outputFileName);
                        Console.WriteLine($"File converted successfully and saved to {outputFileWithPath}");
                        context.ExitCode = 0;

                        if (!convertOnly)
                        {
                            context.ExitCode = await Uploader.UploadData(outputFileName, configuration);
                        }
                    }
                    else
                    {
                        var fullInputFileName = Path.GetFullPath(inputFileName);
                        Console.WriteLine($"File {fullInputFileName} not found.");
                        context.ExitCode = -1;
                    }
                    Console.WriteLine("Press ENTER to close program.");
                    Console.Read();
                });

        uploadCommand.SetHandler(async (context) =>
                {
                    var fileToUpload = context.ParseResult.GetValueForArgument(uploadCommandArgument);
                    context.ExitCode = await Uploader.UploadData(fileToUpload, configuration);
                    Console.WriteLine("Press ENTER to close program.");
                    Console.Read();
                });

        return rootCommand;
    }
}
