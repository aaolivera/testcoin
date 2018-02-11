using System;

namespace Dominio.Entidades
{
    public class Estado
    {
        public DateTime UltimoUpdate { get; set; }
        public bool UpdateEnProgreso { get; set; }
        public int CantidadDeRelaciones { get; set; }
        public int RelacionesActualizadas { get; set; }
        public int Paginas { get; set; }
    }
}
