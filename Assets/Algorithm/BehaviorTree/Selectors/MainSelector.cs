using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BehaviorTree.Sequences;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Sequences;

namespace Assets.Algorithm.BehaviorTree.Selectors
{
    public class MainSelector : UtilitySelector
    {
        public MainSelector(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new HealSequence(CurrentEnemy));
            this.Children.Add(new DefendSequence(CurrentEnemy));
            this.Children.Add(new FleeSequence(CurrentEnemy));
            this.Children.Add(new AttackSelector(CurrentEnemy));
            this.Children.Add(new ReloadSequence(CurrentEnemy));
        }
        public override bool OnEnter()
        {
            if(GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected) != true)
            {
                return false;
            }
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            if (GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected) != true)
                return NodeState.Failure;
            return base.Evaluate();
        }
    }
}
