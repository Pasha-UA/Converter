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
            DateTime start = DateTime.UtcNow;

            XmlDocument xmlDocument = ReadXmlDocument(inputFileNameInStock);

            XmlDocumentParser(xmlDocument);

            DateTime end = DateTime.UtcNow;
            TimeSpan timeDiff = end - start;
            var logString = $"{timeDiff.TotalMilliseconds} milliseconds";
            // Console.WriteLine(logString);
            Log.Information(logString);

        }

        public Shop(XmlDocument xmlDocument)
        {
            XmlDocumentParser(xmlDocument);
        }

        private void XmlDocumentParser(XmlDocument xmlDocument)
        {
            XmlItemParser<Currency> currenciesParser = new CurrenciesParser(xmlDocument);
            this.Currencies = currenciesParser.Parse();

            XmlItemParser<PriceType> priceTypesParser = new PriceTypesParser(xmlDocument);
            this.PriceTypes = priceTypesParser.Parse();

            XmlItemParser<ProductCategory> categoriesParser = new CategoriesParser(xmlDocument);
            this.Categories = categoriesParser.Parse();

            XmlItemParser<OfferItem> offerItemsParser = new OfferItemsParser(xmlDocument, this.PriceTypes);
            this.Offers = offerItemsParser.Parse();
        }

        private XmlDocument ReadXmlDocument(string inputFileNameInStock)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(inputFileNameInStock));

            return xmlDocument;
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
