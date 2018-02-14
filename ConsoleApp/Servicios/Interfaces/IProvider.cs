using Dominio.Dto;
using System.Collections.Generic;

namespace Servicios.Interfaces
{
    public interface IProvider
    {
        void CargarMonedas(IOperadorInput operador);
        void CargarOrdenes(IOperadorInput operador, List<string> ordenesAActualizar = null);
        void CargarEstadosDeOrdenes(IOperadorInput operador, List<string> ordenesAActualizar = null);
        List<Orden> ObtenerOrdenesNecesarias(string actual, string siguiente, decimal inicial, bool usarPromedio, out string relacion);
        decimal EjecutarOrden(Orden i, string relacion);
        decimal ConsultarSaldo(string moneda);
        bool HayOrdenesActivas(string relacion);
    }
}
