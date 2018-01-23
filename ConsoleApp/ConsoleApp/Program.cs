using Dominio.Entidades;
using Dominio.Interfaces;
using Servicios;
using System.Collections.Generic;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var providers = new List<IProvider> { new YobitProvider() };
            var mercado = new Mercado(providers);
            mercado.ActualizarOrdenes();

            var resultado = mercado.ObtenerOperacionOptima("btc", "usd", 10);

            foreach(var o in resultado)
            {
                System.Console.WriteLine(o.MonedaAnterior);
                System.Console.WriteLine(o.Orden);
            }
            System.Console.ReadLine();
        }
    }
}
