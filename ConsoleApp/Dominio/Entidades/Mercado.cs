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
            var monedaOrigen = Monedas[origen];
            var monedaDestino = Monedas[destino];
            monedaOrigen.Cantidad = cantidad;

            var stack = new Queue<Moneda>();
            stack.Enqueue(monedaOrigen);
            RecorrerMercado(stack, monedaDestino);

            return Recorrido(monedaDestino);
        }

        public void EliminarOrdenes(List<Moneda> resultado)
        {
            foreach(var moneda in resultado)
            {
                if(moneda.OrdenesDeCompraMonedaAnterior != null)
                {
                    foreach (var orden in moneda.OrdenesDeCompraMonedaAnterior.Ordenes)
                    {
                        moneda.OrdenesDeCompraMonedaAnterior.MonedaQueQuieroVender.OrdenesDeCompraPorMoneda[moneda].Ordenes.Remove(orden);
                    }
                }
            }
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

        private void Resetear()
        {
            foreach (var m in Monedas)
            {
                m.Value.Cantidad = Decimal.MinValue;
                m.Value.OrdenesDeCompraMonedaAnterior = null;
                m.Value.Marcado = false;
            }
        }

        private void RecorrerMercado(Queue<Moneda> stack, Moneda destino)
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

                monedaActual.Marcado = true;
                
                foreach (var n in monedaActual.OrdenesDeCompraPorMoneda.Where(x => !x.Value.MonedaQueQuieroComprar.Marcado && x.Value.Ordenes.Any()))
                {
                    var monedaAComprar = n.Value.MonedaQueQuieroComprar;
                    if (monedaActual.ConvertirA(monedaAComprar))
                    {
                        stack.Enqueue(monedaAComprar);
                    }
                }
            }
        }

        private List<Moneda> Recorrido(Moneda nodo)
        {
            if (nodo.OrdenesDeCompraMonedaAnterior == null)
            {
                return new List<Moneda>() { nodo };
            }
            var lista = Recorrido(nodo.OrdenesDeCompraMonedaAnterior.MonedaQueQuieroVender);
            lista.Add(nodo);
            return lista;
        }
    }
}
