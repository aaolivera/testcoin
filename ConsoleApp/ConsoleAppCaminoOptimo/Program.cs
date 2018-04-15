using Dominio.Entidades;
using Dominio.Interfaces;
using Providers;
using System.Collections.Generic;

namespace ConsoleAppCaminoOptimo
{
    class Program
    {
        static void Main(string[] args)
        {
            var providers = new List<IProvider> { new YobitProvider() };
            var mercado = new Mercado(providers, new List<string>{ });
            mercado.ActualizarOrdenes();

            while (true)
            {
                System.Console.WriteLine("----------------------------");
                System.Console.WriteLine("----------------------------");
                System.Console.Write("-Desde:");
                var desde = System.Console.ReadLine();
                System.Console.Write("-Hasta:");
                var hasta = System.Console.ReadLine();
                var cantidad = 46.77M;
                cantidad = ObtenerOperacion(mercado, desde, hasta, cantidad);
                ObtenerOperacion(mercado, hasta, desde, cantidad);
                System.Console.Write("Fin");
                System.Console.ReadLine();
            }
        }

        private static decimal ObtenerOperacion(Mercado mercado, string desde, string hasta, decimal cantidad)
        {
            var resultado = mercado.ObtenerOperacionOptima(desde, hasta, cantidad, out string ej1);
            System.Console.WriteLine("");
            for (var j = 0; j < resultado.Count; j++)
            {
                var moneda = resultado[j];
                System.Console.Write($"{moneda.Nombre}({moneda.Cantidad(ej1).ToString("#.##########")}) => ");
                if (j == resultado.Count - 1) cantidad = moneda.Cantidad(ej1);
            }
            return cantidad;
        }
    }
}
