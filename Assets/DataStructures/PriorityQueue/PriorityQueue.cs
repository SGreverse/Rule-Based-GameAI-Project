using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.PriorityQueue
{
    public class PriorityQueue<T> where T:IComparable<T>
    {
        private Heap<T> _heap;
        public PriorityQueue(List<T> values,HeapType heaptype)
        {
            this._heap=new Heap<T>(values,heaptype);
        }
        public PriorityQueue(HeapType heaptype)
        {
            this._heap = new Heap<T>(heaptype);
        }
        public void Enqueue(T item)
        {
            this._heap.InsertHeap(item);
        }
        public T Peek()
        {
            if (this._heap == null) return default(T);
            return this._heap.Peek();
        }
        public T Dequeue()
        {
            return this._heap.PopFromHeap();
        }
        public bool IsEmpty()
        {
            return this._heap.GetHeapSize() == 0;
        }
        public void Clear()
        {
            this._heap.Clear();
        }
        public int Count()
        {
            return this._heap.GetHeapSize();
        }
        /// <summary>
        /// Can only call this if the item implements IHeapTrackable
        /// </summary>
        public void UpdatePriority(T item)
        {
            if(!(item is IHeapTrackable))
            {
                throw new InvalidOperationException("Cannot use operation for non heap trackable item");
            }
            this._heap.UpdateItem(item);
        }
        /// <summary>
        /// Can only call this if the item implements IHeapTrackable
        /// </summary>
        public void RemoveItem(T item)
        {
            if (!(item is IHeapTrackable))
            {
                throw new InvalidOperationException("Cannot use operation for non heap trackable item");
            }
            this._heap.RemoveItem(item);
        }
    }
}
