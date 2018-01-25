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
                System.Console.WriteLine("----------------------------");
                System.Console.WriteLine("----------------------------");
                System.Console.Write("Desde:");
                var desde = System.Console.ReadLine();
                System.Console.Write(" Hasta:");
                var hasta = System.Console.ReadLine();
                var resultado = mercado.ObtenerOperacionOptima(desde, hasta, 1);
                System.Console.WriteLine("");
                foreach (var o in resultado)
                {
                    System.Console.Write($"{o.Nombre}({o.Peso.ToString("#.##########")}) => ");
                }
                System.Console.Write("Fin");
                System.Console.ReadLine();
            }
        }
    }
}
