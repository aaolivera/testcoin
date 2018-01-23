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
    }
}
