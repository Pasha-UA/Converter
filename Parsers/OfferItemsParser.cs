using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Xml;
using AutoMapper;
using ConverterProject;
using Serilog;
using xml2json_converter.DataTypes;

namespace xml2json_converter.Parsers
{
    public class OfferItemsParser : XmlItemParser<OfferItem>
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
            // первый проход. Заполнение списка товаров и части полей
            Log.Information("Filling product list ...");
            List<OfferItem> offers = OfferItemsParse(this.RootNode.SelectSingleNode("ПакетПредложений/Предложения").ChildNodes);
            Log.Information($"Filling product list complete. Total {offers.Count}");

            // fill goods list --start
            // второй проход. Заполнение оставшихся полей товара.
            Log.Information("Filling product characteristics ...");
            var goodsXml = this.RootNode.SelectSingleNode("ПакетПредложений/Товары").ChildNodes;
            var updatedOffers = new List<OfferItem>();

            var keywords = new KeywordsAdder().Keywords;
            var counter = 0;
            foreach (XmlNode node in goodsXml)
            {
                counter++;

                // если товар имеет разновидности, то его id имеет вид 'id_товара#id_уникальной_разновидности_товара', символ '#' - разделитель двух частей id
                var splittedId = node.SelectNodes("Ид").Item(0).InnerText.Split('#');

                OfferItem item = offers.FirstOrDefault(o => o.Id == splittedId.LastOrDefault());// || o.Id == splittedId.LastOrDefault().GetCustomHashStringValue()));

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

                    item.Available = parameters.First(p => p.Id == "ИД-Наличие").Value;
                    if (item.Available == "true")
                    {
                        item.Presence = "available";
                    }

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
                        // если нет оптовых цен
                        {
                            item.PriceItems = null;
                        }
                    }

                    // если есть и оптовая(ые) цена(ы) и розничная, то тип продажи 'u' (universal)
                    // если есть только оптовая(ые) цена(ы) и нет розничной, то тип продажи 'w' (wholesale)
                    // если есть только розничная цена и нет оптовых, то тип продажи 'r' (retail)
                    // Check if there are both wholesale and retail prices or neither wholesale and retail
                    if (item.PriceItems != null && item.RetailPrice != null || item.PriceItems == null && item.RetailPrice == null)
                    {
                        item.SellingType = "u"; // Universal
                    }
                    // Check if there are only wholesale prices
                    else if (item.PriceItems != null && item.RetailPrice == null)
                    {
                        item.SellingType = "w"; // Wholesale
                    }
                    // Check if there is only a retail price
                    else if (item.PriceItems == null && item.RetailPrice != null)
                    {
                        item.SellingType = "r"; // Retail
                    }

                    // Количество и Наличие записаны как "параметры". Фильтруем параметры от этих двух значений
                    var pars = parameters.Where(p => String.Compare(p.Name, "Количество") != 0 && String.Compare(p.Name, "Наличие") != 0);
                    if (pars.Any())
                    {
                        item.Parameters = pars.ToArray();
                    }
                }

                Log.Information("Loaded {0} of {1}, {2} {3}", counter, goodsXml.Count, item.BarCode, item.Name);

                updatedOffers.Add(item);
            }
            //var offersIds = offers.Select(p => p.Id);
            //var updatedOffersIds = updatedOffers.Select(p => p.Id);
            //var difference = offersIds.Except(updatedOffersIds);
            offers = updatedOffers.OrderByDescending(offer => offer.BarCode)
                        .ThenBy(offer => offer.QuantityInStock > 0 ? 0 : offer.RetailPrice > 0 ? 1 : 2) // 1) Остаток > 0, 2) Остаток == 0 и цена > 0, 3) Остаток == 0 и цена == 0
                        .ThenBy(offer => offer.QuantityInStock > 0 ? offer.RetailPrice : 0)             // Если остаток > 0, сортируем по цене
                        .ToList();

            Log.Information("Filling product characteristics complete.");

            return offers.ToArray();
        }

        private List<PriceItem> ParsePricesForItem(XmlNode pricesNodeXml)
        {
            // XmlNode pricesNodeXml = node;
            List<PriceItem> prices = new List<PriceItem>();
            foreach (XmlNode price in pricesNodeXml)
            {
                var Id = price.SelectNodes("ИдТипаЦены").Item(0).InnerText;
                var p = price.SelectNodes(" ЦенаЗаЕдиницу").Item(0).InnerText
                    .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                PriceItem priceItem = Mapper.Map<PriceItem>(PriceTypes.First(p => p.Id == Id));
                priceItem.Price = Decimal.Parse(p);

                prices.Add(priceItem);
            }
            return prices;
        }

        private List<OfferItem> OfferItemsParse(XmlNodeList offersXml)
        {
            var offers = new List<OfferItem>();
            foreach (XmlNode node in offersXml)
            {
                var prices = ParsePricesForItem(node.SelectSingleNode("Цены"));

                // если товар имеет разновидности, то его id имеет вид 'id_товара#id_уникальной_разновидности_товара', символ '#' - разделитель двух частей id
                var splittedId = node.SelectNodes("Ид").Item(0).InnerText.Split('#');

                OfferItem item = new OfferItem()
                {
                    // если разновидности товара, то id должен быть числовой, поэтому вычисляем его hash, если разновидностей нет, то можно оставить как есть изначально
                    Id = splittedId.LastOrDefault(),
                    GroupId = splittedId.Length > 1 ? splittedId[0].GetCustomHashStringValue() : "",
                    RetailPrice = prices.FirstOrDefault(p => p.IsRetail)?.Price,
                    RetailPriceCurrencyId = prices.FirstOrDefault(p => p.IsRetail)?.CurrencyId,
                    PriceItems = prices.Where(p => !p.IsRetail).Any() ? prices.Where(p => !p.IsRetail).ToArray() : null,
                };
                offers.Add(item);
            }
            return offers;
        }

    }
}
