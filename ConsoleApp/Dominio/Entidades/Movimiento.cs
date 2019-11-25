using System;
using System.Globalization;

namespace Dominio.Entidades
{
    public class Movimiento
    {
        public Moneda Origen { get; set; }
        public Moneda Destino { get; set; }
        public bool Compra { get; set; }

        public decimal Precio { get; internal set; }
        public decimal CantidadOrigen { get; internal set; }
        public decimal CantidadDestino { get; internal set; }
        public Relacion Relacion { get; internal set; }

        public override string ToString()
        {
            var cantidad = Relacion.MonedaA == Origen ? CantidadOrigen : CantidadDestino;
            return $"--{Relacion.ToString()} - {(Compra?"Buy":"Sell")} - rate {Precio.ToString("0.########", CultureInfo.InvariantCulture)} -amount {cantidad.ToString("0.########", CultureInfo.InvariantCulture)} // Vol>{Relacion.VolumenEnBtc}";
        }
    }
}
