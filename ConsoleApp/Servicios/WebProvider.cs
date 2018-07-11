using CloudFlareUtilities;
using Dominio.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Providers
{
    public static class WebProvider
    {
        private static readonly List<string> Proxys = new List<string> {
            "local",
            "http://proxyphp1.gear.host/?url=",
            "http://proxy2.gearhostpreview.com/?url=",
            "http://proxy3.gearhostpreview.com/?url=",
            "http://proxy4.gearhostpreview.com/?url=",
            "http://proxy5.gearhostpreview.com/?url=",
            "http://proxy6.gearhostpreview.com/?url=",
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
            "http://proxy22.gearhostpreview.com/?url=",
            "http://proxy23.gearhostpreview.com/?url=",
            "http://proxy24.gearhostpreview.com/?url=",
            "http://proxy25.gearhostpreview.com/?url=",
            "http://proxy26.gearhostpreview.com/?url=",
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
        
        public static void DownloadPages(List<string> urls, Action<dynamic> callBack)
        {
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine($"Iniciando descarga {urls.Count}");
            var retorno = new List<dynamic>();
            var tasks = new List<Task<Bloque>>();
            var bloquesDeUrls = urls.Split(Proxys.Count).ToArray();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Bloques listos {bloquesDeUrls.Count()}");

            for (var i = 0; i < Proxys.Count && i < bloquesDeUrls.Count(); i++)
            {
                tasks.Add(DownloadPagesAsync(bloquesDeUrls[i].ToList(), Proxys[i], callBack));
            }
            Console.WriteLine($"Taks Instanciados");
            var bloques = (Task.WhenAll(tasks).Result).ToList();

            var descargasFallidas = bloques.Where(x => x.PaginasFallidas.Any());
            if (descargasFallidas.Any())
            {
                var paginas = bloques.SelectMany(x => x.PaginasFallidas).ToList();
                Console.WriteLine($"Existen {paginas.Count} paginas fallidas, reprocesandolas");
                DownloadPages(paginas, callBack);
            }
            stopwatch.Stop();
            Console.WriteLine("Descarga Completa en " + stopwatch.ElapsedMilliseconds * 0.001M);
            Console.WriteLine("---------------------------------------------------------------");
        }

        private static async Task<Bloque> DownloadPagesAsync(List<string> urls, string proxy, Action<dynamic> callBack)
        {
            return await Task.Run(() => DownloadPages(urls, proxy, callBack));
        }

        private static Bloque DownloadPages(List<string> urls, string proxy, Action<dynamic> callBack)
        {
            var retorno = new Bloque();
            var n = 1;
            try
            {
                foreach (var url in urls)
                {
                    var response = GetPage(proxy == "local" ? url : proxy + url);
                    callBack(response);
                    Console.WriteLine($"{n++}/{urls.Count} - {proxy}");
                }
            }
            catch
            {
                Console.WriteLine($"Proxy {proxy} en error, encolando paginas fallidas y quitando de la lista");
                Proxys.Remove(proxy);
                retorno.PaginasFallidas = urls.Skip(n).ToList();
            }
            return retorno;
        }
        
        private static dynamic GetPage(string url, int intento = 0)
        {
            try
            {
                //var handler = new ClearanceHandler
                //{
                //    MaxRetries = 2
                //};
                var client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsByteArrayAsync().Result;
                var responseStr = Encoding.UTF8.GetString(result);

                return JsonConvert.DeserializeObject(responseStr);
            }
            catch (Exception e)
            {
                if (intento == 2)
                {
                    throw new Exception();
                }
                return GetPage(url, intento + 1);
            }
        }

        public static dynamic PostPage(string url, string body, Dictionary<string, string> headers = null)
        {
            //var handler = new ClearanceHandler
            //{
            //    MaxRetries = 2
            //};
            var client = new HttpClient();
            HttpContent queryString = new StringContent(body);
            queryString.Headers.Clear();
            foreach (var i in headers?.Keys)
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

    }
}
