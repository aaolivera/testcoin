
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominio.Interfaces
{
    public interface IMercadoActualizar
    {
        Task ActualizarMonedas();
        Task ActualizarRelaciones();
    }
}
