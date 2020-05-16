
using System.Collections.Generic;

namespace Dominio.Interfaces
{
    public interface IMercadoCargar
    {
        void CargarPrecio(string monedaNameA, string monedaNameB, decimal volumen, decimal compra, decimal venta);
        void CargarRelacionEntreMonedas(string monedaNameA, string monedaNameB);
        List<string> RelacionesEntreMonedas { get; }
       
    }
}
