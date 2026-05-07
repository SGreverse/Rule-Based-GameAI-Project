using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class FindSafePositionAction : Action
    {
        public FindSafePositionAction(EnemyManager enemyController) : base(enemyController)
        {
        }

        public override bool OnEnter()
        {
            this.CurrentEnemy.Movment.StopMovment();
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            CurrentState=  CurrentEnemy.Movment.FindSafeSpot()?NodeState.Success:NodeState.Failure;
            return CurrentState;
        }
    }
}
