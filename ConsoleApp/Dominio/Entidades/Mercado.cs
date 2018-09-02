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
        private HashSet<string> RelacionesEntreMonedasHash { get; }

        public List<string> RelacionesEntreMonedas => RelacionesEntreMonedasHash.ToList();
        public List<Moneda> Monedas => MonedasPorNombre.Values.ToList();

        public Mercado(List<IProvider> providers, List<string> monedasExlcuidas)
        {
            this.Providers = new List<IProvider>(providers);
            this.MonedasPorNombre = new Dictionary<string, Moneda>();
            this.MonedasExcluidas = new List<string>(monedasExlcuidas);
            this.RelacionesEntreMonedasHash = new HashSet<string>();
        }

        public async Task ActualizarMonedas()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarMonedas(this, MonedasExcluidas);
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

        private Moneda ObtenerMoneda(string moneda)
        {
            if (!MonedasPorNombre.TryGetValue(moneda, out Moneda retorno))
            {
                retorno = new Moneda(moneda);
                MonedasPorNombre.Add(moneda, retorno);
            }
            return retorno;
        }

        public void AgregarOrdenDeCompra(string monedaAcomprar, string monedaAVender, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaAcomprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), precio, cantidad);
        }

        public void AgregarOrdenDeVenta(string monedaAVender, string monedaAComprar, decimal precio, decimal cantidad)
        {
            AgregarOrdenDeCompra(monedaAComprar, monedaAVender, 1 / precio, precio * cantidad);
        }
        
        public List<Moneda> ObtenerOperacionOptima(string origen, string destino, decimal cantidad, out string ejecucion)
        {
            ejecucion = (origen + destino).ToLower();
            var monedaOrigen = MonedasPorNombre[origen.ToLower()];
            var monedaDestino = MonedasPorNombre[destino.ToLower()];
            monedaOrigen.SetCantidad(cantidad, ejecucion);

            var stack = new Queue<Moneda>();
            stack.Enqueue(monedaOrigen);
            RecorrerMercado(stack, monedaDestino, ejecucion);

            return Recorrido(monedaDestino, ejecucion);
        }

        public async Task EjecutarMovimientos(List<Moneda> movimientos, decimal inicial)
        {
            var cantidad = inicial;
            var provider = Providers[0];
            for (var i = 0; i < movimientos.Count - 1; i++)
            {
                var actual = movimientos[i];
                var siguiente = movimientos[i + 1];

                var ordenesNecesarias = await provider.ObtenerOrdenesNecesarias(actual, siguiente, cantidad);

                var cantidadResultado = 0M;
                System.Console.WriteLine("https://yobit.net/en/trade/" + ordenesNecesarias.First().Relacion.Replace('_', '/').ToUpper());
                foreach (var orden in ordenesNecesarias)
                {
                    cantidadResultado += await provider.EjecutarOrden(orden);
                }

                while (await provider.HayOrdenesActivas(ordenesNecesarias.First().Relacion))
                {
                    Thread.Sleep(1500);
                }

                //cantidad = provider.ConsultarSaldo(siguiente.Nombre);
                cantidad = cantidadResultado;
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
                
                foreach (var n in monedaActual.OrdenesDeCompraPorMoneda.Where(x => !x.Key.Vicitado(ejecucion) && x.Value.Any()))
                {
                    var monedaAComprar = n.Key;
                    if (monedaActual.ConvertirA(monedaAComprar, ejecucion))
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
