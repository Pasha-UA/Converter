using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using ConverterProject;
using xml2json_converter.DataTypes;

namespace xml2json_converter.Parsers
{
    public class OfferItemsParser : XmlParser<OfferItem>
    {
        private PriceType[] PriceTypes { get; set; }
        private MapperConfiguration MapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<PriceType, PriceItem>());
        private Mapper Mapper;

        public OfferItemsParser(XmlDocument xmlDocument, PriceType[] priceTypes) : base(xmlDocument)
        {
            this.PriceTypes = priceTypes;
            Mapper = new Mapper(MapperConfig);
        }

        public override OfferItem[] Parse()
        {
            var offers = new List<OfferItem>();

            Console.WriteLine("Filling product list ...");
            var offersXml = this.RootNode.SelectSingleNode("ПакетПредложений/Предложения").ChildNodes;
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

                // если товар имеет разновидности, то его id имеет вид 'id_товара#id_уникальной_разновидности_товара', символ '#' - разделитель двух частей id
                var splittedId = node.SelectNodes("Ид").Item(0).InnerText.Split('#');

                OfferItem item = new OfferItem()
                {
                    // если разновидности товара, то id должен быть числовой, поэтому вычисляем его hash, если разновидностей нет, то можно оставить как есть изначально
                    Id = splittedId.LastOrDefault(),
                    GroupId = splittedId.Length > 1 ? splittedId[0].GetCustomHashStringValue() : "",
                    RetailPrice = prices.FirstOrDefault(p => p.IsRetail, null).Price,
                    RetailPriceCurrencyId = prices.FirstOrDefault(p => p.IsRetail, null).CurrencyId,
                    PriceItems = prices.Where(p => !p.IsRetail).Any() ? prices.Where(p => !p.IsRetail).ToArray() : null,
                    //                            SellingType = prices.Where(p => !p.IsRetail).Any() ? "u" : "r"

                };
                offers.Add(item);
            }
            Console.WriteLine("Filling product list complete. Total {0}", offers.Count);
            // fill offers list --end

            // fill goods list --start
            Console.WriteLine("Filling product characteristics ...");
            var goodsXml = this.RootNode.SelectSingleNode("ПакетПредложений/Товары").ChildNodes;
            var updatedOffers = new List<OfferItem>();

            DateTime start = DateTime.UtcNow;

            var keywords = new FillKeywords().Keywords;
            var counter = 0;
            foreach (XmlNode node in goodsXml)
            {
                counter++;

                var splittedId = node.SelectNodes("Ид").Item(0).InnerText.Split('#');

                OfferItem item = (offers.FirstOrDefault(o => o.Id == splittedId.LastOrDefault()));// || o.Id == splittedId.LastOrDefault().GetCustomHashStringValue()));

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
                        // если есть оптовые цены, то тип продажи 'u' (universal), подразумевается что розничная цена есть всегда
                        {
                            item.PriceItems = filteredPriceItems;
                        }
                        else
                        // если есть только розничная цена
                        {
                            item.PriceItems = null;
                        }
                    }

                    item.SellingType = item.PriceItems != null ? "u" : "r";

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
