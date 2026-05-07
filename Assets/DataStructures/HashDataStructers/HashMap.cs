using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.HashDataStructers
{
    public struct KeyValuePair<TKey, TValue>: IEquatable<KeyValuePair<TKey, TValue>>
    {
        public TKey Key; 
        public TValue Value;

        public KeyValuePair(TKey key,TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        //to prevent object boxing while comparing we use the EqualityComparer
        public bool Equals(KeyValuePair<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(this.Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            return obj is KeyValuePair<TKey, TValue> pair && Equals(pair);
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

    }
    public class GameHashMap<TKey,TValue>:IEnumerable<KeyValuePair<TKey,TValue>>
    {
        private HashTable<KeyValuePair<TKey,TValue>> _table;

        private const int CAPACITY = 50;

        public GameHashMap(int capacity = CAPACITY)
        {
            this._table = new HashTable<KeyValuePair<TKey, TValue>>(capacity);
        }
        public TValue this[TKey key]
        {
            get
            {
                // by creating a search dummy we can search the table using our key even without knowing the value. 
                var searchDummy = new KeyValuePair<TKey, TValue>(key, default(TValue));

                if (this._table.TryGetValue(searchDummy, out KeyValuePair<TKey, TValue> foundPair))
                {
                    return foundPair.Value;
                }

                // If it doesn't exist, throw an exception
                throw new KeyNotFoundException($"The key '{key}' was not found in the HashMap.");
            }
            set
            {
                this._table.Put(new KeyValuePair<TKey, TValue>(key, value));
            }
        }
        public bool ContainsKey(TKey key)
        {
            KeyValuePair<TKey, TValue> searchDummy = new KeyValuePair<TKey, TValue>(key, default(TValue));
            return this._table.TryGetValue(searchDummy, out KeyValuePair<TKey, TValue> foundPair);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var searchDummy = new KeyValuePair<TKey, TValue>(key, default(TValue));

            if (this._table.TryGetValue(searchDummy, out KeyValuePair<TKey, TValue> foundPair))
            {
                value = foundPair.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public bool Remove(TKey key)
        {
            var removeDummy = new KeyValuePair<TKey, TValue>(key, default(TValue));
            return this._table.Remove(removeDummy);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Clear()
        {
            this._table.Clear();
        }

    }
}
