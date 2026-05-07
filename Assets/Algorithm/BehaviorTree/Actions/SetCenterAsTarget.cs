using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class SetArenaCenterTargetAction : Action
    {
        private Vector2 _arenaCenter;

        public SetArenaCenterTargetAction(EnemyManager enemyController, Vector2 arenaCenter) : base(enemyController)
        {
            this._arenaCenter = arenaCenter;
        }

        public override bool OnEnter()
        {
            this.CurrentEnemy.Movment.StopMovment();
            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {
            // Feed the center coordinate to the pathfinder
            bool pathFound = CurrentEnemy.Movment.SetTarget(_arenaCenter);

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