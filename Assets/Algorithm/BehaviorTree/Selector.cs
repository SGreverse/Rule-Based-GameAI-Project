using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.Utility;

namespace Assets.Algorithm.MainAlgorithm
{
    public abstract class Selector : Composite
    {
        public Selector(EnemyManager CurrentEnemy) : base(CurrentEnemy) { }
        public override NodeState Evaluate()
        {
            while(this.CurrChildIndex < this.Children.Count)
            {
                Node child=this.Children[this.CurrChildIndex];

                if (!TryEnterNode(child))//entering falis if atleast one of the child's pre Conditions return false
                {
                    CurrChildIndex++;
                }
                else// if we successfully entered the child
                {
                    switch (child.Evaluate())// return status based on the child return status
                    {
                        case NodeState.Failure:
                            child.OnExit();
                            CurrChildIndex++;
                            break;

                        case NodeState.Success:
                            child.OnExit();
                            CurrentState = NodeState.Success;
                            return CurrentState;

                        case NodeState.Running:
                            CurrentState = NodeState.Running;
                            return CurrentState;
                    }
                }
            }
            CurrentState = NodeState.Failure;
            return CurrentState;
        }
    }
}
