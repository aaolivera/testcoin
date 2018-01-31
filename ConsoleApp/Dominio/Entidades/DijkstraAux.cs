using System.Collections.Generic;

namespace Dominio.Entidades
{
    public class DijkstraAux
    {
        public decimal Cantidad { get; set; }
        public bool Marcado { get; set; }
        public List<Orden> OrdenesDeCompraMonedaAnterior { get; set; }
    }
}
