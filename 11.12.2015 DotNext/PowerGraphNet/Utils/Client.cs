using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PowerGraphNet.Utils
{
    internal static class Client
    {
        private static readonly HttpClient ServiceClient = new HttpClient();

        public static async Task<string> GetString(Uri serviceAddress, string path, string appRequestId, string referer)
        {
            var service = new UriBuilder(serviceAddress) { Path = path };
            var request = new HttpRequestMessage(HttpMethod.Get, service.Uri);
            request.Headers.Add("Referer", referer);
            request.Headers.Add("AppRequestId", appRequestId);

            var response = await ServiceClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<string>(content);
        }
    }
}