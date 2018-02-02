using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IProvider
    {
        void CargarMonedas(Mercado mercado);
        void CargarOrdenes(Mercado mercado);
        decimal EjecutarMovimiento(Moneda actual, Moneda siguiente, decimal inicial);
    }
}
