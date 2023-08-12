using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FriendpasteClient
{
    public static class FriendpasteClient
    {
        public const string BaseUrl = "https://www.friendpaste.com/";
        private const string JsonContentType = "application/json";
        private static readonly HttpClient HttpClient = CreateHttpClient();

        public static async Task<string> PostDataAsync(string postUrl, string title, string body)
        {
            var postData = GenerateJsonData(title, body);
            using var content = new StringContent(postData, Encoding.UTF8, JsonContentType);

            var response = await HttpClient.PostAsync(postUrl, content);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task PutDataAsyncWithTimeout(string putUrl, string title, string body)
        {
            var putData = GenerateJsonData(title, body);
            using var content = new StringContent(putData, Encoding.UTF8, JsonContentType);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Timeout of x seconds

            try
            {
                Console.WriteLine($"PUT will be sent to {putUrl}");
                await HttpClient.PutAsync(putUrl, content, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"PUT operation timed out after 5 seconds.");
            }
        }

        public static void PutDataFireAndForget(string putUrl, string title, string body)
        {
            var putData = GenerateJsonData(title, body);
            using var content = new StringContent(putData, Encoding.UTF8, JsonContentType);

            // Fire and forget by not awaiting the task
            _ = HttpClient.PutAsync(putUrl, content);
            Console.WriteLine($"PUT sent to {putUrl}");
        }

        public static async Task<string> GetDataAsync(string getUrl)
        {
            var response = await HttpClient.GetAsync(getUrl);
            return await response.Content.ReadAsStringAsync();
        }

        private static string GenerateJsonData(string title, string body)
        {
            var jsonData = $@"{{""title"": ""{title}"",""snippet"": ""{body}"",""language"": ""text""}}";
            return jsonData;
        }

        public static string PrepareJSONStringForBodyArgument(string input)
        {
            // Remove line breaks and double quotes from the input string
            return input.Replace("\n", "").Replace("\r", "").Replace("\"", "`");
        }

        public static string PrepareFriendPasteSnippetForCSharpJSONParse(string input)
        {
            // Replace backticks with double quotes for JSON serialization in C#
            return input.Replace("\n", "").Replace("\r", "").Replace("`", "\"");
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));
            return httpClient;
        }
    }
}
