using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class MeleeAttackAction : Action
    {
        private float _meleeDuration; 
        private float _startTime;
        public MeleeAttackAction(EnemyManager currEnemy,float duration):base(currEnemy)
        {
            this._meleeDuration = duration;
        }
        public override bool OnEnter()
        {
            _startTime = Time.time;
            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {
            if (Time.time >= _startTime+_meleeDuration)
            {
                CurrentEnemy.Combat.MeleeAttack();
                CurrentState = NodeState.Success;
                return CurrentState; 
            }
            CurrentState = NodeState.Running;
            return NodeState.Running;
        }

    }
}
