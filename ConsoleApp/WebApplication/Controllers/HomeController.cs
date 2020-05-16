using Dominio.Interfaces;
using Servicios.Impl;
using Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        static IMercadoActualizar mercadoActualizar = null;
        static IMercadoBuscar mercadoBuscar = null;

        public HomeController()
        {
            mercadoActualizar = new MercadoCargar(new List<IProvider> { new YobitProvider() });
            mercadoBuscar = new MercadoBuscar();

        }

        public async Task<JsonResult> ActualizarMonedas()
        {
            var stopwatch = Stopwatch.StartNew();
            await mercadoActualizar.ActualizarMonedas();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}, Monedas: {0}" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ActualizarRelaciones()
        {
            var stopwatch = Stopwatch.StartNew();
            await mercadoActualizar.ActualizarRelaciones();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CalcularJugadas()
        {
            var stopwatch = Stopwatch.StartNew();
            mercadoBuscar.CalcularJugadas();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }


    }
}