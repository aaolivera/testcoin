using Dominio.Entidades;
using Dominio.Interfaces;
using Servicios;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var providers = new List<IProvider> { new YobitProvider() };
            var mercado = new Mercado(providers);
            mercado.ActualizarOrdenes();
        }
    }
}
