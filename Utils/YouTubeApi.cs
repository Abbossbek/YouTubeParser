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
        private static string BASE_URL = $"https://youtube.googleapis.com/youtube/v3/channels?part=snippet%2CcontentDetails%2Cstatistics&";
        private static Dictionary<string, string> HEADERS = new Dictionary<string, string>() { { "Accept", "application/json" } };
        
        public async Task<string> GetIdFromLink(string url)
        {
            if (url.Contains("/channel/"))
                return url.Split("/channel/")[1];
            var username = url.Remove(0,url.LastIndexOf('/')+1);
            BASE_URL += $"forUsername=${username}&key=${API_KEY}";
            using (var request = CreateRequestMessage(HttpMethod.Post, url, HEADERS))
            {
                var result = await SendAsync(request);
                var jElement = JsonSerializer.Deserialize<JsonElement>(await result.Content.ReadAsByteArrayAsync());
                return jElement.EnumerateArray().ElementAt(0).GetString();
            }

        }
        public async Task<Channel> GetChannelById(string id)
        {
            BASE_URL += $"id={id}&key={API_KEY}";
            using (var request = CreateRequestMessage(HttpMethod.Post, BASE_URL, HEADERS))
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
