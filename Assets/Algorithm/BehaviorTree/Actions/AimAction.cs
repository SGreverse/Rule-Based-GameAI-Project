using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class AimAction : Action
    {
        private float _aimDuration;
        private float _startTime; // the time in which we started the animation
        public AimAction(EnemyManager enemyController, float aimDuration) : base(enemyController)
        {
            _aimDuration = aimDuration;
        }

        public override bool OnEnter()
        {
            _startTime=Time.time;
            
            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {
            //keep rotating towards the player
            Vector2 playerPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
            CurrentEnemy.Movment.RotateToFace(playerPos);

            //once aim time is up we check if the player can be shot at. if he can, we shoot, otherwise we fail
            if (Time.time>= this._startTime + this._aimDuration)
            {
                if (CurrentEnemy.GetBrain().CurrentPlayerExposure > 0)
                {
                    CurrentState = NodeState.Success;
                    return NodeState.Success;
                }
                else
                {
                    CurrentState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }

            CurrentState = NodeState.Running;
            return NodeState.Running;
        }
    }
}