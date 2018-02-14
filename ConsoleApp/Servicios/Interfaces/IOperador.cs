using Dominio.Dto;
using Dominio.Entidades;
using System.Collections.Generic;

namespace Servicios.Interfaces
{
    public interface IOperador
    {
        IEnumerable<Relacion> ListarRelacionesReelevantes();
        IEnumerable<Relacion> ListarRelaciones();
        Moneda ObtenerMoneda(string moneda);

        void ActualizarOrdenes();
    }
}
