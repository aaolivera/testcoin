using CloudFlareUtilities;
using Dominio.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Providers
{
    public static class WebProvider
    {
        private static readonly List<string> Proxys = new List<string> {
            "local",
            "http://proxyphp1.gear.host/?url=",
            //"http://proxy2.gear.host/?url=",
            //"http://proxy3.gear.host/?url=",
            //"http://proxy4.gear.host/?url=",
            //"http://proxy5.gear.host/?url=",
            //"http://proxy6.gear.host/?url=",
            "http://proxy7.gear.host/?url=",
            "http://proxy8.gear.host/?url=",
            "http://proxy9.gear.host/?url=",
            "http://proxy10.gear.host/?url=",
            "http://proxy11.gear.host/?url=",
            //"http://proxy12.gear.host/?url=",
            //"http://proxy13.gear.host/?url=",
            //"http://proxy14.gear.host/?url=",
            //"http://proxy15.gear.host/?url=",
            //"http://proxy16.gear.host/?url=",
            "http://proxy17.gear.host/?url=",
            "http://proxy18.gear.host/?url=",
            "http://proxy19.gear.host/?url=",
            "http://proxy20.gear.host/?url=",
            "http://proxy21.gear.host/?url=",
            //"http://proxy22.gear.host/?url=",
            //"http://proxy23.gear.host/?url=",
            //"http://proxy24.gear.host/?url=",
            //"http://proxy25.gear.host/?url=",
            //"http://proxy26.gear.host/?url=",
            "http://proxy27.gear.host/?url=",
            "http://proxy28.gear.host/?url=",
            "http://proxy29.gear.host/?url=",
            "http://proxy30.gear.host/?url=",
            "http://proxy31.gear.host/?url=",
            //"http://proxy32.gear.host/?url=",
            //"http://proxy33.gear.host/?url=",
            //"http://proxy34.gear.host/?url=",
            //"http://proxy35.gear.host/?url=",
            //"http://proxy36.gear.host/?url="
        };

        public static dynamic PostPage(string url, string body, Dictionary<string, string> headers)
        {
            var handler = new ClearanceHandler
            {
                MaxRetries = 2
            };
            var client = new HttpClient(handler);
            HttpContent queryString = new StringContent(body);
            queryString.Headers.Clear();
            foreach (var i in headers.Keys)
            {
                queryString.Headers.Add(i, headers[i]);
            }
            HttpResponseMessage response = client.PostAsync(url, queryString).Result;
            var result = response.Content.ReadAsByteArrayAsync().Result;
            var responseStr = Encoding.UTF8.GetString(result);
            
            dynamic dinamic = JsonConvert.DeserializeObject(responseStr);
            if (dinamic["success"] != 1)
            {
                throw new Exception(dinamic["error"]);
            }
            return dinamic;
        }

        public static dynamic DownloadPage(string url, int intento = 0)
        {
            try
            {
                var handler = new ClearanceHandler
                {
                    MaxRetries = 2
                };
                var client = new HttpClient(handler);
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsByteArrayAsync().Result;
                var responseStr =  Encoding.UTF8.GetString(result);
                
                return JsonConvert.DeserializeObject(responseStr);
            }
            catch (Exception e)
            {
                //Console.WriteLine($"{url} - {intento} - {e.Message}");
                if (intento == 4)
                {
                    throw new Exception();
                }
                //Thread.Sleep(1500);
                return DownloadPage(url, intento + 1);
            }
        }

        public static List<dynamic> DownloadPages(List<string> urls)
        {
            Console.WriteLine($"Iniciando descarga {urls.Count}");
            var retorno = new List<dynamic>();
            var tasks = new List<Task<Bloque>>();
            var bloquesDeUrls = urls.Split(Proxys.Count).ToArray();
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Bloques listos {Proxys.Count}");

            for (var i = 0; i < Proxys.Count; i++)
            {
                tasks.Add(DownloadPagesAsync(bloquesDeUrls[i].ToList(), Proxys[i]));
            }
            Console.WriteLine($"Taks Instanciados " + stopwatch.ElapsedMilliseconds * 0.001M);
            var bloques = (Task.WhenAll(tasks).Result).ToList();
            

            var descargasFallidas = bloques.Where(x => x.PaginasFallidas.Any());
            if (descargasFallidas.Any())
            {
                var paginas = bloques.SelectMany(x => x.PaginasFallidas).ToList();
                Console.WriteLine($"Existen {paginas.Count} paginas fallidas, reprocesandolas");
                
                var bloqueReprocesado = new Bloque { PaginasDescargadas = DownloadPages(paginas) };
                bloques.Add(bloqueReprocesado);
            }
            stopwatch.Stop();
            Console.WriteLine("Descarga Completa en " + stopwatch.ElapsedMilliseconds * 0.001M);
            return bloques.SelectMany(x => x.PaginasDescargadas).ToList();
        }

        private static async Task<Bloque> DownloadPagesAsync(List<string> urls, string proxy)
        {
            return await Task.Run(() => DownloadPages(urls, proxy));
        }

        private static Bloque DownloadPages(List<string> urls, string proxy)
        {
            //Console.WriteLine($"Inicio {urls.Count} - {proxy} Obteniendo operaciones");
            var retorno = new Bloque();
            var n = 0;
            try
            {
                foreach (var url in urls)
                {
                    if (proxy == "local")
                    {
                        retorno.PaginasDescargadas.Add(DownloadPage(url));
                    }
                    else
                    {
                        retorno.PaginasDescargadas.Add(DownloadPage(proxy + url));
                    }
                    Console.WriteLine($"{n++}/{urls.Count} - {proxy}");
                }
            }
            catch
            {
                Console.WriteLine($"Proxy en error: {proxy}, encolando paginas fallidas y quitando de la lista");
                Proxys.Remove(proxy);
                retorno.PaginasFallidas = urls.Skip(n + 1).ToList();
            }
            return retorno;
        }
    }
}
