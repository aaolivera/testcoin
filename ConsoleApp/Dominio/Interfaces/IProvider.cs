using Dominio.Entidades;
using System.Collections.Generic;

namespace Dominio.Interfaces
{
    public interface IProvider
    {
        void Inicializar(Mercado mercado, List<string> exclude);
        void CargarOrdenes(Mercado mercado);
        List<Orden> ObtenerOrdenesNecesarias(Moneda actual, Moneda siguiente, decimal inicial, out string relacion);
        decimal EjecutarOrden(Orden i, string relacion);
        decimal ConsultarSaldo(string moneda);
        bool HayOrdenesActivas(string relacion);
    }
}
