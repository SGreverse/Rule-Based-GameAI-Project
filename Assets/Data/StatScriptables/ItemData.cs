using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Data
{
    public enum EquipmentSlot { Sword, Bow, Armor }
    public enum Rarity { Common,Rare,Legendary}

    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Loot/Equipment")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info")]
        public string ItemID; // VERY IMPORTANT for the Save System! (e.g., "sword_iron")
        public string ItemName;
        public EquipmentSlot Slot;
        public Rarity Rarity;

        [Header("Stat Bonuses")]
        public float BonusDamage;
        public float BonusDefense;

        [Header("Visuals")]
        public Sprite ItemIcon; // For your UI Inventory
    }
}
