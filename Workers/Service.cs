using Serilog;

namespace ConverterProject
{
    public static class Service
    {
        public static void RenameUsedInputFileInWorkingDirectory()
        {
            // Новое имя для файла после перемещения
            string dataPath = Path.GetDirectoryName(Defaults.DefaultInputFileName);
            string newFileName = $"{Path.GetFileName(Defaults.DefaultInputFileName)}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.{Path.GetExtension(Defaults.DefaultInputFileName)}";
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

            // Имя файла, который нужно переместить
            string fileName = Defaults.DefaultInputFileNameWithoutPath;
            // Полный путь к файлу в папке Downloads
            string sourceFilePath = Path.Combine(downloadsPath, fileName);

            // Полный путь, куда нужно переместить файл
            string destinationFilePath = Path.Combine(Defaults.BaseDataPath, fileName);


            try
            {
                // Проверка, существует ли файл в папке Downloads
                if (File.Exists(sourceFilePath))
                {
                    if (!Directory.Exists(Defaults.BaseDataPath)) Directory.CreateDirectory(Defaults.BaseDataPath);
                    
                    if (File.Exists(destinationFilePath))
                    {
                        try
                        {
                            Service.RenameUsedInputFileInWorkingDirectory();
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
 //                   Log.Error("No input file. Run program with -h key for help.");
                    return null; 
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"Error: {ex.Message}  {sourceFilePath}  {destinationFilePath}");
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