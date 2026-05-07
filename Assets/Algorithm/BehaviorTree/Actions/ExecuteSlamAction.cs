using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.MainAlgorithm;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class ExecuteSlamAction : Action
    {
        private float _slamDuration;
        private float _startTime;

        public ExecuteSlamAction(EnemyManager currEnemy, float duration) : base(currEnemy)
        {
            this._slamDuration = duration;
        }

        public override bool OnEnter()
        {
            _startTime = Time.time;

            if (this.CurrentEnemy is BossManager boss)
            {
                boss.ExecuteGroundSlam();
            }
            else
            {
                Debug.LogWarning("ExecuteSlamAction was called on an EnemyManager that is NOT a BossManager!");
            }

            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {

            if (Time.time >=this._startTime+ this._slamDuration)
            {
                CurrentState = NodeState.Success;
                return CurrentState;
            }

            CurrentState = NodeState.Running;
            return NodeState.Running;
        }
    }
}
