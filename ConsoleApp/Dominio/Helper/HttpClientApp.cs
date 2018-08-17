
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
                AutomaticDecompression = DecompressionMethods.GZip,
                Proxy = null,
                UseProxy = false
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        public async Task<byte[]> Get(string url)
        {
            return await client.GetByteArrayAsync(url);
        }

        public async Task<dynamic> PostAsync(string url, string body, Dictionary<string, string> headers)
        {
            HttpContent queryString = new StringContent(body);
            queryString.Headers.Clear();
            foreach (var i in headers.Keys)
            {
                queryString.Headers.Add(i, headers[i]);
            }
            var response = await client.PostAsync(url, queryString);
            var result = await response.Content.ReadAsByteArrayAsync();
            return Encoding.UTF8.GetString(result);
        }
    }
}
