using System.Collections.Generic;

namespace Servicios.Auxiliares
{
    class Bloque
    {
        public List<string> PaginasFallidas { get; set; }

        public Bloque()
        {
            PaginasFallidas = new List<string>();
        }
    }
}
