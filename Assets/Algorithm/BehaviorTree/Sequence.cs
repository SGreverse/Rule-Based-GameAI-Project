using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;
namespace Assets.Algorithm.MainAlgorithm
{
    public abstract class Sequence : Composite
    {
        protected Sequence(EnemyManager CurrentEnemy) : base(CurrentEnemy) { }

        public override NodeState Evaluate()
        {

            while (CurrChildIndex < Children.Count)
            {
                Node child = Children[CurrChildIndex];
                if (!TryEnterNode(child))//entering falis if atleast one of the child's pre Conditions return false
                {
                    //if one child node fails, the entire sequence fails
                    CurrentState = NodeState.Failure;
                    return NodeState.Failure;
                }

                switch (child.Evaluate())//we run the child and check his return state
                {
                    case NodeState.Success:
                        child.OnExit();
                        CurrChildIndex++;
                        break;

                    case NodeState.Failure:
                        child.OnExit();
                        CurrentState = NodeState.Failure;
                        return CurrentState;

                    case NodeState.Running:
                        //since each sequence represents a "role" the AI can have, as long as we are running the sequence
                        //we keep updating the utility of the role to match with ther most recent data.
                        GameBlackboard.Instance.UpdateRoleUtility(this.CurrentEnemy.CurrentRole, CurrentEnemy.InstanceID, this.CurrentUtility);
                        CurrentState = NodeState.Running;
                        return CurrentState;
                }
            }
            CurrentState = NodeState.Success;
            return CurrentState;
        }
    }
}
