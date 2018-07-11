using System.Collections.Generic;

namespace Providers
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
