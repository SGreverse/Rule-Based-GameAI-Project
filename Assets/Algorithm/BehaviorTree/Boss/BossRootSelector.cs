using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BehaviorTree.Sequences;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Sequences;

namespace Assets.Algorithm.BehaviorTree.Boss
{
    public class BossRootSelector : UtilitySelector
    {
        public BossRootSelector(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new HealSequence(CurrentEnemy));
            this.Children.Add(new DefendSequence(CurrentEnemy));
            this.Children.Add(new FleeSequence(CurrentEnemy));
            this.Children.Add(new BossAttackSelector(CurrentEnemy));
            this.Children.Add(new ReloadSequence(CurrentEnemy));
        }

    }
}
