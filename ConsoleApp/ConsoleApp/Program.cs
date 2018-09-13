using Dominio.Entidades;
using Dominio.Interfaces;
using Providers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
                var providers = new List<IProvider> { new YobitProvider() };
                //var mercado = new Mercado(providers, new List<string> { "dash" }, new List<string> { "edr2_btc", "edr2_ltc", "ltc_btc" });
            var mercado = new Mercado(providers, new List<string> { "btcu" });
            await mercado.ActualizarMonedas();
                while (true)
                {
                    mercado.LimpiarOrdenes();
                    await mercado.ActualizarOrdenes();

                    var monedaPilar = mercado.ObtenerMoneda("btc");

                    System.Console.WriteLine("Iniciar busqueda");
                    var inicial = 0.0001002M;

                    var tasks = new List<Task>();
                    foreach (var moneda in mercado.Monedas)
                    {
                        await ChequearMonedaAsync(mercado, monedaPilar, inicial, moneda);
                    }
                    System.Console.WriteLine("Buscando...");
                    Task.WaitAll(tasks.ToArray());
                    System.Console.WriteLine("Fin");
                    System.Console.ReadLine();
                }
        }

        //private static async Task ChequearMonedaAsync(Mercado mercado, string monedaPilar, decimal inicial, string monedaDestino)
        //{
        //    await Task.Run((Action)(() =>
        //    {
        //        Program.ChequearMonedaAsync((Mercado)mercado, (string)monedaPilar, (decimal)inicial, (string)monedaDestino);
        //    }));
        //}
        
        private static async Task ChequearMonedaAsync(Mercado mercado, Moneda monedaPilar, decimal inicial, Moneda monedaDestino)
        {
            
            var movimientos = mercado.ObtenerOperacionOptima(monedaPilar, monedaDestino, inicial, out string ejecucionIda, out string ejecucionvuelta);
            var cantidadDestino = movimientos.First().Cantidad(ejecucionvuelta);
            var porcentaje = (((cantidadDestino - inicial) * 100) / inicial);
            if (porcentaje > 1 && movimientos.Count <= 6)
            {
                var texto = $"{(movimientos.Count).ToString("00")}|{porcentaje.ToString("00.00")}|";
                var ida = true;
                foreach (var m in movimientos)
                {

                    texto += $"({m.Nombre}:{m.Cantidad(ida?ejecucionIda:ejecucionvuelta).ToString("F08", CultureInfo.InvariantCulture)})";
                    if (m.Nombre == monedaDestino.Nombre && ida) ida = false;
                }
                System.Console.WriteLine("--//" + monedaPilar.Nombre + "-"+ monedaDestino.Nombre + "//--");
                System.Console.WriteLine(texto);
                await mercado.EjecutarMovimientos(movimientos, monedaDestino, ejecucionIda, ejecucionvuelta);
                System.Console.WriteLine("////////////////////////////////////////////////////////////////");
                //System.Console.ReadLine();
            };
        }
    }
}
