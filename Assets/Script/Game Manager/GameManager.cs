using System;
using System.Collections.Generic;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.HashDataStructers;
using Assets.Algorithm.Map;
using Assets.Data;
using Assets.Data.StatScriptables;
using Assets.SavingSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.VolumeComponent;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [Header("Current Scene")]
    [SerializeField] private string _currentSceneName;

    [Header("References")]

    [Tooltip("Drag the Player GameObject here in the Unity Inspector")]
    [SerializeField] private PlayerManager player;

    [Tooltip("Drag the Unity Tilemap containing your walls/obstacles here")]
    [SerializeField] public Tilemap obstacleTilemap;
    [SerializeField] public Tilemap floorTilemap;
    [SerializeField] public Tilemap waterTilemap;

    [Header("Room Settings")]
    public float tileSize = 1f;
    public int roomWidthInTiles;
    public int roomHeightInTiles;
    public Vector2Int currentPlayerRoomIndex;
    public LayerMask EnemyMask;

    [Header("Path Finding Settings")]
    public MapfConfiguration Config;

    [HideInInspector]
    public GameMap Map;

    [Header("All Items")]
    public ItemDatabase DataBase;

    [Header("Game State")]
    public int KeysCollected = 0;
    public readonly int TotalKeysRequired = 3;

    public List<string> DefeatedEnemyIDs = new List<string>();

    public List<string> OpenedChestIDs = new List<string>();

    // Event to update UI when a key is picked up
    public event Action OnKeyCollected;


    void Awake()
    {

        Instance = this;
        Physics2D.SyncTransforms();
        this._currentSceneName = SceneManager.GetActiveScene().name;

        // 2. Self-Healing: Secure the Player immediately BEFORE calculating map logic!
        if (player == null)
        {
            PlayerManager[] allPlayers = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
            foreach (PlayerManager p in allPlayers)
            {
                if (p.gameObject.scene == this.gameObject.scene) player = p;
            }
        }

        // if we load a saved state
        if (PlayerPrefs.GetInt("LoadFromSave", 0) == 1)
        {
            // Reset the flag immediately so we don't accidentally load again if the player dies
            PlayerPrefs.SetInt("LoadFromSave", 0);
            PlayerPrefs.Save();

            // Get the player position from the file
            PlayerData savedData = SaveSystem.PeekSavedPlayerData();
            if (savedData != null)
            {
                int indexX = Mathf.FloorToInt(savedData.Position.x / (roomWidthInTiles * tileSize));
                int indexY = Mathf.FloorToInt(savedData.Position.y / (roomHeightInTiles * tileSize));
                currentPlayerRoomIndex = new Vector2Int(indexX, indexY);
            }

            // Load the last save without reloading the scene
            LoadLastSave(false);

            // Force physics to update so UpdateMap doesn't use ghost coordinates
            Physics2D.SyncTransforms();
        }
        else // If we're starting a new game
        {
            if (player != null)
            {
                int indexX = Mathf.FloorToInt(player.transform.position.x / (roomWidthInTiles * tileSize));
                int indexY = Mathf.FloorToInt(player.transform.position.y / (roomHeightInTiles * tileSize));
                currentPlayerRoomIndex = new Vector2Int(indexX, indexY);
            }
        }

    }

    // can only be run once while playing to generate the map
    private void Start()
    {
        this.Map = new GameMap(currentPlayerRoomIndex, roomWidthInTiles, roomHeightInTiles, this.obstacleTilemap, this.floorTilemap,this.waterTilemap, EnemyMask);
    }
    void Update()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerManager>();
        }
        if(this.obstacleTilemap == null || this.floorTilemap == null)
        {
            Grid grid = FindAnyObjectByType<Grid>();
            grid.GetComponent<MapInitializer>().Initalize();
        }
        if (player != null && this.obstacleTilemap != null && this.floorTilemap != null)
        {
            UpdateMap();
        }
    }

    public void UpdateMap()
    {
        // Calculate the indices using math.
        int indexX = Mathf.FloorToInt(player.transform.position.x / (this.Map.roomWidthInTiles * tileSize));
        int indexY = Mathf.FloorToInt(player.transform.position.y / (this.Map.roomHeightInTiles * tileSize));

        Vector2Int newRoomIndex = new Vector2Int(indexX, indexY);

        //If the player is in a different scene,Re-initialize the map
        if(_currentSceneName!= SceneManager.GetActiveScene().name)
        {
            this._currentSceneName= SceneManager.GetActiveScene().name;
            currentPlayerRoomIndex = newRoomIndex;
            this.Map = new GameMap(currentPlayerRoomIndex, roomWidthInTiles, roomHeightInTiles,this.obstacleTilemap, this.floorTilemap,this.waterTilemap, EnemyMask);
        }
        // Update only if the player moved to a new room.
        else if (newRoomIndex != currentPlayerRoomIndex)
        {
            currentPlayerRoomIndex = newRoomIndex;
            this.Map.UpdateActiveRooms(currentPlayerRoomIndex,this.obstacleTilemap,this.floorTilemap,this.waterTilemap);
        }
    }

    public void RegisterDeadEnemy(string enemyID)
    {
        if (!DefeatedEnemyIDs.Contains(enemyID))
        {
            DefeatedEnemyIDs.Add(enemyID);
        }
    }
    public void RegisterOpenedChest(string chestID)
    {
        if (!OpenedChestIDs.Contains(chestID))
        {
            OpenedChestIDs.Add(chestID);
        }
    }
    private void OnDrawGizmos()//draw the map outlines
    {
        if (this.Map == null) return;

        float chunkWorldWidth = this.Map.roomWidthInTiles * tileSize;
        float chunkWorldHeight = this.Map.roomHeightInTiles * tileSize;
        Vector3 chunkSize = new Vector3(chunkWorldWidth, chunkWorldHeight, 0.1f);

        foreach (var kvp in this.Map)
        {
            Vector2Int roomIndex = kvp.Key;

            float startX = roomIndex.x * chunkWorldWidth;
            float startY = roomIndex.y * chunkWorldHeight;

            Vector3 centerPosition = new Vector3(
                startX + (chunkWorldWidth / 2f),
                startY + (chunkWorldHeight / 2f),
                0f
            );

            // 3. Color code our chunks! 
            // Green for the chunk the player is in, Yellow for the neighbors.
            if (roomIndex == currentPlayerRoomIndex)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            // 4. Draw the outline of the chunk
            Gizmos.DrawWireCube(centerPosition, chunkSize);
        }
    }



    #region Events

    public void AddKey()
    {
        KeysCollected++;

        // Trigger event
        OnKeyCollected?.Invoke();

        if (KeysCollected >= TotalKeysRequired)
        {
            UnlockBossDoor();
        }
    }
    private void UnlockBossDoor()
    {
        Debug.Log("All keys collected! The path to the Boss is open.");

        BossDoor door= FindFirstObjectByType<BossDoor>();
        door.OpenDoor();
    }

    public void ShowDeathScreen()
    {
        GetComponent<GameOverScript>().OnPlayerDeath();
        
    }
    #endregion

    #region Items
    public ItemData GetItemByID(string id)
    {
        return DataBase.GetItemByID(id);
    }
    #endregion 

    #region Saving And Loading
    public void LoadLastSave(bool forceReloadScene = true,string SceneName="MainGameScene")
    {

        if (GameBlackboard.Instance != null)
        {
            GameBlackboard.Instance.ResetBlackboard();
        }
        KeysCollected = 0;
        BossDoor door = FindFirstObjectByType<BossDoor>();
        door.LockDoor();
        SaveSystem.Load(forceReloadScene,SceneName);

    }

    //insure that after reloading all enemies and the player,we reload the map
    public void PostLoadRefresh()
    {
        if (this.obstacleTilemap != null && this.floorTilemap != null)
        {
            this.Map = new GameMap(currentPlayerRoomIndex, roomWidthInTiles, roomHeightInTiles, this.obstacleTilemap, this.floorTilemap,this.waterTilemap, EnemyMask);
        }
    }

    #endregion


}
