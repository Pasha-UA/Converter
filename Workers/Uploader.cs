using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            public string Mark_missing_product_as { get; set; } = MissingProduct.Deleted;
            public string[] Updated_fields { get; set; } = new[] { "quantity_in_stock", "sku", "price", "presence", "description", "name" };
        }

        private static readonly string ApiUrl = "https://my.prom.ua/api/v1/products/import_file";

        public static async Task<int> UploadData(string fileName, string secretToken)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = CreateMultipartContent(fileName);
                    AddAuthorizationHeader(client, secretToken);

                    // Console.Write("Uploading file to server... ");
                    Log.Information("Uploading file to server...");

                    var response = await client.PostAsync(new Uri(ApiUrl), content);
                    return await HandleResponse(response);
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Console.ForegroundColor = ConsoleColor.Red;
                // Console.WriteLine("Error: " + httpEx.Message);
                // Console.ResetColor();
                Log.Error($"HTTP Request Error: {httpEx.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                // Console.ForegroundColor = ConsoleColor.Red;
                // Console.WriteLine("Error: " + ex.Message);
                // Console.ResetColor();
                Log.Error($"Unexpected Error: {ex.Message}");
                return -1;
            }
        }

        private static MultipartFormDataContent CreateMultipartContent(string fileName)
        {
            byte[] fileBytes = File.ReadAllBytes(fileName);

            var serializerOptions = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
                PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            };

            var importParameters = new ImportParameters();
            var jsonAttributes = JsonSerializer.Serialize(importParameters, serializerOptions);

            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileBytes), "file", "import.xml" },
                { new StringContent(jsonAttributes), "attributes" }
            };

            return content;
        }

        private static void AddAuthorizationHeader(HttpClient client, string secretToken)
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretToken);
        }

        private static async Task<int> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                Log.Information($"File upload to server successful. Result code: {response.StatusCode}");
            }
            else
            {
                Log.Warning($"File upload to server failed. Result code: {response.StatusCode}");
            }

            return (int)response.StatusCode;
        }
    }
}
