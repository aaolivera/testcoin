
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Helper
{
    public class HttpClientApp
    {
        private HttpClient client;

        public HttpClientApp()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        public async Task<byte[]> GetAsync(string url)
        {
            return await Task.Run(() => Get(url));
        }

        private async Task<byte[]> Get(string url)
        {
            return await client.GetByteArrayAsync(url);
        }

        public dynamic Post(string url, string body, Dictionary<string, string> headers)
        {
            HttpContent queryString = new StringContent(body);
            queryString.Headers.Clear();
            foreach (var i in headers.Keys)
            {
                queryString.Headers.Add(i, headers[i]);
            }
            var response = client.PostAsync(url, queryString).Result;
            var result = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(result);
        }
    }
}
