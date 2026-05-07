using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BehaviorTree.Sequences;

namespace Assets.Algorithm.BehaviorTree.Boss
{
    public class BossAttackSelector : AttackSelector
    {
        public BossAttackSelector(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new GroundSlamSequence(CurrentEnemy));
            this.Children.Add(new MeteorStrikeSequence(CurrentEnemy));
        }
    }
}
