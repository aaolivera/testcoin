using System;

namespace Dominio.Dto
{
    public class Orden : IComparable<Orden>
    {
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool EsDeVenta { get; set; }

        public int CompareTo(Orden other)
        {
            return (PrecioUnitario > other.PrecioUnitario ? -1 : (PrecioUnitario < other.PrecioUnitario ? 1 : 0)) * (EsDeVenta ? -1 : 1);
        }

        public override string ToString()
        {
            return "Orden: c:" + Cantidad + " p:" + PrecioUnitario;
        }
    }
}
