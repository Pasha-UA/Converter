using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace xml2json_converter.DataTypes
{
        public class PriceItem : PriceType
        {
            [XmlElement(ElementName = "value")]
            public decimal Price { get; set; }
        }

}