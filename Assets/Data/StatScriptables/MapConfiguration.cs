using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Data.StatScriptables
{
    /// <summary>
    /// ScriptableObject containing all configuration parameters for the Multi-Agent Pathfinding (MAPF) system.
    /// Separates configuration data from runtime logic to adhere to Clean Code principles.
    /// </summary>
    [CreateAssetMenu(fileName = "MapfConfiguration", menuName = "AI/MAPF Configuration", order = 1)]
    public class MapfConfiguration : ScriptableObject
    {


        public float TimeWindowLimit = 4.0f;


        public float TemporalPadding = 0.15f;



    }
}
