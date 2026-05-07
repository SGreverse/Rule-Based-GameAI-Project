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
    public class ReloadSequence : Sequence
    {
        public ReloadSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new ReloadAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));

            this.PriorityCurve = new InverseLogisticCurve(20, 0.2f);
            this.PriorityFetcher = (enemy) =>
            {
                float currentAmmo = enemy.GetBrain().ProjectileAmount;
                float maxAmmo = Mathf.Max(1f, enemy.Stats.MaxProjectilesAmount);
                return Mathf.Clamp01(currentAmmo / maxAmmo);
            };

            // Factor 1: AI Health 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "AI Health",
                Curve = new LogisticCurve(15, 0.3f), // k=15, m=0.3
                Weight = 0.499f,
                ParameterFetcher = (enemy) =>
                {
                    float currentHealth = enemy.GetBrain().CurrentHealth;
                    float maxHealth = Mathf.Max(0.1f, enemy.Stats.maxHealth);
                    return Mathf.Clamp01(currentHealth / maxHealth);
                }
            });

            // Factor 2: Distance from Human Player 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Distance from Human Player",
                Curve = new LogisticCurve(12, 0.3f), // k=12, m=0.3 
                Weight = 2.01f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / enemy.Stats.viewRadius);
                }
            });

            // Factor 3: Cover / Attacking Teammates 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Attacking Teammates",
                Curve = new PolynomialCurve(-1,1,2,1),
                Weight = 2.145f,
                ParameterFetcher = (enemy) =>
                {
                    

                    float attackingEnemies = GameBlackboard.Instance.GetRoleCount(RoleType.Flanking, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Charging, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Shooting, enemy.InstanceID);
                    float maxCover=GameBlackboard.Instance.CombatSettings.Reload_MaxAttackingTeammates;
                    return Mathf.Clamp01(attackingEnemies / maxCover);
                }
            });

            // Factor 4: Time Since Last Damage 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Time Since Last Damage",
                Curve = new LogisticCurve(20, 0.4f), // k=20, m=0.4 
                Weight = 1.16f,
                ParameterFetcher = (enemy) =>
                {
                    float timeSinceDamage = enemy.GetBrain().TimeSinceLastHit;

                    float maxTime = GameBlackboard.Instance.CombatSettings.Reload_MaxTimeSinceLastDamage;
                    return Mathf.Clamp01(timeSinceDamage / maxTime);
                }
            });

            // Factor 5: Angle of Human Player / Is Player Looking Away
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Angle of Human Player",
                Curve = new LogisticCurve(10, 0.25f), // k=10, m=0.25
                Weight = 0.79f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    Vector2 pForward = GameBlackboard.Instance.ReadData<Vector2>(EnvironmentKey.PlayerVelocity).normalized;

                    // Direction from player to enemy
                    Vector2 dirToEnemy = ((Vector2)enemy.transform.position - pPos).normalized;

                    // Angle between where player is looking and where enemy is standing
                    float angle = Vector2.Angle(pForward, dirToEnemy);
                    // Normalize the angle
                    return Mathf.Clamp01(angle / 180f);
                }
            });

            this._preConditions.Add((enemy) =>
            {
                return enemy.GetBrain().ProjectileAmount < enemy.Stats.MaxProjectilesAmount;
            });

            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.RequestRole(RoleType.Reloading, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
            });
        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Reloading); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Reloading, CurrentEnemy.InstanceID);
            base.OnExit();
        }

    }
}
