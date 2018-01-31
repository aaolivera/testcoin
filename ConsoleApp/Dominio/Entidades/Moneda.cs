using Dominio.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominio.Entidades
{
    public class Moneda
    {
        public string Nombre { get; set; }
        //Gente que busca comprar this
        public Dictionary<Moneda, List<Orden>> OrdenesDeCompraPorMoneda { get; }

        //Variable auxiliar dijktra
        public decimal Cantidad { get; set; }
        public bool Marcado { get; set; }
        public List<Orden> OrdenesDeCompraMonedaAnterior { get; set; }

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, List<Orden>>();
            Nombre = nombre;
        }

        public bool ConvertirA(Moneda monedaDestino)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out List<Orden> ordenesDeCompra);

            if (ordenesDeCompra == null) return false;

            List<Orden> ordenesDecompraNecesarias = new List<Orden>();
            var cantidadDestino = 0M;
            var cantidadOrigen = 0M;
            foreach (var orden in ordenesDeCompra)
            {
                var cantidadAVender = 0M;
                if (cantidadOrigen + orden.Cantidad < Cantidad)
                {
                    cantidadAVender = orden.Cantidad;
                }
                else if (cantidadOrigen + orden.Cantidad > Cantidad)
                {
                    cantidadAVender = (Cantidad - cantidadOrigen);
                }
                else
                {
                    break;
                }
                cantidadDestino += cantidadAVender * orden.PrecioUnitario;
                cantidadOrigen += cantidadAVender;
                ordenesDecompraNecesarias.Add(orden);
            }

            if(cantidadOrigen == Cantidad && monedaDestino.Cantidad < cantidadDestino)
            {
                monedaDestino.Cantidad = cantidadDestino;
                monedaDestino.OrdenesDeCompraMonedaAnterior = ordenesDecompraNecesarias;
                return true;
            }
            return false;
        }

        public void AgregarRelacionPorMoneda(Moneda destino)
        {
            if (!OrdenesDeCompraPorMoneda.ContainsKey(destino))
            {
                OrdenesDeCompraPorMoneda.Add(destino, new List<Orden>());
            }
        }

        public void AgregarOrdenDeCompra(Moneda monedaAcomprar, decimal precio, decimal cantidad)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaAcomprar, out List<Orden> ordenes);
            if(ordenes == null)
            {
                ordenes = new List<Orden>();
                OrdenesDeCompraPorMoneda[monedaAcomprar] = ordenes;
            }
            ordenes.AddSorted(new Orden { Cantidad = cantidad, PrecioUnitario = precio, MonedaQueQuieroComprar= monedaAcomprar, MonedaQueQuieroVender = this});

        }
        
        public override string ToString()
        {
            return "Moneda: " + Nombre + " - Cantidad: "+ Cantidad;
        }
    }
}
