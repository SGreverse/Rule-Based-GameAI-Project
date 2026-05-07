using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.LinkedList
{
    public class ListNode<T>
    {
        public T Value;
        public ListNode<T> Next;

        public ListNode(T value, ListNode<T> next=null)
        {
            Value = value;
            Next = next;
        }
    }
}
