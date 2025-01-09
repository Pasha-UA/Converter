using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using xml2json_converter.DataTypes;
using System.Reflection;
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
            return xmlDocument.FirstChild.NextSibling;
        }

        public abstract T[] Parse();

        public object[] Parse(Type outputType)
        {
            try
            {
                var methodInfo = GetType().GetMethod("Parse");
                if (methodInfo == null)
                {
                    throw new InvalidOperationException("Parse method not found.");
                }

                var genericMethod = methodInfo.MakeGenericMethod(outputType);
                var result = genericMethod.Invoke(this, null);
                return ((Array)result).Cast<object>().ToArray();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while parsing: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}