using Dominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominio.Interfaces
{
    public interface IProvider
    {
        Task ActualizarMonedas(IMercadoCargar mercado, List<string> exclude);
        Task ActualizarOrdenes(IMercadoCargar mercado);

        Task<List<Orden>> ObtenerOrdenesNecesarias(Moneda actual, Moneda siguiente, decimal inicial);
        Task<decimal> EjecutarOrden(Orden i);
        Task<decimal> ConsultarSaldo(string moneda);
        Task<bool> HayOrdenesActivas(string relacion);
    }
}
