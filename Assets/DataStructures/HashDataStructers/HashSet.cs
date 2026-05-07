using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.HashDataStructers
{
    public class GameHashSet<T>:IEnumerable<T>
    {
        private const int CAPACITY = 50;

        private HashTable<T> _table;

        public GameHashSet(int capacity=CAPACITY)
        {
            this._table = new HashTable<T>(capacity);
        }

        public bool Contains(T item)
        {
            return this._table.TryGetValue(item, out T temp);
        }
        public void Add(T item)
        {
            this._table.Put(item);
        }
        public void Remove(T item)
        {
            this._table.Remove(item);
        }
        public void Clear()
        {
            this._table.Clear();
        }
        public int Count()
        {
            return this._table.Count();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
