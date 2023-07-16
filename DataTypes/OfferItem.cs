using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConverterProject;

namespace xml2json_converter.DataTypes
{
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
}