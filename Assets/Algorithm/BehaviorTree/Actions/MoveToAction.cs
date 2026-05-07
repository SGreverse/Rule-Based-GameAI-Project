using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class MoveToAction : Action
    {
        public MoveToAction(EnemyManager enemyController) : base(enemyController)
        {
        }
        public override NodeState Evaluate()
        {
            if (this.CurrentEnemy.Movment.IsMovmentFinished())
            {
                CurrentState = NodeState.Success;
                return NodeState.Success;
            }
            CurrentState = NodeState.Running;
            return NodeState.Running;
        }
        public override void OnExit()
        {
            this.CurrentEnemy.Movment.StopMovment();
            base.OnExit();
        }
    }
}
