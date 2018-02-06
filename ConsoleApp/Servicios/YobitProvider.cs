using Dominio.Entidades;
using Dominio.Helper;
using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Providers
{
    public class YobitProvider : IProvider
    {
        private readonly string info = @"https://yobit.net/api/3/info";
        private readonly string depth = @"https://yobit.net/api/3/depth/{0}?ignore_invalid=1";
        private readonly string priv = @"https://yobit.net/tapi/";
        
        public void CargarOrdenes(Mercado mercado)
        {
            var relaciones = mercado.ObetenerRelacionesEntreMonedas();
            var page = string.Empty;
            
            //Armo Urls de paginas
            var paginas = new List<string>();
            foreach(var i in relaciones)
            {
                if (page.Length + i.Length > 510)
                {
                    paginas.Add(string.Format(depth, page));
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
                paginas.Add(string.Format(depth, page));
            }
            
            //Obtengo y cargo       
            var responses = WebProvider.DownloadPages(paginas).Result;
            foreach(var response in responses)
            {
                CargarPaginaDeOrdenes(response, mercado);
            }
        }
        
        public void CargarMonedas(Mercado mercado)
        {
            dynamic response = WebProvider.DownloadPage(info);


            foreach (var relacion in response.pairs)
            {
                var monedas = relacion.Name.Split('_');
                mercado.AgregarRelacionEntreMonedas(monedas[0], monedas[1]);
            }
        }

        public decimal ConsultarSaldo(string moneda)
        {
            var body = "method=getInfo&nonce={0}";
            dynamic response = PostPage(priv, body);
            var saldo = response == null || response["return"]["funds"][moneda] == null ? 0 : response["return"]["funds"][moneda].Value;
            return decimal.Parse(saldo.ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
        }

        public bool HayOrdenesActivas(string relacion)
        {
            var body = $"method=ActiveOrders&pair={relacion}&nonce={{0}}";
            dynamic response = PostPage(priv, body);
            var resultado = (response != null && response["return"] != null);
            return resultado;
        }

        public decimal EjecutarOrden(Orden i, string relacion)
        //private void EjecutarOrden(Orden i, string relacion)
        {
            var body = $"method=Trade&pair={relacion}&type={(i.EsDeVenta ? "buy" : "sell")}&rate={i.PrecioUnitario.ToString("0.########", CultureInfo.InvariantCulture)}&amount={i.Cantidad.ToString("0.########", CultureInfo.InvariantCulture)}&nonce={{0}}";
            System.Console.WriteLine(body);
            //PostPage(priv, body);
            return i.EsDeVenta ? i.Cantidad : ((i.Cantidad * i.PrecioUnitario) - (0.02M / 100 * (i.Cantidad * i.PrecioUnitario)));
            
        }

        public List<Orden> ObtenerOrdenesNecesarias(Moneda actual, Moneda siguiente, decimal inicial, out string relacion)
        {
            var ordenesActivas = ObtenerOrdenesActivas(actual, siguiente, out relacion);
            var ordenesNecesarias = new List<Orden>();
            
            var cantidadActual = 0M;

            foreach (var orden in ordenesActivas)
            {
                if (orden.EsDeVenta)
                {
                    var cantidadActualQuePuedoGastar = inicial - 0.02M / 100 * inicial;
                    var cantidadActualAgastarEnEstaOrden = 0M;
                    var cantidadActualDeLaOrden = orden.Cantidad * orden.PrecioUnitario;
                    
                    if (cantidadActual + cantidadActualDeLaOrden < cantidadActualQuePuedoGastar)
                    {
                        cantidadActualAgastarEnEstaOrden = cantidadActualDeLaOrden;
                    }
                    else if (cantidadActual + cantidadActualDeLaOrden > cantidadActualQuePuedoGastar)
                    {
                        //OJO, aca puedo estar intentando emitir una orden muy chica
                        cantidadActualAgastarEnEstaOrden = (cantidadActualQuePuedoGastar - cantidadActual);
                        if (cantidadActualAgastarEnEstaOrden == 0) break;
                    }
                    else
                    {
                        break;
                    }
                    orden.Cantidad = cantidadActualAgastarEnEstaOrden / orden.PrecioUnitario;
                    cantidadActual += cantidadActualAgastarEnEstaOrden;
                }
                else
                {
                    var cantidadActualAVender = 0M;
                    if (cantidadActual + orden.Cantidad < inicial)
                    {
                        cantidadActualAVender = orden.Cantidad;
                    }
                    else if (cantidadActual + orden.Cantidad > inicial)
                    {
                        cantidadActualAVender = (inicial - cantidadActual);
                        if (cantidadActualAVender == 0) break;
                    }
                    else
                    {
                        break;
                    }
                    orden.Cantidad = cantidadActualAVender;
                    cantidadActual += cantidadActualAVender;
                }
                ordenesNecesarias.Add(orden);
            }
            return ordenesNecesarias;
        }
        
        private List<Orden> ObtenerOrdenesActivas(Moneda actual, Moneda siguiente, out string relacion)
        {
            var url = string.Format(depth, $"{actual.Nombre}_{siguiente.Nombre}-{siguiente.Nombre}_{actual.Nombre}");
            relacion = string.Empty;
            var response = WebProvider.DownloadPage(url);
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
                body = string.Format(body, GenerateNonce());
                var key = "3BE490E44CD07B641608DD5574DB5B00";
                var secret = "ff02c6aa3163f430ba7de4e533d16564";
                var headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/x-www-form-urlencoded" },
                    {"Key", key },
                    {"Sign", body.HmacShaDigest(secret) }
                };
                return WebProvider.PostPage(url, body, headers);
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
