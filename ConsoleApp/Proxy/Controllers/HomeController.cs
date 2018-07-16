using CloudFlareUtilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Proxy.Controllers
{
    public class HomeController : Controller
    {
        public async Task<string> Index(string url)
        {
            var result = await GetExternalResponse(url);

            return result;
        }

        [HttpPost]
        public async Task<string> Index(List<string> urls)
        {
            var tasks = new List<Task<string>>();
            foreach (var url in urls)
            {
                tasks.Add(GetExternalResponseAsync(url));
            }
            var results = await Task.WhenAll(tasks);
            
            var json = JsonConvert.SerializeObject(results);
            return json;
        }

        private async Task<string> GetExternalResponseAsync(string url)
        {
            return await Task.Run(() => GetExternalResponse(url));
        }

        private async Task<string> GetExternalResponse(string url)
        {
            //var handler = new ClearanceHandler
            //{
            //    MaxRetries = 2
            //};
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsByteArrayAsync();
            var resultstr = Encoding.UTF8.GetString(result);
            return resultstr;
        }

        public string Version()
        {
            return "3";
        }
    }
}