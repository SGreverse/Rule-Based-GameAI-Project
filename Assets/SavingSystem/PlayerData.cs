using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.SavingSystem
{
    [System.Serializable]
    public class PlayerData
    {
        public Vector2 Position;
        public float Health;
        public int PotionAmount;
        public int ProjectileAmount;
        public int KeysCollected;
        public string SwordID;
        public string BowID;
        public string ArmorID;
        public PlayerData(PlayerManager player,int Keys)
        {
            Position = player.transform.position;
            Health = player.GetBrain().CurrentHealth;
            PotionAmount = player.GetBrain().PotionAmount;
            ProjectileAmount = player.GetBrain().ProjectileAmount;
            KeysCollected = Keys;
            SwordID = player.GetBrain().Equipment.EquippedSword.ItemID;
            BowID = player.GetBrain().Equipment.EquippedBow.ItemID;
            ArmorID = player.GetBrain().Equipment.EquippedArmor.ItemID;
        }
    }
}
