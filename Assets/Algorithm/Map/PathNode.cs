using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.PathFinding;
using Assets.Algorithm.PriorityQueue;

namespace Assets.Algorithm.Map
{
    public class PathNode:IComparable<PathNode>,IHeapTrackable
    {
        public MapNode NodeReference { get; set; }
        public PathNode ParentNode { get; set; }

        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;

        public int HeapIndex { get; set; }

        public float ArrivalTime { get; set; }

        public PathNode(MapNode node)
        {
            NodeReference = node;
            ParentNode = null;
            GCost = float.MaxValue; // Default to infinity
            HCost = 0;
        }

        public int CompareTo(PathNode other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                // If FCosts are equal, prefer the one closer to the target
                compare = HCost.CompareTo(other.HCost);
            }
            return compare;
        }
    }
}
