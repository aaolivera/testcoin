using Dominio.Entidades;
using Dominio.Interfaces;
using Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var providers = new List<IProvider> { new YobitProvider() };
            var mercado = new Mercado(providers, new List<string> { "btcu", "edc_ltc", "ltc_edc" });
            await mercado.ActualizarMonedas();

            while (true)
            {
                await mercado.ActualizarRelaciones();

                //BUSCAR
                foreach (var r in mercado.ListarJugadas(0.001m))
                {
                    Console.WriteLine($"{r.Inicial} -> {r.Final} = Ganancia {r.Ganancia}");
                    foreach(var m in r.Movimientos)
                    {
                        Console.WriteLine($"------{m.Origen.Nombre} ({m.CantidadOrigen})-> {m.Destino.Nombre}({m.CantidadDestino}), precio {m.Precio}");
                    }
                }
                System.Console.WriteLine("Fin");
                Console.Read();
                //Thread.Sleep(30000);
            }
            
        }

    }
}
