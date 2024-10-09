using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ConverterProject
{
    public static class Defaults
    {
        // public const string DefaultInputFileName = "..\\..\\..\\Data\\instock.cml";
        // public const string DefaultInputFileName = "..\\..\\..\\Data\\instock_raznov.cml";
        // public const string DefaultInputFileName = "Data\\instock_raznov.cml";
        // public const string DefaultOutputFileName = "Data\\import_raznov.xml";

        // public const string DefaultOutputFileName = "..\\..\\..\\Data\\import_offers.xml";

        // private static readonly string BaseDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        // private static string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string BaseDataPath => Path.Combine(appDirectory, "Data");
        public static string LogsPath => Path.Combine(appDirectory, "Logs");
        // public static string DefaultInputFileName => Path.Combine(BaseDataPath, "instock.cml");
        private static readonly string DefaultInputFileNameWithoutPath = "instock.cml";

        private static string _defaultInputFileName;
        public static string DefaultInputFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_defaultInputFileName))
                {
                    _defaultInputFileName = Path.Combine(BaseDataPath, DefaultInputFileNameWithoutPath);
                }

                // If not default file at the default Data folder try to find downloaded file in 'Downloads' folder
                if (!File.Exists(_defaultInputFileName))
                {
                    try
                    {
                        return GetInputFileFromDownloadsFolder();
                    }
                    catch
                    {
                        Log.Fatal($"Input file not found. Specify input file name using -i (or --input) command line key. Run program with -h key for help.");
                        Log.Fatal("Program closed.");
                        Environment.Exit(-3);
                    }
                }



                return _defaultInputFileName;
            }
            set
            {
                _defaultInputFileName = value;
            }
        }

        public static string DefaultOutputFileName => Path.Combine(BaseDataPath, "import_offers.xml");

        public static string DefaultSecretKeyFileName => "secrets.json";

        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    Log.Fatal($"Can't create directory {path}. Program closed.");
                    Environment.Exit(-2);
                }
            }
        }

        /// <summary>
        /// Searches for input file from '{user}/Downloads' folder and moves it to 'Data' folder of the program
        /// Returns input file filename.
        /// </summary>
        public static string GetInputFileFromDownloadsFolder()
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            // Имя файла, который нужно переместить
            string fileName = DefaultInputFileNameWithoutPath;
            // Полный путь к файлу в папке Downloads
            string sourceFilePath = Path.Combine(downloadsPath, fileName);

            // Полный путь, куда нужно переместить файл
            string destinationFilePath = Path.Combine(BaseDataPath, fileName);


            try
            {
                // Проверка, существует ли файл в папке Downloads
                if (File.Exists(sourceFilePath))
                {
                    if (File.Exists(destinationFilePath))
                    {
                        try
                        {

                        }
                        // rename destination file
                        catch
                        {
                            Log.Error("Can't replace old input file with new one. Exiting");
                            Environment.Exit(-1);
                        }

                    }
                    // Перемещение файла
                    File.Move(sourceFilePath, destinationFilePath);
                    Log.Information("Input file successfully moved from 'Downloads' to the 'Data' folder.");

                }
                else
                {
                    Log.Fatal("No input file. Run program with -h key for help.");
                    Environment.Exit(-1);

                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"Error: {ex.Message}");
                Environment.Exit(-10);
            }

            return destinationFilePath;

        }
    }
}
