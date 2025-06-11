using ConverterProject;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;
using System.CommandLine.Parsing;

internal class Program
{
    private static IConfiguration Configuration { get; set; } = new ConfigurationBuilder()
                 .SetBasePath(Defaults.AppDirectory)
                 .AddJsonFile(Defaults.DefaultSecretKeyFileName, optional: true, reloadOnChange: true)
                 .Build();

    private static async Task<int> Main(string[] args)
    {
        // Subscribe to the CancelKeyPress event
        Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

        Service.EnsureDirectoryExists(Defaults.LogsPath);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(
                $"{Defaults.LogsPath}/log-.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
            ), bufferSize: 10000)
            .CreateLogger();
        try
        {
            var rootCommand = CreateRootCommand();

            return await rootCommand.InvokeAsync(args);
        }
        finally
        {
            // Close and flush the log when the application exits
            Log.CloseAndFlush();
        }
    }

    // <<<<<<<<<<<<<  ✨ Codeium AI Suggestion  >>>>>>>>>>>>>>
    // Creates the root command for the command-line interface (CLI) tool, including options, commands, and handlers.
    // <<<<<  bot-08d5d5a7-8457-4843-ba4c-25ad5066ac70  >>>>>
    private static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand();

        var defaultInputFileName = Defaults.DefaultInputFileName;
        var inputCmlNameOption = CreateOption<string>("--input", "File with input data", defaultInputFileName, "-i");
        var outputXmlNameOption = CreateOption<string>("--output", "Output file name", Defaults.DefaultOutputFileName, "-o");
        var parametersOption = CreateOption<string>("--parameters", "File with import parameters", "params.json", "-p");
        var secretTokenOption = CreateOption<string>("--secret-token", $"Secret token for uploading file. Default token you can save in the '{Defaults.DefaultSecretKeyFileName}' file.", null, "-st");
        var convertOnlyOption = CreateOption<bool>("--convert-only", "Convert file only. Do not upload", false, "-co");
        var logDirectoryName = CreateOption<string>("--logs-path", "Log directory name", Defaults.LogsPath, "-lp");

        rootCommand.Add(inputCmlNameOption);
        rootCommand.Add(outputXmlNameOption);
        rootCommand.Add(parametersOption);
        rootCommand.Add(secretTokenOption);
        rootCommand.Add(convertOnlyOption);
        rootCommand.Add(logDirectoryName);

        var uploadCommand = new Command("upload", "Upload previously prepared file.");
        rootCommand.AddCommand(uploadCommand);

        var uploadCommandArgument = new Argument<string>(name: "filename", getDefaultValue: () => Defaults.DefaultOutputFileName, description: "File to upload");
        uploadCommand.AddArgument(uploadCommandArgument);

        rootCommand.SetHandler(async (context) =>
                {
                    var inputFileName = context.ParseResult.GetValueForOption(inputCmlNameOption) ?? defaultInputFileName;
                    var outputFileName = context.ParseResult.GetValueForOption(outputXmlNameOption) ?? Defaults.DefaultOutputFileName;
                    var parameters = context.ParseResult.GetValueForOption(parametersOption);
                    var convertOnly = context.ParseResult.GetValueForOption(convertOnlyOption);
                    var secretToken = context.ParseResult.GetValueForOption(secretTokenOption);
                    var logDirectory = context.ParseResult.GetValueForOption(logDirectoryName);

                    Log.Information("New session started.");

                    try
                    {
                        if (File.Exists(inputFileName))
                        {
                            var outputFileWithPath = await Converter.Convert(inputFileName, outputFileName);
                            Log.Information($"File converted successfully and saved to {outputFileWithPath}");

                            // Rename input file
                            Service.RenameUsedInputFileInWorkingDirectory();
                            context.ExitCode = 0;

                            if (!convertOnly)
                            {
                                context.ExitCode = await Uploader.UploadData(outputFileName, GetSecretToken(secretToken));
                            }
                        }
                        else
                        {
                            var inputFileWithPath = inputFileName != null ? Path.GetFullPath(inputFileName) : "";
                            Log.Error($"Input file {inputFileWithPath} not found or no file specified.");
                            context.ExitCode = -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"An unexpected error occurred: {ex.Message}");
                        context.ExitCode = -1;
                    }
                });

        uploadCommand.SetHandler(async (context) =>
                {
                    var secretToken = context.ParseResult.GetValueForOption(secretTokenOption);
                    var fileToUpload = context.ParseResult.GetValueForArgument(uploadCommandArgument);
                    context.ExitCode = await Uploader.UploadData(fileToUpload, GetSecretToken(secretToken));

                    // await WaitForEnterKeyPress();
                });

        return rootCommand;
    }

    // Retrieves the secret token. If the input token is empty or whitespace, it retrieves the token from the Configuration settings. 
    // Returns the secret token. If an exception occurs, it logs the exception and returns an empty string.
    private static string GetSecretToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = Configuration["promUploadToken"];
            }

            return token;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it based on your application's requirements
            Log.Error($"Error retrieving the secret token: {ex.Message}");

            // Provide a default value or throw the exception again, depending on your needs
            return "";
        }
    }

    /// <summary>
    /// Create an option for a command line argument with a name, description, and optional default value and alias.
    /// </summary>
    /// <typeparam name="T">Type of the option value.</typeparam>
    /// <param name="name">Name of the option.</param>
    /// <param name="description">Description of the option.</param>
    /// <param name="defaultValue">Default value of the option (optional).</param>
    /// <param name="alias">Alias for the option (optional).</param>
    /// <returns>The created option.</returns>
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

    // private static async Task WaitForEnterKeyPress()
    // {
    //     Console.WriteLine("Press ENTER to close program.");
    //     await Task.Run(() => Console.ReadLine());
    // }

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        // TODO: cleanup, close all open files        

        Log.Warning("Ctrl+C detected. Exiting...");
        e.Cancel = true; // Prevents the application from immediately terminating


        Log.CloseAndFlush();

        // exit application
        Environment.Exit(-1);
    }

    private static GoogleCredential Credential()
    {
        GoogleCredential googleCredential = GoogleCredential.FromAccessToken(accessToken: "");
        return googleCredential;
    }
}
