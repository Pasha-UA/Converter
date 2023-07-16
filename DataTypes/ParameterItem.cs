using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ConverterProject;

namespace xml2json_converter.DataTypes
{
    public class ParameterItem : BaseItem
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Value { get; set; }

        public bool ShouldSerializeId()
        {
            return false;
        }
    }
}
