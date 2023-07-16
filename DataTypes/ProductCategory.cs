using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ConverterProject;

namespace xml2json_converter.DataTypes
{
        public class ProductCategory : BaseItem
        {
            [XmlAttribute(AttributeName = "parentId")]
            public string ParentId { get; set; }
            [XmlText]
            public string Name { get; set; }
        }
}