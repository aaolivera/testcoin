using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers
{
    class Bloque
    {
        public List<dynamic> PaginasDescargadas { get; set; }
        public List<string> PaginasFallidas { get; set; }

        public Bloque()
        {
            PaginasDescargadas = new List<dynamic>();
            PaginasFallidas = new List<string>();
        }
    }
}
