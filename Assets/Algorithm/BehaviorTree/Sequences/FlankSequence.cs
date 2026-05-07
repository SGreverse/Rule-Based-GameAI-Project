using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.Deque;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Actions;
using Assets.Algorithm.Utility;
using UnityEngine;
namespace Assets.Algorithm.BehaviorTree.Sequences
{
    public class FlankSequence : Sequence
    {
        public FlankSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new NavigateToFlankAction(CurrentEnemy, FlankSide.Back));
            this.Children.Add(new ChaseAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));
            this.Children.Add(new MeleeAttackAction(CurrentEnemy, 1.5f));
            this.Children.Add(new FindTileToStepBackAction(CurrentEnemy));
            this.Children.Add(new MoveToAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));

            this.PriorityCurve = new LinearCurve(1, 0);
            this.PriorityFetcher = (enemy) =>
            {
                float playerHealth = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerHealth);
                float playerDefence = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerDefence);
                float rawDamage = enemy.GetBrain().CalculateStubDamage();
                float finaldamage = rawDamage - (playerDefence / 100) * rawDamage;
                return finaldamage / playerHealth;
            };

            // Factor 1: Angle 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Angle",
                Curve = new LogisticCurve(12, 0.5f), 
                Weight = 1.4f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    Vector2 dirToEnemy = ((Vector2)enemy.transform.position - pPos).normalized;
                    Vector2 pForward = (Vector2)GameBlackboard.Instance.ReadLastRecordedEvent(EnvironmentKey.PlayerDirectionChange).Value;

                    float dotProduct = Vector2.Dot(pForward, dirToEnemy);
                    return (1f - dotProduct) / 2f;
                }
            });

            // Factor 2: Distractors
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Amount of Distractors",
                Curve = new SquareRootCurve(1, 0),
                Weight = 1.13f,
                ParameterFetcher = (enemy) =>
                {
                    float maxExpectedEnemies = GameBlackboard.Instance.CombatSettings.Flank_MaxExpectedDistractors;

                    float distractors = GameBlackboard.Instance.GetRoleCount(RoleType.Charging, enemy.InstanceID) +
                                        GameBlackboard.Instance.GetRoleCount(RoleType.Defending, enemy.InstanceID);
                    return Mathf.Clamp01(distractors / maxExpectedEnemies);
                }
            });

            // Factor 3: Flankers Density 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Flankers Density",
                Curve = new LinearCurve(-1, 1), 
                Weight = 1.0f,
                ParameterFetcher = (enemy) =>
                {
                    float maxExpectedEnemies = GameBlackboard.Instance.CombatSettings.Flank_MaxExpectedFlankers;

                    float flankers = GameBlackboard.Instance.GetRoleCount(RoleType.Flanking, enemy.InstanceID);
                    return Mathf.Clamp01(flankers / maxExpectedEnemies);
                }
            });
            // Factor 4: Player Environmental Awareness 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Player Environmental Awareness",
                Curve = new ExponentialCurve(1f),
                Weight = 0.8f,
                ParameterFetcher = (enemy) =>
                {
                    
                    float directionChangeTime= GameBlackboard.Instance.CombatSettings.Flank_DirectionChangeTime;
                    return (GameBlackboard.Instance.ReadData<GameEventDeque<BlackboardData>>(EnvironmentKey.PlayerDirectionChange).Count-1)/ directionChangeTime;// direction change rate in the last 3 seconds
                }
            });

            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.RequestRole(RoleType.Flanking, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
            });
        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Flanking); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Flanking, CurrentEnemy.InstanceID);
            base.OnExit();
        }
    }
}
