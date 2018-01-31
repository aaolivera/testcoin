using Dominio.Entidades;
using Dominio.Interfaces;
using Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var providers = new List<IProvider> { new YobitProvider() };
            var mercado = new Mercado(providers);
            mercado.ActualizarOrdenes();
            var monedas = mercado.ObtenerMonedas();
            System.Console.Write("Moneda Pilar: ");
            var monedaPilar = System.Console.ReadLine();
            System.Console.WriteLine("");
            var inicial = 0.00005M;
            var tasks = new List<Task>();
            foreach (var moneda in monedas)
            {
                tasks.Add(ChequearMoneda(mercado, monedaPilar, inicial, moneda));
            }
            Task.WaitAll(tasks.ToArray());
            System.Console.ReadLine();
        }
        
        private static async Task ChequearMoneda(Mercado mercado, string monedaPilar, decimal inicial, string monedaDestino)
        {
            await Task.Run(() =>
            {
                var movimientosIda = mercado.ObtenerOperacionOptima(monedaPilar, monedaDestino, inicial, out Guid ejecucionIda);
                var cantidadDestino = movimientosIda.Last().Cantidad(ejecucionIda);
                if (cantidadDestino > 0)
                {
                    var movimientosVuelta = mercado.ObtenerOperacionOptima(monedaDestino, monedaPilar, cantidadDestino, out Guid ejecucionvuelta);
                    var cantidadVuelta = movimientosVuelta.Last().Cantidad(ejecucionvuelta);
                    if ((cantidadVuelta - inicial) > 0)
                    {
                        var texto = $"{(movimientosIda.Count + movimientosVuelta.Count).ToString("00")},{(((cantidadVuelta - inicial) * 100) / inicial).ToString("00.00")},";

                        foreach(var m in movimientosIda)
                        {
                            texto += $"({m.Nombre}:{m.Cantidad(ejecucionIda)})";
                        }


                        System.Console.Write($"({movimientos.ToString("00")}){moneda.Nombre.PadRight(10)} = {(((resultado - inicial) * 100) / inicial).ToString("00.00")}%");
                        System.Console.WriteLine("");
                    }
                };
            });
        }        
    }
}
