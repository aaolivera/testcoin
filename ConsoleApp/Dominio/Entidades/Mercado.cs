using Dominio.Helper;
using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Mercado : IMercado, IMercadoCargar
    {
        private List<IProvider> Providers { get; }
        private Dictionary<string, Moneda> MonedasPorNombre { get; }
        private List<string> MonedasExcluidas { get; }
        private List<string> MonedasIncluidas { get; }
        private HashSet<string> RelacionesEntreMonedasHash { get; }

        public List<string> RelacionesEntreMonedas => RelacionesEntreMonedasHash.ToList();
        public List<Moneda> Monedas => MonedasPorNombre.Values.ToList();

        public Mercado(List<IProvider> providers, List<string> excluidas = null, List<string> incluidas = null)
        {
            this.Providers = new List<IProvider>(providers);
            this.MonedasPorNombre = new Dictionary<string, Moneda>();
            this.MonedasExcluidas = excluidas;
            this.MonedasIncluidas = incluidas;
            this.RelacionesEntreMonedasHash = new HashSet<string>();
        }

        public async Task ActualizarMonedas()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarMonedas(this, MonedasExcluidas, MonedasIncluidas);
            }
        }

        public async Task ActualizarOrdenes()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarOrdenes(this);
            }
        }

        public void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            if(!RelacionesEntreMonedasHash.Contains(monedaNameA + "_" + monedaNameB)) 
            {
                var monedaA = ObtenerMoneda(monedaNameA);
                var monedaB = ObtenerMoneda(monedaNameB);
                RelacionesEntreMonedasHash.Add(monedaNameA + "_" + monedaNameB);
                monedaA.AgregarRelacionPorMoneda(monedaB);
                monedaB.AgregarRelacionPorMoneda(monedaA);
            }
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

        public void LimpiarOrdenes()
        {
            foreach (var moneda in Monedas)
            {
                moneda.LimpiarOrdenes();
            }
        }

        public void AgregarOrdenDeCompra(string monedaAcomprar, string monedaAVender, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaAcomprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), precio, cantidad * -1, false);
        }

        public void AgregarOrdenDeVenta(string monedaAVender, string monedaAComprar, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaAComprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), 1 / precio, precio * cantidad * -1, true);
        }
        
        public async Task EjecutarMovimientos(List<Moneda> movimientos, Moneda monedaDestino, string ejecucionIda, string ejecucionVuelta)
        {
            var provider = Providers[0];
            movimientos.RemoveAt(0);
            var ejecucion = ejecucionIda;
            foreach(var actual in movimientos)
            {
                var ordenesNecesarias = actual.OrdenesDeCompraMonedaAnterior(ejecucion);
                if (actual.Nombre == monedaDestino.Nombre) ejecucion = ejecucionVuelta;
                Console.WriteLine("https://yobit.net/en/trade/" + ordenesNecesarias.First().Relacion.Replace('_', '/').ToUpper());
                foreach (var orden in ordenesNecesarias)
                {
                    await provider.EjecutarOrden(orden);
                }

                while (await provider.HayOrdenesActivas(ordenesNecesarias.First().Relacion))
                {
                    Thread.Sleep(200);
                }
                Console.WriteLine("ok");
            }
        }
        
        public List<Moneda> ObtenerOperacionOptima(Moneda origen, Moneda destino, decimal cantidad, out string ejecucionIda, out string ejecucionVuelta)
        {
            ejecucionIda = origen.Nombre.ToLower();
            ejecucionVuelta = (destino.Nombre + origen.Nombre).ToLower();
            cantidad = cantidad * -1;
            var timestamp = DateTime.Now.Ticks + ejecucionVuelta;

            var cantidadDestino = destino.Cantidad(ejecucionIda);

            if (cantidadDestino == Moneda.CantidadDefault && !origen.EsMonedaOrigen(ejecucionIda))
            {
                origen.SetCantidad(cantidad, ejecucionIda);
                origen.EsMonedaOrigen(ejecucionIda, true);
                RecorrerMercado(ejecucionIda);
                cantidadDestino = destino.Cantidad(ejecucionIda);
            }
            
            if (cantidadDestino != Moneda.CantidadDefault)
            {
                destino.SetCantidad(cantidadDestino, ejecucionVuelta);
                destino.EsMonedaOrigen(ejecucionVuelta, true);
                RecorrerMercado(ejecucionVuelta);

                if(origen.Cantidad(ejecucionVuelta) > cantidad)
                {
                    var ida = Recorrido(destino, ejecucionIda, timestamp);
                    var vuelta = Recorrido(origen, ejecucionVuelta, timestamp);

                    vuelta.RemoveAt(0);
                    ida.AddRange(vuelta);
                    return ida;
                }
            }
            return new List<Moneda>();
        }

        private void RecorrerMercado(string ejecucion)
        {
            //Algoritmo de Bellman - Ford
            Console.WriteLine($"////////////////////{ejecucion}////////////////////////////");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var i = 1;
            var monedas = Monedas.Where(d => !d.EsMonedaOrigen(ejecucion));
            for (; i < Monedas.Count - 1; i++)
            {
                var doItAgain = false;
                foreach (var monedaActual in monedas)
                {
                    foreach (var otraMoneda in monedaActual.OrdenesDeCompraPorMoneda)
                    {
                        if (otraMoneda.Key.Comprar(monedaActual, ejecucion))
                        {
                            doItAgain = true;
                        }
                    }
                }
                if (!doItAgain) break;
            }
            stopwatch.Stop();
            Console.WriteLine($"ChequearMoneda {ejecucion} en {stopwatch.ElapsedMilliseconds * 0.001M} con {i} loops");
        }

        private List<Moneda> Recorrido(Moneda nodo, string ejecucion, string timestamp)
        {
            if (nodo.OrdenesDeCompraMonedaAnterior(ejecucion) == null || !nodo.OrdenesDeCompraMonedaAnterior(ejecucion).Any() || nodo.Recorrida(ejecucion + timestamp))
            {
                return new List<Moneda>() { nodo };
            }
            var monedaAVender = nodo.OrdenesDeCompraMonedaAnterior(ejecucion).First().MonedaQueQuieroVender;
            nodo.Recorrida(ejecucion + timestamp, true);
            var lista = Recorrido(monedaAVender, ejecucion, timestamp);
            lista.Add(nodo);
            return lista;
        }
    }
}
