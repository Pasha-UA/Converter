﻿using ConverterProject;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;
using System.CommandLine.Parsing;

internal class Program
{
    private static IConfiguration Configuration { get; set; } = new ConfigurationBuilder()
                 .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                 .AddJsonFile(Defaults.DefaultSecretKeyFileName, optional: true, reloadOnChange: true)
                 .Build();

    private static async Task<int> Main(string[] args)
    {
        // Subscribe to the CancelKeyPress event
        Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

        Defaults.EnsureDirectoryExists(Defaults.LogsPath);
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

    private static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand();

        var inputCmlNameOption = CreateOption<string>("--input", "File with input data", Defaults.DefaultInputFileName, "-i");
        var outputXmlNameOption = CreateOption<string>("--output", "Output file name", Defaults.DefaultOutputFileName, "-o");
        var parametersOption = CreateOption<string>("--parameters", "File with import parameters", "params.json", "-p");
        var secretTokenOption = CreateOption<string>("--secret-token", $"Secret token for uploading file. Default token you can save in the '{Defaults.DefaultSecretKeyFileName}' file.", null, "-st");
        var convertOnlyOption = CreateOption<bool>("--convert-only", "Convert file only. Do not upload", false, "-co");
        var logDirectoryName = CreateOption<string>("--logs_path", "Log directory name", Defaults.LogsPath, "-lp");

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
                    var inputFileName = context.ParseResult.GetValueForOption(inputCmlNameOption) ?? Defaults.DefaultInputFileName;
                    var outputFileName = context.ParseResult.GetValueForOption(outputXmlNameOption) ?? Defaults.DefaultOutputFileName;
                    var parameters = context.ParseResult.GetValueForOption(parametersOption);
                    var convertOnly = context.ParseResult.GetValueForOption(convertOnlyOption);
                    var secretToken = context.ParseResult.GetValueForOption(secretTokenOption);
                    var logDirectory = context.ParseResult.GetValueForOption(logDirectoryName);

                    Log.Information("New session started.");


                    if (File.Exists(inputFileName))
                    {
                        var outputFileWithPath = await Converter.Convert(inputFileName, outputFileName);
                        Log.Information($"File converted successfully and saved to {outputFileWithPath}");
                        context.ExitCode = 0;

                        if (!convertOnly)
                        {
                            context.ExitCode = await Uploader.UploadData(outputFileName, GetSecretToken(secretToken));
                        }
                    }
                    else
                    {
                        var inputFileWithPath = Path.GetFullPath(inputFileName);
                        Log.Error($"File {inputFileWithPath} not found.");
                        context.ExitCode = -1;
                    }

                    // await WaitForEnterKeyPress();
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

    private static string GetSecretToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = Configuration["uploadToken"];
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
}

