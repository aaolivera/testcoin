using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominio.Helper
{
    public class PriorityQueue<T> where T : IComparableE<T>
    {
        public List<T> data;

        public PriorityQueue(List<T> inicial)
        {
            //data2.Push(inicial.First());
            this.data = new List<T>(inicial);
        }

        public bool Any()
        {
            //return data2.Count > 0;
            return data.Count > 0;
        }
        //public Stack<T> data2 = new Stack<T>();
        public void Enqueue(T item)
        {
            //data2.Push(item);
            data.Add(item);
        }

        public T Dequeue(IComparer<T> compare)
        {
            //return data2.Pop();

            data.Sort(compare);
            var retorno = data[0];
            data.RemoveAt(0);
            return retorno;
        }

    } // PriorityQueue
}
