using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.PriorityQueue;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm
{
    public abstract class UtilitySelector : Selector
    {
        private const float TIME_TO_EVALUATE_UTILITY = 0.5f;

        private PriorityQueue<Node> _priorityQueue;

        private Node _currentlyRunningNode=null;

        private float _lastEvalTime = 0; 
        protected UtilitySelector(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
        }
        public override bool OnEnter()
        {
            //upon entering a utility selector, make sure no child is running, and calculate the utility of every child
            _currentlyRunningNode = null;
            foreach (Node child in this.Children)
            {
                child.CalculateUtility();
            }
            this._priorityQueue = new PriorityQueue<Node>(this.Children, HeapType.Max);
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            //if enough time passed since the last evaluation, evaluate the utility again to make sure the utility is the most relevant
            if (Time.time- _lastEvalTime >= TIME_TO_EVALUATE_UTILITY)
            {
                foreach (Node child in this.Children)
                {
                    child.CalculateUtility();
                }
                this._priorityQueue = new PriorityQueue<Node>(this.Children, HeapType.Max);

                _lastEvalTime = Time.time;
            }
            
            //since one child can fail, we try the best nodes one by one until one succeeds or all fail
            while (!_priorityQueue.IsEmpty())
            {
                Node bestNode = this._priorityQueue.Peek();

                // if we cant enter the child, remove him from the qeueu
                if ((!TryEnterNode(bestNode)))
                {
                    this._priorityQueue.Dequeue();
                }
                else
                {
                    //evaulate the node currently running
                    NodeState state = _currentlyRunningNode.Evaluate();

                    switch (state)
                    {
                        //if child is running, return running
                        case NodeState.Running:
                            this.CurrentState = NodeState.Running;
                            return NodeState.Running;

                        //if child succeeds, exit the child and return success
                        case NodeState.Success:
                            _currentlyRunningNode.OnExit();
                            _currentlyRunningNode = null;
                            this.CurrentState = NodeState.Success;
                            return NodeState.Success;

                        //if child return fail, we exit him and remove him from the queue and move to the next 
                        case NodeState.Failure:
                            _currentlyRunningNode.OnExit();
                            _currentlyRunningNode = null;
                            this._priorityQueue.Dequeue();
                            break;
                    }
                }
            }
            //if all children failed, return failure
            this.CurrentState = NodeState.Failure;
            return NodeState.Failure;
        }
        protected override bool TryEnterNode(Node bestNode)
        {
            // if the best child remain(hes already running), we have no need to enter the node and return true
            if (_currentlyRunningNode == bestNode) return true;

            //if we cant enter the child, return false
            if (!bestNode.OnEnter()) return false;

            //if theres already another node running, we make sure to exit him
            if (_currentlyRunningNode != null)
            {
                Debug.Log($"Utility Switch: Aborting {_currentlyRunningNode.GetType().Name} for {bestNode.GetType().Name}");
                _currentlyRunningNode.OnExit();
            }

            _currentlyRunningNode = bestNode;
            return true;
        }
        public override void OnExit()
        {
            // to exit we need to exit first the child
            if (_currentlyRunningNode != null)
            {
                _currentlyRunningNode.OnExit();
                _currentlyRunningNode = null;

            }
            CurrentState = NodeState.None;
        }
    }
}
