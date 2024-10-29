using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace xml2json_converter
{
    public static class StringExtensions
    {
        // рассчитывает Hash от строки. результат - число максимально до 999999999.
        // используется для формирования нового id товара, когда у товара есть разновидности (характеристики) 
        public static string GetCustomHashStringValue(this string input)
        {
            // Ensure input is not null
            if (input == null)
                // throw new ArgumentNullException(nameof(input));
                return null;

            // Convert the input string to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            // Calculate the hash using a simple algorithm
            uint hash = 7;

            unchecked
            {

                foreach (byte b in bytes)
                {
                    hash = hash * 11 + b;
                    hash %= 1000000000;
                }
            }
            return hash.ToString();
        }

        // проверяет содержит ли строка HTML-теги
        public static bool ContainsHtmlTags(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false; // Return false for empty input
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(input);

            return doc.DocumentNode.Descendants().Any(node => node.NodeType == HtmlNodeType.Element);
        }
    }
}