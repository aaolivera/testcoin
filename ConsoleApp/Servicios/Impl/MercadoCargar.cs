using Dominio.Entidades;
using Dominio.Interfaces;
using Repositorio;
using Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servicios.Impl
{
    public class MercadoCargar : IMercadoCargar, IMercadoActualizar
    {
        private List<IProvider> Providers { get; }
        private List<string> MonedasExcluidas { get; }
        private List<string> MonedasIncluidas { get; }
        public Dictionary<string, Relacion> RelacionesEntreMonedasHash { get; } = new Dictionary<string, Relacion>();

        public List<string> RelacionesEntreMonedas => RelacionesEntreMonedasHash.Keys.ToList();
        public IRepositorio Repositorio;

        public MercadoCargar(List<IProvider> providers, IRepositorio repositorio, List<string> excluidas = null, List<string> incluidas = null)
        {
            Providers = new List<IProvider>(providers);
            MonedasExcluidas = excluidas;
            MonedasIncluidas = incluidas;
            Repositorio = repositorio;
            var relaciones = Repositorio.Listar<Relacion>(x => true);
            RelacionesEntreMonedasHash = relaciones.ToDictionary(x => x.MonedaA + "_" + x.MonedaB, x => x);
        }

        public async Task ActualizarMonedas()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarMonedas(this, MonedasExcluidas, MonedasIncluidas);
            }
        }

        public async Task ActualizarRelaciones()
        {
            foreach (var p in Providers)
            {
                await p.ActualizarRelaciones(this);
            }
        }

        public void CargarPrecio(string monedaNameA, string monedaNameB, decimal volumen, decimal compra, decimal venta)
        {
            CargarRelacionEntreMonedas(monedaNameA, monedaNameB);
            var r = RelacionesEntreMonedasHash[monedaNameA + "_" + monedaNameB];
            Repositorio.Agregar(new PrecioHistorico
            {
                Volumen = volumen,
                Compra = compra,
                Venta = venta,
                Relacion = r,
                Fecha = DateTime.Now
            });
        }
        public void CargarRelacionEntreMonedas(string monedaNameA, string monedaNameB)
        {
            if (!RelacionesEntreMonedasHash.ContainsKey(monedaNameA + "_" + monedaNameB))
            {
                var relacion = Repositorio.Agregar(new Relacion { MonedaA = monedaNameA, MonedaB = monedaNameB });
                RelacionesEntreMonedasHash[monedaNameA + "_" + monedaNameB] = relacion;
            }
        }
    }
}
