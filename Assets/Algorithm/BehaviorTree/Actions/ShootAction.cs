using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class ShootAction : Action
    {
        public ShootAction(EnemyManager enemyController) : base(enemyController)
        { }

        public override NodeState Evaluate()
        {
            this.CurrentEnemy.Combat.RangedAttack();
            this.CurrentState = NodeState.Success;
            return CurrentState;
        }

    }
}
