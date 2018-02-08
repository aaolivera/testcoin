﻿using Dominio.Entidades;
using Dominio.Helper;
using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Providers
{
    public class YobitProvider : IProvider
    {
        private readonly string info = @"https://yobit.net/api/3/info";
        private readonly string depth = @"https://yobit.net/api/3/depth/{0}?limit=40&ignore_invalid=1"; // ordenes acttivas
        private readonly string relacionInfo = @"https://yobit.net/api/3/ticker/{0}?limit=40&ignore_invalid=1";
        private readonly string priv = @"https://yobit.net/tapi/";
        
        public void CargarOrdenes(Mercado mercado)
        {
            var relaciones = mercado.ObetenerRelacionesEntreMonedas().Take(500);
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
        {
            var body = $"method=Trade&pair={relacion}&type={(i.EsDeVenta ? "buy" : "sell")}&rate={i.PrecioUnitario.ToString("0.########", CultureInfo.InvariantCulture)}&amount={i.Cantidad.ToString("0.########", CultureInfo.InvariantCulture)}&nonce={{0}}";
            System.Console.WriteLine(body);
            PostPage(priv, body);
            return i.EsDeVenta ? i.Cantidad : Decimal.Round((i.Cantidad * i.PrecioUnitario) - (0.2M / 100 * (i.Cantidad * i.PrecioUnitario)));
            
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
                    var cantidadActualQuePuedoGastar = Decimal.Round((inicial - 0.2M / 100 * inicial), 8, MidpointRounding.ToEven);
                    var cantidadActualAgastarEnEstaOrden = 0M;
                    var cantidadActualDeLaOrden = Decimal.Round(orden.Cantidad * orden.PrecioUnitario, 8, MidpointRounding.ToEven);
                    
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
                    orden.Cantidad = Decimal.Round(cantidadActualAgastarEnEstaOrden / orden.PrecioUnitario, 8, MidpointRounding.ToEven);
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
                relacion = ordenesPorMoneda.Name;
                var ventas = ordenesPorMoneda.Value["asks"];
                var compras = ordenesPorMoneda.Value["bids"];

                if (ventas != null)
                {
                    foreach (var ordenVenta in ventas)
                    {
                        resultado.Add(new Orden
                        {
                            PrecioUnitario = (decimal)ordenVenta[0].Value, // de la moneda a vender
                            Cantidad = (decimal)ordenVenta[1].Value,
                            EsDeVenta = true
                        });
                    }
                }

                if (compras != null)
                {
                    foreach (var ordenCompra in compras)
                    {
                        resultado.Add(new Orden
                        {
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
                if (ordenesPorMoneda.Name == "edr2_ltc") continue;
                var ventas = ordenesPorMoneda.Value["asks"];
                var compras = ordenesPorMoneda.Value["bids"];

                if (ventas != null)
                {
                    foreach (var ordenVenta in ventas)
                    {
                        mercado.AgregarOrden(ordenesPorMoneda.Name, (decimal)ordenVenta[0].Value, (decimal)ordenVenta[1].Value, true);
                    }
                }

                if (compras != null)
                {
                    foreach (var ordenCompra in compras)
                    {
                        mercado.AgregarOrden(ordenesPorMoneda.Name, (decimal)ordenCompra[0].Value, (decimal)ordenCompra[1].Value, false);
                    }
                }
            }
        }

        #region getpost

        private static dynamic PostPage(string url, string body)
        {
            try
            {
                var bodyNonce = string.Format(body, GenerateNonce());
                var key = "D9840F8C5CBA7A19BD2E7EFD79140F0F";
                var secret = "f796a8118682696bb0efe51cb5bb802e";
                var headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/x-www-form-urlencoded" },
                    {"Key", key },
                    {"Sign", bodyNonce.HmacShaDigest(secret) }
                };
                return WebProvider.PostPage(url, bodyNonce, headers);
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
