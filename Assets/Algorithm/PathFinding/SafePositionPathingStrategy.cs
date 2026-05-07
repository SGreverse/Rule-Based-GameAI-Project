using Assets.Algorithm.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Algorithm.PathFinding
{
    public class SafePositionPathingStrategy : IPathingStrategy
    {
        private List<Vector2> _allyPositions;
        private LayerMask _obstacleLayer;
        private float _searchRadius;

        public SafePositionPathingStrategy(List<Vector2> allyPositions, LayerMask obstacleLayer, float searchRadius)
        {
            _allyPositions = allyPositions;
            _obstacleLayer = obstacleLayer;
            _searchRadius = searchRadius;
        }

        public float GetExtraCost(PathFinder context, MapNode neighbor, Vector2 playerPos, Vector2 playerForward,float agentSpeed)
        {
            // For this strategy, playerForward isn't used, we use the playerPos and our constructor variables
            Vector2 neighborWorldPos = context.Map.GetWorldPositionFromGlobalIndex(neighbor.GridPosition); // Requires _map to be internal/public, or pass it via context
            return context.CalculateDangerScore(neighborWorldPos, playerPos, _allyPositions, _obstacleLayer, _searchRadius, agentSpeed);
        }
        //since the safeposition function doesnt require Hcost, we just return 0 so the Hcost would not matter
        public float CalculateHCost(Vector2Int currentIndex, Vector2Int targetIndex, float agentSpeed) => 0f;

    }
}
