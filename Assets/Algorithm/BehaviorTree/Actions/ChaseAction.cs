using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public class ChaseAction : Action
    {
        public ChaseAction(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
        }
        public override bool OnEnter()
        {
            return base.OnEnter();
        }
        public override NodeState Evaluate()
        {
            // if the player is in attack range, stop chasing
            Vector2 playerPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
            float distToPlayer = Vector2.Distance(CurrentEnemy.transform.position, playerPos);
            if (distToPlayer <= CurrentEnemy.Stats.attackRange)
            {
                CurrentEnemy.Movment.StopMovment();
                CurrentState = NodeState.Success;
                return NodeState.Success;
            }

            //if the chasing position is no longer close enough to hit the player from, repath.
            float PlayerOffset= Vector2.Distance(CurrentEnemy.Movment.TargetPosition, playerPos);
            if(PlayerOffset> CurrentEnemy.Stats.attackRange)
            {
                CurrentEnemy.Movment.SetTarget(playerPos);
            }
            CurrentState = NodeState.Running;
            return NodeState.Running;

        }
        public override void OnExit()
        {
            this.CurrentEnemy.Movment.StopMovment();
            base.OnExit();
        }
    }
}
