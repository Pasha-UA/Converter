using AutoMapper;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using static ConverterProject.ItemTypes;

namespace ConverterProject
{

    public static class Converter
    {
        private static List<PriceType> PriceTypes { get; set; } = new List<PriceType>();
        private static List<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        private static List<OfferItem> Offers { get; set; } = new List<OfferItem>();
        private static MapperConfiguration MapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<PriceType, PriceItem>());
        private static Mapper Mapper = new Mapper(MapperConfig);
        private static MapperConfiguration OfferMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<OffersItem, OfferItem>()

        );
        private static Mapper OfferMapper = new Mapper(OfferMapperConfig);


        private static void WriteXml(string outputFileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;

            using (XmlWriter writer = XmlWriter.Create(outputFileName, settings))
            {
                writer.WriteStartDocument();
                writer.WriteDocType("yml_catalog", null, "shops.dtd", null);
                writer.WriteStartElement("yml_catalog");
                writer.WriteAttributeString("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                writer.WriteStartElement("shop");


                // currencies block --start
                writer.WriteStartElement("currencies");
                var currencies = FillCurrencies();
                foreach (var currency in currencies)
                {
                    writer.WriteStartElement("currency");
                    writer.WriteAttributeString("id", currency.CurrencyId);
                    writer.WriteAttributeString("rate", currency.Rate.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // currencies block --end

                // categories block --start
                {
                    writer.WriteStartElement("categories");
                    foreach (var cat in Categories)
                    {
                        writer.WriteStartElement("category");
                        writer.WriteAttributeString("id", cat.Id);
                        if (cat.ParentId != null) writer.WriteAttributeString("parentId", cat.ParentId);
                        writer.WriteString(cat.Name);
                        writer.WriteFullEndElement();
                    }
                    writer.WriteFullEndElement();
                }
                // categories block --end

                // offers block --start
                {
                    writer.WriteStartElement("offers");
                    foreach (var offer in Offers)
                    {
                        writer.WriteStartElement("offer");
                        writer.WriteAttributeString("id", offer.Id);
                        writer.WriteAttributeString("available", offer.Available);

                        writer.WriteAttributeString("presence", offer.Presence);

                        var sellingType = "";
                        if (offer.PriceItems != null && offer.PriceItems.Any()) sellingType = "u";
                        else sellingType = "r";
                        writer.WriteAttributeString("selling_type", sellingType);


                        if (offer.RetailPrice != null)
                        {
                            writer.WriteStartElement("price");
                            writer.WriteString(offer.RetailPrice.Price.ToString());
                            writer.WriteFullEndElement();

                            writer.WriteStartElement("currencyId");
                            writer.WriteString(offer.RetailPrice.CurrencyId);
                            writer.WriteFullEndElement();
                        }

                        if (offer.PriceItems != null && offer.PriceItems.Any())
                        {
                            writer.WriteStartElement("prices");

                            foreach (var priceItem in offer.PriceItems)
                            {
                                writer.WriteStartElement("price");

                                writer.WriteStartElement("value");
                                writer.WriteString(priceItem.Price.ToString());
                                writer.WriteFullEndElement();

                                writer.WriteStartElement("currencyId");
                                writer.WriteString(priceItem.CurrencyId);
                                writer.WriteFullEndElement();

                                if (priceItem.Quantity > 0)
                                {
                                    writer.WriteStartElement("quantity");
                                    writer.WriteString(priceItem.Quantity.ToString());
                                    writer.WriteFullEndElement();
                                }

                                writer.WriteFullEndElement(); //price
                            }

                            writer.WriteFullEndElement(); //prices
                        }

                        writer.WriteStartElement("categoryId");
                        writer.WriteString(offer.CategoryId);
                        writer.WriteFullEndElement(); // category

                        if (offer.Description != null)
                        {
                            writer.WriteStartElement("description");
                            writer.WriteCData(offer.Description);
                            writer.WriteFullEndElement(); // description
                        }

                        writer.WriteStartElement("name");
                        writer.WriteString(offer.Name);
                        writer.WriteFullEndElement(); // name

                        writer.WriteStartElement("barcode");
                        writer.WriteString(offer.BarCode);
                        Console.WriteLine(offer.BarCode + " " + offer.Name);
                        writer.WriteFullEndElement(); // barcode

                        if (offer.QuantityInStock != 0 && offer.QuantityInStock != null)
                        {
                            writer.WriteStartElement("quantity_in_stock");
                            writer.WriteValue(offer.QuantityInStock);
                            writer.WriteFullEndElement(); // quantity_in_stock
                        }

                        //writer.WriteStartElement("presence");
                        //writer.WriteString(offer.Presence);
                        //writer.WriteFullEndElement(); // presence


                        if (offer.SearchStrings != null && offer.SearchStrings.Any())
                        {
                            writer.WriteStartElement("keywords");

                            var keywords = string.Join(',', offer.SearchStrings);

                            writer.WriteString(keywords);

                            //foreach (var searchString in offer.SearchStrings)
                            //{
                            //    writer.WriteStartElement("searchString");
                            //    writer.WriteString(searchString);
                            //    writer.WriteFullEndElement(); // searchString
                            //}

                            writer.WriteFullEndElement(); // searchStrings
                        }

                        writer.WriteFullEndElement(); // offer
                    }
                    writer.WriteFullEndElement(); // offers
                }
                // offers block --end

                writer.WriteFullEndElement();
                writer.WriteEndDocument();

            }
        }

        private static List<Currency> FillCurrencies()
        {
            return new List<Currency>
            {
                new Currency { CurrencyId = "UAH", Rate = 1, CurrencyCode = "980", Symbol="₴" },
                new Currency { CurrencyId = "USD", Rate = 42, CurrencyCode = "840", Symbol = "$"},
                new Currency { CurrencyId = "EUR", Rate = 45, CurrencyCode = "978", Symbol = "€" },
            };
        }

        private static List<PriceType> FillPriceTypes(XmlNode rootNode)
        {
            Console.WriteLine("Filling price types ...");
            var priceTypesXml = rootNode.SelectSingleNode("ПакетПредложений/ТипыЦен").ChildNodes;
            var priceTypes = new List<PriceType>();
            foreach (XmlNode node in priceTypesXml)
            {
                PriceType item = new PriceType();
                if (String.Compare(node.SelectNodes("ИДЦенаСайт").Item(0).InnerText, "Розничная") == 0)
                {
                    item = new PriceType()
                    {
                        Id = node.SelectNodes("Ид").Item(0).InnerText,
                        CurrencyId = node.SelectNodes("Валюта").Item(0).InnerText,
                        IsRetail = true,
                    };
                }
                else
                {
                    item = new PriceType()
                    {
                        Id = node.SelectNodes("Ид").Item(0).InnerText,
                        Quantity = Int32.Parse(Regex.Match(node.SelectNodes("Наименование").Item(0).InnerText, @"\d+").Value),
                        CurrencyId = node.SelectNodes("Валюта").Item(0).InnerText,
                        IsRetail = false,
                    };
                }
                priceTypes.Add(item);
            }
            Console.WriteLine("Filling price types complete. Total {0}", priceTypes.Count);

            return priceTypes;
        }

        private static List<ProductCategory> FillCategories(XmlNode rootNode)
        {
            var categories = new List<ProductCategory>();
            Console.WriteLine("Filling categories list ...");
            var categoriesXml = rootNode.SelectSingleNode("Классификатор/Группы").ChildNodes;
            foreach (XmlNode node in categoriesXml)
            {
                ProductCategory item = new ProductCategory()
                {
                    Id = node.SelectNodes("Ид").Item(0).InnerText,
                    ParentId = node.SelectNodes("Родитель").Item(0)?.InnerText,
                    Name = node.SelectNodes("Наименование").Item(0).InnerText
                };
                categories.Add(item);
            }
            Console.WriteLine("Filling categories list complete. Total {0}", categories.Count);

            return categories;
        }

        private static List<OfferItem> FillOfferItems(XmlNode rootNode, bool instock)
        {
            var offers = new List<OfferItem>();

            Console.WriteLine("Filling product list ...");
            var offersXml = rootNode.SelectSingleNode("ПакетПредложений/Предложения").ChildNodes;
            foreach (XmlNode node in offersXml)
            {
                var prices = new List<PriceItem>();
                XmlNode pricesNodeXml = node.SelectSingleNode("Цены");
                foreach (XmlNode price in pricesNodeXml)
                {
                    var Id = price.SelectNodes("ИдТипаЦены").Item(0).InnerText;
                    PriceItem priceItem = Mapper.Map<PriceItem>(PriceTypes.First(p => p.Id == Id));
                    priceItem.Price = Decimal.Parse(price.SelectNodes(" ЦенаЗаЕдиницу").Item(0).InnerText);

                    prices.Add(priceItem);
                }

                OfferItem item = new OfferItem()
                {
                    Id = node.SelectNodes("Ид").Item(0).InnerText,
                    RetailPrice = prices.FirstOrDefault(p => p.IsRetail, null),
                    PriceItems = prices.Where(p => !p.IsRetail) ?? null,
                };
                offers.Add(item);
            }
            Console.WriteLine("Filling product list complete. Total {0}", offers.Count);
            // fill offers list --end

            // fill goods list --start
            Console.WriteLine("Filling product characteristics ...");
            var goodsXml = rootNode.SelectSingleNode("ПакетПредложений/Товары").ChildNodes;
            var updatedOffers = new List<OfferItem>();
            var keywords = new FillKeywords().Keywords;
            var counter = 0;
            foreach (XmlNode node in goodsXml)
            {
                counter++;

                OfferItem item = offers.FirstOrDefault(o => o.Id == node.SelectNodes("Ид").Item(0).InnerText);
                item.Name = node.SelectNodes("Наименование")?.Item(0)?.InnerText ?? "";
                item.Description = node.SelectNodes("Описание")?.Item(0)?.InnerText;
                item.CategoryId = node.SelectNodes("Группы/Ид").Item(0).InnerText ?? "";
                item.BarCode = node.SelectNodes("КодТовара").Item(0).InnerText;

                // keywords
                if (keywords.Any())
                {
                    item.SearchStrings = keywords.First(kw => kw.Id == item.Id).Keys;
                }
                //

                var parametersXml = node.SelectNodes("ЗначенияСвойств/ЗначенияСвойства");
                if (parametersXml != null)
                {
                    var parameters = new List<ParameterItem>();
                    foreach (XmlNode paramNode in parametersXml)
                    {
                        ParameterItem parameter = new ParameterItem()
                        { // TODO: пока что параметры (свойства товаров) не используются, кроме "наличие" и "остаток"
                            Id = paramNode.SelectNodes("Ид").Item(0).InnerText,
                            Name = paramNode.SelectNodes("Наименование").Item(0).InnerText,
                            Value = paramNode.SelectNodes("Значение").Item(0).InnerText
                        };
                        parameters.Add(parameter);
                    }
                    if (instock)
                    {
                        item.Available = "true"; // bool.Parse(parameters.Find(p => p.Id == "ИД-Наличие").Value).ToString();
                        item.Presence = "available";
                        item.QuantityInStock = Int32.Parse(parameters.First(p => p.Id == "ИД-Количество").Value);
                        item.Parameters = parameters.Where(p => String.Compare(p.Name, "Количество") != 0 && String.Compare(p.Name, "Наличие") != 0);
                    }
                    else // order
                    {
                        item.Available = "false";
                        item.Presence = "order";
                    }
                }

                Console.WriteLine("Loaded {0} of {1}, {2} {3}", counter, goodsXml.Count, item.BarCode, item.Name);

                updatedOffers.Add(item);
            }
            offers = updatedOffers;
            Console.WriteLine("Filling product characteristics complete.");

            return offers;
        }

        private static List<OfferItem> Deserialization(XmlDocument xmlInStock, XmlDocument xmlOrder)
        {
            var offers = new List<OfferItem>();
            //            var categories = new List<ProductCategory>();

            XmlNode rootInStock = xmlInStock.FirstChild.NextSibling; // для товаров in stock

            XmlNode rootOrder = (xmlOrder != null) ? (xmlOrder.FirstChild.NextSibling != null) ? xmlOrder.FirstChild.NextSibling : null : null; // для товаров "под заказ"

            PriceTypes = FillPriceTypes(rootInStock);
            if (rootOrder != null)
            {
                var priceTypesOrder = FillPriceTypes(rootOrder);
                PriceTypes = PriceTypes.Union(priceTypesOrder, new Comparer<PriceType>()).ToList();
            }
            Console.WriteLine("Filling price types complete. Total {0}", PriceTypes.Count);

            Categories = FillCategories(rootInStock);
            if (rootOrder != null)
            {
                var categories = FillCategories(rootOrder);
                Categories = Categories.Union(categories, new Comparer<ProductCategory>()).ToList();
            }
            Console.WriteLine("Filling categories list complete. Total {0}", Categories.Count);

            offers = FillOfferItems(rootInStock, true);

            // adding items avaiable for order (not in stock)
            if (rootOrder != null)
            {
                //                rootOrder = xmlOrder.FirstChild.NextSibling;
                var offersOrder = FillOfferItems(rootOrder, false);
                offers = offers.Union(offersOrder, new Comparer<OfferItem>()).ToList();

            }

            return offers;

        }

        public static async Task<string> Convert(string inputFileNameInStock = Defaults.DefaultInputFileName, 
                                                 string outputFileName = Defaults.DefaultOutputFileName
                                                )
        {
            //var inputFileNameOrder = "..\\..\\..\\Data\\order.cml";

            //var xml = await File.ReadAllTextAsync(inputFileNameInStock);

            //
            //            var priceList = new ItemTypes.yml_catalog();
            var priceList = new ItemTypes.yml_catalog(inputFileNameInStock);

            XmlSerializer serializer = new XmlSerializer(typeof(ItemTypes.yml_catalog));
            if (File.Exists(outputFileName)) File.Delete(outputFileName);
            FileStream o = new FileStream(outputFileName, FileMode.Create);
            serializer.Serialize(o, priceList);
            o.Close();  

            // add DOCTYPE string
            List<string> lines = File.ReadAllLines(outputFileName).ToList();
            lines.Insert(1, "<!DOCTYPE yml_catalog SYSTEM \"shops.dtd\">");
            await File.WriteAllLinesAsync(outputFileName, lines);

            return Path.GetFullPath(outputFileName);

        }
    }
}
