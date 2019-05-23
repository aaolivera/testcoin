using System.Collections.Generic;

namespace Dominio.Entidades
{
    public class DijkstraAux
    {
        public static decimal CantidadDefault => decimal.MinValue;
        public decimal Cantidad { get; set; }
        public List<Orden> OrdenesDeCompraMonedaAnterior { get; set; }
        public bool EsMonedaOrigen { get; set; }
        public bool Recorrida { get; set; }

        public DijkstraAux()
        {
            Cantidad = CantidadDefault;
            OrdenesDeCompraMonedaAnterior = new List<Orden>();
        }
    }
}
