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
            var inicial = 0.000105M;
            var tasks = new List<Task>();
            foreach (var moneda in monedas)
            {
                //ChequearMoneda(mercado, monedaPilar, inicial, moneda.Nombre);
                tasks.Add(ChequearMoneda(mercado, monedaPilar, inicial, moneda.Nombre));
            }
            System.Console.WriteLine("Esperando");
            Task.WaitAll(tasks.ToArray());
            System.Console.WriteLine("Fin");
            System.Console.ReadLine();
        }

        private static async Task ChequearMoneda(Mercado mercado, string monedaPilar, decimal inicial, string monedaDestino)
        //private static void ChequearMoneda(Mercado mercado, string monedaPilar, decimal inicial, string monedaDestino)
        {
            await Task.Run(() =>
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
                    //if (porcentaje > 4 && cantidadMovimientos <= 5)
                    if (porcentaje > 4)
                    {
                            var texto = $"{(cantidadMovimientos).ToString("00")}|{porcentaje.ToString("00.00")}|";

                            foreach (var m in movimientosIda)
                            {
                                texto += $"({m.Nombre}:{m.Cantidad(ejecucionIda)})";
                            }
                            foreach (var m in movimientosVuelta)
                            {
                                texto += $"({m.Nombre}:{m.Cantidad(ejecucionvuelta)})";
                            }
                        System.Console.WriteLine(texto);
                        //mercado.EjecutarMovimientos(todos, inicial);
                        System.Console.ReadLine();
                    }
                }
                };
            });
        }        
    }
}
