using AutoMapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using xml2json_converter;
using xml2json_converter.DataTypes;

namespace ConverterProject
{
    // public class ItemTypes
    // {

        [Serializable]
        public class yml_catalog
        {
            public Shop shop { get; set; }

            public yml_catalog(string inputFileNameInStock = Defaults.DefaultInputFileName)
            {
                this.shop = new Shop(inputFileNameInStock);
            }

            private yml_catalog() // parameterless constructor is needed for serialization
            {
                this.shop = new Shop();
            }
        }
    // }
}
