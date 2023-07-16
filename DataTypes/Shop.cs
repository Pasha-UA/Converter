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
using xml2json_converter.Fillers;
using xml2json_converter.Parsers;

namespace xml2json_converter.DataTypes
{
    public class Shop
    {
        private XmlDocument xmlDocument;
        // private XmlNode rootNode;
        private Shop() // parameterless constructor is needed for serialization
        {

        }

        public Shop(string inputFileNameInStock = Defaults.DefaultInputFileName)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(inputFileNameInStock));

            DateTime start = DateTime.UtcNow;

            XmlParser<Currency> currenciesParser = new CurrenciesParser(xmlDocument);
            this.Currencies = currenciesParser.Parse();

            XmlParser<PriceType> priceTypesParser = new PriceTypesParser(xmlDocument);
            this.PriceTypes = priceTypesParser.Parse();

            XmlParser<ProductCategory> categoriesParser = new CategoriesParser(xmlDocument);
            this.Categories = categoriesParser.Parse();

            XmlParser<OffersItem> offerItemsParser = new OfferItemsParser(xmlDocument, this.PriceTypes);
            this.Offers = offerItemsParser.Parse();

            DateTime end = DateTime.UtcNow;
            TimeSpan timeDiff = end - start;
            Console.WriteLine("{0} milliseconds", timeDiff.TotalMilliseconds.ToString());
        }

        [XmlArray(ElementName = "currencies", Order = 0), XmlArrayItem(ElementName = "currency")]
        public Currency[] Currencies { get; set; }

        [XmlArray(Order = 1)]
        //[XmlArray(ElementName = "price_types", Order = 1), XmlArrayItem(ElementName = "price_type")]
        public PriceItem[] Prices { get; set; }

        [XmlArray(ElementName = "offers", Order = 3), XmlArrayItem(ElementName = "offer")]
        public OffersItem[] Offers { get; set; }

        [XmlIgnore]
        //[XmlArray(ElementName = "price_types", Order = 4), XmlArrayItem(ElementName = "price_type")]
        public PriceType[] PriceTypes { get; set; }

        [XmlArray(ElementName = "categories", Order = 2), XmlArrayItem(ElementName = "category")]
        public ProductCategory[] Categories { get; set; }
    }
}
