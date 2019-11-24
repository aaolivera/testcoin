using Dominio.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dominio.Entidades
{
    public class Moneda
    {
        public string Nombre { get; private set; }
        public Dictionary<Moneda, Relacion> Relaciones { get; set; } = new Dictionary<Moneda, Relacion>();
        public Moneda(string nombre)
        {
            Nombre = nombre;
        }

        public void AgregarRelacion(Relacion relacion)
        {
            Relaciones[relacion.MonedaA != this ? relacion.MonedaA : relacion.MonedaB] = relacion;
        }

    }
}
