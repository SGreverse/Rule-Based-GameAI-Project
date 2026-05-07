using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;
namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class SetMeleeTargetAction : Action
    {
        public SetMeleeTargetAction(EnemyManager enemyController) : base(enemyController)
        {
        }


        public override bool OnEnter()
        {
            this.CurrentEnemy.Movment.StopMovment();
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            Vector2 playerPosition = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;

            bool pathFound = CurrentEnemy.Movment.SetTarget(playerPosition);

            if (pathFound)
            {
                CurrentState = NodeState.Success;
                return NodeState.Success;
            }
            CurrentState = NodeState.Failure;
            return NodeState.Failure;
        }
    }
}
