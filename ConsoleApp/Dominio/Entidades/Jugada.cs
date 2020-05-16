using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    public class Jugada
    {
        [Key]
        public string MonedaA { get; set; }
        [Key]
        public string MonedaB { get; set; }

        [InverseProperty("Jugada")]
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public decimal Inicial { get; set; }
        public decimal Final { get; set; }

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
