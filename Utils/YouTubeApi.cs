using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using YouTubeParser.Models;

namespace YouTubeParser.Utils
{
    public class YouTubeApi:HttpClient
    {
        private static string API_KEY = "AIzaSyBcVjOPtbwM0UjVZeKb495n14453N9cQ6w";
        private static string TOKEN = "";
        private static string url = $"https://youtube.googleapis.com/youtube/v3/channels?part=snippet%2CcontentDetails%2Cstatistics&";
        private static Dictionary<string, string> HEADERS = new Dictionary<string, string>() { { "Authorization", "Bearer " + TOKEN }, { "Accept", "application/json" } };
        public async Task<Channel> GetChannelById(string id)
        {
            url = url + $"id={id}&key={API_KEY}";
            using (var request = CreateRequestMessage(HttpMethod.Post, url, HEADERS))
            {
                var result = await SendAsync(request);
                return JsonSerializer.Deserialize<Channel>(await result.Content.ReadAsByteArrayAsync());
            }
        }
        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url, Dictionary<string, string> headers = null)
        {
            var httpRequestMessage = new HttpRequestMessage(method, url);
            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            return httpRequestMessage;
        }
    }
}
