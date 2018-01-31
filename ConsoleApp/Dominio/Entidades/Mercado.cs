﻿using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        
        public List<Moneda> ObtenerOperacionOptima(string origen, string destino, decimal cantidad, out Guid ejecucion)
        {
            ejecucion = Guid.NewGuid();
            var monedaOrigen = Monedas[origen];
            var monedaDestino = Monedas[destino];
            monedaOrigen.SetCantidad(cantidad, ejecucion);

            var stack = new Queue<Moneda>();
            stack.Enqueue(monedaOrigen);
            RecorrerMercado(stack, monedaDestino, ejecucion);

            return Recorrido(monedaDestino, ejecucion);
        }

        public void EliminarOrdenes(List<Moneda> resultado)
        {
            //foreach(var moneda in resultado)
            //{
            //    if(moneda.OrdenesDeCompraMonedaAnterior != null)
            //    {
            //        foreach (var orden in moneda.OrdenesDeCompraMonedaAnterior)
            //        {
            //            orden.MonedaQueQuieroVender.OrdenesDeCompraPorMoneda[moneda].Remove(orden);
            //        }
            //    }
            //}
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
        
        private void RecorrerMercado(Queue<Moneda> stack, Moneda destino, Guid ejecucion)
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

        private List<Moneda> Recorrido(Moneda nodo, Guid ejecucion)
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
