using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.Data;
using Assets.Algorithm.Deque;
using Assets.Algorithm.HashDataStructers;
using Assets.Algorithm.PathFinding;
using UnityEngine;

namespace Assets.Algorithm.BlackBoard
{
    public enum RoleType
    {
        Healing,
        Reloading,
        Defending,
        Fleeing,
        Charging,
        Flanking,
        Shooting,
        Patroling,
        MeteorStriking,
        GroundSlamming
    }
    public enum EnvironmentKey
    {
        PlayerDetected,
        PlayerPosition,
        PlayerHealth,
        PlayerDefence,
        PlayerState,
        IsShieldBroken,
        ShieldStamina,
        PlayerVelocity,
        PlayerSpeed,
        PlayerDirectionChange,
        PlayerAmountOfAttacks,
        AmountOfKeys
    }
    public class GameBlackboard
    {
        private static GameBlackboard instance;

        public static GameBlackboard Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameBlackboard();
                }
                return instance;
            }
        }
        //the data structure for storing all the environment data enemies share with each other
        private GameHashMap<EnvironmentKey, BlackboardData> _environmentData;

        //the data structure for managing the enemy roles
        private GameHashMap<RoleType, RoleData> _roleRegistry;

        //the global path finder each enemy use to navigate the map
        public PathFinder GlobalPathFinder;

        //a list that holds all enemies that are currently active in the map
        public List<EnemyManager> ActiveEnemies = new List<EnemyManager>();

        public CombatSettingsSO CombatSettings;

        private GameBlackboard()
        {

            GameManager.Instance.OnKeyCollected += AnnounceKeyCollection;//activate upon key collection

            InitializeEnvironment();
            InitializeRoleRegistry();

            GlobalPathFinder = new PathFinder(GameManager.Instance.Config);

        }
        private void InitializeRoleRegistry()
        {
            this._roleRegistry = new GameHashMap<RoleType, RoleData>();

            foreach (RoleType type in Enum.GetValues(typeof(RoleType)))
            {
                if (type!=RoleType.Charging && type!=RoleType.Flanking) {
                    this._roleRegistry[type] = new RoleData(int.MaxValue, type);
                }
            }
            this._roleRegistry[RoleType.Charging] = new RoleData(5, RoleType.Charging);
            this._roleRegistry[RoleType.Flanking] = new RoleData(3, RoleType.Flanking);
        }
        private void InitializeEnvironment()
        {
            this._environmentData = new GameHashMap<EnvironmentKey, BlackboardData>();

            PlayerIsUnkown();

            WriteData(EnvironmentKey.PlayerHealth, 100f);
            WriteData(EnvironmentKey.AmountOfKeys, 0);
        }

        #region Environment
        public T ReadData<T>(EnvironmentKey key)
        {
            if (this._environmentData.ContainsKey(key))
            {
                try
                {
                    return (T)this._environmentData[key].Value;
                }
                catch (InvalidCastException)
                {
                    Debug.LogError($"Couldnt cast key:{key} to type:{typeof(T).Name}");
                }
            }
            Debug.LogWarning($"Asked for Non-existant key:{key}");
            return default;
        }
        public float ReadTimeStamp(EnvironmentKey key)
        {
            if (this._environmentData.ContainsKey(key))
            {
                return this._environmentData[key].Timestamp;
            }
            Debug.LogWarning($"Asked for Non-existant key:{key}");
            return default;
        }
        public void WriteData(EnvironmentKey key, object value)
        {
            float currentTime = Time.time;
            this._environmentData[key] = new BlackboardData(value, currentTime);
        }

        /// <summary>
        /// Records the exact time an event occurred into a queue for sliding-window calculations.
        /// </summary>
        public void RecordEvent(EnvironmentKey key, object value)
        {
            // Try to read the queue. 
            GameEventDeque<BlackboardData> eventQueue = ReadData<GameEventDeque<BlackboardData>>(key);

            //if the queue exists, queue the new event value
            if (eventQueue != null)
            {
                //enqueue the new value
                float currentTime = Time.time;
                eventQueue.AddLast(new BlackboardData(value, currentTime));
            }
        }
        /// <summary>
        /// Reads the first value pushed into the event queue
        /// </summary>
        public BlackboardData ReadFirstRecordedEvent(EnvironmentKey key)
        {
            //if the deque exists, read the first recorded event
            GameEventDeque<BlackboardData> eventQueue = ReadData<GameEventDeque<BlackboardData>>(key);

            if (eventQueue != null && eventQueue.Count > 0)
            {
                return eventQueue.PeekFirst();
            }
            return null;
        }

        /// <summary>
        /// Reads the last value pushed into the event queue
        /// </summary>
        public BlackboardData ReadLastRecordedEvent(EnvironmentKey key)
        {
            //if the deque exists, read the last recorded event
            GameEventDeque<BlackboardData> eventQueue = ReadData<GameEventDeque<BlackboardData>>(key);

            if (eventQueue != null && eventQueue.Count > 0)
            {
                return eventQueue.PeekLast();
            }
            return null;
        }
        /// <summary>
        /// removes the first recorded event
        /// </summary>
        public void RemoveEvent(EnvironmentKey key)
        {
            GameEventDeque<BlackboardData> eventQueue = ReadData<GameEventDeque<BlackboardData>>(key);
            if (eventQueue != null && eventQueue.Count > 0)
            {
                eventQueue.RemoveFirst();
            }
        }
        public void PlayerDetected(PlayerManager player)
        {
            Vector2 PlayerFacingDirection = player.Movement.FacingDirection;
            BlackboardData LastRecordedEvent = ReadLastRecordedEvent(EnvironmentKey.PlayerDirectionChange);
            if (LastRecordedEvent==null || PlayerFacingDirection != (Vector2)LastRecordedEvent.Value)
            {
                RecordEvent(EnvironmentKey.PlayerDirectionChange,PlayerFacingDirection);
            }

            WriteData(EnvironmentKey.PlayerDetected, true);
            WriteData(EnvironmentKey.PlayerPosition, player.transform);
            WriteData(EnvironmentKey.PlayerState, player.CurrentState);
            WriteData(EnvironmentKey.IsShieldBroken, player.GetBrain().IsShieldBroken);
            WriteData(EnvironmentKey.ShieldStamina, player.GetBrain().ShieldStamina);
            WriteData(EnvironmentKey.PlayerVelocity, player.GetComponent<Rigidbody2D>().linearVelocity);
            WriteData(EnvironmentKey.PlayerSpeed, player.GetBrain().Stats.moveSpeed);
            WriteData(EnvironmentKey.PlayerDefence, player.GetBrain().CalculateDefence());
        }
        public void PlayerIsUnkown()
        {
            WriteData(EnvironmentKey.PlayerDetected, false);
            WriteData(EnvironmentKey.PlayerState, EntityState.Free);
            WriteData(EnvironmentKey.IsShieldBroken, false);
            WriteData(EnvironmentKey.ShieldStamina, 100f);
            WriteData(EnvironmentKey.PlayerAmountOfAttacks, new GameEventDeque<BlackboardData>());
            WriteData(EnvironmentKey.PlayerDirectionChange, new GameEventDeque<BlackboardData>());
            WriteData(EnvironmentKey.PlayerVelocity, Vector2.zero);
            WriteData(EnvironmentKey.PlayerSpeed, 0);
        }
        public void AnnounceKeyCollection()
        {
            WriteData(EnvironmentKey.AmountOfKeys, ReadData<int>(EnvironmentKey.AmountOfKeys) + 1);
        }
        #endregion

        #region roles
        public bool RequestRole(RoleType roleType, string agentID, float agentUtility, EnemyManager agent)
        {
            if (!_roleRegistry.ContainsKey(roleType))
            {
                Debug.LogError($"[Blackboard] Role '{roleType}' does not exist");
                return false;
            }
            return _roleRegistry[roleType].Add(agentID, agentUtility, agent);
        }
        public void ReleaseRole(RoleType roletype, string agentID)
        {
            if(_roleRegistry.TryGetValue(roletype, out RoleData data))
            {
                data.Remove(agentID);
            }
        }
        public void UpdateRoleUtility(RoleType roleType,string agentID,float utility)
        {
            if (_roleRegistry.TryGetValue(roleType, out RoleData data))
            {
                data.UpdateUtility(agentID, utility);
            }
        }
        /// <returns>how many agents hold that role except the asking agent</returns>
        public int GetRoleCount(RoleType roleType, string agentID)
        {
            return this._roleRegistry[roleType].GetOtherRoleHoldersCount(agentID);
        }
        #endregion

        /// <summary>
        /// returns for a certain enemies where all his allies are(if there are any)
        /// </summary>
        public List<Vector2> GetAlliesPositions(EnemyManager RequestingEnemy)
        {
            List<Vector2> ally_positions = ActiveEnemies.Where(enemy => enemy != RequestingEnemy).Select(enemy => enemy.GetComponent<Rigidbody2D>().position).ToList();
            return ally_positions;
        }

        #region Active Enemies Managment
        public void ActivateEnemy(EnemyManager enemy)
        {
            if (!this.ActiveEnemies.Contains(enemy))
            {
                enemy.ResumeEnemy();
                ActiveEnemies.Add(enemy);
                Debug.Log($"[Activate] Added {enemy.name}. | Count: {ActiveEnemies.Count}");
            }
        }
        public void KickEnemy(EnemyManager enemy)
        {
            if (this.ActiveEnemies.Contains(enemy))
            {
                enemy.SuspendEnemy();
                ActiveEnemies.Remove(enemy);
                ReleaseRole(enemy.CurrentRole,enemy.InstanceID);
                Debug.Log($"Removed {enemy.name}. | Count: {ActiveEnemies.Count}");
            }
        }
        public void GameOver()
        {
            foreach (EnemyManager enemy in ActiveEnemies)
            {
                enemy.SuspendEnemy();
            }
            ActiveEnemies.Clear();
        }
        //reset all the info in the blackboard if we reload the scene
        public void ResetBlackboard()
        {
            foreach (EnemyManager enemy in ActiveEnemies)
            {
                enemy.SuspendEnemy();
            }
            ActiveEnemies.Clear();
            InitializeEnvironment();
            InitializeRoleRegistry();
        }
        #endregion

        /// <summary>
        /// Function used to debug how the blackboard role data looks like
        /// </summary>
        public RoleData GetRoleData(RoleType roleType)
        {
            if (_roleRegistry.TryGetValue(roleType, out RoleData data))
            {
                return data;
            }
            return null;
        }
    }
}
