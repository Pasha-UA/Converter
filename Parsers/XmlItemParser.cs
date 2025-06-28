using System;
using System.Linq;
using System.Xml;
using Serilog;

namespace xml2json_converter.Parsers
{
    public abstract class XmlItemParser<T>
    {
        public XmlNode RootNode { get; set; }

        public XmlItemParser(XmlDocument xmlDocument)
        {
            this.RootNode = GetRootNode(xmlDocument);
        }

        public XmlNode GetRootNode(XmlDocument xmlDocument)
        {
            // Using DocumentElement is more robust than FirstChild.NextSibling.
            // It correctly gets the root element regardless of whether an
            // <?xml ... ?> declaration is present or not.
            return xmlDocument.DocumentElement;
        }

        public abstract T[] Parse();
    }
}