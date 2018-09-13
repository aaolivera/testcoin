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


        
        public bool Comprar(Moneda monedaDestino, string ejecucion)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out List<Orden> ordenesDeCompra);

            if (ordenesDeCompra == null) return false;

            var ordenesDecompraNecesarias = new List<Orden>();
            var cantidadDestino = 0M;
            var cantidadOrigen = 0M;
            var i = 0;
            foreach (var orden in ordenesDeCompra)
            {
                i++;
                var copiaDeOrden = orden.Clonar();
                var cantidadOrigenDeOrdenActual = 0M;
                if (cantidadOrigen + copiaDeOrden.Cantidad < Cantidad(ejecucion))
                {
                    cantidadOrigenDeOrdenActual = copiaDeOrden.Cantidad;
                }
                else if (cantidadOrigen + copiaDeOrden.Cantidad > Cantidad(ejecucion))
                {
                    cantidadOrigenDeOrdenActual = (Cantidad(ejecucion) - cantidadOrigen);
                    if (cantidadOrigenDeOrdenActual == 0) break;
                    copiaDeOrden.Cantidad = cantidadOrigenDeOrdenActual;
                    if (copiaDeOrden.EsDeVenta)
                    {
                        copiaDeOrden.Cantidad = cantidadOrigenDeOrdenActual - 0.199600798M / 100 * cantidadOrigenDeOrdenActual;
                    }
                }
                else
                {
                    break;
                }
                
                cantidadDestino += cantidadOrigenDeOrdenActual * copiaDeOrden.PrecioUnitario;
                cantidadOrigen += cantidadOrigenDeOrdenActual;
                ordenesDecompraNecesarias.Add(copiaDeOrden);
            }
            
            cantidadDestino = cantidadDestino - (ordenesDecompraNecesarias.First().EsDeVenta ? 0.199600798M : 0.2M) / 100 * cantidadDestino;
            
            if (cantidadOrigen == Cantidad(ejecucion) && monedaDestino.Cantidad(ejecucion) < cantidadDestino)
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

        public void LimpiarOrdenes()
        {
            OrdenesDeCompraPorMoneda.Clear();
        }

        public void AgregarOrdenDeCompra(Moneda monedaAcomprar, decimal precio, decimal cantidad, bool esDeVenta)
        {
            OrdenesDeCompraPorMoneda.TryGetValue(monedaAcomprar, out List<Orden> ordenes);
            if (ordenes == null)
            {
                ordenes = new List<Orden>();
                OrdenesDeCompraPorMoneda[monedaAcomprar] = ordenes;
            }

            var ordenDeCompra = new Orden
            {
                Cantidad = cantidad,
                Relacion = esDeVenta ? monedaAcomprar.Nombre + "_" + Nombre : Nombre + "_" + monedaAcomprar.Nombre,
                EsDeVenta = esDeVenta,
                PrecioUnitario = precio,
                MonedaQueQuieroComprar = monedaAcomprar,
                MonedaQueQuieroVender = this
            };
            
            ordenes.AddSorted(ordenDeCompra);
        }

        public override string ToString()
        {
            return "Moneda: " + Nombre + " - Cantidad: ";
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
