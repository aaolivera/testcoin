using Dominio.Entidades;
using Dominio.Interfaces;
using Providers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        static Mercado mercado = null;

        public HomeController()
        {
            if(mercado == null)
            {
                mercado = new Mercado(new List<IProvider> { new YobitProvider() }, new List<string> { "dash" });
            }
        }

        public async Task<JsonResult> ActualizarMonedas()
        {
            var stopwatch = Stopwatch.StartNew();
            await mercado.ActualizarMonedas();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}, Monedas: {mercado.Monedas.Count}"},JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ActualizarOrdenes()
        {
            var stopwatch = Stopwatch.StartNew();
            await mercado.ActualizarRelaciones();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }


    }
}