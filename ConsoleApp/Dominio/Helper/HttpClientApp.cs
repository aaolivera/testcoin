
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Helper
{
    public class HttpClientApp : HttpClient
    {
        public HttpClientApp(): base(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip,
            Proxy = null,
            UseProxy = false
        })
        {
            this.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            this.Timeout = new System.TimeSpan(0,0,0,15);
        }

        public async Task<byte[]> Get(string url)
        {
            return await GetByteArrayAsync(url);
        }

        public async Task<dynamic> PostAsync(string url, string body, Dictionary<string, string> headers)
        {
            HttpContent queryString = new StringContent(body);
            queryString.Headers.Clear();
            foreach (var i in headers.Keys)
            {
                queryString.Headers.Add(i, headers[i]);
            }
            var response = await PostAsync(url, queryString);
            var result = await response.Content.ReadAsByteArrayAsync();
            return Encoding.UTF8.GetString(result);
        }
    }
}
