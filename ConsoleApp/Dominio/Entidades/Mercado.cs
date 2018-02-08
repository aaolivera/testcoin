using Dominio.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dominio.Entidades
{
    public class Mercado
    {
        private List<IProvider> Providers = new List<IProvider>();
        private Dictionary<string, Moneda> Monedas { get; } = new Dictionary<string, Moneda>();
        private Dictionary<string, Relacion> RelacionesEntreMonedas { get; } = new Dictionary<string, Relacion>();

        public Mercado(List<IProvider> providers)
        {
            foreach(var p in providers)
            {
                p.CargarMonedas(this);
                Providers.Add(p);
            }
        }

        public List<Relacion> ObtenerRelacionesReelevantes()
        {
            foreach (var r in RelacionesEntreMonedas.Values)
            {
                r.Limpiar();
            }
            ActualizarOrdenes();
            var relaciones = RelacionesEntreMonedas.Values.ToList();
            relaciones.Sort();
            return relaciones.Take(50).ToList();
        }

        private void ActualizarOrdenes()
        {
            foreach (var p in Providers)
            {
                p.CargarOrdenes(this);
            }
        }
        
        public List<string> ObetenerRelacionesEntreMonedas()
        {
            return RelacionesEntreMonedas.Keys.ToList();
        }
        
        public void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            var monedaA = ObtenerMoneda(monedaNameA);
            var monedaB = ObtenerMoneda(monedaNameB);
            
            RelacionesEntreMonedas.Add(monedaNameA + "_" + monedaNameB, new Relacion(monedaA, monedaB));
        }

        public void AgregarOrden(string relacionName, decimal precio, decimal cantidad, bool esDeVenta)
        {
            RelacionesEntreMonedas.TryGetValue(relacionName, out Relacion relacion);
            var orden = new Orden()
            {
                Cantidad = cantidad,
                EsDeVenta = esDeVenta,
                PrecioUnitario = precio
            };

            relacion?.AgregarOrden(orden);
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

        //public void EjecutarMovimientos(List<Moneda> movimientos, decimal inicial)
        //{
        //    var cantidad = inicial;
        //    var provider = Providers[0];
        //    for (var i = 0; i < movimientos.Count - 1; i++)
        //    {
        //        var actual = movimientos[i];
        //        var siguiente = movimientos[i + 1];

        //        var ordenesNecesarias = provider.ObtenerOrdenesNecesarias(actual, siguiente, cantidad, out string relacion);

        //        var cantidadResultado = 0M;
        //        System.Console.WriteLine("https://yobit.net/en/trade/" + relacion.Replace('_', '/').ToUpper());
        //        foreach (var orden in ordenesNecesarias)
        //        {
        //            cantidadResultado += provider.EjecutarOrden(orden, relacion);
        //        }

        //        while (provider.HayOrdenesActivas(relacion))
        //        {
        //            Thread.Sleep(1500);
        //        }

        //        cantidad = provider.ConsultarSaldo(siguiente.Nombre);
        //        cantidad = cantidadResultado;
        //    }
        //}
    }
}
