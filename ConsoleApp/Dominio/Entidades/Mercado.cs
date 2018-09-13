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
            ObtenerMoneda(monedaAcomprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), precio, cantidad, false);
        }

        public void AgregarOrdenDeVenta(string monedaAVender, string monedaAComprar, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaAComprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), 1 / precio, precio * cantidad, true);
        }
        
        public List<Moneda> ObtenerOperacionOptima(Moneda origen, Moneda destino, decimal cantidad, out string ejecucionIda, out string ejecucionVuelta)
        {
            ejecucionIda = (origen.Nombre + destino.Nombre).ToLower();
            ejecucionVuelta = (destino.Nombre + origen.Nombre).ToLower();

            origen.SetCantidad(cantidad, ejecucionIda);            
            RecorrerMercado(new Queue<Moneda>(new List<Moneda> { origen }), destino, ejecucionIda);
            var ida = Recorrido(destino, ejecucionIda);

            if(destino.Cantidad(ejecucionIda) > 0)
            {
                destino.SetCantidad(destino.Cantidad(ejecucionIda), ejecucionVuelta);
                RecorrerMercado(new Queue<Moneda>(new List<Moneda> { destino }), origen, ejecucionVuelta);
                var vuelta = Recorrido(origen, ejecucionVuelta);
                vuelta.RemoveAt(0);
                ida.AddRange(vuelta);
            }
            
            return ida;
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
                System.Console.WriteLine("https://yobit.net/en/trade/" + ordenesNecesarias.First().Relacion.Replace('_', '/').ToUpper());
                foreach (var orden in ordenesNecesarias)
                {
                    await provider.EjecutarOrden(orden);
                }

                while (await provider.HayOrdenesActivas(ordenesNecesarias.First().Relacion))
                {
                    Thread.Sleep(100);
                }
            }
        }
        
        private void RecorrerMercado(Queue<Moneda> stack, Moneda destino, string ejecucion)
        {
            while (stack.Any())
            {
                var monedaActual = stack.Dequeue();

                if(monedaActual == destino && stack.Any())
                {
                    if (!stack.Contains(monedaActual))
                    {
                        stack.Enqueue(destino);
                    }
                    continue;
                }
                monedaActual.Vicitar(ejecucion);
                foreach (var n in monedaActual.OrdenesDeCompraPorMoneda.Where(c => !c.Key.Vicitado(ejecucion) && c.Value.Any()))
                {
                    var monedaAComprar = n.Key;
                    if (monedaActual.Comprar(monedaAComprar, ejecucion))
                    {
                        stack.Enqueue(monedaAComprar);
                    }
                }
            }
        }

        private List<Moneda> Recorrido(Moneda nodo, string ejecucion)
        {
            if (nodo.OrdenesDeCompraMonedaAnterior(ejecucion) == null || !nodo.OrdenesDeCompraMonedaAnterior(ejecucion).Any())
            {
                return new List<Moneda>() { nodo };
            }
            var lista = Recorrido(nodo.OrdenesDeCompraMonedaAnterior(ejecucion).First().MonedaQueQuieroVender, ejecucion);
            lista.Add(nodo);
            return lista;
        }
    }
}
