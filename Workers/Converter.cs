using AutoMapper;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Serilog;

namespace ConverterProject
{

    public static class Converter
    {
        public static async Task<string> Convert(string inputFileNameInStock = null, string outputFileName = null)
        {
            try
            {
                // CreateDirectoryIfNotExists
                inputFileNameInStock ??= Defaults.DefaultInputFileName;
                outputFileName ??= Defaults.DefaultOutputFileName;
                Service.EnsureDirectoryExists(Path.GetDirectoryName(inputFileNameInStock));

                var priceList = new yml_catalog(inputFileNameInStock);

                XmlSerializer serializer = new XmlSerializer(typeof(yml_catalog));

                await using (FileStream outputStream = new FileStream(outputFileName, FileMode.Create))
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                    {
                        Async = true,
                        Indent = true,
                        Encoding = new UTF8Encoding(false) // Use encoderShouldEmitUTF8Identifier = false for compatibility
                    };

                    await using (XmlWriter xmlWriter = XmlWriter.Create(outputStream, xmlWriterSettings))
                    {
                        serializer.Serialize(xmlWriter, priceList);
                        await xmlWriter.FlushAsync();
                    }
                }

//                Log.Information($"File converted successfully and saved to {outputFileName}");
                return outputFileName;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during conversion.");
                throw;
            }
        }
    }
}
