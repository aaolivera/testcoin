using System;
using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    public class PrecioHistorico
    {
        [Key]
        public Relacion Relacion { get; set; }
        [Key]
        public DateTime Fecha { get; set; }
        public decimal Volumen { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
    }
}
