using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BehaviorTree.Boss;
using Assets.Data;
using UnityEngine;

namespace Assets.EntityLogic
{
    public class BossLogic : EnemyLogic
    {
        private BossStats _bossStats;

        private float _meteorStrikeTimer = 0f;
        public float _slamTimer = 0f;

        public bool CanMeteorStrike => _meteorStrikeTimer >= _bossStats.MetoerStrikeCooldown;

        public BossLogic(BossStats stats,BossManager manager) : base(stats,manager)
        {
            brain_root = new BossRootSelector(manager);
            _bossStats = stats;
        }
        public float CalculateSlamDamage()
        {
            return _bossStats.BodySlamDamage;
        }
        public float CalculateMeteorHitDamage()
        {
            return _bossStats.MeteorStrikeDamage;
        }
        public override void Tick(float delta)
        {
            _meteorStrikeTimer += delta;
            _slamTimer += delta;
            base.Tick(delta);
        }
        public void PerformMeteorStrike()
        {
            _meteorStrikeTimer = 0;
        }
        public void PerformSlamAttack()
        {
            _slamTimer = 0;
        }
    }
}
