using Assets.Algorithm.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Algorithm.PathFinding
{
    public interface IPathingStrategy
    {
        float GetExtraCost(PathFinder pathFinder,MapNode neighbor, Vector2 playerPos, Vector2 playerForward,float agentSpeed);

        float CalculateHCost(Vector2Int currentIndex, Vector2Int targetIndex, float agentSpeed);
    }
}
