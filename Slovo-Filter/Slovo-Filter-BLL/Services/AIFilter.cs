using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Slovo_Filter_BLL.Services
{
    public class AIFilter
    {
        private string _content;
        public string content { get => _content; set => _content = value; }

        public async Task<string> AnalyzeContentAsync()
        {
            using HttpClient client = new HttpClient();

            string apiKey = "CgrdaCSnF5FSZLOsfK9qdXUTMGblVNOg4OStGJnCzqidF2Vtbo44JQQJ99BAACHYHv6XJ3w3AAAAACOGXKs5";

            client.DefaultRequestHeaders.Add("api-key", apiKey);
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpClient/1.0");

            string jsonBody = $@"{{
                ""messages"": [
                    {{
                        ""role"": ""system"",
                        ""content"": ""Analyze the given text for offensive content. Provide a JSON response with 'score' (0-5), 'type' (racism, hate speech, etc.), and 'meaning' (brief explanation, max 15 words, in Bulgarian). Output must be a valid JSON.""
                    }},
                    {{
                        ""role"": ""user"",
                        ""content"": ""{_content}""
                    }}
                ],
                ""max_tokens"": 100,
                ""temperature"": 0.7
            }}";

            HttpContent httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                string url = "https://tyata-m6919x7b-eastus2.cognitiveservices.azure.com/openai/deployments/gpt-4-2/chat/completions?api-version=2024-08-01-preview";

                HttpResponseMessage response = await client.PostAsync(url, httpContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDoc = JsonDocument.Parse(responseBody);
                string messageContent = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                return messageContent;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return e.Message;
            }
        }

        static async Task Main()
        {
            
            
            string input = Console.ReadLine();
            AIFilter filter = new AIFilter();
            filter.content = input;
            Console.InputEncoding = Encoding.UTF8;
            Console.WriteLine(await filter.AnalyzeContentAsync());
        }
    }
}
