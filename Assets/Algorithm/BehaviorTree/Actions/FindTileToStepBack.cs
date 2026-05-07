using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class FindTileToStepBackAction : Action
    {
        public FindTileToStepBackAction(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
        }

        public override NodeState Evaluate()
        {
            Vector2 backwardDir = -CurrentEnemy.Movment.FacingDirection;
            if (CurrentEnemy.Movment.FindRandomTileInDirection(CurrentEnemy.transform.position, backwardDir, 60f, 60f, 2f, 4f, out Vector2 targetPos,true))
            {
                CurrentEnemy.Movment.SetTarget(targetPos);
            }
            //even if we couldnt find the tile or a path to it,we dont want to consider this a failed sequence,and moveto action will immediatly finish since we have no target
            CurrentState = NodeState.Success;
            return NodeState.Success;
        }
    }
}
