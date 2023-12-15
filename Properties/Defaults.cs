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
        private static readonly string BaseDataPath = "Data";
        public static string LogsPath = "Logs";
        public static string DefaultInputFileName => Path.Combine(BaseDataPath, "instock.cml");
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


    }
}
