using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.Map;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class FindNextPatrolSpotAction : Action
    {
        private float _patrolRadius;
        public FindNextPatrolSpotAction(EnemyManager currEnemy) : base(currEnemy)
        {
            this._patrolRadius = currEnemy.Stats.viewRadius;
        }

        public override NodeState Evaluate()
        {
            GameMap map=GameManager.Instance.Map;

            Vector2 currentPos = CurrentEnemy.transform.position;

            //if the enemy is fully surrounded by walls, he cant walk anywhere
            if (map.GetNeighbors(map.GetGlobalIndexFromWorldPosition(currentPos)).Count == 0)
            {
                CurrentState = NodeState.Failure;
                return NodeState.Failure;
            }

            //if we managed to find a tile to walk to
            if (CurrentEnemy.Movment.FindRandomTileInDirection(CurrentEnemy.transform.position, Vector2.up, 180f, 180f, CurrentEnemy.Stats.viewRadius - 4, CurrentEnemy.Stats.viewRadius, out Vector2 patrolPos))
            {
                //if we found a path to that tile
                if (CurrentEnemy.Movment.SetTarget(patrolPos))
                {
                    CurrentState = NodeState.Success;
                    return NodeState.Success;
                }
            }
            CurrentState = NodeState.Failure;
            return NodeState.Failure;
        }
    }
}
