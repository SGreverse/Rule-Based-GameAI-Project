using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.PriorityQueue
{
    public enum HeapType
    {
        Min,
        Max
    }
    public class Heap<T> where T:IComparable<T>
    {
        private List<T> _heapArray;
        private HeapType _type;

        public Heap(HeapType type)
        {
            this._type = type;
            this._heapArray = new List<T>();
        }
        public Heap(List<T> arr,HeapType type)
        {
            this._type = type;
            BuildHeap(arr);
        }

        public void BuildHeap(List<T> arr)//O(n)
        {
            this._heapArray= new List<T>(arr);
            for(int i = arr.Count / 2; i >= 0; i--)
            {
                Heapify(i);
            }
        }
        public void Heapify(int index)//O(logn)
        {
            int n = this._heapArray.Count;
            int L = 2 * index + 1;
            int R = 2 * index + 2;
            int nextIndex=index;
            if(this._type == HeapType.Max)
            {
                if (L < n && this._heapArray[L].CompareTo(this._heapArray[nextIndex])>0)
                    nextIndex = L;

                if(R<n && this._heapArray[R].CompareTo(this._heapArray[nextIndex]) > 0)
                    nextIndex = R;
            }
            else
            {
                if (L < n && this._heapArray[L].CompareTo(this._heapArray[nextIndex]) < 0)
                    nextIndex = L;

                if (R < n && this._heapArray[R].CompareTo(this._heapArray[nextIndex]) < 0)
                    nextIndex = R;

            }
            if (nextIndex != index)
            {
                Swap(index, nextIndex);
                Heapify(nextIndex);
            }
        }
        public void BubbleUp(int index)
        {
            int parent = (index - 1) / 2;
            if (this._type == HeapType.Max)
            {
                while (index != 0 && this._heapArray[index].CompareTo(this._heapArray[parent]) > 0)
                {
                    Swap(index, parent);
                    index = parent;
                    parent = (index - 1) / 2;

                }
            }
            else
            {
                while (index != 0 && this._heapArray[index].CompareTo(this._heapArray[parent]) < 0)
                {
                    Swap(index, parent);
                    index = parent;
                    parent = (index - 1) / 2;
                }
            }
        }
        public T Peek()
        {
            if (this._heapArray==null || this._heapArray.Count == 0)
            {
                return default(T);
            }
            return this._heapArray[0];
        }

        public void InsertHeap(T value)
        {
            this._heapArray.Add(value);
            int index = this._heapArray.Count - 1;

            if (value is IHeapTrackable trackable)//set the initial index
            {
                trackable.HeapIndex = index;
            }
            BubbleUp(index);

        }
        public T PopFromHeap()
        {
            if (this._heapArray.Count == 0)
                return default(T);

            T x = this._heapArray[0];
            Swap(0,this._heapArray.Count-1);
            this._heapArray.RemoveAt(_heapArray.Count - 1);

            if (this._heapArray.Count > 0)
            {
                Heapify(0);
            }
            return x;
        }
        private void Swap(int i1,int i2)
        {
            T temp=this._heapArray[i1];
            this._heapArray[i1]=this._heapArray[i2];
            this._heapArray[i2] = temp;

            if (this._heapArray[i1] is IHeapTrackable trackable1)
            {
                trackable1.HeapIndex = i1;
            }

            if (this._heapArray[i2] is IHeapTrackable trackable2)
            {
                trackable2.HeapIndex = i2;
            }

        }
        public int GetHeapSize()
        {
            return this._heapArray.Count;
        }
        public void Clear()
        {
            this._heapArray.Clear();
        }
        public void UpdateItem(T item)
        {
            if (item is IHeapTrackable trackableItem)
            {
                int index = trackableItem.HeapIndex;
                int parent = (index - 1) / 2;

                bool flag = false;
                if (index > 0)
                {
                    if (this._type == HeapType.Max && this._heapArray[index].CompareTo(this._heapArray[parent]) > 0)
                    {
                        BubbleUp(index);
                        flag = true;
                    }
                    else if (this._type == HeapType.Min && this._heapArray[index].CompareTo(this._heapArray[parent]) < 0)
                    {
                        BubbleUp(index);
                        flag = true;
                    }
                }
                if (!flag)// if we couldnt buble up, we might need to bubble down(heapify)
                {
                    Heapify(index);
                }
            }
        }
        public void RemoveItem(T item)
        {
            if(item is IHeapTrackable trackableItem)
            {
                int index = trackableItem.HeapIndex;
                Swap(index, this._heapArray.Count - 1);
                this._heapArray.RemoveAt(_heapArray.Count - 1);

                if (this._heapArray.Count > 0)
                {
                    Heapify(index);
                }
            }
        }
    }
}
