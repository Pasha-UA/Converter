using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private static readonly string ApiUrl = Defaults.DefaultApiUrl;
        // private static readonly string ApiUrl = "https://my.prom.ua/api/v1/products/import_file";
        private static readonly HttpClient client = new HttpClient();

        public static async Task<int> UploadData(string fileName, string secretToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(secretToken))
                {
                    Log.Error("Secret token is missing. Cannot upload file.");
                    return -1;
                }

                await using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using var content = CreateMultipartContent(fileStream);

                // Use a request message to set the per-request authorization header.
                // This is safer than modifying the static client's DefaultRequestHeaders.
                using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretToken);

                Log.Information("Uploading file '{FileName}' to server...", Path.GetFileName(fileName));

                var response = await client.SendAsync(request);
                return await HandleResponse(response);
            }
            catch (HttpRequestException httpEx)
            {
                Log.Error(httpEx, "An HTTP error occurred during file upload");
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred during file upload");
                return -1;
            }
        }

        private static MultipartFormDataContent CreateMultipartContent(Stream fileStream)
        {
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
                { new StreamContent(fileStream), "file", "import.xml" },
                { new StringContent(jsonAttributes), "attributes" }
            };

            return content;
        }

        private static async Task<int> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                Log.Information("File upload to server successful. Result code: {StatusCode}", response.StatusCode);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Warning("File upload to server failed. Result code: {StatusCode}. Response: {ErrorResponse}",
                    response.StatusCode, errorContent);
            }

            return (int)response.StatusCode;
        }
    }
}
