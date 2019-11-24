﻿using System;

namespace Dominio.Entidades
{
    public class Orden : IComparable<Orden>, ICloneable
    {
        public Moneda MonedaQueQuieroVender { get; set; }
        public Moneda MonedaQueQuieroComprar { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool EsDeVenta { get; set; }
        public string Relacion { get; set; }



        public Orden Clonar()
        {
            return (Orden)this.Clone();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public int CompareTo(Orden other)
        {
            return PrecioUnitario > other.PrecioUnitario ? -1 : (PrecioUnitario < other.PrecioUnitario ? 1 : 0);
        }
        
        public override string ToString()
        {
            return "Orden: c:" + Cantidad + " p:" + PrecioUnitario;
        }
    }
}
