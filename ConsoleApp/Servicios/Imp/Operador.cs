using Dominio.Entidades;
using Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Servicios.Imp
{
    public class Operador : IOperador
    {
        private List<IProvider> Providers = new List<IProvider>() { new YobitProvider() };
        private Dictionary<string, Moneda> Monedas { get; } = new Dictionary<string, Moneda>();
        private Dictionary<string, Relacion> RelacionesEntreMonedas { get; } = new Dictionary<string, Relacion>();
        private Estado Estado;
        public Operador()
        {
            foreach (var p in Providers)
            {
                p.CargarMonedas(this);
            }
            Estado = new Estado()
            {
                CantidadDeRelaciones = RelacionesEntreMonedas.Count,
                RelacionesActualizadas = 0,
                UltimoUpdate = DateTime.Now,
                UpdateEnProgreso = false
            };
        }
        
        public IEnumerable<Relacion> ListarRelacionesReelevantes()
        {
            var relaciones = RelacionesEntreMonedas.Values.ToList();
            relaciones.Sort();
            return relaciones.Take(100).OrderByDescending(x => x.Volumen);
        }

        public IEnumerable<Relacion> ListarRelaciones()
        {
            var relaciones = RelacionesEntreMonedas.Values.ToList();
            return relaciones;
        }

        public async void ActualizarOrdenes()
        {
            await Task.Run(() =>
            {
                Estado.UpdateEnProgreso = true;
                Estado.RelacionesActualizadas = 0;
                foreach (var p in Providers)
                {
                    p.CargarOrdenes(this);
                    p.CargarEstadosDeOrdenes(this);
                }
                PruebaDelBitcoin();
                Estado.UpdateEnProgreso = false;
                Estado.UltimoUpdate = DateTime.Now;
            });
        }
        
        public Estado ObtenerEstado()
        {
            return Estado;
        }

        private void PruebaDelBitcoin()
        {
            var btc = ObtenerMoneda("btc");
            var cantidadInicial = 0.0001M;
            foreach (var relacion in ListarRelacionesReelevantes())
            {
                var cantidadPrincipal = ConvertirMoneda(btc, relacion.Principal, cantidadInicial, false);
                var cantidadSecundaria = ConvertirMoneda(relacion.Principal, relacion.Secundaria, cantidadPrincipal, true);
                var resultadoBtc = ConvertirMoneda(relacion.Secundaria, btc, cantidadSecundaria, false);
                relacion.PruebaDelBitcoin = (resultadoBtc - cantidadInicial) * 100 / cantidadInicial;
            }
        }

        private decimal ConvertirMoneda(Moneda inicial, Moneda final, decimal cantidad, bool usarPromedio)
        {
            var ordenes = Providers.First().ObtenerOrdenesNecesarias(inicial, final, cantidad, usarPromedio, out string r);
            var cantidadDestino = 0M;
            foreach (var i in ordenes)
            {
                cantidadDestino += i.EsDeVenta ? i.Cantidad : Decimal.Round((i.Cantidad * i.PrecioUnitario) - (0.2M / 100 * (i.Cantidad * i.PrecioUnitario)), 8);
            }
            return cantidadDestino;
        }

        #region iprovider
        public void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            var monedaA = ObtenerMoneda(monedaNameA);
            var monedaB = ObtenerMoneda(monedaNameB);

            RelacionesEntreMonedas.Add(monedaNameA + "_" + monedaNameB, new Relacion(monedaA, monedaB));
        }

        public void AgregarOrden(string relacionName, decimal precio, decimal cantidad, bool esDeVenta)
        {
            RelacionesEntreMonedas.TryGetValue(relacionName, out Relacion relacion);
            var orden = new Orden()
            {
                Cantidad = cantidad,
                EsDeVenta = esDeVenta,
                PrecioUnitario = precio
            };

            relacion?.AgregarOrden(orden);
        }

        public void AgregarEstadoOrden(string relacionName, decimal mayorPrecioDeVentaAjecutada, decimal volumen, decimal compra, decimal venta)
        {
            RelacionesEntreMonedas.TryGetValue(relacionName, out Relacion relacion);
            if (relacion != null)
            {
                relacion.MayorPrecioDeVentaAjecutada = mayorPrecioDeVentaAjecutada;
                relacion.Venta = venta;
                relacion.Compra = compra;
                relacion.Volumen = volumen;
            }
        }

        public Moneda ObtenerMoneda(string moneda)
        {
            if (!Monedas.TryGetValue(moneda, out Moneda retorno))
            {
                retorno = new Moneda(moneda);
                Monedas.Add(moneda, retorno);
            }
            return retorno;
        }

        public void Limpiar(string relacionName)
        {
            RelacionesEntreMonedas.TryGetValue(relacionName, out Relacion relacion);
            if (relacion != null)
            {
                relacion.Limpiar();
            }
        }

        public void NotificarPaginas(int v)
        {
            Estado.Paginas = v;
        }

        public void NotificarAvance()
        {
            Estado.RelacionesActualizadas += Estado.CantidadDeRelaciones / Estado.Paginas;
        }
        #endregion
    }
}
