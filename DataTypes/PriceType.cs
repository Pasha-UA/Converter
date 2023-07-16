using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ConverterProject;

namespace xml2json_converter.DataTypes
{
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

}