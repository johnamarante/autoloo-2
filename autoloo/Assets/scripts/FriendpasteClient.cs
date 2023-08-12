using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FriendpasteClient
{
    public static class FriendpasteClient
    {
        public const string baseURL = "https://www.friendpaste.com/";
        private const string JsonContentType = "application/json";

        public static async Task<string> PostDataAsync(string postURL, string title, string body)
        {
            var postData = GenerateJsonData(title, body);

            using (var client = CreateHttpClient())
            using (var content = new StringContent(postData, Encoding.UTF8, JsonContentType))
            {
                var response = await client.PostAsync(postURL, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task PutDataAsyncWithTimeout(string postURL, string title, string body)
        {
            var putData = GenerateJsonData(title, body);

            using (var client = CreateHttpClient())
            using (var content = new StringContent(putData, Encoding.UTF8, JsonContentType))
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5))) // Timeout of x seconds
            {
                try
                {
                    Console.WriteLine($"PUT will be sent to {postURL}");
                    await client.PutAsync(postURL, content, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"PUT operation timed out after 5 second.");
                }
            }
        }

        public static void PutDataFireAndForget(string postURL, string title, string body)
        {
            var putData = GenerateJsonData(title, body);

            using (var client = CreateHttpClient())
            using (var content = new StringContent(putData, Encoding.UTF8, JsonContentType))
            {
                // Fire and forget by not awaiting the task
                _ = client.PutAsync(postURL, content);
                Console.WriteLine($"PUT sent to {postURL}");
            }
        }


        public static async Task<string> GetDataAsync(string postURL)
        {
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync(postURL);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string GenerateJsonData(string title, string body)
        {
            var jsonData = $@"{{""title"": ""{title}"",""snippet"": ""{body}"",""language"": ""text""}}";
            //breaking this into a variable for readability and easier debugging
            return jsonData;
        }

        public static string PrepareJSONStringForBodyArgument(string input)
        {
            // Remove line breaks and double quotes from the input string
            string output = input.Replace("\n", "").Replace("\r", "").Replace("\"", "`");
            return output;
        }
        public static string PrepareFriendPasteSnippetForCSharpJSONParse(string input)
        {
            // Replace backticks with double quotes for JSON serialization in C#
            string output = input.Replace("\n", "").Replace("\r", "").Replace("`", "\"");
            return output;
        }
        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));
            return httpClient;
        }
    }
}
