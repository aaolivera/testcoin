using Dominio.Entidades;
using System.Collections.Generic;

namespace Servicios.Interfaces
{
    public interface IOperadorInput
    {
        void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB);
        void AgregarOrden(string relacionName, decimal precio, decimal cantidad, bool esDeVenta);
        void ActualizarEstadoOrden(string relacionName, decimal mayorPrecioDeVentaAjecutada, decimal volumen, decimal compra, decimal venta);
        void NotificarPaginas(int v);
        void NotificarAvance(string url);

        IEnumerable<Relacion> ListarRelacionesReelevantes();
        IEnumerable<Relacion> ListarRelaciones();
        Moneda ObtenerMoneda(string moneda);
    }
}
