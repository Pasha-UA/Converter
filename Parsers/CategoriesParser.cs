using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Serilog;
using xml2json_converter.DataTypes;

namespace xml2json_converter.Parsers
{
    public class CategoriesParser : XmlItemParser<ProductCategory>
    {

        public CategoriesParser(XmlDocument xmlDocument) : base(xmlDocument)
        {
        }
        public override ProductCategory[] Parse()
        {
            var categories = new List<ProductCategory>();
            // Console.WriteLine("Filling categories list ...");
            var categoriesXml = this.RootNode.SelectSingleNode("Классификатор/Группы").ChildNodes;
            foreach (XmlNode node in categoriesXml)
            {
                ProductCategory item = new ProductCategory()
                {
                    Id = node.SelectNodes("Ид").Item(0)?.InnerText ?? string.Empty,
                    ParentId = node.SelectNodes("Родитель").Item(0)?.InnerText,
                    Name = node.SelectNodes("Наименование").Item(0)?.InnerText ?? "Unnamed Category"
                };
                categories.Add(item);
                Log.Information($"Category {item.Name} added.");
            }
            var logString = $"Filling categories list complete. Total {categories.Count}" ;
            Log.Information(logString);

            return categories.ToArray();
        }

    }

}