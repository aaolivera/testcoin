using Dominio.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    public class Movimiento
    {
        [Key]
        public int Id { get; set; }
        public Relacion Relacion { get; set; }
        public Jugada Jugada { get; set; }
        public decimal PrecioMinimo { get; set; }
        public decimal PrecioMaximo { get; set; }
    }
}
