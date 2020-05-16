using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Dominio.Entidades
{
    public class Relacion
    {
        [InverseProperty("Relacion")]
        public ICollection<PrecioHistorico> PrecioHistoricos { get; set; }

        [Key]
        public string MonedaA { get; set; }
        [Key]
        public string MonedaB { get; set; }



        [NotMapped]
        public decimal Volumen { get { return PrecioHistoricos == null || !PrecioHistoricos.Any() ? 0 : PrecioHistoricos.Last().Volumen; } }
        [NotMapped]
        public decimal Compra { get { return PrecioHistoricos == null || !PrecioHistoricos.Any() ? 0 : PrecioHistoricos.Last().Compra; } }
        [NotMapped]
        public decimal Venta { get { return PrecioHistoricos == null || !PrecioHistoricos.Any() ? 0 : PrecioHistoricos.Last().Venta; } }
        [NotMapped]
        public decimal Spread {
            get {
                return Venta > 0 ? (Venta - Compra) * 100 / Venta : 0;
            }
        }
        [NotMapped]
        public decimal Precio
        {
            get
            {
                var p = (Compra + Venta) / 2;
                return p < 0.00000001M ? 0.00000001M : p - p % 0.00000001M;
            }
        }
        public bool Contiene(string moneda)
        {
            return moneda == MonedaA || moneda == MonedaB;
        }
        public override string ToString()
        {
            return $"{MonedaA.ToUpper()}/{MonedaB.ToUpper()}";
        }
    }
}
