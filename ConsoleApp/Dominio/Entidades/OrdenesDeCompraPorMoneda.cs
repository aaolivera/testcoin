using System.Collections.Generic;
using System.Linq;

namespace Dominio.Entidades
{
    public class OrdenesDeCompraPorMoneda
    {
        public Moneda MonedaQueQuieroVender { get; set; }
        public Moneda MonedaQueQuieroComprar { get; set; }
        public List<Orden> Ordenes { get; set; }

        public OrdenesDeCompraPorMoneda(Moneda monedaAVender, Moneda monedaAComprar)
        {
            MonedaQueQuieroVender = monedaAVender;
            MonedaQueQuieroComprar = monedaAComprar;
            Ordenes = new List<Orden>();
        }

        public OrdenesDeCompraPorMoneda ObtenerOrdenesDeCompraPorCantidad(decimal cantidad)
        {
            var retorno = new List<Orden>();
            foreach(var orden in Ordenes.OrderByDescending(x => x.PrecioUnitario))
            {
                cantidad -= orden.Cantidad;
                retorno.Add(orden);
                if (cantidad <= 0) break;
            }

            if(cantidad > 0)
            {
                return null;
            }
            else
            {
                var ordenes = new OrdenesDeCompraPorMoneda(MonedaQueQuieroVender, MonedaQueQuieroComprar);
                ordenes.Ordenes = retorno;
                return ordenes;
            }            
        }
    }
}
