using Dominio.Entidades;
using System.Collections.Generic;

namespace Dominio.Interfaces
{
    public interface IProvider
    {
        void ActualizarMonedas(IMercadoCargar mercado, List<string> exclude);
        void ActualizarOrdenes(IMercadoCargar mercado);

        List<Orden> ObtenerOrdenesNecesarias(Moneda actual, Moneda siguiente, decimal inicial);
        decimal EjecutarOrden(Orden i);
        decimal ConsultarSaldo(string moneda);
        bool HayOrdenesActivas(string relacion);
    }
}
