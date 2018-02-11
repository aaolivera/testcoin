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
        private List<Orden> Compras { get; }
        private List<Orden> Ventas { get; }
        public decimal MayorPrecioDeVentaAjecutada { get; set; }
        public decimal Volumen { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
        public string Nombre { get { return Principal.Nombre.ToLower() + "_" + Secundaria.Nombre.ToLower(); } }
        public decimal PruebaDelBitcoin { get; set; }
        public decimal DeltaEjecutado
        {
            get
            {
                if (MayorPrecioDeVentaAjecutada == 0 || Compra == 0 || Volumen == 0) return 0;
                return Math.Round((MayorPrecioDeVentaAjecutada - Compra) * 100 / (MayorPrecioDeVentaAjecutada == 0 ? 1 : MayorPrecioDeVentaAjecutada), 0);
            }
        }

        public decimal DeltaActual {
            get
            {
                return Math.Round((Venta - Compra) * 100 / (Venta == 0 ? 1 : Venta), 0);
            }
        }

        public Relacion(Moneda principal, Moneda secundaria)
        {
            Principal = principal;
            Secundaria = secundaria;
            Compras = new List<Orden>();
            Ventas = new List<Orden>();
        }

        public void AgregarOrden(Orden orden)
        {
            if (orden.EsDeVenta)
            {
                Ventas.AddSorted(orden);
            }
            else
            {
                Compras.AddSorted(orden);
            }
        }

        public void Limpiar()
        {
            Compras.Clear();
            Ventas.Clear();
        }

        public int CompareTo(Relacion other)
        {
            return DeltaEjecutado > other.DeltaEjecutado ? -1 : (DeltaEjecutado < other.DeltaEjecutado ? 1 : 0);
        }
    }
}
