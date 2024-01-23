﻿using CloudFlareUtilities;
using Dominio.Dtos;
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
            //"http://localhost:63011/?url=",
           

        
            "http://proxy30.gear.host/?url=",
            "http://proxy31.gear.host/?url="
        };

        

        public static async Task DownloadPages(List<string> urls, Action<dynamic> callBack)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine($"Iniciando descarga {urls.Count}");
            if (Proxys.Count == 0)
            {
                Console.WriteLine($"Nn hay Proxys disponibles");
                return;
            }
            var retorno = new List<dynamic>();
            var tasks = new List<Task<Bloque>>();
            var client = new HttpClientApp();
            var bloquesDeUrls = urls.Split(Proxys.Count).ToArray();
            Console.WriteLine($"Bloques listos {bloquesDeUrls.Count()} en {stopwatch.ElapsedMilliseconds * 0.001M}");

            for (var i = 0; i < Proxys.Count && i < bloquesDeUrls.Count(); i++)
            {
                tasks.Add(DownloadPagesAsync(bloquesDeUrls[i].ToList(), Proxys[i], callBack, client));
            }
            Console.WriteLine($"Taks Instanciados en {stopwatch.ElapsedMilliseconds * 0.001M}");

            var bloques = await Task.WhenAll(tasks);
            Console.WriteLine($"Taks Terminados en {stopwatch.ElapsedMilliseconds * 0.001M}");

            var descargasFallidas = bloques.Where(x => x.PaginasFallidas.Any());
            if (descargasFallidas.Any())
            {
                var paginas = bloques.SelectMany(x => x.PaginasFallidas).ToList();
                
                Console.WriteLine($"Existen {paginas.Count} paginas fallidas, reprocesandolas");
                await DownloadPages(paginas, callBack);
            }
            stopwatch.Stop();
            Console.WriteLine("Descarga Completa en " + stopwatch.ElapsedMilliseconds * 0.001M);
            Console.WriteLine("---------------------------------------------------------------");
        }
        
        private static async Task<Bloque> DownloadPagesAsync(List<string> urls, string proxy, Action<dynamic> callBack, HttpClientApp client)
        {
            var retorno = new Bloque();
            try
            {
                var myContent = JsonConvert.SerializeObject(urls);
                var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
                Stopwatch stopwatch = Stopwatch.StartNew();

                var it = stopwatch.ElapsedMilliseconds;
                var responseStr = await client.PostAsync(proxy, myContent, headers);
                it = stopwatch.ElapsedMilliseconds - it;
                
                var c = stopwatch.ElapsedMilliseconds;
                ProxyResult dinamic = JsonConvert.DeserializeObject<ProxyResult>(responseStr);
                callBack?.Invoke(dinamic.Responses.Select(v => Encoding.UTF8.GetString(v)));
                c = stopwatch.ElapsedMilliseconds - c;

                Console.WriteLine($"Post {proxy}: Proxy {dinamic.Tiempo}, Consola: {it * 0.001M}, Procesado: {c * 0.001M} segs, {dinamic.Responses.Sum(x => x.Length)}");
            }
            catch
            {
                Console.WriteLine($"Proxy {proxy} en error, encolando paginas fallidas y quitando de la lista");
                Proxys.Remove(proxy);
                retorno.PaginasFallidas = urls;
            }
            return retorno;
        }
    }
}
