using Dominio.Entidades;
using Dominio.Interfaces;
using Repositorio;
using Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Servicios.Impl
{
    public class MercadoBuscar : IMercadoBuscar
    {
        private const decimal CANTIDADBTC = 0.0001m;
        private const decimal VOLUMENMINIMO = 0.001m;
        private Dictionary<string, Relacion> RelacionesEntreMonedasHash { get; } = new Dictionary<string, Relacion>();
        public IRepositorio Repositorio;

        public MercadoBuscar(IRepositorio repositorio)
        {
            Repositorio = repositorio;
            var relaciones = Repositorio.Listar<Relacion>(x => true);
            RelacionesEntreMonedasHash = relaciones.ToDictionary(x => x.MonedaA + "_" + x.MonedaB, x => x);
        }

        private decimal Convertir(string origen, string destino, decimal cantidadOrigen, Jugada jugada)
        {
            RelacionesEntreMonedasHash.TryGetValue(origen + "_" + destino, out Relacion relacion);
            if(relacion == null)
            {
                RelacionesEntreMonedasHash.TryGetValue(destino + "_" + origen, out relacion);
            }

            if (relacion == null) return 0;
            var cantidadDestino = 0M;
            if (relacion.MonedaA == origen)
            {
                cantidadDestino = cantidadOrigen * relacion.Precio;
                cantidadDestino -= 0.2M / 100 * cantidadDestino;
                jugada.Movimientos.Add(new Movimiento
                {
                    Origen = origen,
                    Destino = destino,
                    Precio = relacion.Precio,
                    CantidadOrigen = cantidadOrigen,
                    CantidadDestino = cantidadDestino,
                    Relacion = relacion,
                    Compra = false,
                    Jugada = jugada
                });

            }
            if (relacion.MonedaB == origen)
            {
                var cantidadOrigenTemp = cantidadOrigen - 0.2M / 100 * cantidadOrigen;
                cantidadDestino = cantidadOrigenTemp / relacion.Precio;
                jugada.Movimientos.Add(new Movimiento
                {
                    Origen = origen,
                    Destino = destino,
                    Precio = relacion.Precio,
                    CantidadOrigen = cantidadOrigenTemp,
                    CantidadDestino = cantidadDestino,
                    Relacion = relacion,
                    Compra = true,
                    Jugada = jugada
                });
            }
            return cantidadDestino;
        }

        private decimal VolumenEnBtc(string monedaB, decimal volumen)
        {
            {
                return "btc" == monedaB || volumen == 0 ? volumen : Convertir(monedaB, "btc", volumen, new Jugada());
            }
        }

        private Jugada CalcularJugada(Relacion relacion, decimal cantidad)
        {
            var resultado = new Jugada
            {
                MonedaA = relacion.MonedaA,
                MonedaB = relacion.MonedaB,
                Inicial = cantidad
            };
            var cantidadMonedaA = Convertir("btc", relacion.MonedaA, cantidad, resultado);
            var cantidadMonedaB = Convertir(relacion.MonedaA, relacion.MonedaB, cantidadMonedaA, resultado);
            resultado.Final = Convertir(relacion.MonedaB, "btc", cantidadMonedaB, resultado);
            return resultado;
        }

        public List<KeyValuePair<string, Relacion>> ListarRelacionesConVolumen()
        {
            return RelacionesEntreMonedasHash.Where(x => VolumenEnBtc(x.Value.MonedaB, x.Value.Volumen) > VOLUMENMINIMO).ToList();
        }

        public void CalcularJugadas()
        {
            var jugadas = ListarRelacionesConVolumen().Where(x => !x.Value.Contiene("btc")).Select(x => CalcularJugada(x.Value, CANTIDADBTC));
            foreach(var j in jugadas)
            {
                var jdb = Repositorio.Obtener<Jugada>(x => x.MonedaA == j.MonedaA && x.MonedaB == j.MonedaB);
                if(jdb != null)
                {
                    jdb.Actualizar(j);
                    foreach (var m in j.Movimientos)
                    {
                        m.Jugada = jdb;
                        var mdb = Repositorio.Obtener<Movimiento>(x => x.Origen == m.Origen && x.Destino == m.Destino && x.MonedaB == m.MonedaB);
                        if (mdb != null)
                        {
                            mdb.Actualizar(m);
                        }
                    }
                }
                else
                {
                    jdb = Repositorio.Agregar(j);
                }
            }
            Repositorio.GuardarCambios();
        }
    }
}
