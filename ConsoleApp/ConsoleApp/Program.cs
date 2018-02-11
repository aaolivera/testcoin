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
                var mercado = new Mercado(providers);
                var relaciones = mercado.ListarRelacionesReelevantes();

                var tasks = new List<Task>();
                foreach (var r in relaciones)
                {
                    System.Console.WriteLine($"{r.Principal.Nombre} - {r.Secundaria.Nombre} => delta {r.DeltaEjecutado} - volumen {r.Volumen}");
                }
                System.Console.WriteLine("Buscando...");
                Task.WaitAll(tasks.ToArray());
                System.Console.WriteLine("Fin");
                System.Console.ReadLine();
            }
        }

    }
}
