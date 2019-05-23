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
            ObtenerMoneda(monedaAcomprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), precio, cantidad, false);
        }

        public void AgregarOrdenDeVenta(string monedaAVender, string monedaAComprar, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaAComprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), 1 / precio, precio * cantidad, true);
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
                var i = 0;
                while (await provider.HayOrdenesActivas(ordenesNecesarias.First().Relacion))
                {
                    Thread.Sleep(20000);
                    if (i++ == 20) break;
                }
                Console.WriteLine("ok");
            }
        }
        
        public List<Moneda> ObtenerOperacionOptimaSoloIda(Moneda origen, Moneda destino, decimal cantidad, out string ejecucionIda, out string ejecucionVuelta)
        {
            ejecucionIda = origen.Nombre.ToLower();
            ejecucionVuelta = (destino.Nombre + origen.Nombre).ToLower();

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
                destino.Comprar(origen, ejecucionVuelta);
                //destino.EsMonedaOrigen(ejecucionVuelta, true);
                //RecorrerMercado(ejecucionVuelta);

                //Console.WriteLine($"{destino.Nombre}: {origen.Nombre}({origen.Cantidad(ejecucionIda)}) => ({origen.Cantidad(ejecucionVuelta)})");

                if (origen.Cantidad(ejecucionVuelta) > cantidad)
                {
                    var ida = Recorrido(destino, ejecucionIda);
                    //    var vuelta = Recorrido(origen, ejecucionVuelta, timestamp);

                    //vuelta.RemoveAt(0);
                    //    ida.AddRange(vuelta);
                    ida.Add(origen);
                    return ida;
                }
            }
            return new List<Moneda>();
        }

        public List<Moneda> ObtenerOperacionOptima(Moneda origen, Moneda destino, decimal cantidad, out string ejecucionIda, out string ejecucionVuelta)
        {
            ejecucionIda = origen.Nombre.ToLower();
            ejecucionVuelta = (destino.Nombre + origen.Nombre).ToLower();

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

                //Console.WriteLine($"{destino.Nombre}: {origen.Nombre}({origen.Cantidad(ejecucionIda)}) => ({origen.Cantidad(ejecucionVuelta)})");

                if (origen.Cantidad(ejecucionVuelta) > cantidad)
                {
                    var ida = Recorrido(destino, ejecucionIda);
                    var vuelta = Recorrido(origen, ejecucionVuelta);

                    vuelta.RemoveAt(0);
                    ida.AddRange(vuelta);
                    return ida;
                }
            }
            return new List<Moneda>();
        }

        private void RecorrerMercado(string ejecucion)
        {
            var stack = new Queue<Moneda>(Monedas.Where(d => d.EsMonedaOrigen(ejecucion)));

            while (stack.Any())
            {
                var monedaActual = stack.Dequeue();

                //if (monedaActual == destino && stack.Any())
                //{
                //    if (!stack.Contains(monedaActual))
                //    {
                //        stack.Enqueue(destino);
                //    }
                //    continue;
                //}
                monedaActual.Recorrida(ejecucion, true);
                foreach (var n in monedaActual.OrdenesDeCompraPorMoneda.Where(c => !c.Key.Recorrida(ejecucion) && c.Value.Any()))
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


        private void ImprimirGrafo(string ejecucion, Moneda inicial, string timestamp, int tab = 0)
        {
            inicial.Recorrida(timestamp, true);
            foreach (var m in inicial.OrdenesDeCompraPorMoneda)
            {
                var monedaAnterior = m.Key.OrdenesDeCompraMonedaAnterior(ejecucion).FirstOrDefault()?.MonedaQueQuieroVender;
                if (monedaAnterior != null && monedaAnterior == inicial && m.Key.Cantidad(ejecucion) != Moneda.CantidadDefault)
                {
                    var linea = $"{inicial.Nombre}({inicial.Cantidad(ejecucion)})->{m.Key.Nombre}({m.Key.Cantidad(ejecucion)})";
                    linea = linea.PadLeft(linea.Length + tab, '-');
                    Console.WriteLine(linea);

                    if (!m.Key.Recorrida(timestamp))ImprimirGrafo(ejecucion, m.Key, timestamp, tab+10);
                }
            }
        }
    }
}
