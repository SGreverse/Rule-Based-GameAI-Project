using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Actions;
using Assets.Algorithm.Utility;
using UnityEngine;

namespace Assets.Algorithm.BehaviorTree.Sequences
{
    public class DefendSequence : Sequence
    {
        public DefendSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new RotateAction(CurrentEnemy));
            this.Children.Add(new DefendAction(CurrentEnemy));

            this.PriorityCurve = new PolynomialCurve(1, 1, 2, 0); //parabola with ease-in
            this.PriorityFetcher = (enemy) =>
            {
                float currentHealth = enemy.GetBrain().CurrentHealth;
                float maxHealth = enemy.Stats.maxHealth;
                return Mathf.Clamp01(currentHealth / maxHealth);
            };

            //Factor 1:Shield HP
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Shield HP",
                Curve = new LogisticCurve(20, 0.4f), // k=20,m=0.4
                Weight = 3.856f,
                ParameterFetcher = (enemy) =>
                {
                    return enemy.GetBrain().ShieldStamina / enemy.Stats.MaxShieldStamina; // Normalized 0 to 1
                }
            });

            // Factor 2: Is Human Player Attacking?
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Is Human Player Attacking?",
                Curve = new LinearCurve(1, 0), // Boolean step 
                Weight = 1.617f,
                ParameterFetcher = (enemy) =>
                {
                    EntityState playerState = GameBlackboard.Instance.ReadData<EntityState>(EnvironmentKey.PlayerState);
                    return playerState == EntityState.Attacking|| playerState ==EntityState.Aiming  ? 1f : 0f;
                }
            });
            // Factor 3: Distance from Player
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Distance from Player",
                Curve = new InverseLogisticCurve(15, 0.15f), // k=15, m=0.15 
                Weight = 1.32f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / enemy.Stats.viewRadius*2);
                }
            });

            //Factor 4: Amount of flankers
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Amount of flankers",
                Curve = new PolynomialCurve(-1, 1, 2, 1), // Inverse parabola with ease out 
                Weight = 1.57f,
                ParameterFetcher = (enemy) =>
                {
                    float MaxExpectedEnemies = GameBlackboard.Instance.CombatSettings.Defend_MaxExpectedEnemies;

                    float flankers = GameBlackboard.Instance.GetRoleCount(RoleType.Flanking, enemy.InstanceID);
                    return Mathf.Clamp01(flankers / MaxExpectedEnemies);
                }
            });
            // Factor 5: Shield Hold Time 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Shield Hold Time",
                Curve = new LinearCurve(-1, 1), // Inverse Linear
                Weight = 1.617f,
                ParameterFetcher = (enemy) =>
                {
                    float maxHoldTime = GameBlackboard.Instance.CombatSettings.Defend_MaxShieldHoldTime;
                    float holdTime = enemy.GetBrain()._timeSinceShieldRaised;
                    return Mathf.Clamp01(holdTime / maxHoldTime);
                }
            });

            this._preConditions.Add((enemy) =>
            {
                return !enemy.GetBrain().IsShieldBroken;
            });

            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.RequestRole(RoleType.Defending, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
            });

        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Defending); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Defending, CurrentEnemy.InstanceID);
            base.OnExit();
        }
    }
}
