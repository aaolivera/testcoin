using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominio.Entidades
{
    public class Moneda
    {
        public string Nombre { get; set; }
        //Gente que busca comprar this
        public Dictionary<Moneda, OrdenesDeCompraPorMoneda> OrdenesDeCompraPorMoneda { get; }

        //Variable auxiliar dijktra
        public decimal Peso { get; set; }
        public bool Marcado { get; set; }
        public Moneda MonedaAnterior { get; set; }
        public Orden Orden { get; set; }

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, OrdenesDeCompraPorMoneda>();
            Nombre = nombre;
        }

        public decimal ConvertirA(Moneda monedaDestino)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out OrdenesDeCompraPorMoneda ordenesDeCompra);
            if (ordenesDeCompra != null)
            {
                return ordenesDeCompra.Ordenes.Max(x => x.Precio) * this.Peso;
            }
            throw new Exception($"{this.Nombre} no se puede convertir a {monedaDestino.Nombre}");
        }

        public void AgregarRelacionPorMoneda(Moneda destino)
        {
            if (!OrdenesDeCompraPorMoneda.ContainsKey(destino))
            {
                OrdenesDeCompraPorMoneda.Add(destino, new OrdenesDeCompraPorMoneda(this, destino));
            }
        }

        public void AgregarOrdenDeCompra(Moneda monedaB, decimal precio, decimal cantidad)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaB, out OrdenesDeCompraPorMoneda ordenes);
            if(ordenes == null)
            {
                ordenes = new OrdenesDeCompraPorMoneda(this, monedaB);
                OrdenesDeCompraPorMoneda[monedaB] = ordenes;
            }
            ordenes.Ordenes.Add(new Orden { Cantidad = cantidad, Precio = precio});

        }

        public Orden ObtenerOrden(Moneda monedaAComprar)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaAComprar, out OrdenesDeCompraPorMoneda ordenesDeCompra);
            if (ordenesDeCompra != null)
            {
                return ordenesDeCompra.Ordenes.OrderByDescending(x => x.Precio).First();
            }
            throw new Exception($"{this.Nombre} no se puede convertir a {monedaAComprar.Nombre}");
        }

        public override string ToString()
        {
            return "Moneda: " + Nombre + " - Cantidad: "+ Peso;
        }
    }
}
