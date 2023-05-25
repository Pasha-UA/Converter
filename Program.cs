using ConverterProject;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Parsing;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
             .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
             .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
             .Build();

        var rootCommand = CreateRootCommand(configuration);

        return await rootCommand.InvokeAsync(args);
    }

    private static RootCommand CreateRootCommand(IConfiguration configuration)
    {
        var rootCommand = new RootCommand();

        var inputCmlNameOption = CreateOption<string>("--input", "File with input data", Defaults.DefaultInputFileName, "-i");
        var outputXmlNameOption = CreateOption<string>("--output", "Output file name", Defaults.DefaultOutputFileName, "-o");
        var parametersOption = CreateOption<string>("--parameters", "File with import parameters", "params.json", "-p");
        var secretTokenOption = CreateOption<string>("--secret-token", "Secret token for uploading file. Default token you can save in the 'secrets.json' file.", null, "-st");
        var convertOnlyOption = CreateOption<bool>("--convert-only", "Convert file only. Do not upload", false, "-co");
        var uploadCommand = new Command("upload", "Upload previously prepared file.");

        rootCommand.Add(inputCmlNameOption);
        rootCommand.Add(outputXmlNameOption);
        rootCommand.Add(parametersOption);
        rootCommand.Add(secretTokenOption);
        rootCommand.Add(convertOnlyOption);

        rootCommand.AddCommand(uploadCommand);

        var uploadCommandArgument = new Argument<string>(name: "filename", getDefaultValue: () => "import_offers.xml", description: "File to upload");
        uploadCommand.AddArgument(uploadCommandArgument);

        rootCommand.SetHandler(async (context) =>
                {
                    var inputFileName = context.ParseResult.GetValueForOption(inputCmlNameOption) ?? Defaults.DefaultInputFileName;
                    var outputFileName = context.ParseResult.GetValueForOption(outputXmlNameOption) ?? Defaults.DefaultOutputFileName;
                    var parameters = context.ParseResult.GetValueForOption(parametersOption);
                    var convertOnly = context.ParseResult.GetValueForOption(convertOnlyOption);
                    var secretToken = context.ParseResult.GetValueForOption(secretTokenOption);

                    if (File.Exists(inputFileName))
                    {
                        var outputFileWithPath = await Converter.Convert(inputFileName, outputFileName);
                        Console.WriteLine($"File converted successfully and saved to {outputFileWithPath}");
                        context.ExitCode = 0;

                        if (!convertOnly)
                        {
                            context.ExitCode = await Uploader.UploadData(outputFileName, GetSecretToken(secretToken, configuration));
                        }
                    }
                    else
                    {
                        var fullInputFileName = Path.GetFullPath(inputFileName);
                        Console.WriteLine($"File {fullInputFileName} not found.");
                        context.ExitCode = -1;
                    }

                    await WaitForEnterKeyPress();
                });

        uploadCommand.SetHandler(async (context) =>
                {
                    var secretToken = context.ParseResult.GetValueForOption(secretTokenOption);
                    var fileToUpload = context.ParseResult.GetValueForArgument(uploadCommandArgument);
                    context.ExitCode = await Uploader.UploadData(fileToUpload, GetSecretToken(secretToken, configuration));

                    await WaitForEnterKeyPress();
                });

        return rootCommand;
    }

    private static Option<T> CreateOption<T>(string name, string description, object defaultValue, string alias = null)
    {
        var option = new Option<T>(name: name, description: description);

        if (defaultValue != null)
        {
            option.SetDefaultValue(defaultValue);
        }

        if (!string.IsNullOrEmpty(alias))
        {
            option.AddAlias(alias);
        }

        return option;
    }

    private static string GetSecretToken(string token, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            token = configuration["uploadToken"];
        }

        return token;
    }

    private static async Task WaitForEnterKeyPress()
    {
        Console.WriteLine("Press ENTER to close program.");
        await Task.Run(() => Console.ReadLine());
    }
}

