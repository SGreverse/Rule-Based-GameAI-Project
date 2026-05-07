using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.MainAlgorithm.Actions;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;

namespace Assets.Algorithm.BehaviorTree.Sequences
{
    public class PatrolSequence : Sequence
    {
        public PatrolSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new FindNextPatrolSpotAction(CurrentEnemy));
            this.Children.Add(new MoveToAction(CurrentEnemy));
            this.Children.Add(new WaitAction(CurrentEnemy,1));

            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected)==false;
            });
        }
        public override NodeState Evaluate()
        {
            if (GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected))
            {
                return NodeState.Failure;
            }
            return base.Evaluate();
        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Patroling); // Change icon to match the specific sequence
            }

            return canEnter;
        }
    }
}
