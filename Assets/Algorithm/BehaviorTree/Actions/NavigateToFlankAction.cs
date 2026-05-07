using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Algorithm.MainAlgorithm.Actions
{
    public enum FlankSide
    {
        Back = 180,
        Left = 90,
        Right = -90,
    }
    public class NavigateToFlankAction:Action
    {
        private FlankSide _sideToFlank;
        private Vector2 _currentFlankTarget;


        public NavigateToFlankAction(EnemyManager enemyController, FlankSide side) : base(enemyController)
        {
            this._sideToFlank = side;
        }

        public override bool OnEnter()
        {
            UpdateFlankPosition();

            Vector2 playerPosition = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;

            bool CanFlank=CurrentEnemy.Movment.SetStealthTarget(_currentFlankTarget, playerPosition);
            if (!CanFlank) return false;
            return base.OnEnter();
        }

        public override NodeState Evaluate()
        {


            // Recalculate the ideal flank position
            UpdateFlankPosition();


            Vector2 playerPosition = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
            Vector2 currentPos = CurrentEnemy.transform.position;

            // Check if we have successfully arrived at the flank position
            float distanceToFlankSpot = Vector2.Distance(currentPos, _currentFlankTarget);


            float distToPlayer = Vector2.Distance(currentPos, playerPosition);

            Vector2 dirToEnemy = (currentPos - playerPosition).normalized;
            Vector2 playerForward = (Vector2)GameBlackboard.Instance.ReadLastRecordedEvent(EnvironmentKey.PlayerDirectionChange).Value;

            //calculate the angle to the player looking direction
            float dotToPlayerForward = Vector2.Dot(playerForward, dirToEnemy);

            bool closeEnoughToStrike = distToPlayer <= distanceToFlankSpot && distToPlayer<=CurrentEnemy.Stats.attackRange;


            if (CurrentEnemy.Movment.IsMovmentFinished() || closeEnoughToStrike)
            {
                CurrentEnemy.Movment.StopMovment();
                CurrentState = NodeState.Success;
                return NodeState.Success;
            }

            // If the player moved significantly, tell the MAPF to update our path to the new flank spot
            float pathOffset = Vector2.Distance(CurrentEnemy.Movment.TargetPosition, _currentFlankTarget);
            if (pathOffset > CurrentEnemy.Stats.attackRange)
            {
                bool CanFlank=CurrentEnemy.Movment.SetStealthTarget(_currentFlankTarget,playerPosition);
                if (!CanFlank)
                {
                    CurrentState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }

            CurrentState = NodeState.Running;
            return NodeState.Running;
        }

        private void UpdateFlankPosition()
        {
            Vector2 playerPosition = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;

            // If the player is standing perfectly still, we use their facing direction.
            // If they are moving, we use their velocity direction.
            Vector2 playerForward =(Vector2) GameBlackboard.Instance.ReadLastRecordedEvent(EnvironmentKey.PlayerDirectionChange).Value;

            // Mathematical rotation to find the blind spot (Left, Right, or Back)
            float angleInRadians = (float)_sideToFlank * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(angleInRadians);
            float sinTheta = Mathf.Sin(angleInRadians);

            Vector2 flankDirection = new Vector2(
                (playerForward.x * cosTheta) - (playerForward.y * sinTheta),
                (playerForward.x * sinTheta) + (playerForward.y * cosTheta)
            );

            // Set the flank target 3 units away from the player in that direction
            _currentFlankTarget = playerPosition + (flankDirection * 3.0f);
        }
        public override void OnExit()
        {
            this.CurrentEnemy.Movment.StopMovment();
            base.OnExit();
        }
    }

}
