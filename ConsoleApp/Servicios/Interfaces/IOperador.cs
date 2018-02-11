using Dominio.Entidades;
using System.Collections.Generic;

namespace Servicios.Interfaces
{
    public interface IOperador
    {
        void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB);
        void AgregarOrden(string relacionName, decimal precio, decimal cantidad, bool esDeVenta);
        void AgregarEstadoOrden(string relacionName, decimal mayorPrecioDeVentaAjecutada, decimal volumen, decimal compra, decimal venta);
        void Limpiar(string relacionName);


        IEnumerable<Relacion> ListarRelacionesReelevantes();
        IEnumerable<Relacion> ListarRelaciones();
        Moneda ObtenerMoneda(string moneda);
        Estado ObtenerEstado();
        void ActualizarOrdenes();
        void NotificarPaginas(int v);
        void NotificarAvance();
    }
}
