using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Actions;
using Assets.Algorithm.Utility;
using UnityEngine;

namespace Assets.Algorithm.BehaviorTree.Sequences
{
    public class ShootSequence : Sequence
    {
        public ShootSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new AimAction(CurrentEnemy, 1.5f));//aim for 1.5 second

            ConditionFunc HasLineOfSight = (CurrentEnemy) =>
            {
                Vector2 playerPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                Vector2 enemyPos = CurrentEnemy.transform.position;
                Vector2 PosDifferenceVector = (playerPos - enemyPos);
                Vector2 direction=PosDifferenceVector.normalized;
                float distance = PosDifferenceVector.magnitude;
                return Physics2D.Raycast(enemyPos,direction,distance,CurrentEnemy.ObstacleLayer).collider==null;
            };
            this.Children.Add(new Condition(CurrentEnemy, HasLineOfSight));
            this.Children.Add(new ShootAction(CurrentEnemy));

            this.PriorityCurve = new LinearCurve(1, 0);
            this.PriorityFetcher = (enemy) =>
            {
                float playerHealth = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerHealth);
                float playerDefence = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerDefence);
                float rawDamage = enemy.GetBrain().CalculateProjectileDamage();
                float finaldamage = rawDamage - (playerDefence / 100) * rawDamage;
                return finaldamage / playerHealth;
            };

            //Factor 1:distance
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name ="Distance From Human Player",
                Curve = new PolynomialCurve(-4, 0.5f, 2, 1), // Inverse Parabola peaking at 0.5 
                Weight = 1.0f, 
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / (enemy.Stats.viewRadius));
                }
            });

            //Factor 2:Pressure
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Enemy Pressure",
                Curve = new SquareRootCurve(1, 0), 
                Weight = 1.054f, 
                ParameterFetcher = (enemy) =>
                {
                    float pressureEnemies = GameBlackboard.Instance.GetRoleCount(RoleType.Charging, enemy.InstanceID) +
                                GameBlackboard.Instance.GetRoleCount(RoleType.Defending, enemy.InstanceID);

                    // Define a threshold for what count as 100% Maximum Pressure
                    float maxPressureThreshold = GameBlackboard.Instance.CombatSettings.Shoot_MaxPressureThreshold;

                    return Mathf.Clamp01(pressureEnemies / maxPressureThreshold);
                }
            });

            // Factor 3: Clear Line of Fire
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Clear Line of Fire",
                // Ally is directly on the line -> Score 0.0
                // Allies are completely clear -> Score 1.0
                Curve = new LinearCurve(1, 0),
                Weight = 1.2f, 
                ParameterFetcher = (enemy) =>
                {
                    Vector2 playerPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    Vector2 enemyPos = enemy.transform.position;

                    int enemyLayerMask = 1 << enemy.gameObject.layer;

                    Vector2 diffVector = playerPos - enemyPos;
                    float distance = diffVector.magnitude;
                    Vector2 direction = diffVector / distance;
                    float safeRadius = GameBlackboard.Instance.CombatSettings.Shoot_SafeRadius;
                    float minDistanceToLine = safeRadius;

                    RaycastHit2D[] hits = Physics2D.CircleCastAll(enemyPos, safeRadius, direction, distance, enemyLayerMask);

                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider.gameObject == enemy.gameObject) continue;

                        Vector2 P = hit.transform.position;
                        Vector2 AP = P - enemyPos;

                        // Project the ally position onto the firing line 
                        float t = Mathf.Clamp01(Vector2.Dot(AP, diffVector) / (distance * distance));
                        Vector2 C = enemyPos + t * diffVector;

                        // Measure exact distance from the center of the tunnel
                        float distToLine = Vector2.Distance(P, C);

                        if (distToLine < minDistanceToLine)
                        {
                            minDistanceToLine = distToLine;
                        }
                    }

                    return Mathf.Clamp01(minDistanceToLine / safeRadius);

                }
            });

            // Factor 4: Exposure
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Exposure",
                Curve = new LinearCurve(1, 0), 
                Weight = 0.9f, 
                ParameterFetcher = (enemy) =>
                {
                    return enemy.GetBrain().CurrentPlayerExposure;
                }
            });

            this._preConditions.Add((enemy) =>
            {
                return enemy.GetBrain().ProjectileAmount > 0;

            });

            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.RequestRole(RoleType.Shooting, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
            });
        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Shooting); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Shooting, CurrentEnemy.InstanceID);
            base.OnExit();
        }
    }
}
