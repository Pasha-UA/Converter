using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace xml2json_converter
{
    public static class StringExtensions
    {
        public static string GetCustomHashStringValue(this string input)
        {
            // Ensure input is not null
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Convert the input string to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            // Calculate the hash using a simple algorithm
            uint hash = 7;

            unchecked
            {

                foreach (byte b in bytes)
                {
                    hash = hash * 11 + b;
                }
            }
            return hash.ToString();
        }
    }
}