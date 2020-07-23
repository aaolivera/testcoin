using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    public class Jugada
    {
        [Key, Column(Order = 0)]
        public virtual string MonedaA { get; set; }
        [Key, Column(Order = 1)]
        public virtual string MonedaB { get; set; }

        [InverseProperty("Jugada")]
        public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public virtual decimal Inicial { get; set; }
        public virtual decimal Final { get; set; }

        public decimal Ganancia { get
            {
                return (Final * 100 / Inicial) - 100;
            } }

        public void Actualizar(Jugada j)
        {
            Inicial = j.Inicial;
            Final = j.Final;
        }
    }
}
