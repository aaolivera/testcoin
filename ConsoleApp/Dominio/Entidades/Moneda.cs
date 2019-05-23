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
        public static decimal CantidadDefault { get { return Entidades.DijkstraAux.CantidadDefault; } }

        private static Mutex mutex = new Mutex();

        public Moneda(string nombre)
        {
            OrdenesDeCompraPorMoneda = new Dictionary<Moneda, List<Orden>>();
            DijkstraAux = new Dictionary<string, DijkstraAux>();
            Nombre = nombre;
        }
               
        public bool Comprar(Moneda monedaDestino, string ejecucion)
        {
            if (Cantidad(ejecucion) == Moneda.CantidadDefault) return false;
            var retorno = Convertir(monedaDestino, ejecucion, out List<Orden> ordenesDecompraNecesarias, out decimal cantidadOrigen, out decimal cantidadDestino);
            if (!retorno) return false;
            //Console.WriteLine($"Intento con {this.Nombre}({Cantidad(ejecucion)}) comprar {monedaDestino.Nombre}({monedaDestino.Cantidad(ejecucion)}): {cantidadDestino} ");
            if (cantidadOrigen == Cantidad(ejecucion) && monedaDestino.Cantidad(ejecucion) < cantidadDestino)
            {
                monedaDestino.SetCantidad(cantidadDestino, ejecucion);
                monedaDestino.SetOrdenesDeCompraMonedaAnterior(ordenesDecompraNecesarias, ejecucion);
                return true;
            }
            return false;
        }

        private bool Convertir(Moneda monedaDestino, string ejecucion, out List<Orden> ordenesDecompraNecesarias, out decimal cantidadOrigen, out decimal cantidadDestino)
        {
            cantidadDestino = 0M;
            cantidadOrigen = 0M;
            ordenesDecompraNecesarias = new List<Orden>();
            var cantidadActual = Cantidad(ejecucion);
            OrdenesDeCompraPorMoneda.TryGetValue(monedaDestino, out List<Orden> ordenesDeCompra);

            if (ordenesDeCompra == null || !ordenesDeCompra.Any()) return false;
            
            var i = 0;
            foreach (var orden in ordenesDeCompra)
            {
                i++;
                var copiaDeOrden = orden.Clonar();
                var cantidadOrigenDeOrdenActual = 0M;
                if (cantidadOrigen + copiaDeOrden.Cantidad < cantidadActual)
                {
                    cantidadOrigenDeOrdenActual = copiaDeOrden.Cantidad;
                }
                else if (cantidadOrigen + copiaDeOrden.Cantidad > cantidadActual)
                {
                    cantidadOrigenDeOrdenActual = (cantidadActual - cantidadOrigen);
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
            if (!ordenesDecompraNecesarias.Any()) return false;

            cantidadDestino = cantidadDestino - (ordenesDecompraNecesarias.First().EsDeVenta ? 0.199600798M : 0.2M) / 100 * cantidadDestino;

            return true;
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
            DijkstraAux.Clear();
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
            ////////////////////////////////
            if (ordenes.Any()) return;
            ///////////////////////////////
            ordenes.AddSorted(ordenDeCompra);
        }

        public override string ToString()
        {
            return "Moneda: " + Nombre + " - Cantidad: ";
        }

        public bool EsMonedaOrigen(string ejecucion, bool? es = null)
        {
            if(es.HasValue)GetAux(ejecucion).EsMonedaOrigen = es.Value;
            return GetAux(ejecucion).EsMonedaOrigen;
        }
        public bool Recorrida(string ejecucion, bool? es = null)
        {
            if (es.HasValue) GetAux(ejecucion).Recorrida = es.Value;
            return GetAux(ejecucion).Recorrida;
        }

        public void SetOrdenesDeCompraMonedaAnterior(List<Orden> ordenes, string ejecucion)
        {
            GetAux(ejecucion).OrdenesDeCompraMonedaAnterior = ordenes;
        }

        public List<Orden> OrdenesDeCompraMonedaAnterior(string ejecucion)
        {
            return GetAux(ejecucion).OrdenesDeCompraMonedaAnterior;
        }

        public void SetCantidad(decimal cantidad, string ejecucion)
        {
            GetAux(ejecucion).Cantidad = cantidad;
        }

        public decimal Cantidad(string ejecucion)
        {
            return GetAux(ejecucion).Cantidad;
        }
        
        private DijkstraAux GetAux(string ejecucion)
        {
            try
            {
                DijkstraAux.TryGetValue(ejecucion, out DijkstraAux aux);
                if (aux == null)
                {
                    mutex.WaitOne();
                    aux = new DijkstraAux();
                    DijkstraAux.Add(ejecucion, aux);
                    mutex.ReleaseMutex();
                }

                return aux;
            }
            catch(Exception e)
            {
                return GetAux(ejecucion);
            }
        }
    }
}
