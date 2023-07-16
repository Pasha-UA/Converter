using AutoMapper;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
// using static ConverterProject.ItemTypes;

namespace ConverterProject
{

    public static class Converter
    {
        public static async Task<string> Convert(string inputFileNameInStock = Defaults.DefaultInputFileName,
                                                 string outputFileName = Defaults.DefaultOutputFileName
                                                )
        {
            var priceList = new yml_catalog(inputFileNameInStock);

            XmlSerializer serializer = new XmlSerializer(typeof(yml_catalog));

            using (FileStream o = new FileStream(outputFileName, FileMode.Create))
            {
                using (XmlTextWriter writer = new XmlTextWriter(o, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteDocType("yml_catalog", null, "shops.dtd", null);
                    serializer.Serialize(writer, priceList);
                }
            }

            return Path.GetFullPath(outputFileName);

        }
    }
}
