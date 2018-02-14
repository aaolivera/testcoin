using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Servicios
{
    public static class WebProvider
    {
        private static readonly List<string> Proxys = new List<string> {
            "local",
            "http://proxyphp1.gear.host/?url=",
            "http://proxy2.gear.host/?url=",
            "http://proxy3.gear.host/?url=",
            "http://proxy4.gear.host/?url=",
            "http://proxy5.gear.host/?url=",
            "http://proxy6.gear.host/?url=",
            "http://proxy7.gear.host/?url=",
            "http://proxy8.gear.host/?url=",
            "http://proxy9.gear.host/?url=",
            "http://proxy10.gear.host/?url=",
            "http://proxy11.gear.host/?url="
        };

        public static dynamic PostPage(string url, string body, Dictionary<string, string> headers)
        {
            //var handler = new ClearanceHandler
            //{
            //    MaxRetries = 2
            //};
            var client = new HttpClient();
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
                Thread.Sleep(1500);
                //var handler = new ClearanceHandler
                //{
                //    MaxRetries = 2
                //};
                var client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsByteArrayAsync().Result;
                var responseStr =  Encoding.UTF8.GetString(result);
                
                return JsonConvert.DeserializeObject(responseStr);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{url} - {intento} - {e.Message}");
                if (intento == 4)
                {
                    throw new Exception();
                }
                Thread.Sleep(1500);
                return DownloadPage(url, intento + 1);
            }
        }

        public static async Task<List<dynamic>> DownloadPages(List<string> urls, Action<string> notificarAvance)
        {
            Console.WriteLine($"Iniciando descarga {urls.Count}");
            var retorno = new List<dynamic>();
            var tamaniobloque = Convert.ToInt32(Math.Ceiling((decimal)(urls.Count / (Proxys.Count * 1M))));
            var indiceBloque = 0;
            var tasks = new List<Task<Bloque>>();

            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (var p in Proxys)
            {
                if(Proxys.Last() == p)
                {
                    tamaniobloque = urls.Count - indiceBloque;
                }
                tasks.Add(DownloadPagesAsync(urls.Skip(indiceBloque).Take(tamaniobloque).ToList(), p, notificarAvance));
                indiceBloque += tamaniobloque;
            }
            var bloques = (await Task.WhenAll(tasks.ToArray())).ToList();
            
            var descargasFallidas = bloques.Where(x => x.PaginasFallidas.Any());
            if (descargasFallidas.Any())
            {
                var paginas = bloques.SelectMany(x => x.PaginasFallidas).ToList();
                Console.WriteLine($"Existen {paginas.Count} paginas fallidas, reprocesandolas");
                
                var bloqueReprocesado = new Bloque { PaginasDescargadas = DownloadPages(paginas, notificarAvance).Result };
                bloques.Add(bloqueReprocesado);
            }
            stopwatch.Stop();
            Console.WriteLine("Descarga Completa en " + stopwatch.ElapsedMilliseconds * 0.001M);
            return bloques.SelectMany(x => x.PaginasDescargadas).ToList();
        }

        private static async Task<Bloque> DownloadPagesAsync(List<string> urls, string proxy, Action<string> notificarAvance)
        {
            var list = await Task.Run(() =>
            {
                Console.WriteLine($"Inicio {urls.Count} - {proxy} Obteniendo operaciones");
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
                        notificarAvance(url);
                    }
                }
                catch
                {
                    Console.WriteLine($"Proxy en error: {proxy}, encolando paginas fallidas y quitando de la lista");
                    Proxys.Remove(proxy);
                    retorno.PaginasFallidas = urls.Skip(n + 1).ToList();
                }                
                return retorno;
            });
            return list;
        }
    }
}
