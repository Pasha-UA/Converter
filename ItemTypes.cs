using AutoMapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ConverterProject
{
    public class ItemTypes
    {
        public class ProductCategory : BaseItem
        {
            [XmlAttribute(AttributeName = "parentId")]
            public string ParentId { get; set; }
            [XmlText]
            public string Name { get; set; }
        }

        public class ParameterItem : BaseItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class PriceType : BaseItem
        {
            public bool ShouldSerializeId()
            {
                return false;
            }
            [XmlElement(ElementName = "quantity")]
            public int? Quantity { get; set; }
            public bool ShouldSerializeQuantity()
            {
                return (Quantity != null);
            }
            [XmlElement(ElementName = "currencyId")]
            public string CurrencyId { get; set; }
            [XmlIgnore]
            public bool IsRetail { get; set; }
        }

        public class PriceItem : PriceType
        {
            [XmlElement(ElementName = "value")]
            public decimal Price { get; set; }
        }

        public class OfferItem : BaseItem
        {
            public string CategoryId { get; set; }
            public string Name { get; set; }
            public PriceItem RetailPrice { get; set; } = null;
            public IEnumerable<PriceItem> PriceItems { get; set; } = null;
            public PriceItem BulkPrice { get; set; } = null;// оптовая цена
            public string Description { get; set; }
            public string BarCode { get; set; } // код товара в 1с
            public string Available { get; set; }
            public string Presence { get; set; } // наличие
            public IEnumerable<string> SearchStrings { get; set; } = null;
            public int? QuantityInStock { get; set; }
            public IEnumerable<ParameterItem> Parameters { get; set; } = null;
            //... add necessary fields

        }


        public class OffersItem : BaseItem // temporary class, replace main OfferItem class after debug is comlete
        {
            [XmlElement(ElementName = "categoryId")]
            public string CategoryId { get; set; }

            [XmlElement(ElementName = "name")]
            public string Name { get; set; }

            [XmlElement(ElementName = "price")]
            public decimal? RetailPrice { get; set; }

            [XmlElement(ElementName = "currencyId")]
            public string RetailPriceCurrencyId { get; set; }

            [XmlArray(ElementName = "prices"), XmlArrayItem(ElementName = "price")]
            public PriceItem[] PriceItems { get; set; } = null;
            public PriceItem BulkPrice { get; set; } = null;// оптовая цена

            [XmlElement(ElementName = "description")]
            public string Description { get; set; }

            [XmlElement(ElementName = "barcode")]
            public string BarCode { get; set; } // код товара в 1с

            [XmlAttribute(AttributeName = "available")]
            public string Available { get; set; }

            [XmlAttribute(AttributeName = "presence")]
            public string Presence { get; set; } // наличие

            [XmlIgnore]
            public string BaseUnit { get; set; } // единица измерения

            public string[] SearchStrings { get; set; } = null;

            [XmlElement(ElementName = "quantity_in_stock")]
            public int? QuantityInStock { get; set; }

            [XmlIgnore]
            public ParameterItem[] Parameters { get; set; } = null;

            [XmlAttribute(AttributeName = "selling_type")]
            public string SellingType { get; set; }

            //... add necessary fields

        }

        public class Currency
        {
            [XmlAttribute(AttributeName = "id")]
            public string CurrencyId { get; set; }

            [XmlIgnore]
            public string CurrencyCode { get; set; }

            [XmlAttribute(AttributeName = "rate")]
            public decimal Rate { get; set; }
            [XmlAttribute(AttributeName = "symbol")]
            public string Symbol { get; set; }

        }

        [Serializable]
        public class yml_catalog
        {
            public Shop shop { get; set; }

            public yml_catalog(string inputFileNameInStock = Defaults.DefaultInputFileName)
            {
                this.shop = new Shop(inputFileNameInStock);
            }

            private yml_catalog() // parameterless constructor is needed for serialization
            {
                this.shop = new Shop();
            }

            public class Shop
            {
                private XmlDocument xmlDocument;
                // private string inputFileNameInStock;
                private XmlNode rootNode;
                private MapperConfiguration MapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<PriceType, PriceItem>());
                private Mapper Mapper;

                private Shop() // parameterless constructor is needed for serialization
                { }

                public Shop(string inputFileNameInStock = Defaults.DefaultInputFileName)
                {
                    //                    inputFileNameInStock = "..\\..\\..\\Data\\instock.cml";
                    xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(File.ReadAllText(inputFileNameInStock));
                    rootNode = xmlDocument.FirstChild.NextSibling;
                    Mapper = new Mapper(MapperConfig);
                    DateTime start = DateTime.UtcNow;
                    this.Currencies = FillCurrencies();
                    this.Categories = FillCategories();
                    this.PriceTypes = FillPriceTypes();
                    this.Offers = FillOfferItems();
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


                private Currency[] FillCurrencies()
                {
                    return new Currency[]
                        {
                        new Currency { CurrencyId = "UAH", Rate = 1, CurrencyCode = "980", Symbol="₴" },
                        new Currency { CurrencyId = "USD", Rate = 42, CurrencyCode = "840", Symbol = "$"},
                        new Currency { CurrencyId = "EUR", Rate = 45, CurrencyCode = "978", Symbol = "€" }
                        };
                }

                private ProductCategory[] FillCategories()
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

                    return categories.ToArray();
                }

                private PriceType[] FillPriceTypes()
                {
                    Console.WriteLine("Filling price types ...");
                    var priceTypesXml = rootNode.SelectSingleNode("ПакетПредложений/ТипыЦен").ChildNodes;
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
                    Console.WriteLine("Filling price types complete. Total {0}", priceTypes.Count);

                    return priceTypes.ToArray();
                }

                private OffersItem[] FillOfferItems()
                {
                    var offers = new List<OffersItem>();

                    Console.WriteLine("Filling product list ...");
                    var offersXml = rootNode.SelectSingleNode("ПакетПредложений/Предложения").ChildNodes;
                    foreach (XmlNode node in offersXml)
                    {
                        var prices = new List<PriceItem>();
                        XmlNode pricesNodeXml = node.SelectSingleNode("Цены");
                        foreach (XmlNode price in pricesNodeXml)
                        {
                            var Id = price.SelectNodes("ИдТипаЦены").Item(0).InnerText;
                            var p = price.SelectNodes(" ЦенаЗаЕдиницу").Item(0).InnerText
                                .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                            PriceItem priceItem = Mapper.Map<PriceItem>(PriceTypes.First(p => p.Id == Id));
                            priceItem.Price = Decimal.Parse(p);

                            prices.Add(priceItem);
                        }

                        OffersItem item = new OffersItem()
                        {
                            Id = node.SelectNodes("Ид").Item(0).InnerText,
                            RetailPrice = prices.FirstOrDefault(p => p.IsRetail, null).Price,
                            RetailPriceCurrencyId = prices.FirstOrDefault(p => p.IsRetail, null).CurrencyId,
                            PriceItems = prices.Where(p => !p.IsRetail).Any() ? prices.Where(p => !p.IsRetail).ToArray() : null,
                            SellingType = prices.Where(p => !p.IsRetail).Any() ? "u" : "r"

                        };
                        offers.Add(item);
                    }
                    Console.WriteLine("Filling product list complete. Total {0}", offers.Count);
                    // fill offers list --end

                    // fill goods list --start
                    Console.WriteLine("Filling product characteristics ...");
                    var goodsXml = rootNode.SelectSingleNode("ПакетПредложений/Товары").ChildNodes;
                    var updatedOffers = new List<OffersItem>();

                    DateTime start = DateTime.UtcNow;

                    var keywords = new FillKeywords().Keywords;
                    var counter = 0;
                    foreach (XmlNode node in goodsXml)
                    {
                        counter++;

                        OffersItem item = offers.FirstOrDefault(o => o.Id == node.SelectNodes("Ид").Item(0).InnerText);
                        item.Name = node.SelectNodes("Наименование")?.Item(0)?.InnerText ?? "";
                        item.Description = node.SelectNodes("Описание")?.Item(0)?.InnerText;
                        item.CategoryId = node.SelectNodes("Группы/Ид").Item(0).InnerText ?? "";
                        item.BarCode = node.SelectNodes("КодТовара").Item(0).InnerText;
                        item.BaseUnit = node.SelectNodes("БазоваяЕдиница").Item(0).InnerText;

                        // keywords
                        //if (keywords.Any())
                        //{
                        //    item.SearchStrings = keywords.First(kw => kw.Id == item.Id).Keys;
                        //}
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
                            item.Available = "true"; // bool.Parse(parameters.Find(p => p.Id == "ИД-Наличие").Value).ToString();
                            item.Presence = "available";
                            item.QuantityInStock = Int32.Parse(parameters.First(p => p.Id == "ИД-Количество").Value);

                            // filtering wholesale prices
                            // Если остаток товара меньше, чем минимальное количество заказа при этой цене, то эта цена не добавляется в список
                            if (item.PriceItems != null && item.PriceItems.Length > 0)
                            {
                                var filteredPriceItems = item.PriceItems.Where(p => p.Quantity <= item.QuantityInStock).ToArray();
                                if (filteredPriceItems.Length > 0)
                                {
                                    item.PriceItems = filteredPriceItems;
                                }
                                else
                                {
                                    item.PriceItems = null;
                                }
                            }

                            var pars = parameters.Where(p => String.Compare(p.Name, "Количество") != 0 && String.Compare(p.Name, "Наличие") != 0);
                            if (pars.Any())
                            {
                                item.Parameters = pars.ToArray();
                            }
                        }

                        Console.WriteLine("Loaded {0} of {1}, {2} {3}", counter, goodsXml.Count, item.BarCode, item.Name);

                        updatedOffers.Add(item);
                    }
                    //var offersIds = offers.Select(p => p.Id);
                    //var updatedOffersIds = updatedOffers.Select(p => p.Id);
                    //var difference = offersIds.Except(updatedOffersIds);
                    offers = updatedOffers;

                    Console.WriteLine("Filling product characteristics complete.");

                    return offers.ToArray();
                }


            }

        }

    }
}
