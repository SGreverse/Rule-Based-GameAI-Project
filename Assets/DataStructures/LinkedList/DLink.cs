using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.LinkedList
{
    public class DLink<T>
    {
        public T Value;
        public DLink<T> Next;
        public DLink<T> Prev;

        public DLink(T value)
        {
            Value = value;
            Next = this;
            Prev = this;
        }
    }
}
