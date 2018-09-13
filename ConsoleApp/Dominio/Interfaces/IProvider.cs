using Dominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominio.Interfaces
{
    public interface IProvider
    {
        Task ActualizarMonedas(IMercadoCargar mercado, List<string> exclude, List<string> include);
        Task ActualizarOrdenes(IMercadoCargar mercado);
        
        Task EjecutarOrden(Orden i);
        Task<decimal> ConsultarSaldo(string moneda);
        Task<bool> HayOrdenesActivas(string relacion);
    }
}
