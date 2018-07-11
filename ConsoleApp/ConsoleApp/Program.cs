using Dominio.Entidades;
using Dominio.Interfaces;
using Providers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var providers = new List<IProvider> { new YobitProvider() };
                var mercado = new Mercado(providers, new List<string>{ "dash" });
                mercado.ActualizarMonedas();
                mercado.ActualizarOrdenes();
                //var monedas = mercado.ObtenerMonedas();

                //foreach (var moneda in mercado.ListarMonedasInfimas())
                //{
                //    System.Console.WriteLine(moneda.Nombre + ": " + string.Join(",", moneda.OrdenesDeCompraPorMoneda.Keys.Select(y =>y.Nombre))); 
                //}
                //System.Console.WriteLine("Bloques descargados...");
                //System.Console.ReadLine();

                //var monedaPilar = "btc";

                //System.Console.WriteLine("");
                //var inicial = 0.0001002M;

                //var tasks = new List<Task>();
                //foreach (var moneda in monedas)
                //{
                //    ChequearMoneda(mercado, monedaPilar, inicial, moneda.Nombre);

                //    //tasks.Add(ChequearMonedaAsync(mercado, monedaPilar, inicial, moneda.Nombre));
                //}
                //System.Console.WriteLine("Buscando...");
                //Task.WaitAll(tasks.ToArray());
                System.Console.WriteLine("Fin");
                System.Console.ReadLine();
            }
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
                var movimientosIda = mercado.ObtenerOperacionOptima(monedaPilar, monedaDestino, inicial, out string ejecucionIda);
                var cantidadDestino = movimientosIda.Last().Cantidad(ejecucionIda);
                if (cantidadDestino > 0)
                {
                    var movimientosVuelta = mercado.ObtenerOperacionOptima(monedaDestino, monedaPilar, cantidadDestino, out string ejecucionvuelta);
                    var cantidadVuelta = movimientosVuelta.Last().Cantidad(ejecucionvuelta);
 
                    if (cantidadVuelta > 0)
                    {
                        movimientosVuelta.RemoveAt(0);
                        var todos = new List<Moneda>();
                        todos.AddRange(movimientosIda);
                        todos.AddRange(movimientosVuelta);
                        var porcentaje = (((cantidadVuelta - inicial) * 100) / inicial);
                        var cantidadMovimientos = todos.Count - 1;
                        if (porcentaje >= 1 && cantidadMovimientos <= 4)
                        {
                            var texto = $"{(cantidadMovimientos).ToString("00")}|{porcentaje.ToString("00.00")}|";

                            foreach (var m in movimientosIda)
                            {
                                texto += $"({m.Nombre}:{m.Cantidad(ejecucionIda).ToString("F08", CultureInfo.InvariantCulture)})";
                            }
                            foreach (var m in movimientosVuelta)
                            {
                                texto += $"({m.Nombre}:{m.Cantidad(ejecucionvuelta).ToString("F08", CultureInfo.InvariantCulture)})";
                            }
                            System.Console.WriteLine("////////////////////////////////////////////////////////////////");
                            System.Console.WriteLine(texto);
                        mercado.EjecutarMovimientos(todos, inicial);
                        System.Console.WriteLine("////////////////////////////////////////////////////////////////");
                        System.Console.ReadLine();
                    }
                }
                };
        }        
    }
}
