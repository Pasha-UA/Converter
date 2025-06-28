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
        public static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string BaseDataPath => Path.Combine(AppDirectory, "Data");
        public static string LogsPath => Path.Combine(AppDirectory, "Logs");
        // public static string DefaultInputFileName => Path.Combine(BaseDataPath, "instock.cml");
        public static readonly string DefaultInputFileNameWithoutPath = "instock.cml";

        private static string _defaultInputFileName;
        public static string DefaultInputFileName
        {
            get => Service.GetDefaultInputFileName(_defaultInputFileName);
            set => _defaultInputFileName = value;
        }

        public static string DefaultOutputFileName => Path.Combine(BaseDataPath, "import_offers.xml");

        public static string DefaultSecretKeyFileName => "secrets.json";

        public static string DefaultApiUrl => "https://my.prom.ua/api/v1/products/import_file";

    }
}
