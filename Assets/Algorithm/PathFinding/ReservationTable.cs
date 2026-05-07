using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.HashDataStructers;
using Assets.Algorithm.Map;
using Assets.Data.StatScriptables;
using UnityEngine;

namespace Assets.Algorithm.PathFinding
{
    public class ReservationTable
    {
        // Maps a global MapNode index to a list of reserved continuous time intervals
        private GameHashMap<Vector2Int, List<TimeInterval>> _table = new GameHashMap<Vector2Int, List<TimeInterval>>();

        //Maps agent ids to a list of reservations
        private GameHashMap<string, GameHashSet<Vector2Int>> _agentNodeTracker = new GameHashMap<string, GameHashSet<Vector2Int>>();

        private MapfConfiguration _config;

        public ReservationTable(MapfConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Registers a specific node as occupied for a continuous duration.
        /// Called when an agent successfully plots a path via SIPP.
        /// MUST be called only after checking IsNodeFree for that node and time interval
        /// </summary>
        public void ReserveNode(Vector2Int globalIndex, float startTime, float endTime, string agentID)
        {
            if (!_table.ContainsKey(globalIndex))
            {
                _table[globalIndex] = new List<TimeInterval>();
            }

            List<TimeInterval> intervals = _table[globalIndex];
            TimeInterval newInterval = new TimeInterval(startTime, endTime, agentID);

            if (intervals.Count == 0 || intervals[intervals.Count - 1].StartTime <= startTime)
            {
                intervals.Add(newInterval);
            }
            else
            {
                // Find where it belongs and insert it
                int insertIndex = intervals.FindLastIndex(i => i.StartTime <= startTime) + 1;
                intervals.Insert(insertIndex, newInterval);
            }

            if (!_agentNodeTracker.ContainsKey(agentID))
            {
                _agentNodeTracker[agentID] = new GameHashSet<Vector2Int>(20);
            }
            _agentNodeTracker[agentID].Add(globalIndex);
        }

        /// <summary>
        /// Validates whether a node is completely free during a requested continuous time frame.
        /// </summary>
        public bool IsNodeFree(Vector2Int globalIndex, float startTime, float endTime)
        {
            bool flag = false;
            if (!_table.TryGetValue(globalIndex, out List<TimeInterval> intervals))
            {
                flag = true;
            }

            for (int i = 0; !flag && i < intervals.Count ; i++)
            {
                var interval = intervals[i];

                // Because the list is sorted chronologically, if this interval starts after
                // our requested end time, no future intervals can overlap us either
                if (interval.StartTime - _config.TemporalPadding >= endTime)
                {
                    flag = true;
                }
                else if (interval.Overlaps(startTime, endTime, _config.TemporalPadding))
                {
                    return false;
                }
            }
            return true;
        }

       
        /// <summary>
        /// Clears all node reservations for a specific agent
        /// </summary>
        public void ClearReservationsForAgent(string targetAgentID)
        {
            if (!_agentNodeTracker.TryGetValue(targetAgentID, out GameHashSet<Vector2Int> agentNodes))
            {
                return; // Agent has no reservations
            }

            foreach (Vector2Int node in agentNodes)
            {
                if (_table.TryGetValue(node, out List<TimeInterval> intervals))
                {
                    // Remove all intervals belonging to this agent on this specific node
                    intervals.RemoveAll(i => i.AgentID == targetAgentID);

                    // if  only he reserved for theat tile, we dont need it anymore
                    if (intervals.Count == 0)
                    {
                        _table.Remove(node);
                    }
                }
            }

            // Clear the tracker for this agent
            _agentNodeTracker.Remove(targetAgentID);
        }

        /// <summary>
        /// Purges expired reservations to prevent dictionary bloat and memory leaks.
        /// </summary>
        public void CleanupExpiredReservations(float currentTime)
        {
            List<Vector2Int> emptyKeys = new List<Vector2Int>();

            // For global cleanup, becauyse time intervals are sorted
            foreach (var kvp in _table)
            {
                var intervals = kvp.Value;

                // Remove from the front since it's sorted chronologically
                while (intervals.Count > 0 && intervals[0].EndTime < currentTime)
                {
                    intervals.RemoveAt(0);
                }

                if (intervals.Count == 0)
                {
                    emptyKeys.Add(kvp.Key);
                }
            }

            foreach (var key in emptyKeys)
            {
                _table.Remove(key);
            }
        }
        public float CalculateWaitTime(Vector2Int globalIndex, float desiredArrival, float traverseTime, float padding)
        {
            if (!_table.ContainsKey(globalIndex)) return 0f; // Free immediately

            List<TimeInterval> intervals = _table[globalIndex];
            float currentCheckTime = desiredArrival;

            bool flag = false;
            for (int i = 0; !flag && i < intervals.Count ; i++)
            {
                TimeInterval interval = intervals[i];

                // Because the list is sorted, if we find a gap, we can take it immediately
                if (interval.StartTime - padding >= currentCheckTime + traverseTime)
                {
                    flag=true; // We found a safe gap before this interval even starts
                }

                else if (interval.EndTime + padding > currentCheckTime)
                {
                    currentCheckTime = interval.EndTime + padding;
                }
            }

            //return how much we need to wait
            return currentCheckTime - desiredArrival;
        }
    }
}
