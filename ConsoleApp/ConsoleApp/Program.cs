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
                //while (true)
                //{
                var providers = new List<IProvider> { new YobitProvider() };
                var mercado = new Mercado(providers, new List<string> { "dash" });
                await mercado.ActualizarMonedas();
                await mercado.ActualizarOrdenes();

                var monedaPilar = "btc";

                System.Console.WriteLine("Iniciar busqueda");
                var inicial = 0.0001002M;
                
                var tasks = new List<Task>();
                foreach (var moneda in mercado.Monedas)
                {
                    ChequearMoneda(mercado, monedaPilar, inicial, moneda.Nombre);

                    // tasks.Add(ChequearMonedaAsync(mercado, monedaPilar, inicial, moneda.Nombre));
                }
                //System.Console.WriteLine("Buscando...");
                //Task.WaitAll(tasks.ToArray());
                //System.Console.WriteLine("Fin");
                System.Console.ReadLine();
                //}

        }

        private static async Task ChequearMonedaAsync(Mercado mercado, string monedaPilar, decimal inicial, string monedaDestino)
        {
            await Task.Run(() =>
            {
                ChequearMoneda(mercado, monedaPilar, inicial, monedaDestino);
            });
        }
        
        private static void ChequearMoneda(Mercado mercado, string monedaPilar, decimal inicial, string monedaDestino)
        {
            var movimientos = mercado.ObtenerOperacionOptima(monedaPilar, monedaDestino, inicial, out string ejecucionIda, out string ejecucionvuelta);
            var cantidadDestino = movimientos.First().Cantidad(ejecucionvuelta);
            var porcentaje = (((cantidadDestino - inicial) * 100) / inicial);
            if (porcentaje > 2 && movimientos.Count < 5)
            {
                var texto = $"{(movimientos.Count).ToString("00")}|{porcentaje.ToString("00.00")}|";
                var ida = true;
                foreach (var m in movimientos)
                {

                    texto += $"({m.Nombre}:{m.Cantidad(ida?ejecucionIda:ejecucionvuelta).ToString("F08", CultureInfo.InvariantCulture)})";
                    if (m.Nombre == monedaDestino && ida) ida = false;
                }
                System.Console.WriteLine("////////////////////////////////////////////////////////////////");
                System.Console.WriteLine(texto);
                //mercado.EjecutarMovimientos(todos, inicial);
                //System.Console.WriteLine("////////////////////////////////////////////////////////////////");
                System.Console.ReadLine();

            };
        }
    }
}
