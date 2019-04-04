using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Jugada
    {
        public List<Moneda> Movimientos { get; set; }
        public Moneda MonedaDestino { get; set; }
        public string EjecucionIda { get; set; }
        public string EjecucionVuelta { get; set; }
    }
}
