using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.PriorityQueue;

namespace Assets.Algorithm.BlackBoard
{
    public class RoleClaim:IComparable<RoleClaim>,IHeapTrackable
    {
        public string agentID { get; private set; }
        public float UtilityScore { get; set; }

        public int HeapIndex {  get; set; }

        public RoleClaim(string agent, float utilityScore)
        {
            agentID = agent;
            UtilityScore = utilityScore;
        }

        public int CompareTo(RoleClaim other)
        {
            return this.UtilityScore.CompareTo(other.UtilityScore);
        }
        // override object.Equals
        public override bool Equals(object obj)
        {
            return obj is RoleClaim rc && this.agentID==rc.agentID;
        }
        public override int GetHashCode()
        {
            return agentID.GetHashCode();
        }
    }
}
