using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class ReloadAction : Action
    {
        private const float TIME_TO_RELOAD= 0.3f;
        private float _reloadAmount;
        private float _startTime;
        public ReloadAction(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
        }

        public override bool OnEnter()
        {
            _reloadAmount = CurrentEnemy.GetBrain().ArrowsMissing();
            _startTime=Time.time;
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            if(_reloadAmount > 0)
            {
                if (Time.time >= _startTime + TIME_TO_RELOAD)
                {
                    CurrentEnemy.GetBrain().ReloadOneArrow();
                    _reloadAmount--;
                    _startTime = Time.time;
                }
                CurrentState = NodeState.Running;
                return NodeState.Running;
            }
            CurrentState = NodeState.Success;
            return NodeState.Success;
        }
        
    }
}
