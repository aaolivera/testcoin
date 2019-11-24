using Dominio.Entidades;
using Dominio.Helper;
using Dominio.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Providers
{
    public class YobitProvider : IProvider
    {
        private static readonly int cantidad = 1;
        private readonly string info = @"https://yobit.net/api/3/info";
        //private readonly string depth = @"https://yobit.net/api/3/depth/{0}?ignore_invalid=1&limit=" + cantidad;
        private readonly string priv = @"https://yobit.net/tapi/";
        private readonly string ticker = @"https://yobit.net/api/3/ticker/{0}?ignore_invalid=1";

        #region CARGA
        public async Task ActualizarMonedas(IMercadoCargar mercado, List<string> exclude, List<string> include)
        {
            await WebProvider.DownloadPages(new List<string> { info }, x => CargarMonedas(x, mercado, exclude, include));
        }

        //public async Task ActualizarOrdenes(IMercadoCargar mercado)
        //{
        //    await Descargar(mercado, ticker, x => CargarPaginaDeOrdenes(x, mercado));
        //}

        public async Task ActualizarRelaciones(IMercadoCargar mercado)
        {
            await Descargar(mercado, ticker, x => CargarPaginaDeRelaciones(x, mercado));
        }

        private async Task Descargar(IMercadoCargar mercado, string url, Action<dynamic> callBack)
        {
            var page = string.Empty;

            //Armo Urls de paginas
            var paginasticker = new List<string>();
            for (int i1 = 0; i1 < mercado.RelacionesEntreMonedas.Count; i1++)
            {
                string i = mercado.RelacionesEntreMonedas[i1];
                if (i1 % 20 == 0 && i1 > 0)
                {
                    paginasticker.Add(string.Format(url, page));
                    page = i;
                }
                else if (page.Length == 0)
                {
                    page = i;
                }
                else
                {
                    page += "-" + i;
                }
            }
            if (!string.IsNullOrEmpty(page))
            {
                paginasticker.Add(string.Format(url, page));
            }

            //Obtengo y cargo       
            await WebProvider.DownloadPages(paginasticker.ToList(), callBack);
        }


        private void CargarPaginaDeRelaciones(IEnumerable<string> responses, IMercadoCargar mercado)
        {
            try
            {
                foreach (string response in responses)
                {   
                    dynamic relaciones = JsonConvert.DeserializeObject<dynamic>(response);
                    foreach (dynamic relacion in relaciones)
                    {
                        dynamic ordenesPorMoneda = relacion;

                        var monedas = ordenesPorMoneda.Name.Split('_');
                        var avg = ordenesPorMoneda.Value["avg"];
                        var vol = ordenesPorMoneda.Value["vol"];
                        var buy = ordenesPorMoneda.Value["buy"];
                        var sell = ordenesPorMoneda.Value["sell"];

                        mercado.CargarRelacionEntreMonedas(monedas[0], monedas[1], (decimal)vol, (decimal)buy, (decimal)sell);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al mapear response CargarPaginaDeOrdenes");
                throw;
            }
            
        }

        //private void CargarPaginaDeOrdenes(IEnumerable<string> responses, IMercadoCargar mercado)
        //{
        //    try
        //    {
        //        foreach (string response in responses)
        //        {
        //            dynamic relaciones = JsonConvert.DeserializeObject<dynamic>(response);
        //            foreach (dynamic relacion in relaciones)
        //            {
        //                dynamic ordenesPorMoneda = relacion;

        //                var monedas = ordenesPorMoneda.Name.Split('_');
        //                var ventas = ordenesPorMoneda.Value["asks"];
        //                var compras = ordenesPorMoneda.Value["bids"];
        //                if (ventas != null)
        //                {
        //                    var i = 0;
        //                    foreach (var ordenVenta in ventas)
        //                    {
        //                        mercado.AgregarOrdenDeVenta(monedas[0], monedas[1], (decimal)ordenVenta[0].Value, (decimal)ordenVenta[1].Value);
        //                    }
        //                }

        //                if (compras != null)
        //                {
        //                    var j = 0;

        //                    foreach (var ordenCompra in compras)
        //                    {
        //                        mercado.AgregarOrdenDeCompra(monedas[0], monedas[1], (decimal)ordenCompra[0].Value, (decimal)ordenCompra[1].Value);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Error al mapear response CargarPaginaDeOrdenes");
        //        throw;
        //    }

        //}

        #endregion



        public async Task<decimal> ConsultarSaldo(string moneda)
        {
            var body = "method=getInfo&nonce={0}";
            dynamic response = await PostPage(priv, body);
            var saldo = response == null || response["return"]["funds"][moneda] == null ? 0 : response["return"]["funds"][moneda].Value;
            return decimal.Parse(saldo.ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
        }

        public async Task<bool> HayOrdenesActivas(string relacion)
        {
            var body = $"method=ActiveOrders&pair={relacion}&nonce={{0}}";
            dynamic response = await PostPage(priv, body);
            var resultado = (response != null && response["return"] != null);
            return resultado;
            //return false;
        }

        public async Task EjecutarOrden(Orden i)
        {
            string body;
            if (i.EsDeVenta)
            {
                var cantidadVenta = i.Cantidad * i.PrecioUnitario;
                var precioVenta = i.Cantidad / cantidadVenta;
                body = $"method=Trade&pair={i.Relacion}&type=buy&rate={precioVenta.ToString("0.########", CultureInfo.InvariantCulture)}&amount={cantidadVenta.ToString("0.########", CultureInfo.InvariantCulture)}&nonce={{0}}";
            }
            else
            {
                body = $"method=Trade&pair={i.Relacion}&type=sell&rate={i.PrecioUnitario.ToString("0.########", CultureInfo.InvariantCulture)}&amount={i.Cantidad.ToString("0.########", CultureInfo.InvariantCulture)}&nonce={{0}}";
            }
            System.Console.WriteLine(body);
            var respuesta = await PostPage(priv, body);
        }

        private void CargarMonedas(IEnumerable<string> response, IMercadoCargar mercado, List<string> exclude, List<string> include)
        {
            try
            {
                string r = response.First();
                dynamic pares = JsonConvert.DeserializeObject(r);
                foreach (var relacion in pares.pairs)
                {
                    var monedas = relacion.Name.Split('_');
                    if ((exclude == null || (!exclude.Contains(monedas[0]) && !exclude.Contains(monedas[1]) && !exclude.Contains(relacion.Name))) 
                        && (include == null || (include.Contains(monedas[0]) || include.Contains(monedas[1]) || include.Contains(relacion.Name))))
                    {
                        mercado.CargarRelacionEntreMonedas(monedas[0], monedas[1], 0, 0,0);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error al mapear response CargarMonedas");
            }
        }
        
        #region getpost

        private static async Task<dynamic> PostPage(string url, string body)
        {
            try
            {
                var bodyNonce = string.Format(body, GenerateNonce());
                var key = "D618EE268266DF1AA1F4454A4F14E880";
                var secret = "2957796d122670d9f8526e083cfa26fc";
                var headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/x-www-form-urlencoded" },
                    {"Key", key },
                    {"Sign", bodyNonce.HmacShaDigest(secret) }
                };
                var client = new HttpClientApp();
                var json = await client.PostAsync(url, bodyNonce, headers);
                return JsonConvert.DeserializeObject<dynamic>(json);

            }
            catch (Exception e)
            {
                return PostPage(url, body);
            }
        }
        
        public static string GenerateNonce()
        {
            var value = Convert.ToInt32(Settings.Default["Nonce"]);
            value++;
            Settings.Default["Nonce"] = value;
            Settings.Default.Save();
            return value.ToString();
        }
        #endregion
    }
}
