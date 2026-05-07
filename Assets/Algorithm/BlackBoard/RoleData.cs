using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.HashDataStructers;
using Assets.Algorithm.PriorityQueue;
    using UnityEngine;

namespace Assets.Algorithm.BlackBoard
{
    public class RoleData
    {
        private RoleType _role;
        public int MaxCapacity { get; private set; }
        public int Size {  get; private set; }

        private GameHashMap<string, EnemyManager> _enemies_Data;

        private GameHashMap<string, RoleClaim> _claim_lookup;

        private PriorityQueue<RoleClaim> _min_tracker;

        public RoleData(int maxCapacity,RoleType role)
        {
            MaxCapacity = maxCapacity;
            Size = 0;
            this._enemies_Data = new GameHashMap<string,EnemyManager>();
            this._min_tracker = new PriorityQueue<RoleClaim>(HeapType.Min);
            this._claim_lookup = new GameHashMap<string, RoleClaim>();
            this._role = role;
        }
        public bool Add(string agentID,float utility,EnemyManager enemy)
        {
            if (_claim_lookup.ContainsKey(agentID)) return false;

            RoleClaim claimer = new RoleClaim(agentID, utility);

            this._enemies_Data[agentID] = enemy;
            this._claim_lookup[agentID] = claimer;
            this._min_tracker.Enqueue(claimer);
            this.Size++;

            if (Size > MaxCapacity)
            {
                RoleClaim minClaimer = this._min_tracker.Dequeue();

                EnemyManager kickedEnemy = this._enemies_Data[minClaimer.agentID];

                RemoveFromTables(minClaimer.agentID);

                if (minClaimer == claimer)
                {
                    Debug.Log($"Enemy {claimer.agentID} couldnt obtain role({_role}) since his utility was too low");
                    return false;
                }
                Debug.Log($"Enemy {claimer.agentID} was kicked from his role({_role}) due to low utility");
                kickedEnemy.GetBrain().RoleKick();

            }
            return true;
        }
        public void Remove(string agentID)
        {
            if (!_claim_lookup.ContainsKey(agentID)) return;

            RoleClaim claimer= this._claim_lookup[agentID];
            this._min_tracker.RemoveItem(claimer);

            RemoveFromTables(agentID);
        }

        private void RemoveFromTables(string agentID)
        {
            this._enemies_Data.Remove(agentID);
            this._claim_lookup.Remove(agentID);
            this.Size--;
        }
        public RoleClaim GetMinUtility()
        {
            if(this.Size == 0) return null;
            return this._min_tracker.Peek();
        }
        public void UpdateUtility(string agentID, float newUtility)
        {
            if (_claim_lookup.TryGetValue(agentID, out RoleClaim claimer) &&
                Mathf.Abs(claimer.UtilityScore - newUtility) > 0.01f)
            {
                //update the utility
                claimer.UtilityScore = newUtility;

                //update his position in the priority queue
                this._min_tracker.UpdatePriority(claimer);
            }
        }
        public int GetOtherRoleHoldersCount(string agentID)
        {
            if (_claim_lookup.ContainsKey(agentID)) return Size - 1;
            return Size;
        }


        public List<RoleClaim> GetSortedClaims()
        {
            List<RoleClaim> activeClaims = new List<RoleClaim>();

            // Loop through your custom HashMap to gather the claims
            foreach (var kvp in _claim_lookup)
            {
                activeClaims.Add(kvp.Value);
            }

            // Sort descending (Highest utility first)
            activeClaims.Sort((a, b) => b.UtilityScore.CompareTo(a.UtilityScore));

            return activeClaims;
        }
    }
}
