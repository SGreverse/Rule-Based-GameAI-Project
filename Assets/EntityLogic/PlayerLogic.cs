using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Data;

namespace Assets.EntityLogic
{
    public class PlayerLogic : EntityLogic
    {
        public Action ShieldBreakEvent;
        private bool _WasShieldEventInvoked;

        public PlayerEquipment Equipment { get; private set; }
        public PlayerLogic(BasicStats stats, ItemData sword, ItemData bow, ItemData armor) : base(stats)
        {
            _WasShieldEventInvoked = false;

            Equipment = new PlayerEquipment(sword,bow,armor);
        }

        public override float CalculateMeleeDamage()
        {
            float baseDamage = base.CalculateMeleeDamage();
            float swordBonus = Equipment.GetBonusMeleeDamage();
            return baseDamage + swordBonus;
        }
        public override float CalculateStubDamage()
        {
            return CalculateMeleeDamage() * 2;
        }
        public override float CalculateProjectileDamage()
        {
            float baseDamage = base.CalculateProjectileDamage();
            float bowBonus = Equipment.GetBonusArrowDamage();
            return baseDamage + bowBonus;
        }

        public override float CalculateDefence()
        {
            float baseDefence = base.CalculateDefence();
            float armorBonus = Equipment.GetBonusShield();
            return baseDefence + armorBonus;
        }
        public float GetBowChargeTime()
        {
            float ChargeByRarity=0;
            switch (Equipment.EquippedBow.Rarity)
            {
                case Rarity.Common:
                    ChargeByRarity=0.25f;
                    break;
                case Rarity.Rare:
                    ChargeByRarity = 0.5f;
                    break;
                case Rarity.Legendary:
                    ChargeByRarity = 0.75f;
                    break;
            }
            return 1-ChargeByRarity;
        }
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            if(IsShieldBroken)
            {
                if (!_WasShieldEventInvoked)
                {
                    ShieldBreakEvent.Invoke();
                    _WasShieldEventInvoked= true;
                }
            }
            else
            {
                _WasShieldEventInvoked = false;
            }
        }
    }
}
