using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Map
{
    public class MapNode
    {
        public bool IsWalkable;
        public float WalkWeight;

        public Vector2Int GridPosition;

        public MapNode(bool isWalkable, float walkWeight, Vector2Int gridPosition)
        {
            this.IsWalkable = isWalkable;
            this.WalkWeight = walkWeight;
            this.GridPosition = gridPosition;
        }
    }
}
