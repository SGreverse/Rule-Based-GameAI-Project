using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Data
{
    [CreateAssetMenu(fileName = "NewBossStats", menuName = "A Hero's Adventure/Boss Stats")]
    public class BossStats:EnemyStats
    {


        [Header("Ground Slam Mechanics")]
        public float BodySlamDamage = 25f;
        public float SlamMaxRadius = 6f;       // How far the wave travels
        public float SlamExpandDuration = 0.5f; // How fast it travels (0.5s is a good, snappy speed)
        public float SlamStunDuration = 3.0f;   // The 3-second stun you requested
        public float SlamCooldownDuration = 15f;

        [Header("Meteor Strike Mechanics")]
        public float MeteorStrikeDamage = 0f;
        public float MeteorRadius = 2.0f;        // How big each circle is
        public float MeteorTelegraphTime = 1.2f; // How much time the player has to dodge
        public int MeteorsToSpawn = 12;          // Total meteors dropped per cast
        public float MeteorSpawnRadius = 7f;     // The "spread" around the player
        public float MetoerStrikeCooldown = 25f;


    }
}
