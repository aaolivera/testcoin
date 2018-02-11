using Ninject.Extensions.Logging;
using Servicios.Interfaces;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ILogger log, IOperador servicio)
        {
            Log = log;
            Servicio = servicio;
        }

        public ILogger Log { get; }
        public IOperador Servicio { get; }

        public ActionResult Index()
        {
            ViewBag.Relaciones = Servicio.ListarRelacionesReelevantes();
            ViewBag.Estado = Servicio.ObtenerEstado();

            return View();
        }

        public int ObtenerCantidadActualizada()
        {
            return Servicio.ObtenerEstado().RelacionesActualizadas;
        }

        public ActionResult Actualizar()
        {
            Task.Run(() => Servicio.ActualizarOrdenes());
            return RedirectToAction("Index");
        }

    }
}