using UnityEngine;

namespace Assets.Algorithm.Data
{

    [CreateAssetMenu(fileName = "NewCombatSettings", menuName = "AI/Combat Settings")]
    public class CombatSettingsSO : ScriptableObject
    {
        [Header("Flee Settings")]
        public float Flee_MaxTimeSinceLastDamage = 5f;
        public float Flee_MaxMapDistance = 20f;

        [Header("Defend Settings")]
        public float Defend_MaxShieldHoldTime = 5f;
        public float Defend_MaxExpectedEnemies = 3f;

        [Header("Heal Settings")]
        public float Heal_MaxTimeSinceLastDamage = 5f;
        public float Heal_MaxImmunity = 100f;
        public float Heal_MaxExpectedEnemies = 3f;

        [Header("Attack Settings")]
        public float Attack_MaxPlayerHealth = 100f;
        public float Attack_MaxKeyAmount = 3f;
        public float Attack_MaxAttackingTeammates = 4f;
        public float Attack_MaxPlayerMomentum = 5f;

        [Header("Reload Settings")]
        public float Reload_MaxAttackingTeammates = 4f;
        public float Reload_MaxTimeSinceLastDamage = 5f;

        [Header("Charge Settings")]
        public float Charge_MaxExpectedEnemies = 4f;
        public float Charge_MaxPlayerShieldStamina = 100f;

        [Header("Shoot Settings")]
        public float Shoot_SafeRadius = 1.5f;
        public float Shoot_MaxPressureThreshold = 4f;

        [Header("Flank Settings")]
        public float Flank_MaxExpectedDistractors = 4f;
        public float Flank_MaxExpectedFlankers = 2f;
        public float Flank_DirectionChangeTime = 3f;

    }
}