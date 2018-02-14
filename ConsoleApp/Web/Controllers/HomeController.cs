using Dominio.Entidades;
using Ninject.Extensions.Logging;
using Repositorio;
using Servicios.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ILogger log, IOperador servicio, IEstadoOperador estadoOperador, IRepositorio repositorio)
        {
            Log = log;
            Servicio = servicio;
            EstadoOperador = estadoOperador;
        }

        public ILogger Log { get; }
        public IOperador Servicio { get; }
        public IEstadoOperador EstadoOperador { get; }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ActualizarServidor()
        {
            if (!EstadoOperador.UpdateEnProgreso)
            {
                Task.Run(() => Servicio.ActualizarOrdenes());
            }
            return SuccessResponse("ok");
        }

        public JsonResult ListarRelaciones()
        {
            return SuccessResponse(Servicio.ListarRelaciones().ToList());
        }

        public JsonResult ObtenerEstadoServidor()
        {
            return SuccessResponse(EstadoOperador);
        }

        public JsonResult CrearJugada(Jugada jugada)
        {
            return null;
        }

        public JsonResult ActualizarJugada(Jugada jugada)
        {
            return null;
        }

        public JsonResult RefrescarDatosRelacion(string relacion)
        {
            return SuccessResponse(new Relacion(), "ok");
        }



    }
}