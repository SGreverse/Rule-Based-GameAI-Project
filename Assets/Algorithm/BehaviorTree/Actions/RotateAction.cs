using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class RotateAction : Action
    {
        public RotateAction(EnemyManager enemyController) : base(enemyController)
        {
        }

        public override NodeState Evaluate()
        {
            Vector2 position = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
            this.CurrentEnemy.Movment.RotateToFace(position);
            CurrentState = NodeState.Success;
            return NodeState.Success;
        }

    }
}
