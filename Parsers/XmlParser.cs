using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using xml2json_converter.DataTypes;

namespace xml2json_converter.Parsers
{
    public abstract class XmlParser<T> : IXmlParser
    {
        public XmlNode RootNode { get; set; }

        public XmlParser(XmlDocument xmlDocument)
        {
            this.RootNode = GetRootNode(xmlDocument);
        }

        public XmlNode GetRootNode(XmlDocument xmlDocument)
        
        {
            return xmlDocument.FirstChild.NextSibling;
        }

        public abstract T[] Parse();

        public object[] Parse(Type outputType)
        {
            var methodInfo = GetType().GetMethod("Parse");
            var genericMethod = methodInfo.MakeGenericMethod(outputType);
            var result = genericMethod.Invoke(this, null);
            return ((Array)result).Cast<object>().ToArray();
        }
    }
}