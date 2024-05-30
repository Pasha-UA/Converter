using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace xml2json_converter
{
    public interface IXmlItemParser 
    {
        XmlNode GetRootNode(XmlDocument xmlDocument);
        object[] Parse(Type outputType);
    }
}