using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    public class PrecioHistorico
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Relacion")]
        public virtual string MonedaA { get; set; }
        [Key, Column(Order = 1)]
        [ForeignKey("Relacion")]
        public virtual string MonedaB { get; set; }
        [Key, Column(Order = 2)]
        public virtual DateTime Fecha { get; set; }

        public virtual Relacion Relacion { get; set; }
        public virtual decimal Volumen { get; set; }
        public virtual decimal Compra { get; set; }
        public virtual decimal Venta { get; set; }
    }
}
