using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class DefendAction : Action
    {
        private const float DEFEND_TIME = 1.5f;
        private float _startTime;
        public DefendAction(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
        }

        public override bool OnEnter()
        {
            CurrentEnemy.Combat.RaiseShield();
            _startTime = Time.time;
            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {

            // keep looking at the player
            Vector2 playerPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
            CurrentEnemy.Movment.RotateToFace(playerPos);

            //if we finished defending or the shield broke while defending- lower shield
            if (CurrentEnemy.GetBrain().IsShieldBroken || Time.time>=_startTime+DEFEND_TIME)
            {
                CurrentEnemy.Combat.LowerShield();
                CurrentState = NodeState.Success;
                return NodeState.Success;
            }
            CurrentState = NodeState.Running;
            return NodeState.Running;
        }
        public override void OnExit()
        {
            CurrentEnemy.Combat.LowerShield();
            base.OnExit();
        }
    }
}
