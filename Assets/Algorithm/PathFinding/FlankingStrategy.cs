using Assets.Algorithm.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Algorithm.PathFinding
{
    public class FlankingStrategy : IPathingStrategy
    {
        public float GetExtraCost(PathFinder pathFinder, MapNode neighbor, Vector2 playerPos, Vector2 playerForward,float agentSpeed)
        {
            return pathFinder.CalculateFlankGCost(neighbor, playerPos, playerForward);
        }
        public float CalculateHCost(Vector2Int currentIndex, Vector2Int targetIndex, float agentSpeed)
        => Vector2.Distance(currentIndex, targetIndex) / agentSpeed* 1.001f;
    }
}
