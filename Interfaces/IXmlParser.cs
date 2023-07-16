using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace xml2json_converter
{
    public interface IXmlParser
    {
        XmlNode GetRootNode(XmlDocument xmlDocument);
        object[] Parse(Type outputType);
    }
}