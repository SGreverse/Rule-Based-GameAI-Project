using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class HealAction : Action
    {

        public HealAction(EnemyManager currEnemy) : base(currEnemy) { }

        public override void CalculateUtility() { }

        public override bool OnEnter()
        {
            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {

            if (CurrentEnemy.Heal())
            {
                CurrentState = NodeState.Success;
                return NodeState.Success;
            }
            CurrentState = NodeState.Failure;
            return NodeState.Failure;
        }

    }
}
