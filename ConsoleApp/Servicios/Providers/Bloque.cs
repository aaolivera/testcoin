using System.Collections.Generic;

namespace Servicios
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
