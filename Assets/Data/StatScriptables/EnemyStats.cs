using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Timeline;

namespace Assets.Data
{
    [CreateAssetMenu(fileName = "NewEnemyStats", menuName = "A Hero's Adventure/Enemy Stats")]
    public class EnemyStats : BasicStats
{
        [Header("Identity")]
        public string enemyType;

        [Header("Vision Stats")]
        public float viewRadius = 10f;
        public float viewAngle = 90f;

    }
}
