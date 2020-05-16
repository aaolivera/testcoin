using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Dominio.Entidades
{
    public class Movimiento
    {
        [Key]
        public string Origen { get; set; }
        [Key]
        public string Destino { get; set; }
        public bool Compra { get; set; }
        public decimal Precio { get; set; }
        public decimal CantidadOrigen { get; set; }
        public decimal CantidadDestino { get; set; }
        public Relacion Relacion { get; set; }
        public Jugada Jugada { get; set; }

        public override string ToString()
        {
            var cantidad = Relacion.MonedaA == Origen ? CantidadOrigen : CantidadDestino;
            return $"--{Relacion.ToString()} - {(Compra?"Buy":"Sell")} - rate {Precio.ToString("0.########", CultureInfo.InvariantCulture)} -amount {cantidad.ToString("0.########", CultureInfo.InvariantCulture)}";
        }

        public void Actualizar(Movimiento m)
        {
            Compra = m.Compra;
            Precio = m.Precio;
            CantidadOrigen = m.CantidadOrigen;
            CantidadDestino = m.CantidadDestino;
        }
    }
}
