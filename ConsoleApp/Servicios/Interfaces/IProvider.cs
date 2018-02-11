using Dominio.Entidades;
using System.Collections.Generic;

namespace Servicios.Interfaces
{
    public interface IProvider
    {
        void CargarMonedas(IOperador operador);
        void CargarOrdenes(IOperador operador, List<string> ordenesAActualizar = null);
        void CargarEstadosDeOrdenes(IOperador operador, List<string> ordenesAActualizar = null);
        List<Orden> ObtenerOrdenesNecesarias(Moneda actual, Moneda siguiente, decimal inicial, bool usarPromedio, out string relacion);
        decimal EjecutarOrden(Orden i, string relacion);
        decimal ConsultarSaldo(string moneda);
        bool HayOrdenesActivas(string relacion);
    }
}
