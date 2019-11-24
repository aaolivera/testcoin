using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Relacion
    {
        public Relacion(Moneda monedaA, Moneda monedaB, decimal volumen, decimal compra, decimal venta, Mercado mercado)
        {
            MonedaA = monedaA;
            MonedaB = monedaB;
            Volumen = volumen;
            Compra = compra;
            Venta = venta;
            Mercado = mercado;
        }
        public decimal Volumen { get; private set; }
        public decimal Compra { get; private set; }
        public decimal Venta { get; private set; }
        public Mercado Mercado { get; private set; }
        public Moneda MonedaA { get; private set; }
        public Moneda MonedaB { get; private set; }

        public decimal VolumenEnBtc { 
            get {
                return Mercado.Convertir(MonedaA, Mercado.ObtenerMoneda("btc"), Volumen, new Jugada());
            }
        }

        public decimal Spread {
            get {
                return Venta > 0 ? (Venta - Compra) * 100 / Venta : 0;
            }
        }

        public decimal Precio
        {
            get
            {
                return (Compra + Venta) / 2;
            }
        }


    }
}
