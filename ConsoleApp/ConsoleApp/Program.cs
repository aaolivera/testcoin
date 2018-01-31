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
            var inicial = 0.00005M;
            foreach (var moneda in monedas)
            {
                var cantidad = ObtenerOperacion(mercado, monedaPilar, moneda.Nombre, inicial, out int m);
                if (cantidad > 0)
                {
                    var resultado = ObtenerOperacion(mercado, moneda.Nombre, monedaPilar, cantidad, out int movimientos);
                    if ((resultado - inicial) > 0)
                    {
                        System.Console.Write($"({i.ToString("0000")}-{movimientos.ToString("00")}){moneda.Nombre.PadRight(10)} = {(((resultado - inicial)*100)/inicial).ToString("00.00")}%");
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
            //    var cantidad = 1M;
            //    cantidad = ObtenerOperacion(mercado, desde, hasta, cantidad, out int mov);
            //    ObtenerOperacion(mercado, hasta, desde, cantidad, out mov);
            //    System.Console.Write("Fin");
            //    System.Console.ReadLine();
            //}
        }

        private async void ObtenerOperacion(Mercado mercado, string desde, string hasta, decimal cantidad)
        {
            int movimientos;
            var resultado = await mercado.ObtenerOperacionOptima(desde, hasta, cantidad);
            //mercado.EliminarOrdenes(resultado);
            
            for (var j = 0; j < resultado.Count; j++)
            {
                var moneda = resultado[j];
                //System.Console.Write($"{moneda.Nombre}({moneda.Cantidad.ToString("#.##########")}) => ");
                if (j == resultado.Count - 1) cantidad = moneda.Cantidad;
            }
            movimientos = resultado.Count;
            if (cantidad > 0)
            {
                var resultadoVuelta = ObtenerOperacion(mercado, moneda.Nombre, monedaPilar, cantidad);
                if ((resultado - inicial) > 0)
                {
                    System.Console.Write($"({i.ToString("0000")}-{movimientos.ToString("00")}){moneda.Nombre.PadRight(10)} = {(((resultado - inicial) * 100) / inicial).ToString("00.00")}%");
                    System.Console.WriteLine("");
                }
            }
        }
    }
}
