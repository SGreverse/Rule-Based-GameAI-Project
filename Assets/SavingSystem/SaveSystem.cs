using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using NUnit.Framework.Constraints;
using UnityEngine.SceneManagement;
using Assets.Algorithm.BlackBoard;
namespace Assets.SavingSystem
{
    public static class SaveSystem
    {
        private const string EXTENSION = ".json";

        public static string SaveFileDirectory()
        {
            string path = Application.persistentDataPath + "/save/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        #region Save
        public static void Save()
        {
            string playerPath = SaveFileDirectory() + "Player" + EXTENSION;
            File.WriteAllText(playerPath, JsonUtility.ToJson(SavePlayer(), true));

            string enemiesPath = SaveFileDirectory() + "Enemies" + EXTENSION;

            File.WriteAllText(enemiesPath, JsonUtility.ToJson(SaveAllExistingEnemies(), true));

            string ChestsPath= SaveFileDirectory() + "Chests" + EXTENSION;

            File.WriteAllText(ChestsPath, JsonUtility.ToJson(SaveChests(), true));


            Debug.Log($"Game Saved Successfully at {SaveFileDirectory()}");
        }

        private static PlayerData SavePlayer()
        {

            PlayerManager player = UnityEngine.Object.FindFirstObjectByType<PlayerManager>();

            if (player != null)
            {
                PlayerData data = new PlayerData(player,GameManager.Instance.KeysCollected);
                return data;
            }
            return null;

        }

        private static WorldSaveData SaveAllExistingEnemies()
        {
            List<EnemyData> allEnemiesData = new List<EnemyData>();

            EnemyManager[] foundEnemies = UnityEngine.Object.FindObjectsByType<EnemyManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (EnemyManager enemy in foundEnemies)
            {
                EnemyData data = new EnemyData(enemy);

                allEnemiesData.Add(data);
            }

            return new WorldSaveData() { aliveEnemies = allEnemiesData, deadEnemyIDs=GameManager.Instance.DefeatedEnemyIDs };
        }
        private static ChestsData SaveChests()
        {
            return new ChestsData() { openedChestsIDs = GameManager.Instance.OpenedChestIDs };
        }
        #endregion

        #region Load
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            
            SceneManager.sceneLoaded -= OnSceneLoaded;

            Handle_Enemies();
            Handle_Player();
            Handle_Chests();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PostLoadRefresh();
            }
        }
        public static void Load(bool forceReloadScene = true,string SceneName = "")
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (forceReloadScene)
            {
                SceneManager.LoadScene(SceneName);
            }
        }
        private static void Handle_Enemies()
        {
            string EnemiesPath = SaveFileDirectory() + "Enemies" + EXTENSION;

            if (!File.Exists(EnemiesPath))// if theres nothing to load
            {
                return;
            }

            // Read the JSON back into our wrapper class
            string json = File.ReadAllText(EnemiesPath);
            WorldSaveData worldData = JsonUtility.FromJson<WorldSaveData>(json);

            // Load the Kill List into the Game Manager so it remembers for next time
            GameManager.Instance.DefeatedEnemyIDs = worldData.deadEnemyIDs;

            //find all the enemies in the scene
            EnemyManager[] allSceneEnemies = UnityEngine.Object.FindObjectsByType<EnemyManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (EnemyManager sceneEnemy in allSceneEnemies)
            {
                // Is this enemy supposed to be dead?
                if (worldData.deadEnemyIDs.Contains(sceneEnemy.InstanceID))
                {
                    if (GameBlackboard.Instance.ActiveEnemies.Contains(sceneEnemy))
                    {
                        GameBlackboard.Instance.ActiveEnemies.Remove(sceneEnemy);
                    }
                    UnityEngine.Object.Destroy(sceneEnemy.gameObject);
                    continue; 
                }

                EnemyData savedStats = worldData.aliveEnemies.Find(e => e.InstanceID == sceneEnemy.InstanceID);

                if (savedStats != null)
                {
                    sceneEnemy.ReloadEnemy(savedStats);
                }
            }

            Debug.Log("Game Progress Loaded Successfully!");
        }
        private static void Handle_Player()
        {

            PlayerData playerData = PeekSavedPlayerData();

            PlayerManager player= UnityEngine.Object.FindFirstObjectByType<PlayerManager>();
            if (player != null)
            {
                player.ReloadPlayer(playerData);
            }


        }
        private static void Handle_Chests()
        {
            string ChestsPath = SaveFileDirectory() + "Chests" + EXTENSION;
            string json = File.ReadAllText(ChestsPath);
            ChestsData chests=JsonUtility.FromJson<ChestsData>(json);

            GameManager.Instance.OpenedChestIDs = chests.openedChestsIDs;

        }
        public static PlayerData PeekSavedPlayerData()
        {
            string playerPath = SaveFileDirectory() + "Player" + EXTENSION;

            if (!File.Exists(playerPath))
            {
                return null; // No save file exists
            }

            // Read the JSON directly and return the data object
            string json = File.ReadAllText(playerPath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        #endregion
    }
}
