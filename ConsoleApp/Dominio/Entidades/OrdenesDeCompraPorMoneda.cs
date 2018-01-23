using System.Collections.Generic;

namespace Dominio.Entidades
{
    public class OrdenesDeCompraPorMoneda
    {
        public Moneda MonedaAVender { get; set; }
        public Moneda MonedaAComprar { get; set; }
        public List<Orden> Ordenes { get; set; }

        public OrdenesDeCompraPorMoneda(Moneda origen, Moneda destino)
        {
            MonedaAVender = origen;
            MonedaAComprar = destino;
            Ordenes = new List<Orden>();
        }
    }
}
