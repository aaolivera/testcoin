using Dominio.Dto;
using Dominio.Entidades;
using Repositorio;
using Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servicios.Imp
{
    public class Operador : IOperador, IOperadorInput
    {
        private List<IProvider> Providers = new List<IProvider>() { new YobitProvider() };
        public IRepositorio Repositorio { get; }
        public IEstadoOperador Estado { get; }

        public Operador(IRepositorio repositorio, IEstadoOperador estado)
        {
            Estado = estado;
            Repositorio = repositorio;            
        }

        public async void ActualizarOrdenes()
        {
            await Task.Run(() =>
            {
                Estado.UpdateEnProgreso = true;
                Estado.RelacionesActualizadas = 0;
                foreach (var p in Providers)
                {
                    p.CargarMonedas(this);
                    p.CargarEstadosDeOrdenes(this);           
                }
                Estado.GuardandoCambios = true;
                Repositorio.GuardarCambios();
                Estado.GuardandoCambios = false;
                Estado.UpdateEnProgreso = false;
                Estado.UltimoUpdate = DateTime.Now;
            });
        }
        
        public IEnumerable<Relacion> ListarRelacionesReelevantes()
        {
            return Repositorio.Listar<Relacion>(x => x.Volumen > 10000).OrderBy(x => x.DeltaEjecutado);
        }

        public IEnumerable<Relacion> ListarRelaciones()
        {
            return Repositorio.Listar<Relacion>();
        }

        #region iprovider
        public void AgregarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            var retorno = Repositorio.Obtener<Relacion>(x => x.Nombre == monedaNameA + "_" + monedaNameB);
            if (retorno == null)
            {
                var monedaA = ObtenerMoneda(monedaNameA);
                var monedaB = ObtenerMoneda(monedaNameB);
                retorno = new Relacion() { Principal = monedaA, Secundaria = monedaB, Nombre = monedaNameA + "_" + monedaNameB, FechaDeActualizacion = DateTime.Now};
                Repositorio.Agregar(retorno);
                Estado.CantidadDeRelaciones++;
            }
        }

        public void AgregarOrden(string relacionName, decimal precio, decimal cantidad, bool esDeVenta)
        {
            var relacion = Repositorio.Obtener<Relacion>(x => x.Nombre == relacionName);
            var orden = new Orden()
            {
                Cantidad = cantidad,
                EsDeVenta = esDeVenta,
                PrecioUnitario = precio
            };

            relacion?.AgregarOrden(orden);
        }

        public void ActualizarEstadoOrden(string relacionName, decimal mayorPrecioDeVentaAjecutada, decimal volumen, decimal compra, decimal venta)
        {
            var relacion = Repositorio.Obtener<Relacion>(x => x.Nombre == relacionName);
            if (relacion != null)
            {
                relacion.MayorPrecioDeVentaAjecutada = mayorPrecioDeVentaAjecutada;
                relacion.Venta = venta;
                relacion.Compra = compra;
                relacion.Volumen = volumen;
                relacion.CalcularDeltas();
                relacion.FechaDeActualizacion = DateTime.Now;
            }
        }

        public Moneda ObtenerMoneda(string moneda)
        {
            var retorno = Repositorio.Obtener<Moneda>(x => x.Nombre == moneda);
            if (retorno == null)
            {
                retorno = new Moneda() { Nombre = moneda };
                Repositorio.Agregar(retorno);                
            }
            return retorno;
        }

        public void NotificarPaginas(int v)
        {
            Estado.Paginas = v;
        }

        public void NotificarAvance(string url)
        {
            Estado.RelacionesActualizadas += url.Count(f => f == '-');
        }
        #endregion
    }
}
