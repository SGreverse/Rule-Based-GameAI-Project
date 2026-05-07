using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Actions;
using Assets.Algorithm.Utility;
using UnityEngine;

namespace Assets.Algorithm.BehaviorTree.Sequences
{
    public class ChargeAttackSequence : Sequence
    {
        public ChargeAttackSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new ChaseAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));
            this.Children.Add(new MeleeAttackAction(CurrentEnemy, 1.5f));//since theres no aniamtion budget,we consider the melee attack duration is 1.5 seconds
            this.Children.Add(new FindTileToStepBackAction(CurrentEnemy));
            this.Children.Add(new MoveToAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));



            this.PriorityCurve = new LinearCurve(1,0);
            this.PriorityFetcher = (enemy) =>
            {
                float playerHealth = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerHealth);
                float playerDefence= GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerDefence);
                float rawDamage = enemy.GetBrain().CalculateMeleeDamage();
                float finaldamage = rawDamage - (playerDefence / 100) * rawDamage;
                return finaldamage / playerHealth;
            };

            // Factor 1: Distance
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Distance",
                Curve = new InverseLogisticCurve(12, 0.6f),
                Weight = 0.955f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / (enemy.Stats.viewRadius * 2));
                }
            });

            // Factor 2: Player Density
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name="Enemy Density",
                Curve = new InverseLogisticCurve(12, 0.6f),
                Weight = 1.17f,
                ParameterFetcher = (enemy) =>
                {
                    float MaxExpectedEnemies = GameBlackboard.Instance.CombatSettings.Charge_MaxExpectedEnemies;

                    return Mathf.Clamp01(GameBlackboard.Instance.GetRoleCount(RoleType.Charging, enemy.InstanceID) / MaxExpectedEnemies);
                }
            });

            // Factor 3: Angle
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name="Angle",
                Curve = new PolynomialCurve(-4, 0.5f, 2, 1),
                Weight = 0.884f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    Vector2 dirToEnemy = ((Vector2)enemy.transform.position - pPos).normalized;
                    Vector2 pForward =(Vector2) GameBlackboard.Instance.ReadLastRecordedEvent(EnvironmentKey.PlayerDirectionChange).Value;
                    float signedAngle = Vector2.SignedAngle(pForward, dirToEnemy);
                    return (signedAngle / 360f) + 0.5f;
                }
            });

            // Factor 4: Shield State
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name="Shield State",
                Curve = new ExponentialCurve(5),
                Weight = 1.22f,
                ParameterFetcher = (enemy) =>
                {
                    if (GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.IsShieldBroken)) return 0f;

                    float maxshieldStamina = GameBlackboard.Instance.CombatSettings.Charge_MaxPlayerShieldStamina;
                    return GameBlackboard.Instance.ReadData<float>(EnvironmentKey.ShieldStamina) / maxshieldStamina;
                }
            });

            this._preConditions.Add((enemy) =>
                {
                    return GameBlackboard.Instance.RequestRole(RoleType.Charging, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
                }
            );

        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Charging); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override NodeState Evaluate()
        {
            return base.Evaluate();
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Charging, CurrentEnemy.InstanceID);
            base.OnExit();
        }
    }
}
