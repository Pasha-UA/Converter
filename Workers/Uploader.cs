using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ConverterProject
{
    public class Uploader
    {
        public class LowerCaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
                    return name;

                return name.ToLower();
            }
        }

        public static class MissingProduct
        {
            public static string None => nameof(None).ToLower();
            public static string Not_available => nameof(Not_available).ToLower();
            public static string Not_on_display => nameof(Not_on_display).ToLower();
            public static string Deleted => nameof(Deleted).ToLower();
        }

        private class ImportParameters
        {
            public bool Force_update { get; set; } = false;
            public bool Only_available { get; set; } = false;
            public string Mark_missing_product_as { get; set; } = MissingProduct.Not_available;

            //[ name, sku, price, images_urls, presence, quantity_in_stock, description, group, keywords, attributes, discount, labels, gtin, mpn ]

            public string[] Updated_fields { get; set; } = new[] { "quantity_in_stock", "sku", "price", "presence", "description", "name" };

            //public string[] Updated_fields { get; set; } = new[] { "sku", "price", "description", "name" };
            //public AdditionalData(bool? force_update, bool? only_avaiable, string? mark_missing_as, string[]? updated_fields)
            //{
            //    Force_update = force_update ?? false;
            //    Only_available = only_avaiable ?? false;
            //    Mark_missing_product_as = mark_missing_as ?? MissingProduct.Not_available;
            // [name, sku, price, images_urls, presence, stock_quantity, description, group, keywords, attributes, discount, labels, gtin, mpn]

            //    Updated_fields = updated_fields ?? new[] { "stock_quantity", "price", "presence" };
        }

        public static async Task<int> UploadData(string FileName, string secretToken)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // var stream = new StreamContent(File.OpenRead(FileName));
                    byte[] fileBytes = File.ReadAllBytes(FileName);

                    var serializerOptions = new JsonSerializerOptions()
                    {
                        IncludeFields = true,
                        WriteIndented = true,
                        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
                    };

                    var importParameters = new ImportParameters();

                    var jsonAttributes = JsonSerializer.Serialize(importParameters, typeof(ImportParameters), serializerOptions);

                    var content = new MultipartFormDataContent
                    {
                        { new ByteArrayContent(fileBytes), "file", "import.xml" },
                        { new StringContent(jsonAttributes), "attributes" }
                    };

                    var uri = new Uri("https://my.prom.ua/api/v1/products/import_file");

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretToken);

                    // Console.Write("Uploading file to server... ");
                    Log.Information("Uploading file to server... ");

                    var message = await client.PostAsync(uri, content);
                    if (message.IsSuccessStatusCode)
                    {
                        Log.Information($"File upload to server successful. Result code: {message.StatusCode}");

                    }
                    else
                    {
                        Log.Warning($"File upload to server failed. Result code: {message.StatusCode}");
                    }

                    return (int)message.StatusCode;
                }
            }
            catch (Exception ex)
            {
                // Console.ForegroundColor = ConsoleColor.Red;
                // Console.WriteLine("Error: " + ex.Message);
                // Console.ResetColor();
                Log.Error("Error: " + ex.Message);
                return -1;
            }
        }
    }
}

