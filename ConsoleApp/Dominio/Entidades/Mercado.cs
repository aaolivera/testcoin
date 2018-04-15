using Dominio.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dominio.Entidades
{
    public class Mercado
    {
        private List<IProvider> Providers = new List<IProvider>();
        private Dictionary<string, Moneda> Monedas { get; } = new Dictionary<string, Moneda>();
        private HashSet<string> RelacionesEntreMonedas { get; } = new HashSet<string>();

        public Mercado(List<IProvider> providers, List<string> exclude)
        {
            foreach(var p in providers)
            {
                p.CargarMonedas(this, exclude);
                Providers.Add(p);
            }
        }

        public void ActualizarOrdenes()
        {
            foreach (var p in Providers)
            {
                p.CargarOrdenes(this);
            }
        }

        public List<Moneda> ListarMonedasInfimas()
        {
            return Monedas.Values.Where(x => x.OrdenesDeCompraPorMoneda.Values.Any(y => y.FirstOrDefault()?.PrecioUnitario == 0.00000001M)).ToList();
        }
        
        public List<Moneda> ObtenerOperacionOptima(string origen, string destino, decimal cantidad, out string ejecucion)
        {
            ejecucion = (origen + destino).ToLower();
            var monedaOrigen = Monedas[origen.ToLower()];
            var monedaDestino = Monedas[destino.ToLower()];
            monedaOrigen.SetCantidad(cantidad, ejecucion);

            var stack = new Queue<Moneda>();
            stack.Enqueue(monedaOrigen);
            RecorrerMercado(stack, monedaDestino, ejecucion);

            return Recorrido(monedaDestino, ejecucion);
        }

        public List<string> ObetenerRelacionesEntreMonedas()
        {
            return RelacionesEntreMonedas.ToList();
        }
        
        public void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            var monedaA = ObtenerMoneda(monedaNameA);
            var monedaB = ObtenerMoneda(monedaNameB);

            RelacionesEntreMonedas.Add(monedaNameA + "_" + monedaNameB);
            monedaA.AgregarRelacionPorMoneda(monedaB);
            monedaB.AgregarRelacionPorMoneda(monedaA);
        }

        public void AgregarOrdenDeCompra(string monedaAcomprar, string monedaAVender, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaAcomprar).AgregarOrdenDeCompra(ObtenerMoneda(monedaAVender), precio, cantidad);
        }

        public void AgregarOrdenDeVenta(string monedaAVender, string monedaAComprar, decimal precio, decimal cantidad)
        {
            AgregarOrdenDeCompra(monedaAComprar, monedaAVender, 1 / precio, precio * cantidad);
        }

        public List<Moneda> ObtenerMonedas()
        {
            return Monedas.Values.ToList(); 
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

        public void EjecutarMovimientos(List<Moneda> movimientos, decimal inicial)
        {
            var cantidad = inicial;
            var provider = Providers[0];
            for (var i = 0; i < movimientos.Count - 1; i++)
            {
                var actual = movimientos[i];
                var siguiente = movimientos[i + 1];

                var ordenesNecesarias = provider.ObtenerOrdenesNecesarias(actual, siguiente, cantidad, out string relacion);

                var cantidadResultado = 0M;
                System.Console.WriteLine("https://yobit.net/en/trade/" + relacion.Replace('_', '/').ToUpper());
                foreach (var orden in ordenesNecesarias)
                {
                    cantidadResultado += provider.EjecutarOrden(orden, relacion);
                }

                while (provider.HayOrdenesActivas(relacion))
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
