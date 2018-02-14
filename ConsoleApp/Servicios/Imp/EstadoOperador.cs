using Dominio.Entidades;
using Repositorio;
using Servicios.Interfaces;
using System;

namespace Servicios.Imp
{
    public class EstadoOperador : IEstadoOperador
    {
        public EstadoOperador(IRepositorio repositorio)
        {
            UltimoUpdate = DateTime.Now;
            CantidadDeRelaciones = repositorio.Contar<Relacion>();
        }
        public DateTime UltimoUpdate { get; set; }
        public bool UpdateEnProgreso { get; set; }
        public bool GuardandoCambios { get; set; }
        public int CantidadDeRelaciones { get; set; }
        public int RelacionesActualizadas { get; set; }
        public int Paginas { get; set; }
    }
}