using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Dominio.Entidades
{
    public class Movimiento
    {
        [Key, Column(Order = 0)]
        public virtual string Origen { get; set; }
        [Key, Column(Order = 1)]
        public virtual string Destino { get; set; }
        [Key, Column(Order = 2)]
        [ForeignKey("Jugada")]
        public virtual string MonedaA { get; set; }
        [Key, Column(Order = 3)]
        [ForeignKey("Jugada")]
        public virtual string MonedaB { get; set; }
        public virtual Jugada Jugada { get; set; }
        public virtual bool Compra { get; set; }
        public virtual decimal Precio { get; set; }
        public virtual decimal CantidadOrigen { get; set; }
        public virtual decimal CantidadDestino { get; set; }
        [ForeignKey("Relacion"), Column(Order = 4)]
        public virtual string MonedaAR { get; set; }
        [ForeignKey("Relacion"), Column(Order = 5)]
        public virtual string MonedaBR { get; set; }
        public virtual Relacion Relacion { get; set; }

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
