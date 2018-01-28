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
        public decimal Cantidad { get; set; }
        public bool Marcado { get; set; }
        public OrdenesDeCompraPorMoneda OrdenesDeCompraMonedaAnterior { get; set; }

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, OrdenesDeCompraPorMoneda>();
            Nombre = nombre;
        }

        public bool ConvertirA(Moneda monedaDestino)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out OrdenesDeCompraPorMoneda ordenesDeCompra);
            var ordenesDeCompraNecesarias = ordenesDeCompra.ObtenerOrdenesDeCompraPorCantidad(Cantidad);

            if (ordenesDeCompraNecesarias == null) return false;

            var cantidadDestino = 0M;
            var cantidadOrigen = 0M;
            foreach (var orden in ordenesDeCompraNecesarias.Ordenes)
            {
                if(cantidadOrigen + orden.Cantidad < Cantidad)
                {
                    cantidadDestino += orden.Cantidad * orden.PrecioUnitario;
                    cantidadOrigen += orden.Cantidad;
                }
                else
                {
                    cantidadDestino += (Cantidad - cantidadOrigen) * orden.PrecioUnitario;
                }
            }

            if(monedaDestino.Cantidad < cantidadDestino)
            {
                monedaDestino.Cantidad = cantidadDestino;
                monedaDestino.OrdenesDeCompraMonedaAnterior = ordenesDeCompraNecesarias;
            }
            return true;
        }

        public void AgregarRelacionPorMoneda(Moneda destino)
        {
            if (!OrdenesDeCompraPorMoneda.ContainsKey(destino))
            {
                OrdenesDeCompraPorMoneda.Add(destino, new OrdenesDeCompraPorMoneda(this, destino));
            }
        }

        public void AgregarOrdenDeCompra(Moneda monedaAVender, decimal precio, decimal cantidad)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaAVender, out OrdenesDeCompraPorMoneda ordenes);
            if(ordenes == null)
            {
                ordenes = new OrdenesDeCompraPorMoneda(this, monedaAVender);
                OrdenesDeCompraPorMoneda[monedaAVender] = ordenes;
            }
            ordenes.Ordenes.Add(new Orden { Cantidad = cantidad, PrecioUnitario = precio});

        }

        public Orden ObtenerOrden(Moneda monedaAComprar)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaAComprar, out OrdenesDeCompraPorMoneda ordenesDeCompra);
            if (ordenesDeCompra != null)
            {
                return ordenesDeCompra.Ordenes.OrderByDescending(x => x.PrecioUnitario).First();
            }
            throw new Exception($"{this.Nombre} no se puede convertir a {monedaAComprar.Nombre}");
        }

        public void RemoverOrden(Moneda monedaAComprar, Orden orden)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaAComprar, out OrdenesDeCompraPorMoneda ordenesDeCompra);
            if (ordenesDeCompra != null)
            {
                ordenesDeCompra.Ordenes.Remove(orden);
            }
            throw new Exception($"{this.Nombre} no se puede convertir a {monedaAComprar.Nombre}");
        }

        public override string ToString()
        {
            return "Moneda: " + Nombre + " - Cantidad: "+ Cantidad;
        }
    }
}
