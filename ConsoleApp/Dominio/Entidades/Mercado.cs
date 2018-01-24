using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominio.Entidades
{
    public class Mercado
    {
        private List<IProvider> Providers = new List<IProvider>();
        private Dictionary<string, Moneda> Monedas { get; } = new Dictionary<string, Moneda>();
        private HashSet<string[]> RelacionesEntreMonedas { get; } = new HashSet<string[]>();

        public Mercado(List<IProvider> providers)
        {
            foreach(var p in providers)
            {
                p.CargarMonedas(this);
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

        public List<Moneda> ObtenerOperacionOptima(string origen, string destino, decimal cantidad)
        {
            Resetear();
            var usd = 
            Monedas[origen].Peso = cantidad;

            var stack = new Queue<Moneda>();
            stack.Enqueue(Monedas[origen]);
            RecorrerMercado(stack);

            return Recorrido(Monedas[destino]);
        }

        public List<string[]> ObetenerRelacionesEntreMonedas()
        {
            return RelacionesEntreMonedas.ToList();
        }

        public void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            var monedaA = ObtenerMoneda(monedaNameA);
            var monedaB = ObtenerMoneda(monedaNameB);

            RelacionesEntreMonedas.Add(new string[] { monedaNameA, monedaNameB });
            monedaA.AgregarRelacionPorMoneda(monedaB);
            monedaB.AgregarRelacionPorMoneda(monedaA);
        }

        public void AgregarOrdenDeCompra(string monedaNameA, string monedaNameB, decimal precio, decimal cantidad)
        {
            ObtenerMoneda(monedaNameA).AgregarOrdenDeCompra(ObtenerMoneda(monedaNameB), precio, cantidad);
        }

        public void AgregarOrdenDeVenta(string monedaNameA, string monedaNameB, decimal precio, decimal cantidad)
        {
            AgregarOrdenDeCompra(monedaNameB, monedaNameA, 1 / precio, precio * cantidad);
        }

        private Moneda ObtenerMoneda(string moneda)
        {
            if (!Monedas.TryGetValue(moneda, out Moneda retorno))
            {
                retorno = new Moneda(moneda);
                Monedas.Add(moneda, retorno);
            }
            return retorno;
        }

        private void Resetear()
        {
            foreach (var m in Monedas)
            {
                m.Value.Peso = Decimal.MinValue;
                m.Value.MonedaAnterior = null;
                m.Value.Orden = null;
                m.Value.Marcado = false;
            }
        }

        private void RecorrerMercado(Queue<Moneda> stack)
        {
            while (stack.Any())
            {
                var monedaActual = stack.Dequeue();

                monedaActual.Marcado = true;
                
                foreach (var n in monedaActual.OrdenesDeCompraPorMoneda.Where(x => x.Value.Ordenes.Any()))
                {
                    try
                    {
                        var monedaAComprar = n.Value.MonedaAComprar;
                        var nuevoPeso = monedaActual.ConvertirA(monedaAComprar);
                        if (nuevoPeso > monedaAComprar.Peso)
                        {
                            monedaAComprar.Peso = nuevoPeso;
                            monedaAComprar.Orden = monedaActual.ObtenerOrden(monedaAComprar);
                            monedaAComprar.MonedaAnterior = monedaActual;
                        }
                        if (!monedaAComprar.Marcado)
                        {
                            stack.Enqueue(monedaAComprar);
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        private List<Moneda> Recorrido(Moneda nodo)
        {
            if (nodo.MonedaAnterior == null)
            {
                return new List<Moneda>() { nodo };
            }
            var lista = Recorrido(nodo.MonedaAnterior);
            lista.Add(nodo);
            return lista;
        }
    }
}
