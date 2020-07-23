using Dominio.Interfaces;
using Repositorio;
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
        private RepositorioEF repositorio;
        static IMercadoActualizar mercadoActualizar = null;
        static IMercadoBuscar mercadoBuscar = null;

        public HomeController()
        {
            repositorio = new RepositorioEF(new DDbContext());
            mercadoActualizar = new MercadoCargar(new List<IProvider> { new YobitProvider() }, repositorio);
            mercadoBuscar = new MercadoBuscar(repositorio);

        }

        public async Task<JsonResult> ActualizarMonedas()
        {
            var stopwatch = Stopwatch.StartNew();
            await mercadoActualizar.ActualizarMonedas();
            repositorio.GuardarCambios();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}, Monedas: {string.Join(",",mercadoActualizar.RelacionesEntreMonedasHash.Keys)}" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ActualizarRelaciones()
        {
            var stopwatch = Stopwatch.StartNew();
            await mercadoActualizar.ActualizarRelaciones();
            repositorio.GuardarCambios();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CalcularJugadas()
        {
            var stopwatch = Stopwatch.StartNew();
            mercadoBuscar.CalcularJugadas();
            repositorio.GuardarCambios();
            return Json(new { Mensaje = $"Tiempo: {stopwatch.ElapsedMilliseconds * 0.001M}" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }


    }
}