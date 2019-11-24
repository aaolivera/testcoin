using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Jugada
    {
        public List<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public decimal Inicial { get; internal set; }
        public decimal Final { get; internal set; }

        public decimal Ganancia { get
            {
                return (Final * 100 / Inicial) - 100;
            } }
    }
}
