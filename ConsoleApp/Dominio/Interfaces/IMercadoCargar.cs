
using System.Collections.Generic;

namespace Dominio.Interfaces
{
    public interface IMercadoCargar
    {
        void CargarRelacionEntreMonedas(string monedaNameA, string monedaNameB, decimal volumen, decimal compra, decimal venta);
        List<string> RelacionesEntreMonedas { get; }
       
    }
}
