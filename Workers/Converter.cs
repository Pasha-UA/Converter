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

            using (FileStream outputStream = new FileStream(outputFileName, FileMode.Create))
            {

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Async = true,
                    Indent = true,
                    Encoding = Encoding.UTF8
                };

                using (XmlWriter writer = XmlWriter.Create(outputStream, xmlWriterSettings))
                {
                    // writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteDocType("yml_catalog", null, "shops.dtd", null);
                    // serializer.Serialize(writer, priceList);
                    await Task.Run(() => serializer.Serialize(writer, priceList));
                    await writer.FlushAsync();
                }
            }

            return Path.GetFullPath(outputFileName);

        }
    }
}
