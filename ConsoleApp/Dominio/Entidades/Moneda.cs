using Dominio.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dominio.Entidades
{
    public class Moneda
    {
        public string Nombre { get; set; }
        //Gente que busca comprar this
        public Dictionary<Moneda, List<Orden>> OrdenesDeCompraPorMoneda { get; }

        //Variable auxiliar dijktra
        public Dictionary<string, DijkstraAux> DijkstraAux { get; set; }
        private static Mutex mutex = new Mutex();

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, List<Orden>>();
            DijkstraAux = new Dictionary<string, DijkstraAux>();
            Nombre = nombre;
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
            if (ordenes == null)
            {
                ordenes = new List<Orden>();
                OrdenesDeCompraPorMoneda[monedaAcomprar] = ordenes;
            }
            ordenes.AddSorted(new Orden { Cantidad = cantidad, PrecioUnitario = precio, MonedaQueQuieroComprar = monedaAcomprar, MonedaQueQuieroVender = this });
        }

        public override string ToString()
        {
            return "Moneda: " + Nombre + " - Cantidad: ";
        }

        
        public bool Comprar(Moneda monedaDestino, string ejecucion)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out List<Orden> ordenesDeCompra);

            if (ordenesDeCompra == null || !ordenesDeCompra.Any()) return false;

            var ordenesDecompraNecesarias = new List<Orden>();
            var cantidadDestino = 0M;
            var cantidadOrigen = 0M;
            foreach (var orden in ordenesDeCompra)
            {
                var cantidadAVender = 0M;
                if (cantidadOrigen + orden.Cantidad <= Cantidad(ejecucion))
                {
                    cantidadAVender = orden.Cantidad;
                }
                else if (cantidadOrigen + orden.Cantidad > Cantidad(ejecucion))
                {
                    cantidadAVender = (Cantidad(ejecucion) - cantidadOrigen);
                    if (cantidadAVender == 0) break;
                }
                else
                {
                    break;
                }

                cantidadDestino += cantidadAVender * orden.PrecioUnitario;
                cantidadOrigen += cantidadAVender;
                ordenesDecompraNecesarias.Add(orden);
            }

            cantidadDestino = cantidadDestino - 0.2M / 100 * cantidadDestino;

            if (cantidadOrigen == Cantidad(ejecucion) && monedaDestino.Cantidad(ejecucion) < cantidadDestino)
            {
                monedaDestino.SetCantidad(cantidadDestino, ejecucion);
                monedaDestino.SetOrdenesDeCompraMonedaAnterior(ordenesDecompraNecesarias, ejecucion);
                return true;
            }
            return false;
        }
        
        public void SetCantidad (decimal cantidad, string ejecucion)
        {
            GetAux(ejecucion).Cantidad = cantidad;
        }

        public void SetOrdenesDeCompraMonedaAnterior(List<Orden> ordenes, string ejecucion)
        {
            GetAux(ejecucion).OrdenesDeCompraMonedaAnterior = ordenes;
        }

        public List<Orden> OrdenesDeCompraMonedaAnterior(string ejecucion)
        {
            return GetAux(ejecucion).OrdenesDeCompraMonedaAnterior;
        }

        public decimal Cantidad(string ejecucion)
        {
            return GetAux(ejecucion).Cantidad;
        }

        public void Vicitar(string ejecucion)
        {
            GetAux(ejecucion).Marcado = true;
        }

        public bool Vicitado(string ejecucion)
        {
            return GetAux(ejecucion).Marcado;
        }

        private DijkstraAux GetAux(string ejecucion)
        {
            try
            {
                DijkstraAux.TryGetValue(ejecucion, out DijkstraAux aux);
                if (aux == null)
                {
                    mutex.WaitOne();
                    aux = new DijkstraAux() { Cantidad = -10000, OrdenesDeCompraMonedaAnterior = new List<Orden>() };
                    DijkstraAux.Add(ejecucion, aux);
                    mutex.ReleaseMutex();
                }

                return aux;
            }
            catch
            {
                return GetAux(ejecucion);
            }
        }
    }
}
