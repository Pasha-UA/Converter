using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace xml2json_converter.DataTypes
{
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
}