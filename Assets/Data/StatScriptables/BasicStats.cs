using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Data
{
    public class BasicStats : ScriptableObject
    {

        [Header("Base Stats")]
        public int maxHealth = 100;
        public float moveSpeed = 3f;
        public float attackRange = 1.5f;
        public float attackWidth = 1f;
        public float Defence = 30f;
        public float arrowSpeed = 15f;

        [Header("Damage Types ")]
        public float MeleeDamage = 10;
        public float StubDamage = 10;
        public float ShotDamage = 5;


        [Header("Utilities")]
        public int MaxProjectilesAmount=20;
        public int MaxPotionsAmount=5;
        public float MaxShieldStamina = 100f;

    }
}
