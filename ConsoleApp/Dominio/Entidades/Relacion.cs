using Dominio.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominio.Entidades
{
    public class Relacion : IComparable<Relacion>
    {
        public Moneda Principal { get;}
        public Moneda Secundaria { get; }
        private List<Orden> Compra { get; }
        private List<Orden> Venta { get; }
        public decimal MayorPrecioDeVenta { get; }
        public decimal Volumen { get; }

        public decimal Delta
        {
            get
            {
                var baseCompra = Compra.Sum(x => x.Cantidad);
                if (MayorPrecioDeVenta == 0 || baseCompra == 0 || Volumen == 0) return 0;
                var compraPonderada = Compra.Sum(x => x.Cantidad * x.PrecioUnitario) / baseCompra;
                return Math.Round((MayorPrecioDeVenta - compraPonderada) * 100 / (MayorPrecioDeVenta == 0 ? 1 : MayorPrecioDeVenta), 0);
            }
        }

        public decimal DeltaPonderado {
            get
            {
                var baseCompra = Compra.Sum(x => x.Cantidad);
                var baseVenta = Venta.Sum(x => x.Cantidad);
                if (baseVenta == 0 || baseCompra == 0 || Volumen == 0) return 0;
                var compraPonderada = Compra.Sum(x => x.Cantidad * x.PrecioUnitario) / baseCompra;
                var ventaPonderada = Venta.Sum(x => x.Cantidad * x.PrecioUnitario) / baseVenta;
                return Math.Round((ventaPonderada - compraPonderada) * 100 / (ventaPonderada == 0 ? 1 : ventaPonderada), 0);
            }
        }

        public Relacion(Moneda principal, Moneda secundaria)
        {
            Principal = principal;
            Secundaria = secundaria;
            Compra = new List<Orden>();
            Venta = new List<Orden>();
        }

        public void AgregarOrden(Orden orden)
        {
            if (orden.EsDeVenta)
            {
                Venta.AddSorted(orden);
            }
            else
            {
                Compra.AddSorted(orden);
            }
        }

        public void Limpiar()
        {
            Compra.Clear();
            Venta.Clear();
        }

        public int CompareTo(Relacion other)
        {
            return Delta > other.Delta ? -1 : (Delta < other.Delta ? 1 : 0);
        }
    }
}
