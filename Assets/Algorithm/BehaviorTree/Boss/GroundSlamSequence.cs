using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.MainAlgorithm.Actions;
using Assets.Algorithm.Utility;
using Assets.Data;
using Assets.EntityLogic;
using UnityEngine;

namespace Assets.Algorithm.BehaviorTree.Boss
{
    public class GroundSlamSequence : Sequence
    {
        public GroundSlamSequence(EnemyManager CurrentEnemy) : base(CurrentEnemy)
        {
            this.Children.Add(new StopMovmentAction(CurrentEnemy));
            this.Children.Add(new RotateAction(CurrentEnemy));
            this.Children.Add(new WaitAction(CurrentEnemy, 1f)); // Telegraph wind-up time
            this.Children.Add(new ExecuteSlamAction(CurrentEnemy, 2f)); // 2s animation duration

            this.PriorityCurve = new SquareRootCurve(1, 0);
            this.PriorityFetcher = (enemy) =>
            {

                if (enemy is BossManager boss)
                {
                    float playerHealth = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerHealth);
                    float playerDefence = GameBlackboard.Instance.ReadData<float>(EnvironmentKey.PlayerDefence);
                    float rawDamage = boss.GetBrain().CalculateSlamDamage();
                    float finaldamage = rawDamage - (playerDefence / 100) * rawDamage;
                    return finaldamage / playerHealth;
                }
                return 0f;
            };

            // Factor 1: Distance (Melee Proximity)
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Distance to Player",
                Curve = new InverseLogisticCurve(15, 0.25f), // k=15, m=0.25
                Weight = 1.0f,
                ParameterFetcher = (enemy) =>
                {
                    BossManager boss=enemy as BossManager;
                    Vector2 pPos = boss.Player.transform.position;
                    float dist = Vector2.Distance(pPos, enemy.transform.position);
                    return Mathf.Clamp01(dist / enemy.Stats.viewRadius);
                }
            });

            // Factor 2: Punishing Greed 
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Player Greed",
                Curve = new LogisticCurve(12, 0.6f), // k=12, m=0.6
                Weight = 0.5f,
                ParameterFetcher = (enemy) =>
                {
                    if (enemy is BossManager boss)
                    {
                        // Normalize against a 6-second maximum greed window
                        return Mathf.Clamp01(boss.TimePlayerInMeleeRange / 6.0f);
                    }
                    return 0f;
                }
            });


            // Factor 4: Internal Cooldown
            this.UtilityFactors.Add(new UtilityFactor
            {
                Name = "Slam Cooldown",
                Curve = new LogisticCurve(14, 0.8f), // k=20, m=0.8
                Weight = 2.0f, // Massive weight to act as a veto when on cooldown
                ParameterFetcher = (enemy) =>
                {
                    if (enemy is BossManager boss)
                    {
                        float timer = boss.GetBrain()._slamTimer;

                        return Mathf.Clamp01(timer/((BossStats)boss.Stats).SlamCooldownDuration);
                    }
                    return 0f;
                }
            });
        }
        public override bool OnEnter()
        {
            bool canEnter = base.OnEnter();

            if (canEnter)
            {
                CurrentEnemy.ShowAction(RoleType.GroundSlamming); // Change icon to match the specific sequence
            }

            return canEnter;
        }
    }
}
