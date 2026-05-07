using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.MainAlgorithm
{
    public abstract class Composite:Node
    {
        protected List<Node> Children;
        protected int CurrChildIndex=0;

        protected Composite(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children = new List<Node>();
        }
        public void AddChild(Node node)
        {
            this.Children.Add(node);
        }

        // for visualizing purposes
        public List<Node> GetChildren()
        {
            return this.Children;
        }
        public override bool OnEnter()
        {
            this.CurrChildIndex = 0;
            return base.OnEnter();
        }
        protected virtual bool TryEnterNode(Node Node)
        {
            //if the node is already the node running, we return true
            if (Node.CurrentState != NodeState.Running)
            {
                //if we fail to enter the node, return false
                if (!Node.OnEnter()) return false;
            }

            return true;
        }
        public override void OnExit()
        {
            // If the composite is aborted from the outside, abort the running child
            if (CurrChildIndex < Children.Count)
            {
                Children[CurrChildIndex].OnExit();
            }
            base.OnExit();
        }
    }
}
