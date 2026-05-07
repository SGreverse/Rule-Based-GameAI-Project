using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.SavingSystem
{
    [Serializable]
    public class WorldSaveData
    {
        public List<EnemyData> aliveEnemies = new List<EnemyData>();
        public List<string> deadEnemyIDs = new List<string>();
    }
}
