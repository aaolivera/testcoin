using CloudFlareUtilities;
using Newtonsoft.Json;
using System.Collections.Generic;
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
            var result = GetExternalResponse(url);

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
            System.Diagnostics.Debug.WriteLine("1111111111111111111111111");
            var results = await Task.WhenAll(tasks);
            System.Diagnostics.Debug.WriteLine("22222222222222222222222");
            var json =  JsonConvert.SerializeObject(results);
            System.Diagnostics.Debug.WriteLine("333333333333333333333");
            return json;
        }

        private async Task<string> GetExternalResponseAsync(string url)
        {
            return await Task.Run(() => GetExternalResponse(url));
        }

        private string GetExternalResponse(string url)
        {
            System.Diagnostics.Debug.WriteLine("iniciooooooooooooooooo");
            var handler = new ClearanceHandler
            {
                MaxRetries = 2
            };
            var client = new HttpClient(handler);
            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsByteArrayAsync().Result;
            var resultstr = Encoding.UTF8.GetString(result);
            System.Diagnostics.Debug.WriteLine("Finnnnnnnnnnnnnnnnnnnnnnn");
            return resultstr;
        }

        public string Version()
        {
            return "1";
        }
    }
}