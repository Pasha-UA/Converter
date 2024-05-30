using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Serilog;
using xml2json_converter.DataTypes;
using xml2json_converter.Parsers;

namespace xml2json_converter.Fillers
{
    public class PriceTypesParser : XmlItemParser<PriceType>
    {
        public PriceTypesParser(XmlDocument xmlDocument) : base(xmlDocument)
        {
        }
        public override PriceType[] Parse()
        {
            // Console.WriteLine("Filling price types ...");
            Log.Information("Filling price types ...");
            var priceTypesXml = this.RootNode.SelectSingleNode("ПакетПредложений/ТипыЦен").ChildNodes;
            var priceTypes = new List<PriceType>();
            foreach (XmlNode node in priceTypesXml)
            {
                PriceType priceType;// = new PriceType();
                if (String.Compare(node.SelectNodes("ИДЦенаСайт").Item(0).InnerText, "Розничная") == 0)
                {
                    priceType = new PriceType()
                    {
                        Id = node.SelectNodes("Ид").Item(0).InnerText,
                        CurrencyId = node.SelectNodes("Валюта").Item(0).InnerText,
                        IsRetail = true,
                    };
                }
                else
                {
                    priceType = new PriceType()
                    {
                        Id = node.SelectNodes("Ид").Item(0).InnerText,
                        Quantity = Int32.Parse(Regex.Match(node.SelectNodes("Наименование").Item(0).InnerText, @"\d+").Value),
                        CurrencyId = node.SelectNodes("Валюта").Item(0).InnerText,
                        IsRetail = false,
                    };
                }
                priceTypes.Add(priceType);
            }
            var logString = $"Filling price types complete. Total {priceTypes.Count}";
            // Console.WriteLine($"Filling price types complete. Total {0}", priceTypes.Count);
            Log.Information(logString);
            return priceTypes.ToArray();
        }

    }
}