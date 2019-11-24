using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Movimiento
    {
        public Moneda Origen { get; set; }
        public Moneda Destino { get; set; }

        public decimal Precio { get; internal set; }
        public decimal CantidadOrigen { get; internal set; }
        public decimal CantidadDestino { get; internal set; }
    }
}
