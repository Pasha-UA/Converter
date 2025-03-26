using Serilog;

namespace ConverterProject
{
    public static class Service
    {
        public static void RenameUsedInputFileInWorkingDirectory()
        {
            // Новое имя для файла после перемещения
            string dataPath = Path.GetDirectoryName(Defaults.DefaultInputFileName);
            string newFileName = $"{Path.GetFileNameWithoutExtension(Defaults.DefaultInputFileName)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(Defaults.DefaultInputFileName)}";
            string newFilePath = Path.Combine(dataPath, newFileName);
            // Переименование файла
            File.Move(Defaults.DefaultInputFileName, newFilePath);
            Log.Information("Input file renamed.");
        }

        /// <summary>
        /// Searches for input file from '{user}/Downloads' folder and moves it to 'Data' folder of the program
        /// Returns input file filename.
        /// </summary>
        public static string TryGetInputFileFromDownloadsFolder()
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fileName = Defaults.DefaultInputFileNameWithoutPath;
            string sourceFilePath = Path.Combine(downloadsPath, fileName);
            string destinationFilePath = Path.Combine(Defaults.BaseDataPath, fileName);

            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    Log.Error("Input file not found in Downloads folder.");
                    return null;
                }

                EnsureDirectoryExists(Defaults.BaseDataPath);

                if (File.Exists(destinationFilePath))
                {
                    RenameUsedInputFileInWorkingDirectory();
                }

                File.Move(sourceFilePath, destinationFilePath);
                Log.Information("Input file successfully moved from 'Downloads' to the 'Data' folder.");
            }
            catch (IOException ioEx)
            {
                Log.Fatal($"IO Error: {ioEx.Message}  {sourceFilePath}  {destinationFilePath}");
                Environment.Exit(-10);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Log.Fatal($"Access Error: {uaEx.Message}  {sourceFilePath}  {destinationFilePath}");
                Environment.Exit(-10);
            }
            catch (Exception ex)
            {
                Log.Fatal($"Unexpected Error: {ex.Message}  {sourceFilePath}  {destinationFilePath}");
                Environment.Exit(-10);
            }

            return destinationFilePath;
        }

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

        public static string GetDefaultInputFileName(string DefaultInputFileName)
        {
            if (string.IsNullOrEmpty(DefaultInputFileName))
            {
                DefaultInputFileName = Path.Combine(Defaults.BaseDataPath, Defaults.DefaultInputFileNameWithoutPath);
            }

            // If not default file at the default Data folder try to find downloaded file in 'Downloads' folder
            if (!File.Exists(DefaultInputFileName))
            {
                try
                {
                    return TryGetInputFileFromDownloadsFolder();
                }
                catch
                {
                    return null;

                    //Log.Fatal($"Input file not found. Specify input file name using -i (or --input) command line key. Run program with -h key for help.");
                    //Log.Fatal("Program closed.");
                    //Environment.Exit(-3);
                }
            }

            return DefaultInputFileName;
        }
    }
}