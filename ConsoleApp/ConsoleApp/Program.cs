using Dominio.Entidades;
using Dominio.Interfaces;
using Servicios;
using System.Collections.Generic;
using System.Linq;

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
            var i = 0;
            foreach (var moneda in monedas)
            {
                var cantidad = ObtenerOperacion(mercado, monedaPilar, moneda.Nombre, 1, out int m);
                if (cantidad > 0)
                {
                    var resultado = ObtenerOperacion(mercado, moneda.Nombre, monedaPilar, cantidad, out int movimientos);
                    if((resultado - 1) > 0)
                    {
                        System.Console.Write($"({i.ToString("0000")}-{movimientos.ToString("00")}){moneda.Nombre.PadRight(10)} = {(resultado - 1).ToString("00.00")} {monedaPilar.ToUpper()}");
                        System.Console.WriteLine("");
                    }
                }
                i++;
            }
            System.Console.ReadLine();
            //while (true)
            //{
            //    System.Console.WriteLine("----------------------------");
            //    System.Console.WriteLine("----------------------------");
            //    System.Console.Write("-Desde:");
            //    var desde = System.Console.ReadLine();
            //    System.Console.Write("-Hasta:");
            //    var hasta = System.Console.ReadLine();
            //    for(var i = 0; i < 4; i++)
            //    {
            //        var cantidad = 1M;
            //        cantidad = ObtenerOperacion(mercado, desde, hasta, cantidad);
            //        ObtenerOperacion(mercado, hasta, desde, cantidad);
            //        System.Console.Write("Fin");
            //    }
            //    System.Console.ReadLine();
            //}
        }

        private static decimal ObtenerOperacion(Mercado mercado, string desde, string hasta, decimal cantidad, out int movimientos)
        {
            var resultado = mercado.ObtenerOperacionOptima(desde, hasta, cantidad);
            //mercado.EliminarOrdenes(resultado);
            
            for (var j = 0; j < resultado.Count; j++)
            {
                var moneda = resultado[j];
                //System.Console.Write($"{moneda.Nombre}({moneda.Cantidad.ToString("#.##########")}) => ");
                if (j == resultado.Count - 1) cantidad = moneda.Cantidad;
            }
            movimientos = resultado.Count;
            return cantidad;
        }
    }
}
