using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Data;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Assets.EntityLogic
{
    public abstract class EntityLogic
    {
        public BasicStats Stats;

        public float CurrentHealth;
        public int PotionAmount;
        public int ProjectileAmount;
        public bool IsDead => CurrentHealth <= 0;

        public bool IsShieldUp;
        public bool IsShieldBroken { get; private set; }
        public float _timeSinceShieldRaised = 0f;
        private float _timeSinceShieldLowered = 0f;
        public float ShieldStamina;
        private float _shieldDrainPerSecond = 10f; // Drains 10 stamina per second while held
        private float _shieldRegenPerSecond = 20f; // Recovers 20 stamina per second
        private float _regenDelayNormal = 1.5f;    // Wait 1.5s after dropping shield to regen
        private float _regenDelayBroken = 4.0f;    // Wait 4.0s if the shield was shattered!

        public event Action OnDeath;
        public EntityLogic(BasicStats stats)
        {
            this.Stats = stats;
            CurrentHealth = stats.maxHealth;
            PotionAmount = stats.MaxPotionsAmount;
            ProjectileAmount = stats.MaxProjectilesAmount;
            ShieldStamina = stats.MaxShieldStamina;
        }
        public virtual float CalculateMeleeDamage()
        {
            return Stats.MeleeDamage;
        }
        public virtual float CalculateStubDamage()
        {
            return Stats.StubDamage;
        }
        public virtual float CalculateProjectileDamage()
        {
            return Stats.ShotDamage;
        }
        public virtual float CalculateDefence()
        {
            return Stats.Defence;
        }
        public virtual float TakeDamage(float rawDamage )
        {
            if (IsDead) return 0;

            float damageAfterArmor = rawDamage - (CalculateDefence() / 100f) * rawDamage;
            damageAfterArmor = Mathf.Max(0, damageAfterArmor);

            if (IsShieldUp && !IsShieldBroken)
            {
                // Shield absorbs 80% of the damage, 20% gets through to HP
                float absorbedByShield = damageAfterArmor * 0.8f;
                float damageToHealth = damageAfterArmor * 0.2f;

                // drains the shield stamina
                ShieldStamina -= absorbedByShield;

                // Did this hit shatter the shield?
                if (ShieldStamina <= 0)
                {
                    // SPILLOVER DAMAGE: Add the unblocked damage back to the health pool
                    float spillover = Mathf.Abs(ShieldStamina);
                    damageToHealth += spillover;

                    BreakShield();
                }

                CurrentHealth -= damageToHealth;
            }
            else
            {
                // Shield is down or broken, take full damage after armor
                CurrentHealth -= damageAfterArmor;
            }

            // Clamp health and check for death
            CurrentHealth = Mathf.Max(0, CurrentHealth);
            if (IsDead)
            {
                OnDeath?.Invoke();
            }
            return CurrentHealth;
        }
        private void BreakShield()
        {
            ShieldStamina = 0;
            IsShieldBroken = true;
            IsShieldUp = false; 
            _timeSinceShieldLowered = 0f;
            _timeSinceShieldRaised = 0f;
        }
        public void SetShieldState(bool isUp)
        {
            // Cannot raise or lower shield if it's broken
            if (IsShieldBroken) return;

            IsShieldUp = isUp;

            // If we just lowered the shield, reset the regen timer
            if (!IsShieldUp)
            {
                _timeSinceShieldLowered = 0f;
            }
            else
            {
                _timeSinceShieldRaised = 0f;
            }
        }
        public bool Heal()
        {
            if(this.PotionAmount > 0)
            {
                CurrentHealth = Mathf.Min(Stats.maxHealth, CurrentHealth+0.2f*Stats.maxHealth);
                this.PotionAmount--;
                return true;
            }
            return false;
        }
        public void Shoot()
        {
            if (this.ProjectileAmount > 0)
            {
                ProjectileAmount -= 1;
            }
        }
        public void ReloadOneArrow()
        {
            if (this.ProjectileAmount < this.Stats.MaxProjectilesAmount)
            {
                this.ProjectileAmount++;
            }
        }
        public int ArrowsMissing()
        {
            return this.Stats.MaxProjectilesAmount - this.ProjectileAmount;
        }
        public virtual void Tick(float deltaTime)
        {
            if (IsDead) return;

            if (IsShieldUp)
            {
                // Drain stamina over time just from holding it
                ShieldStamina -= _shieldDrainPerSecond * deltaTime;
                _timeSinceShieldRaised += deltaTime;
                if (ShieldStamina <= 0)
                {
                    BreakShield();
                }
            }
            else if (ShieldStamina < Stats.MaxShieldStamina)
            {
                _timeSinceShieldLowered += deltaTime;

                float requiredDelay = IsShieldBroken ? _regenDelayBroken : _regenDelayNormal;

                if (_timeSinceShieldLowered >= requiredDelay)
                {
                    // Regenerate stamina
                    ShieldStamina += _shieldRegenPerSecond * deltaTime;

                    // Once it reaches max, it is no longer broken
                    if (ShieldStamina >= Stats.MaxShieldStamina)
                    {
                        ShieldStamina = Stats.MaxShieldStamina;
                        IsShieldBroken = false;
                    }
                }
            }
        }

    }

}
