using Assets.Algorithm.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Algorithm.PathFinding
{
    public class NormalStrategy : IPathingStrategy
    {
        public float GetExtraCost(PathFinder pathFinder, MapNode neighbor, Vector2 playerPos, Vector2 playerForward, float agentSpeed) => 0f;
        public float CalculateHCost(Vector2Int currentIndex, Vector2Int targetIndex, float agentSpeed)
        => Vector2.Distance(currentIndex, targetIndex) / agentSpeed*1.001f;
    }
}
