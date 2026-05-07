using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.LinkedList;

namespace Assets.Algorithm.Deque
{
    public class GameEventDeque<T>
    {

        private DLink<T> _head;

        public int Count { get; private set; }

        public GameEventDeque()
        {
            _head = null;
            Count = 0;
        }

        /// <summary>
        /// Adds a new element to the back of the queue (Newest Event).
        /// O(1) Time Complexity.
        /// </summary>
        public void AddLast(T value)
        {
            DLink<T> newNode = new DLink<T>(value);

            if (_head == null)
            {
                // If empty, the node points to itself in both directions
                _head = newNode;
            }
            else
            {
                DLink<T> tail = _head.Prev;

                // Insert the new node between the old tail and the head
                tail.Next = newNode;
                newNode.Prev = tail;

                //  connect the new node back to the head
                newNode.Next = _head;
                _head.Prev = newNode;
            }
            Count++;
        }

        /// <summary>
        /// Removes the oldest element from the front of the queue.
        /// O(1) Time Complexity.
        /// </summary>
        public void RemoveFirst()
        {
            if (_head == null) return; 

            if (Count == 1)
            {
                // If there's only one item, clear the head and break the circle
                _head.Next = null;
                _head.Prev = null;
                _head = null;
            }
            else
            {
                // Find the newest item 
                DLink<T> tail = _head.Prev;
                DLink<T> oldHead = _head;

                // Move the Head pointer to the next oldest event
                _head = _head.Next;

                // Re-link the circle
                tail.Next = _head;
                _head.Prev = tail;

                // Sever the old head's connections entirely so the Garbage Collector can delete it safely
                oldHead.Next = null;
                oldHead.Prev = null;
            }
            Count--;
        }

        /// <summary>
        /// Returns the oldest Event . O(1)
        /// </summary>
        public T PeekFirst()
        {
            if (_head != null) return _head.Value;
            return default;
        }

        /// <summary>
        /// Returns the newest Event  O(1)
        /// </summary>
        public T PeekLast()
        {
            if (_head != null) return _head.Prev.Value;
            return default;
        }

        /// <summary>
        /// Clears the structure
        /// </summary>
        public void Clear()
        {
            // Break the circle for the remaining nodes to help the Garbage Collector
            if (_head != null)
            {
                _head.Prev = null;
                _head = null;
            }
            Count = 0;
        }

    }
}

