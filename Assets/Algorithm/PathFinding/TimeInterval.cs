using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.PathFinding
{
    /// <summary>
    /// Represents a continuous, floating-point block of time during which a map node is occupied.
    /// </summary>
    public struct TimeInterval
    {
        public float StartTime { get; private set; }
        public float EndTime { get; private set; }

        public string AgentID { get; private set; }

        public TimeInterval(float start, float end,string agentID)
        {
            StartTime = start;
            EndTime = end;
            AgentID = agentID;
        }

        /// <summary>
        /// Evaluates if a requested continuous time frame intersects with this reserved interval.
        /// </summary>
        public bool Overlaps(float start, float end, float padding)
        {
        
            return start < (EndTime + padding) && end > (StartTime - padding);
        }
    }
}
