using Dominio.Helper;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Dominio.Entidades
{
    public class Moneda
    {
        public string Nombre { get; set; }
        //Gente que busca comprar this
        public Dictionary<Moneda, List<Orden>> OrdenesDeCompraPorMoneda { get; }

        //Variable auxiliar dijktra
        public Dictionary<Guid, DijkstraAux> DijkstraAux { get; set; }
        private static Mutex mutex = new Mutex();

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, List<Orden>>();
            DijkstraAux = new Dictionary<Guid, DijkstraAux>();
            Nombre = nombre;
        }

        public bool ConvertirA(Moneda monedaDestino, Guid ejecucion)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out List<Orden> ordenesDeCompra);

            if (ordenesDeCompra == null) return false;

            List<Orden> ordenesDecompraNecesarias = new List<Orden>();
            var cantidadDestino = 0M;
            var cantidadOrigen = 0M;
            foreach (var orden in ordenesDeCompra)
            {
                var cantidadAVender = 0M;
                if (cantidadOrigen + orden.Cantidad < Cantidad(ejecucion))
                {
                    cantidadAVender = orden.Cantidad;
                }
                else if (cantidadOrigen + orden.Cantidad > Cantidad(ejecucion))
                {
                    cantidadAVender = (Cantidad(ejecucion) - cantidadOrigen);
                }
                else
                {
                    break;
                }
                cantidadDestino += cantidadAVender * orden.PrecioUnitario;
                cantidadOrigen += cantidadAVender;
                ordenesDecompraNecesarias.Add(orden);
            }

            if(cantidadOrigen == Cantidad(ejecucion) && monedaDestino.Cantidad(ejecucion) < cantidadDestino)
            {
                monedaDestino.SetCantidad(cantidadDestino, ejecucion);
                monedaDestino.SetOrdenesDeCompraMonedaAnterior(ordenesDecompraNecesarias, ejecucion);
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
            return "Moneda: " + Nombre + " - Cantidad: ";
        }

        public void SetCantidad (decimal cantidad, Guid ejecucion)
        {
            GetAux(ejecucion).Cantidad = cantidad;
        }

        public void SetOrdenesDeCompraMonedaAnterior(List<Orden> ordenes, Guid ejecucion)
        {
            GetAux(ejecucion).OrdenesDeCompraMonedaAnterior = ordenes;
        }

        public List<Orden> OrdenesDeCompraMonedaAnterior(Guid ejecucion)
        {
            return GetAux(ejecucion).OrdenesDeCompraMonedaAnterior;
        }

        public decimal Cantidad(Guid ejecucion)
        {
            return GetAux(ejecucion).Cantidad;
        }

        public void Vicitar(Guid ejecucion)
        {
            GetAux(ejecucion).Marcado = true;
        }

        public bool Vicitado(Guid ejecucion)
        {
            return GetAux(ejecucion).Marcado;
        }

        private DijkstraAux GetAux(Guid ejecucion)
        {
            DijkstraAux.TryGetValue(ejecucion, out DijkstraAux aux);
            if (aux == null)
            {
                mutex.WaitOne();
                aux = new DijkstraAux() { Cantidad = decimal.MinValue , OrdenesDeCompraMonedaAnterior = new List<Orden>()};
                DijkstraAux.Add(ejecucion, aux);
                mutex.ReleaseMutex();
            }
            
            return aux;
        }
    }
}
