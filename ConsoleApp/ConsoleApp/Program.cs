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

            while (true)
            {
                System.Console.WriteLine("Desde:");
                var desde = System.Console.ReadLine();
                System.Console.WriteLine("Hasta:");
                var hasta = System.Console.ReadLine();
                var resultado = mercado.ObtenerOperacionOptima(desde, hasta, 1);

                foreach (var o in resultado)
                {
                    System.Console.WriteLine($"{o.Nombre}({o.Peso}) =>");
                }
                System.Console.ReadLine();
            }
        }
    }
}
