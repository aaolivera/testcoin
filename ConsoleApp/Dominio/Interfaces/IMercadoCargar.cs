
using System.Collections.Generic;

namespace Dominio.Interfaces
{
    public interface IMercadoCargar
    {
        void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB);
        List<string> RelacionesEntreMonedas { get; }
        void AgregarOrdenDeCompra(string monedaAcomprar, string monedaAVender, decimal precio, decimal cantidad);
        void AgregarOrdenDeVenta(string monedaAVender, string monedaAComprar, decimal precio, decimal cantidad);
    }
}
