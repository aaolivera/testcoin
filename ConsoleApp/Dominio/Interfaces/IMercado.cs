using Dominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominio.Interfaces
{
    public interface IMercado
    {
        Task ActualizarMonedas();
        //Task ActualizarOrdenes();
        Task ActualizarRelaciones();
    }
}
