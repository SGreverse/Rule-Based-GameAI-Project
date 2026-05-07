using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.Deque;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.Utility;
using UnityEngine;

namespace Assets.Algorithm.BehaviorTree.Sequences
{
    public class AttackSelector : UtilitySelector
    {
        public AttackSelector(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new FlankSequence(CurrentEnemy));
            this.Children.Add(new ChargeAttackSequence(CurrentEnemy));
            this.Children.Add(new ShootSequence(CurrentEnemy));

            this.PriorityCurve = new LogisticCurve(15, 0.15f);
            this.PriorityFetcher = (enemy) =>
            {
                float currentHealth = enemy.GetBrain().CurrentHealth;
                float maxHealth = Mathf.Max(0.1f, enemy.Stats.maxHealth);
                return Mathf.Clamp01(currentHealth / maxHealth);
            };

            // Factor 1: Human Player's Health 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Human Player's Health",
                Curve = new PolynomialCurve(-1,0,2,1), // -x^2 + 1
                Weight = 2.86f,
                ParameterFetcher = (enemy) =>
                {
                    float playerHealth = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerHealth);

                    float maxPlayerHealth = GameBlackboard.Instance.CombatSettings.Attack_MaxPlayerHealth;
                    return Mathf.Clamp01(playerHealth / maxPlayerHealth);
                }
            });
            // Factor 2: Amount of Keys
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Amount of Keys",
                Curve = new InverseRootCurve(-1, 1), // 1 - sqrt(1-x)
                Weight = 2.744f,
                ParameterFetcher = (enemy) =>
                {
                    float currentKeys = GameBlackboard.Instance.ReadData<int>(EnvironmentKey.AmountOfKeys);
                    float maxKeys = GameBlackboard.Instance.CombatSettings.Attack_MaxKeyAmount; 
                    return Mathf.Clamp01(currentKeys / maxKeys);
                }
            });

            // Factor 3: Percentage of Attacking Teammates 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Percentage of Attacking Teammates",
                Curve = new PolynomialCurve(-1,1,2,1), // Ease-out parabola
                Weight = 2.796f,
                ParameterFetcher = (enemy) =>
                {
                    

                    float attackingEnemies = GameBlackboard.Instance.GetRoleCount(RoleType.Shooting,enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Charging, enemy.InstanceID) +
                                             GameBlackboard.Instance.GetRoleCount(RoleType.Flanking, enemy.InstanceID);

                    float maxAttackingTeammates = GameBlackboard.Instance.CombatSettings.Attack_MaxAttackingTeammates;
                    return Mathf.Clamp01(attackingEnemies / maxAttackingTeammates);
                }
            });

            // Factor 4: Opportunity 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Opportunity",
                Curve = new LinearCurve(1,0), 
                Weight = 2.01f,
                ParameterFetcher = (enemy) =>
                {
                    bool isPlayerVulnerable = GameBlackboard.Instance.ReadData<EntityState>(EnvironmentKey.PlayerState)!=EntityState.Free;
                    return isPlayerVulnerable ? 1f : 0f;
                }
            });
            //Factor 5: Human Player Momentum
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name= "Human Player Momentum",
                Curve = new ExponentialCurve(5), // e^(-5x)
                Weight = 0.736f,
                ParameterFetcher = (enemy) =>
                {
                    float playerAttackDealt = GameBlackboard.Instance.ReadData<GameEventDeque<BlackboardData>>(EnvironmentKey.PlayerAmountOfAttacks).Count;

                    float maxAttacksDealt= GameBlackboard.Instance.CombatSettings.Attack_MaxPlayerMomentum;
                    return (playerAttackDealt/maxAttacksDealt);
                }
            });
        }
        //public override void CalculateUtility()
        //{
        //    this.CurrentUtility = 1;
        //}
    }
}
