using CloudFlareUtilities;
using System.Net.Http;
using System.Text;
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

        private async Task<string> GetExternalResponse(string url)
        {
            var handler = new ClearanceHandler
            {
                MaxRetries = 2
            };
            var client = new HttpClient(handler);
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsByteArrayAsync();

            return Encoding.UTF8.GetString(result);
        }
    }
}