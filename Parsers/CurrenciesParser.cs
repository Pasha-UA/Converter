using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using xml2json_converter.DataTypes;

namespace xml2json_converter.Parsers
{
    public class CurrenciesParser : XmlItemParser<Currency>
    {
        public CurrenciesParser(XmlDocument xmlDocument) : base(xmlDocument)
        {
        }

        public override Currency[] Parse()
        {
            return new Currency[]
            {
            new Currency { CurrencyId = "UAH", Rate = 1, CurrencyCode = "980", Symbol="₴" },
            new Currency { CurrencyId = "USD", Rate = 42, CurrencyCode = "840", Symbol = "$"},
            new Currency { CurrencyId = "EUR", Rate = 45, CurrencyCode = "978", Symbol = "€" }
            };
        }
    }
}