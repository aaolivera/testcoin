using Dominio.Entidades;
using Dominio.Helper;
using Dominio.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Globalization;

namespace Servicios
{
    public class YobitProvider : IProvider
    {
        private readonly string info = @"https://yobit.net/api/3/info";
        private readonly string depth = @"https://yobit.net/api/3/depth/{0}?filter=10&ignore_invalid=1";
        private readonly string priv = @"https://yobit.net/tapi/";

        public void CargarOrdenes(Mercado mercado)
        {
            var relaciones = mercado.ObetenerRelacionesEntreMonedas();
            var page = string.Empty;
            var n = 0;
            foreach(var i in relaciones)
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
        
        public void CargarMonedas(Mercado mercado)
        {
            dynamic response = DownloadPage(info);
            
            foreach (var relacion in response.pairs)
            {
                var monedas = relacion.Name.Split('_');
                mercado.AgregarRelacionEntreMonedas(monedas[0], monedas[1]);
            }
        }

        public decimal EjecutarMovimiento(Moneda actual, Moneda siguiente, decimal inicial)
        {
            var ordenesNecesarias = ObtenerOrdenesNecesarias(actual, siguiente, inicial, out string relacion);

            foreach (var i in ordenesNecesarias)
            {
                EjecutarOrden(i, relacion);
            }
            
            while (HayOrdenesActivas(relacion))
            {
                Thread.Sleep(1500);
            }
            return ConsultarSaldo(siguiente.Nombre);
        }

        private decimal ConsultarSaldo(string moneda)
        {
            var body = $"method=getInfo&nonce={GenerateNonce()}";
            dynamic response = PostPage(priv, body);

            var saldo = response == null || response["return"]["funds"][moneda] == null ? 0 : response["return"]["funds"][moneda].Value;
            return decimal.Parse(saldo.ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
        }

        private bool HayOrdenesActivas(string relacion)
        {
            var body = $"method=ActiveOrders&pair={relacion}&nonce={GenerateNonce()}";
            dynamic response = PostPage(priv, body);
            
            return (response != null && response["return"] != null);
        }

        private void EjecutarOrden(Orden i, string relacion)
        {
            var body = $"method=Trade&pair={relacion}&type={(i.EsDeVenta ? "buy" : "sell")}&rate={i.PrecioUnitario.ToString("0.########", CultureInfo.InvariantCulture)}&amount={i.Cantidad.ToString("0.########", CultureInfo.InvariantCulture)}&nonce={GenerateNonce()}";

            dynamic response = PostPage(priv, body);
        }

        private List<Orden> ObtenerOrdenesNecesarias(Moneda actual, Moneda siguiente, decimal inicial, out string relacion)
        {
            var ordenesActivas = ObtenerOrdenesActivas(actual, siguiente, out relacion);
            var ordenesNecesarias = new List<Orden>();
            var cantidadActual = 0M;

            foreach (var orden in ordenesActivas)
            {
                if (orden.EsDeVenta)
                {
                    var cantidadAComprar = 0M;
                    var cantidadDestino = orden.Cantidad * orden.PrecioUnitario;
                    var inicialSinComision = inicial - 0.02M / 100 * inicial;
                    if (cantidadActual + cantidadDestino < inicialSinComision)
                    {
                        cantidadAComprar = cantidadDestino;
                    }
                    else if (cantidadActual + cantidadDestino > inicialSinComision)
                    {
                        cantidadAComprar = (inicialSinComision - cantidadActual);
                        if (cantidadAComprar == 0) break;
                    }
                    else
                    {
                        break;
                    }
                    orden.Cantidad = cantidadAComprar / orden.PrecioUnitario;
                    cantidadActual += cantidadAComprar;
                }
                else
                {
                    var cantidadAVender = 0M;
                    if (cantidadActual + orden.Cantidad < inicial)
                    {
                        cantidadAVender = orden.Cantidad;
                    }
                    else if (cantidadActual + orden.Cantidad > inicial)
                    {
                        cantidadAVender = (inicial - cantidadActual);
                        if (cantidadAVender == 0) break;
                    }
                    else
                    {
                        break;
                    }
                    orden.Cantidad = cantidadAVender;
                    cantidadActual += cantidadAVender;
                }
                ordenesNecesarias.Add(orden);
            }
            return ordenesNecesarias;
        }
        
        private List<Orden> ObtenerOrdenesActivas(Moneda actual, Moneda siguiente, out string relacion)
        {
            var url = string.Format(depth, $"{actual.Nombre}_{siguiente.Nombre}-{siguiente.Nombre}_{actual.Nombre}");
            relacion = string.Empty;
            var response = DownloadPage(url);
            var resultado = new List<Orden>();
            foreach (var ordenesPorMoneda in response)
            {
                var monedas = ordenesPorMoneda.Name.Split('_');
                relacion = ordenesPorMoneda.Name;
                var ventas = ordenesPorMoneda.Value["asks"];
                var compras = ordenesPorMoneda.Value["bids"];

                if (ventas != null && monedas[1] == actual.Nombre)
                {
                    foreach (var ordenVenta in ventas)
                    {
                        resultado.Add(new Orden
                        {
                            MonedaQueQuieroVender = actual,
                            MonedaQueQuieroComprar = siguiente,
                            PrecioUnitario = (decimal)ordenVenta[0].Value, // de la moneda a vender
                            Cantidad = (decimal)ordenVenta[1].Value,
                            EsDeVenta = true
                        });
                    }
                }

                if (compras != null && monedas[0] == actual.Nombre)
                {
                    foreach (var ordenCompra in compras)
                    {
                        resultado.Add(new Orden
                        {
                            MonedaQueQuieroVender = actual,
                            MonedaQueQuieroComprar = siguiente,
                            PrecioUnitario = (decimal)ordenCompra[0].Value, // de la moneda a comprar
                            Cantidad = (decimal)ordenCompra[1].Value,
                            EsDeVenta = false
                        });
                    }
                }
            }
            return resultado;
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

                if (ventas != null)
                {
                    foreach (var ordenVenta in ventas)
                    {
                        mercado.AgregarOrdenDeVenta(monedas[0], monedas[1], (decimal)ordenVenta[0].Value, (decimal)ordenVenta[1].Value);
                    }
                }

                if (compras != null)
                {
                    foreach (var ordenCompra in compras)
                    {
                        mercado.AgregarOrdenDeCompra(monedas[0], monedas[1], (decimal)ordenCompra[0].Value, (decimal)ordenCompra[1].Value);
                    }
                }
            }
        }

        #region getpost

        private static dynamic PostPage(string url, string body)
        {
            try
            {
                Thread.Sleep(1500);

                var headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/x-www-form-urlencoded" },
                    {"Key", "CCCFCF17F223DC5862A4B6135186EFAD" },
                    {"Sign", body.HmacShaDigest("ad242c9ceb5ad5debdf420c267f828c8") }
                };

                var cliente = new WebClient();
                foreach (var i in headers.Keys)
                {
                    cliente.Headers.Add(i, headers[i]);
                }
                var response = cliente.UploadString(url, body);
                dynamic dinamic = JsonConvert.DeserializeObject(response);
                if (dinamic["success"] != 1)
                {
                    throw new Exception(dinamic["error"]);
                }
                return dinamic;
            }
            catch (Exception e)
            {
                return PostPage(url, body);
            }
        }

        private static dynamic DownloadPage(string url, Dictionary<string, string> header = null, string body = null)
        {
            try
            {
                Thread.Sleep(1400);
                var cliente = new WebClient();
                var response = cliente.DownloadString(url);
                return JsonConvert.DeserializeObject(response);
            }
            catch (Exception e)
            {
                return DownloadPage(url);
            }
        }

        public string GenerateNonce()
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
