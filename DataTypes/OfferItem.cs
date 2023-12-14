using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ConverterProject;

namespace xml2json_converter.DataTypes
{
    public class OfferItem : BaseItem // temporary class, replace main OfferItem class after debug is comlete
    {
        [XmlElement(ElementName = "categoryId")] // родительская категория товаров
        public string CategoryId { get; set; }

        [XmlAttribute(AttributeName = "group_id")]
        public string GroupId { get; set; } // если товар с разновидностями, то это будет его основным id, а в поле id - уникальные разновидности
        public bool ShouldSerializeGroupId() // не выводить в xml это поле если оно пустое
        {
            return !string.IsNullOrEmpty(GroupId);
        }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "price")]
        public decimal? RetailPrice { get; set; }

        [XmlElement(ElementName = "currencyId")]
        public string RetailPriceCurrencyId { get; set; }

        [XmlArray(ElementName = "prices"), XmlArrayItem(ElementName = "price")]
        public PriceItem[] PriceItems { get; set; } = null;
        public PriceItem BulkPrice { get; set; } = null;// оптовая цена

        // всe эти манипуляции с description - для того что оно правильно сериализовывалось. нужно оставлять угловые скобки, не заменяя их на &lt &gt
        // и оборачивать все в тег CDATA
        private string _description;

        [XmlIgnore]
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        [XmlElement("description")]
        public XmlNode[] DescriptionCData
        {
            get
            {
                var dummy = new XmlDocument();
                return new XmlNode[] { dummy.CreateCDataSection(_description) };
            }
            set
            {
                if (value != null && value.Length > 0 && value[0] is XmlCDataSection cdata)
                {
                    _description = cdata.Value;
                }
            }
        }

        // [XmlElement(ElementName = "description")]
        // public string Description { get; set; }

        [XmlElement(ElementName = "barcode")]
        public string BarCode { get; set; } // код товара в 1с

        [XmlAttribute(AttributeName = "available")]
        public string Available { get; set; }

        [XmlAttribute(AttributeName = "presence")]
        public string Presence { get; set; } // наличие

        [XmlIgnore]
        public string BaseUnit { get; set; } // единица измерения

        public string[] SearchStrings { get; set; } = null;

        [XmlElement(ElementName = "stock_quantity")]
        public int? QuantityInStock { get; set; }

        [XmlElement(ElementName = "param")]
        // [XmlIgnore]
        public ParameterItem[] Parameters { get; set; } = null;

        [XmlAttribute(AttributeName = "selling_type")]
        public string SellingType { get; set; }

        //... add necessary fields

    }
}