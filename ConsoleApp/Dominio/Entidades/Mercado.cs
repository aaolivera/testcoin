using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Mercado : IMercado, IMercadoCargar
    {
        private List<IProvider> Providers { get; }
        private Dictionary<string, Moneda> MonedasPorNombre { get; } = new Dictionary<string, Moneda>();
        private List<string> MonedasExcluidas { get; }
        private List<string> MonedasIncluidas { get; }
        private Dictionary<string, Relacion> RelacionesEntreMonedasHash { get; } = new Dictionary<string, Relacion>();

        public List<string> RelacionesEntreMonedas => RelacionesEntreMonedasHash.Keys.ToList();
        public List<Moneda> Monedas => MonedasPorNombre.Values.ToList();

        public Mercado(List<IProvider> providers, List<string> excluidas = null, List<string> incluidas = null)
        {
            this.Providers = new List<IProvider>(providers);
            this.MonedasExcluidas = excluidas;
            this.MonedasIncluidas = incluidas;
        }

        public async Task ActualizarMonedas()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarMonedas(this, MonedasExcluidas, MonedasIncluidas);
            }
        }

        public async Task ActualizarRelaciones()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarRelaciones(this);
            }
        }

        //public async Task ActualizarOrdenes()
        //{
        //    foreach (var p in Providers)
        //    {
        //        await p.ActualizarOrdenes(this);
        //    }
        //}

        public void CargarRelacionEntreMonedas(string monedaNameA, string monedaNameB, decimal volumen, decimal compra, decimal venta)
        {
            var monedaA = ObtenerMoneda(monedaNameA);
            var monedaB = ObtenerMoneda(monedaNameB);
            var relacion = new Relacion(monedaA, monedaB, volumen, compra, venta, this);

            monedaA.AgregarRelacion(relacion);
            monedaB.AgregarRelacion(relacion);
            RelacionesEntreMonedasHash[monedaNameA + "_" + monedaNameB] = relacion;
        }

        public Moneda ObtenerMoneda(string moneda)
        {
            if (!MonedasPorNombre.TryGetValue(moneda, out Moneda retorno))
            {
                retorno = new Moneda(moneda);
                MonedasPorNombre.Add(moneda, retorno);
            }
            return retorno;
        }

        public decimal Convertir(Moneda origen, Moneda destino, decimal cantidadOrigen, Jugada jugada)
        {
            origen.Relaciones.TryGetValue(destino, out Relacion relacion);
            
            if (relacion == null) return 0;
            var cantidadDestino = 0M;
            if (relacion.MonedaA == origen)
            {
                cantidadDestino = cantidadOrigen * relacion.Precio;
                cantidadDestino -= 0.199600798M / 100 * cantidadDestino;
                jugada.Movimientos.Add(new Movimiento 
                { 
                    Origen = origen, 
                    Destino = destino, 
                    Precio = relacion.Precio,
                    CantidadOrigen = cantidadOrigen,
                    CantidadDestino = cantidadDestino
                });

            }
            if (relacion.MonedaB == origen)
            {
                cantidadOrigen -= 0.2M / 100 * cantidadOrigen;
                cantidadDestino = cantidadOrigen / relacion.Precio;
                jugada.Movimientos.Add(new Movimiento 
                { 
                    Origen = origen, 
                    Destino = destino, 
                    Precio = relacion.Precio,
                    CantidadOrigen = cantidadOrigen,
                    CantidadDestino = cantidadDestino
                });
            }
            return cantidadDestino;
        }

        public Jugada CalcularJugada(Relacion relacion, decimal cantidad)
        {
            var resultado = new Jugada();
            resultado.Inicial = cantidad;
            var btc = ObtenerMoneda("btc");
            var cantidadMonedaA = Convertir(btc, relacion.MonedaA, cantidad, resultado);
            var cantidadMonedaB = Convertir(relacion.MonedaA, relacion.MonedaB, cantidadMonedaA, resultado);
            resultado.Final = Convertir(relacion.MonedaB, btc, cantidadMonedaB, resultado);
            return resultado;
        }

        public List<KeyValuePair<string, Relacion>> ListarRelacionesConAltoSpreed()
        {
            return RelacionesEntreMonedasHash.Where(x => x.Value.VolumenEnBtc > 1).ToList();
        }

        public List<Jugada> ListarJugadas(decimal cantidadBtc)
        {
            return ListarRelacionesConAltoSpreed().Select(x => CalcularJugada(x.Value, cantidadBtc)).OrderByDescending(x => x.Ganancia).Where(x => x.Ganancia > 10).ToList();
        }
    }
}
