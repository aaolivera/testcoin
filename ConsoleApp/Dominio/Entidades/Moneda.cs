using System;
using System.Collections.Generic;

namespace Dominio.Entidades
{
    public class Moneda
    {
        public string Nombre { get; set; }
        public Dictionary<Moneda, OrdenesDeCompraPorMoneda> OrdenesDeCompraPorMoneda { get; }

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, OrdenesDeCompraPorMoneda>();
            Nombre = nombre;
        }

        public void AgregarRelacionPorMoneda(Moneda destino)
        {
            if (!OrdenesDeCompraPorMoneda.ContainsKey(destino))
            {
                OrdenesDeCompraPorMoneda.Add(destino, new OrdenesDeCompraPorMoneda(this, destino));
            }
        }

        internal void AgregarOrdenDeCompra(Moneda monedaB, decimal precio, decimal cantidad)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaB, out OrdenesDeCompraPorMoneda ordenes);
            if(ordenes == null)
            {
                ordenes = new OrdenesDeCompraPorMoneda(this, monedaB);
                OrdenesDeCompraPorMoneda[monedaB] = ordenes;
            }
            ordenes.Ordenes.Add(new Orden { Cantidad = cantidad, Precio = precio});

        }
    }
}
