using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using static ConverterProject.ItemTypes;

namespace ConverterProject
{
    public class FillOffers
    {
        private XmlNode RootNode { get; set; }
        public FillOffers(XmlNode rootNode)
        {
            RootNode = rootNode;
            FillPriceTypes();
            FillCategories();
        }

        private List<PriceType> FillPriceTypes()
        {
            Console.WriteLine("Filling price types ...");
            var priceTypesXml = RootNode.SelectSingleNode("ПакетПредложений/ТипыЦен").ChildNodes;
            var PriceTypes = new List<PriceType>();
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
                PriceTypes.Add(item);
            }
            Console.WriteLine("Filling price types complete. Total {0}", PriceTypes.Count);

            return PriceTypes;
        }

        private List<ProductCategory> FillCategories()
        {
            var categories = new List<ProductCategory>();
            Console.WriteLine("Filling categories list ...");
            var categoriesXml = RootNode.SelectSingleNode("Классификатор/Группы").ChildNodes;
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


    }
}
