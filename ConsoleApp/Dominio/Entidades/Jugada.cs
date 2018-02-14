using Dominio.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    public class Jugada
    {
        [Key]
        public int Id { get; set; }
        public ICollection<Movimiento> Relacion { get; set; }
        public Estado Estado { get; set; }
    }
}
