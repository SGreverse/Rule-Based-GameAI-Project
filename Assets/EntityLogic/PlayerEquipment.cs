using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Data;

namespace Assets.EntityLogic
{
    public class PlayerEquipment
    {
        public ItemData EquippedSword { get; private set; }
        public ItemData EquippedBow { get; private set; }
        public ItemData EquippedArmor { get; private set; }

        public event Action OnEquipmentChanged;

        public PlayerEquipment(ItemData sword,ItemData bow,ItemData armor)
        {
            EquippedSword = sword;
            EquippedBow = bow;
            EquippedArmor = armor;
        }
        public void EquipBetterItem(ItemData newItem)
        {
            if (newItem == null) return;

            switch (newItem.Slot)
            {
                case EquipmentSlot.Sword:
                    if (newItem.Rarity > EquippedSword.Rarity)
                        EquippedSword = newItem;
                    break;
                case EquipmentSlot.Bow:
                    if (newItem.Rarity > EquippedBow.Rarity)
                        EquippedBow = newItem; 
                    break;
                case EquipmentSlot.Armor:
                    if (newItem.Rarity > EquippedArmor.Rarity)
                        EquippedArmor = newItem; 
                    break;
            }

            OnEquipmentChanged?.Invoke();
        }
        public float GetBonusMeleeDamage()
        {
            return EquippedSword.BonusDamage + EquippedArmor.BonusDamage;
        }
        public float GetBonusArrowDamage()
        {
            return EquippedBow.BonusDamage + EquippedArmor.BonusDamage;
        }
        public float GetBonusShield()
        {
            return EquippedBow.BonusDefense + EquippedArmor.BonusDefense + EquippedSword.BonusDefense;
        }
    }
}
