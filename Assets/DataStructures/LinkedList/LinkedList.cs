using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.LinkedList
{
    public class LinkedNodeList<T>
    {
        private ListNode<T> _list;
        private int _size;
        public LinkedNodeList()
        {
            this._list = null;
            this._size = 0;
        }

        public ListNode<T> GetHead()
        {
            return this._list;
        }
        public void AddFirst(T item)
        {
            ListNode<T> new_node = new ListNode<T>(item);
            new_node.Next = this._list;
            this._list= new_node;
            this._size++;
        }

        public void AddLast(T item)
        {
            ListNode<T> new_node = new ListNode<T>(item);
            ListNode<T> temp = this._list;
            if (temp == null)
            {
                AddFirst(item);
                return;
            }
            while(temp.Next!=null)
            {
                temp = temp.Next;
            }
            temp.Next = new_node;
            this._size++;
        }
        public void DeleteFirst()
        {
            if (this._size > 0)
            {
                this._list = this._list.Next;
                this._size--;
            }
        }

        /// <summary>
        /// must call this function after manually removing an item form the linked list
        /// </summary>
        /// <param name="i"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void SubCount(int i)
        {
            if (this._size< 0)
            {
                throw new IndexOutOfRangeException("Cannot remove more items");
            }
            this._size -= i;
        }
        public int Count()
        {
            return this._size;
        }

    }
}
