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
    public class FleeSequence : Sequence
    {
        public FleeSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new FindSafePositionAction(CurrentEnemy));
            this.Children.Add(new MoveToAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));


            this.PriorityCurve = new InverseLogisticCurve(10, 0.4f);
            this.PriorityFetcher = (enemy) =>
            {
                float currentHealth = enemy.GetBrain().CurrentHealth;
                float maxHealth = enemy.Stats.maxHealth;
                return Mathf.Clamp01(currentHealth / maxHealth);
            };

            // Factor 1: Distance from Human Player
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Distance from Human Player",
                Curve = new InverseLogisticCurve(10, 0.3f), // k=10, m=0.3
                Weight = 1.0f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / enemy.Stats.viewRadius);
                }
            });

            // Factor 2: Speed Ratio
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Speed Ratio",
                Curve = new LinearCurve(-1, 1), // Inverse Linear
                Weight = 1.867f,
                ParameterFetcher = (enemy) =>
                {
                    float playerSpeed = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerSpeed);
                    float enemySpeed = enemy.Stats.moveSpeed;
                    return Mathf.Clamp01(playerSpeed / enemySpeed);
                }
            });

            // Factor 3: Percentage of Non-Attacking Teammates / Alone in Battle 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Percentage of Non-Attacking Teammates",
                Curve = new PolynomialCurve(-1,1,2,1), // 1 - (1-x)^2
                Weight = 2.417f,
                ParameterFetcher = (enemy) =>
                {
                    float totalEnemies = GameBlackboard.Instance.ActiveEnemies.Count-1;
                    if (totalEnemies == 0) return 1f; //only himself

                    float NonattackingEnemies = GameBlackboard.Instance.GetRoleCount(RoleType.Fleeing, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Reloading, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Healing, enemy.InstanceID);

                    return Mathf.Clamp01(NonattackingEnemies / totalEnemies);
                }
            });

            // Factor 4: Time Since Last Damage 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Time Since Last Damage",
                Curve = new InverseLogisticCurve(20, 0.4f), // k=20, m=0.4
                Weight = 1.0f,
                ParameterFetcher = (enemy) =>
                {
                    float timeSinceDamage = enemy.GetBrain().TimeSinceLastHit;

                    float maxTime = GameBlackboard.Instance.CombatSettings.Flee_MaxTimeSinceLastDamage;
                    return Mathf.Clamp01(timeSinceDamage / maxTime);
                }
            });

            // Factor 5: Isolation from the Group 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Isolation from the Group",
                Curve = new LinearCurve(1, 0),
                Weight = 0.729f,
                ParameterFetcher = (enemy) =>
                {
                    List<Vector2> AlliesPos=GameBlackboard.Instance.GetAlliesPositions(enemy);
                    if(AlliesPos.Count==0) return 1;
                    Vector2 SquadCenter=Vector2.zero;
                    foreach (Vector2 pos in AlliesPos)
                    {
                        SquadCenter+=pos;
                    }
                    SquadCenter/=AlliesPos.Count;
                    float distToSquad = Vector2.Distance(enemy.transform.position, SquadCenter);

                    float maxMapDistance = GameBlackboard.Instance.CombatSettings.Flee_MaxMapDistance; 
                    return Mathf.Clamp01(distToSquad / maxMapDistance);
                }
            });

            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.RequestRole(RoleType.Fleeing, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
            });
        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Fleeing); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Fleeing, CurrentEnemy.InstanceID);
            base.OnExit();
        }
    }
}
