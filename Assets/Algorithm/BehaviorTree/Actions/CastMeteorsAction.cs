using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class CastMeteorsAction : Action
    {
        private float _castDuration;
        private float _startTime;
        public CastMeteorsAction(EnemyManager currEnemy, float duration) : base(currEnemy)
        {
            this._castDuration = duration;
        }

        public override bool OnEnter()
        {
            _startTime =Time.time;

            if (this.CurrentEnemy is BossManager boss)
            {
                boss.ExecuteMeteorStrike();
            }
            else
            {
                Debug.LogWarning("CastMeteorsAction was called on an EnemyManager that is NOT a BossManager");
            }

            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {

            if (Time.time >= this._startTime + this._castDuration)
            {
                CurrentState = NodeState.Success;
                return CurrentState;
            }

            CurrentState = NodeState.Running;
            return NodeState.Running;
        }
        public override void OnExit()
        {
            if (this.CurrentEnemy is BossManager boss)
            {
                boss.StopMeteorStrike();
            }
        }
    }
}