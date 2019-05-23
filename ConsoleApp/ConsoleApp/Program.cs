using Dominio.Entidades;
using Dominio.Interfaces;
using Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ConsoleApp
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var providers = new List<IProvider> { new YobitProvider() };
            var mercado = new Mercado(providers, new List<string> { "btcu", "edc_ltc", "ltc_edc" });
            await mercado.ActualizarMonedas();

            while (true)
            {
                mercado.LimpiarOrdenes();
                await mercado.ActualizarOrdenes();

                //BUSCAR
                System.Console.WriteLine("Iniciar busqueda");
                var monedaPilar = mercado.ObtenerMoneda("btc");
                var inicial = 0.00010022M;
                var resultado = new ConcurrentBag<Jugada>();

                //ChequearMoneda(mercado, monedaPilar, inicial, mercado.Monedas.Where(c => c != monedaPilar).First());
                //var tasks = new List<Task>();
                foreach (var moneda in mercado.Monedas.Where(c => c != monedaPilar))
                {
                    if (ChequearMoneda(mercado, monedaPilar, inicial, moneda)) break;
                    //tasks.Add(ChequearMonedaAsync(mercado, monedaPilar, inicial, moneda));
                }

                System.Console.WriteLine("Buscando...");
                //await Task.WhenAll(tasks);
                System.Console.WriteLine("Fin");
                Console.Read();
                //Thread.Sleep(30000);
            }
            
        }

        private static async Task ChequearMonedaAsync(Mercado mercado, Moneda monedaPilar, decimal inicial, Moneda monedaDestino)
        {
            await Task.Run(() => ChequearMoneda(mercado, monedaPilar, inicial, monedaDestino));
        }

        private static bool ChequearMoneda(Mercado mercado, Moneda monedaPilar, decimal inicial, Moneda monedaDestino)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            var movimientos = mercado.ObtenerOperacionOptima(monedaPilar, monedaDestino, inicial, out string ejecucionIda, out string ejecucionvuelta);
            if(movimientos.Any())
            {
                //Console.WriteLine("--//" + monedaPilar.Nombre + "-" + monedaDestino.Nombre + "//--");

                var monedaInicial = movimientos.First();
                var cantidadDestino = monedaInicial.Cantidad(ejecucionvuelta);

                if (monedaInicial.Nombre == monedaPilar.Nombre && cantidadDestino > inicial)
                {
                    var porcentaje = (((cantidadDestino - inicial) * 100) / inicial);
                    //if (porcentaje > 0)
                    //{
                    var texto = $"{(movimientos.Count).ToString("00")}|{porcentaje.ToString("00.00")}|";
                    var ida = true;
                    foreach (var m in movimientos)
                    {

                        texto += $"({m.Nombre}:{m.Cantidad(ida ? ejecucionIda : ejecucionvuelta).ToString("F08", CultureInfo.InvariantCulture)})";
                        if (m.Nombre == monedaDestino.Nombre && ida) ida = false;
                    }
                    Console.WriteLine(texto);
                    mercado.EjecutarMovimientos(movimientos, monedaDestino, ejecucionIda, ejecucionvuelta).Wait();
                    return true;
                    //};
                }
            }
            return false;

            //stopwatch.Stop();
            //Console.WriteLine($"ChequearMoneda {monedaDestino.Nombre} en " + stopwatch.ElapsedMilliseconds * 0.001M);
            //return false;
        }
    }
}
