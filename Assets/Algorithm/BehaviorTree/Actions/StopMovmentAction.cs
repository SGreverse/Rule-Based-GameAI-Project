using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class StopMovmentAction : Action
    {
        public StopMovmentAction(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
        }

        public override NodeState Evaluate()
        {
            this.CurrentEnemy.Movment.StopMovment();

            this.CurrentState = NodeState.Success;
            return NodeState.Success;
        }
    }
}
