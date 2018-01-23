using CloudFlareUtilities;
using Dominio.Entidades;
using Dominio.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Servicios
{
    public class YobitProvider : IProvider
    {
        private readonly string info = @"https://yobit.net/api/3/info";
        private readonly string depth = @"https://yobit.net/api/3/depth/{0}?limit=2&ignore_invalid=1";

        public void CargarOrdenes(Mercado mercado)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var relaciones = mercado.ObetenerRelacionesEntreMonedas().Take(100);
            System.Console.WriteLine(sw.ElapsedMilliseconds + " - Relaciones");
            var page = string.Empty;
            foreach(var i in relaciones.Select(x => x[0] + "_" + x[1]))
            {
                if(page.Length + i.Length > 510)
                {
                    var url = string.Format(depth, page);
                    System.Console.WriteLine(sw.ElapsedMilliseconds + " - inicia DownloadPage");
                    var response = DownloadPage(url);
                    System.Console.WriteLine(sw.ElapsedMilliseconds + " - inicia CargarPaginaDeOrdenes");
                    CargarPaginaDeOrdenes(response, mercado);
                    System.Console.WriteLine(sw.ElapsedMilliseconds + " - fin CargarPaginaDeOrdenes");
                    page = string.Empty;
                }
                page += "-" + i;
            }
            if (!string.IsNullOrEmpty(page))
            {
                var url = string.Format(depth, page);
                System.Console.WriteLine(sw.ElapsedMilliseconds + " - inicia DownloadPage2");
                var response = DownloadPage(url);
                System.Console.WriteLine(sw.ElapsedMilliseconds + " - inicia CargarPaginaDeOrdenes2");
                CargarPaginaDeOrdenes(response, mercado);
                System.Console.WriteLine(sw.ElapsedMilliseconds + " - fin CargarPaginaDeOrdenes2");
            }

            sw.Stop();

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
            var response = DownloadPageAsync(url).Result;
            var responseString = Encoding.UTF8.GetString(response);
            return JsonConvert.DeserializeObject(responseString);
        }

        private static async Task<byte[]> DownloadPageAsync(string url)
        {
            // ... Use HttpClient.
            var handler = new ClearanceHandler
            {
                MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
            };

            using (HttpClient client = new HttpClient(handler))
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                return await content.ReadAsByteArrayAsync();
            }
        }
    }
}
