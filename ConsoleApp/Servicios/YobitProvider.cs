using Dominio.Entidades;
using Dominio.Interfaces;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace Servicios
{
    public class YobitProvider : IProvider
    {
        private readonly string info = @"https://yobit.net/api/3/info";
        private readonly string depth = @"https://yobit.net/api/3/depth/{0}?ignore_invalid=1";

        public void CargarOrdenes(Mercado mercado)
        {
            var relaciones = mercado.ObetenerRelacionesEntreMonedas();
            var page = string.Empty;
            var n = 0;
            foreach(var i in relaciones.Select(x => x[0] + "_" + x[1]))
            {
                n++;
                if (page.Length + i.Length > 510)
                {
                    System.Console.WriteLine($"{n}/{relaciones.Count()} Obteniendo operaciones");
                    CargarOrdenesPagina(page, mercado);
                    page = i;
                }
                else
                {
                    page += "-" + i;
                }                
            }
            if (!string.IsNullOrEmpty(page))
            {
                System.Console.WriteLine($"{n}/{relaciones.Count()} Obteniendo operaciones");
                CargarOrdenesPagina(page, mercado);                
            }
        }

        private void CargarOrdenesPagina(string page, Mercado mercado)
        {
            var url = string.Format(depth, page);
            var response = DownloadPage(url);
            CargarPaginaDeOrdenes(response, mercado);
        }

        private void CargarPaginaDeOrdenes(dynamic response, Mercado mercado)
        {
            foreach (var ordenesPorMoneda in response)
            {
                var monedas = ordenesPorMoneda.Name.Split('_');
                var ventas = ordenesPorMoneda.Value["asks"];
                var compras = ordenesPorMoneda.Value["bids"];

                if(ventas != null)
                {
                    foreach (var ordenVenta in ventas)
                    {
                        mercado.AgregarOrdenDeVenta(monedas[0], monedas[1], (decimal)ordenVenta[0].Value, (decimal)ordenVenta[1].Value);
                    }
                }
                
                if(compras != null)
                {
                    foreach (var ordenCompra in compras)
                    {
                        mercado.AgregarOrdenDeCompra(monedas[0], monedas[1], (decimal)ordenCompra[0].Value, (decimal)ordenCompra[1].Value);
                    }
                }
            }
        }

        public void CargarMonedas(Mercado mercado)
        {
            dynamic response = DownloadPage(info);
            
            foreach (var relacion in response.pairs)
            {
                var monedas = relacion.Name.Split('_');
                mercado.AgregarRelacionEntreMonedas(monedas[0], monedas[1]);
            }
        }
        
        private static dynamic DownloadPage(string url)
        {
            try
            {
                Thread.Sleep(1000);
                var response = new WebClient().DownloadString(url);
                return JsonConvert.DeserializeObject(response);
            }
            catch (Exception e)
            {
                return DownloadPage(url);
            }
        }
    }
}
