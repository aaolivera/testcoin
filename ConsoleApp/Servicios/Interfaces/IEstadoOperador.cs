using System;

namespace Servicios.Interfaces
{
    public interface IEstadoOperador
    {
        DateTime UltimoUpdate { get; set; }
        bool UpdateEnProgreso { get; set; }
        int CantidadDeRelaciones { get; set; }
        int RelacionesActualizadas { get; set; }
        int Paginas { get; set; }
        bool GuardandoCambios { get; set; }
    }
}
