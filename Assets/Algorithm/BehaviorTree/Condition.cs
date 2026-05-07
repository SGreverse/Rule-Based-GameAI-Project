using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.MainAlgorithm
{
    public delegate bool ConditionFunc(EnemyManager CurrentEnemy);
    public class Condition : Node
    {
        private ConditionFunc c_Func;
        public Condition(EnemyManager CurrentEnemy, ConditionFunc func) : base(CurrentEnemy)
        {
            c_Func = func;
        }
        public override NodeState Evaluate()
        {
            CurrentState= c_Func.Invoke(this.CurrentEnemy) ? NodeState.Success : NodeState.Failure;// runs the condition and return the status based on the function result
            return CurrentState;
        }

    }
}
