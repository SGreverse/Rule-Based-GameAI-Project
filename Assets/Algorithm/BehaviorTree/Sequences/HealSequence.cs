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

namespace Assets.Algorithm.MainAlgorithm.Sequences
{
    public class HealSequence : Sequence
    {
        public HealSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {

            this.Children.Add(new WaitAction(CurrentEnemy, 0.5f));
            this.Children.Add(new HealAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));

            this.PriorityCurve = new InverseLogisticCurve(12, 0.4f);
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
                Curve = new LogisticCurve(10, 0.4f), // k=10, m=0.4 
                Weight = 1.0f,
                ParameterFetcher = (enemy) =>
                {
                    Vector2 pPos = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / (enemy.Stats.viewRadius));
                }
            });

            // Factor 2: Potions Remaining  
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Potions Remaining",
                Curve = new SquareRootCurve(1, 0),
                Weight = 1.5f,
                ParameterFetcher = (enemy) =>
                {
                    float currentPotions = enemy.GetBrain().PotionAmount;
                    float maxPotions = enemy.Stats.MaxPotionsAmount;
                    if (maxPotions == 0) return 0f;
                    return Mathf.Clamp01(currentPotions / maxPotions);
                }
            });

            // Factor 3: Cover / Attacking Teammates
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Attacking Teammates",
                Curve = new PolynomialCurve(-1,1,2,1), // 1 - (1-x)^2 
                Weight = 2.73f,
                ParameterFetcher = (enemy) =>
                {
                    float MaxExpectedEnemies = GameBlackboard.Instance.CombatSettings.Heal_MaxExpectedEnemies;

                    float attackingEnemies = GameBlackboard.Instance.GetRoleCount(RoleType.Shooting, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Charging, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Flanking, enemy.InstanceID);
                    return Mathf.Clamp01(attackingEnemies / MaxExpectedEnemies);
                }
            });
            // Factor 4: Time Since Last Damage
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Time Since Last Damage",
                Curve = new LogisticCurve(20, 0.4f), // k=20, m=0.4 ( 2 seconds out of 5)
                Weight = 2.0f,
                ParameterFetcher = (enemy) =>
                {
                    float timeSinceDamage = enemy.GetBrain().TimeSinceLastHit;
                    float maxTime = GameBlackboard.Instance.CombatSettings.Heal_MaxTimeSinceLastDamage;
                    return Mathf.Clamp01(timeSinceDamage / maxTime);
                }
            });
            // Factor 5: Immunity Percentage 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Immunity Percentage",
                Curve = new LinearCurve(1, 0),
                Weight = 1.0f,
                ParameterFetcher = (enemy) =>
                {
                    float MaxImmunity = GameBlackboard.Instance.CombatSettings.Heal_MaxImmunity;
                    return Mathf.Clamp01(enemy.Stats.Defence/MaxImmunity);
                }
            });

            this._preConditions.Add((enemy) =>
            {
                return CurrentEnemy.GetBrain().PotionAmount > 0;
            });
            this._preConditions.Add((enemy) =>
            {
                return GameBlackboard.Instance.RequestRole(RoleType.Healing, CurrentEnemy.InstanceID, this.CurrentUtility, CurrentEnemy);
            });

        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.Healing); // Change icon to match the specific sequence
            }

            return canEnter;
        }
        public override void OnExit()
        {
            GameBlackboard.Instance.ReleaseRole(RoleType.Healing, CurrentEnemy.InstanceID);
            base.OnExit();
        }
    }
}
