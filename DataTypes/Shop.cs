using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using ConverterProject;
using Serilog;
using xml2json_converter.Fillers;
using xml2json_converter.Parsers;

namespace xml2json_converter.DataTypes
{
    public class Shop
    {
        private Shop() // parameterless constructor is needed for serialization
        {

        }

        public Shop(string inputFileNameInStock = null)
        {
            inputFileNameInStock ??= Defaults.DefaultInputFileName;

            DateTime start = DateTime.UtcNow;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(inputFileNameInStock));

            XmlParser<Currency> currenciesParser = new CurrenciesParser(xmlDocument);
            this.Currencies = currenciesParser.Parse();

            XmlParser<PriceType> priceTypesParser = new PriceTypesParser(xmlDocument);
            this.PriceTypes = priceTypesParser.Parse();

            XmlParser<ProductCategory> categoriesParser = new CategoriesParser(xmlDocument);
            this.Categories = categoriesParser.Parse();

            XmlParser<OfferItem> offerItemsParser = new OfferItemsParser(xmlDocument, this.PriceTypes);
            this.Offers = offerItemsParser.Parse();

            DateTime end = DateTime.UtcNow;
            TimeSpan timeDiff = end - start;
            var logString = $"{timeDiff.TotalMilliseconds} milliseconds";
            // Console.WriteLine(logString);
            Log.Information(logString);

        }

        [XmlArray(ElementName = "currencies", Order = 0), XmlArrayItem(ElementName = "currency")]
        public Currency[] Currencies { get; set; }

        [XmlArray(Order = 1)]
        //[XmlArray(ElementName = "price_types", Order = 1), XmlArrayItem(ElementName = "price_type")]
        public PriceItem[] Prices { get; set; }

        [XmlArray(ElementName = "offers", Order = 3), XmlArrayItem(ElementName = "offer")]
        public OfferItem[] Offers { get; set; }

        [XmlIgnore]
        //[XmlArray(ElementName = "price_types", Order = 4), XmlArrayItem(ElementName = "price_type")]
        public PriceType[] PriceTypes { get; set; }

        [XmlArray(ElementName = "categories", Order = 2), XmlArrayItem(ElementName = "category")]
        public ProductCategory[] Categories { get; set; }
    }
}
