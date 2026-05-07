using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.LinkedList;
using UnityEngine.InputSystem;

namespace Assets.Algorithm.HashDataStructers
{
    public class HashTable<T>:IEnumerable<T>
    {
        private LinkedNodeList<T>[] _buckets;
        private int _capacity;
        private int _threshold;
        private int _size;
        public HashTable(int capacity = 50)
        {
            this._capacity= capacity;
            this._threshold = (int)(capacity * 0.75f);
            this._size= 0;
            this._buckets = new LinkedNodeList<T>[_capacity];
            InitializeBuckets();
        }
        private void InitializeBuckets()
        {
            for (int i = 0; i < this._capacity; i++)
            {
                this._buckets[i] = new LinkedNodeList<T>();
            }
        }
        private int GetBucketIndex(T key)
        {
            // EqualityComparer handles nulls safely and avoids boxing for structs
            int hash = EqualityComparer<T>.Default.GetHashCode(key);
            // if the hash is negative, we ignore the sign bit
            return (hash & 0x7FFFFFFF) % _capacity;
        }

        public void Put(T key)
        {
            int index = GetBucketIndex(key);
            LinkedNodeList<T> bucket = this._buckets[index];

            ListNode<T> current = bucket.GetHead();
            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.Value, key))
                {
                    current.Value = key;
                    return;
                }
                current = current.Next;
            }
            bucket.AddFirst(key);
            this._size++;

            if (this._size >= this._threshold) Resize();

        }

        public bool TryGetValue(T key, out T value)
        {
            int index = GetBucketIndex(key);
            LinkedNodeList<T> bucket = this._buckets[index];

            ListNode<T> current = bucket.GetHead();
            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.Value, key))
                {
                    value = current.Value;
                    return true;
                }
                current = current.Next;
            }

            value = default(T);
            return false;
        }

        public bool Remove(T key)
        {
            int index = GetBucketIndex(key);
            LinkedNodeList<T> bucket = this._buckets[index];

            ListNode<T> current = bucket.GetHead();
            ListNode<T> prev=null;
            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.Value, key))
                {

                    if(prev== null)
                    {
                        bucket.DeleteFirst();
                    }
                    else
                    {
                        prev.Next = current.Next;
                        bucket.SubCount(1);
                    }
                    this._size--;
                    return true;
                }
                prev=current;
                current = current.Next;
            }
            return false;
        }
        public int Count()
        {
            return this._size;
        }
        private void Resize()
        {
            this._capacity = this._capacity * 2;
            this._threshold = (int)(this._capacity * 0.75f);
            LinkedNodeList<T>[] temp = this._buckets;
            this._buckets = new LinkedNodeList<T>[this._capacity];
            InitializeBuckets();
            for(int i = 0;i< temp.Length; i++)//re-hash all values and put in the new table
            {
                ListNode<T> current_bucket = temp[i].GetHead();
                while (current_bucket != null)
                {
                    int newIndex = GetBucketIndex(current_bucket.Value);
                    this._buckets[newIndex].AddFirst(current_bucket.Value);

                    current_bucket = current_bucket.Next;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this._capacity; i++)
            {
                ListNode<T> current = this._buckets[i].GetHead();
                while (current != null)
                {
                    yield return current.Value;
                    current = current.Next;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            for (int i = 0; i < this._capacity; i++)
            {
                this._buckets[i] = new LinkedNodeList<T>();
            }
            this._size = 0;
        }
    }
}
