using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Helper
{
    public interface IComparableE<T> where T : IComparableE<T>
    {
        int CompareTo(T other, string ejecucion) ;
    }

    //public class Compare : IComparer<Moneda>
    //{
    //    public Compare(string ejecucion)
    //    {
    //        Ejecucion = ejecucion;
    //    }

    //    public string Ejecucion { get; }

    //    //int IComparer<Moneda>.Compare(Moneda x, Moneda y) => x.CompareTo(y, Ejecucion);
    //}
}
