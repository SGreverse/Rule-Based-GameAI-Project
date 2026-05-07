using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Data;
using UnityEngine;

namespace Assets.SavingSystem
{
    [System.Serializable]
    public class EnemyData
    {
        public string InstanceID;
        public Vector2 Position;
        public float Health;
        public int PotionAmuont;
        public int ProjectileAmount;

        public EnemyData(EnemyManager enemy)
        {
            InstanceID = enemy.InstanceID;
            Position = enemy.transform.position;
            Health = enemy.GetBrain().CurrentHealth;
            PotionAmuont = enemy.GetBrain().PotionAmount;
            ProjectileAmount=enemy.GetBrain().ProjectileAmount;
        }
    }

}
