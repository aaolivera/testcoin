using CloudFlareUtilities;
using Dominio.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Providers
{
    public static class WebProvider
    {
        private static readonly List<string> Proxys = new List<string> {
            "http://localhost:63011/?url=",
            //"http://proxy2.gearhostpreview.com/?url=",
            //"http://proxy3.gearhostpreview.com/?url=",
            //"http://proxy4.gearhostpreview.com/?url=",
            //"http://proxy5.gearhostpreview.com/?url=",
            //"http://proxy6.gearhostpreview.com/?url=",
            //"http://proxy17.gear.host/?url=",
            //"http://proxy18.gear.host/?url=",
            //"http://proxy19.gear.host/?url=",
            //"http://proxy20.gear.host/?url=",
            //"http://proxy21.gear.host/?url=",
            //"http://proxy22.gearhostpreview.com/?url=",
            //"http://proxy23.gearhostpreview.com/?url=",
            //"http://proxy24.gearhostpreview.com/?url=",
            //"http://proxy25.gearhostpreview.com/?url=",
            //"http://proxy26.gearhostpreview.com/?url=",
            //"http://proxy27.gear.host/?url=",
            //"http://proxy28.gear.host/?url=",
            //"http://proxy29.gear.host/?url=",
            //"http://proxy30.gear.host/?url=",
            //"http://proxy31.gear.host/?url=",
            //"http://proxy32.gearhostpreview.com/?url=",
            //"http://proxy33.gearhostpreview.com/?url=",
            //"http://proxy34.gearhostpreview.com/?url=",
            //"http://proxy35.gearhostpreview.com/?url=",
            //"http://proxy36.gearhostpreview.com/?url=",
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

        public static void CheckProxys()
        {
            var tasks = new List<Task>();
            
            for (var i = 0; i < Proxys.Count; i++)
            {
                tasks.Add(CheckProxyAsync(Proxys[i]));
            }
            Task.WhenAll(tasks).Wait();
        }

        private static async Task CheckProxyAsync(string proxy)
        {
            await Task.Run(() => CheckProxy(proxy));
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
                //foreach (var url in urls)
                //{
                //    var response = GetPage(proxy == "local" ? url : proxy + url);
                //    callBack(response);
                //    Console.WriteLine($"{n++}/{urls.Count} - {proxy}");
                //}
                var myContent = JsonConvert.SerializeObject(urls);
                var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };

                PostPage(proxy, myContent, headers, callBack);
            }
            catch
            {
                Console.WriteLine($"Proxy {proxy} en error, encolando paginas fallidas y quitando de la lista");
                Proxys.Remove(proxy);
                retorno.PaginasFallidas = urls.Skip(n).ToList();
            }
            return retorno;
        }

        private static void CheckProxy(string proxy)
        {
            try
            {

                var r = GetPage(proxy.Replace("?url=", "home/version"));
                if(r != 1)
                {
                    Console.WriteLine($"Proxy {proxy} antigua");
                    Proxys.Remove(proxy);
                }
            }
            catch
            {
                Console.WriteLine($"Proxy {proxy} en error");
                Proxys.Remove(proxy);
            }
        }

        private static dynamic GetPage(string url, int intento = 0)
        {
            try
            {
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

        public static dynamic PostPage(string url, string body, Dictionary<string, string> headers, Action<dynamic> callBack = null, int intento = 0)
        {
            try
            {
                var client = new HttpClient();
                HttpContent queryString = new StringContent(body);
                queryString.Headers.Clear();
                foreach (var i in headers.Keys)
                {
                    queryString.Headers.Add(i, headers[i]);
                }
                var response = client.PostAsync(url, queryString).Result;
                var result = response.Content.ReadAsByteArrayAsync().Result;
                var responseStr = Encoding.UTF8.GetString(result);

                dynamic dinamic = JsonConvert.DeserializeObject(responseStr);
                //if (dinamic["success"] != 1)
                //{
                //    throw new Exception(dinamic["error"]);
                //}
                //else
                //{
                    
                //}
                callBack?.Invoke(dinamic);
                return dinamic;
            }
            catch (Exception e)
            {
                if (intento == 2)
                {
                    throw new Exception();
                }
                return PostPage(url, body, headers, callBack, intento + 1);
            }
        }

        
    }
}
