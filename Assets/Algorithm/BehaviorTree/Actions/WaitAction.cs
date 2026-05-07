using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class WaitAction:Action
    {
        private float _waitTime;
        private float _startTime;

        public WaitAction(EnemyManager enemyController, float waitTime) : base(enemyController)
        {
            _waitTime = waitTime;
        }

        public override bool OnEnter()
        {
            _startTime = Time.time;
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            if (Time.time >=this._startTime + this._waitTime)
            {
                CurrentState = NodeState.Success;
                return NodeState.Success; 
            }
            CurrentState = NodeState.Running;
            return NodeState.Running;
        }

    }
}
