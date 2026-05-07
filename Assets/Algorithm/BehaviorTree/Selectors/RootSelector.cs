using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BehaviorTree.Sequences;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Actions;

namespace Assets.Algorithm.BehaviorTree.Selectors
{
    public class RootSelector : Selector
    {
        public RootSelector(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            Children.Add(new MainSelector(CurrentEnemy));
            Children.Add(new PatrolSequence(CurrentEnemy));
            //Children.Add(new WaitAction(CurrentEnemy,1f));
        }
    }
}
