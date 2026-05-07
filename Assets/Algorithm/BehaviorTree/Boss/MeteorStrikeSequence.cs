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

namespace Assets.Algorithm.BehaviorTree.Boss
{
    public class MeteorStrikeSequence : Sequence
    {
        private Vector2 _arenaCenter = Vector2.zero; 

        public MeteorStrikeSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            // 1. The Sequence of Actions
            this.Children.Add(new SetArenaCenterTargetAction(CurrentEnemy, _arenaCenter));
            this.Children.Add(new MoveToAction(CurrentEnemy)); // Walk to the center
            this.Children.Add(new RotateAction(CurrentEnemy)); // Face the player menacingly
            this.Children.Add(new WaitAction(CurrentEnemy, 0.5f)); // Wind-up
            this.Children.Add(new CastMeteorsAction(CurrentEnemy, 3.0f)); // Cast for 3 seconds

            // 2. Priority: Damage vs Player Health
            this.PriorityCurve = new SquareRootCurve(1, 0);
            this.PriorityFetcher = (enemy) =>
            {
                if (enemy is BossManager boss)
                {
                    float playerHealth = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerHealth);
                    float playerDefence = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerDefence);
                    float rawDamage = boss.GetBrain().CalculateMeteorHitDamage();
                    float finaldamage = rawDamage - (playerDefence / 100) * rawDamage;
                    return finaldamage / playerHealth;
                }
                return 0f;
            };

            // Factor 1: Center Proximity (Inverse Parabola: 1 - x^2)
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Distance from Center",
                // Polynomial parameters: a*(x-b)^n + c. Here: -1*(x-0)^2 + 1 = 1 - x^2
                Curve = new PolynomialCurve(-1, 0, 2, 1),
                Weight = 0.817f,
                ParameterFetcher = (enemy) =>
                {
                    float distToCenter = Vector2.Distance(enemy.transform.position, _arenaCenter);
                    float maxArenaRadius = 15f; 
                    return Mathf.Clamp01(distToCenter / maxArenaRadius);
                }
            });

            // Factor 2: Enrage / Phase Transition (Boss Health)
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Boss Health Phase",
                Curve = new InverseLogisticCurve(25, 0.5f), // k=25, m=0.5
                Weight = 1.501f,
                ParameterFetcher = (enemy) =>
                {
                    float currentHealth = enemy.GetBrain().CurrentHealth;
                    float maxHealth = Mathf.Max(0.1f, enemy.Stats.maxHealth);
                    return Mathf.Clamp01(currentHealth / maxHealth);
                }
            });

            // Factor 3: Combo Breaker (Player Momentum)
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Combo Breaker",
                Curve = new LogisticCurve(15, 0.6f), // k=15, m=0.6
                Weight = 0.442f,
                ParameterFetcher = (enemy) =>
                {
                    float playerAttackDealt = GameBlackboard.Instance.ReadData<GameEventDeque<BlackboardData>>(EnvironmentKey.PlayerAmountOfAttacks).Count;
                    return Mathf.Clamp01(playerAttackDealt / 5f); // Normalized against max momentum (5)
                }
            });

            this._preConditions.Add((enemy) =>
            {
                if (enemy is BossManager boss) {
                    return boss.GetBrain().CanMeteorStrike;
                }
                return false;
            });
        }

        // --- SHIELD MANAGEMENT: Protect the boss while casting! ---
        public override bool OnEnter()
        {

            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.MeteorStriking); // Change icon to match the specific sequence
                if (this.CurrentEnemy is BossManager boss)
                {
                    boss.Combat.RaiseShield();// Turn on Shield while walking and casting
                }
            }

            return canEnter;
        }

        public override void OnExit()
        {
            if (this.CurrentEnemy is BossManager boss)
            {
                boss.Combat.LowerShield();
            }
            base.OnExit();
        }
    }
}
