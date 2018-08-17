using Dominio.Dtos;
using Dominio.Helper;
using Proxy.Filter;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Proxy.Controllers
{
    [Compress]
    public class HomeController : Controller
    {
        [HttpPost]
        public async Task<JsonResult> Index(List<string> urls)
        {
            var resultado = new ProxyResult();
            Stopwatch stopwatch = Stopwatch.StartNew();

            var tasks = new List<Task<byte[]>>();
            var client = new HttpClientApp();

            foreach (var url in urls)
            {
                tasks.Add(client.Get(url));
            }
            resultado.Responses = await Task.WhenAll(tasks);
            stopwatch.Stop();
            resultado.Tiempo = stopwatch.ElapsedMilliseconds * 0.001M;

            var jsonResult = Json(resultado, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public string Version()
        {
            return "3";
        }
    }
}