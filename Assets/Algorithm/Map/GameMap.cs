using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.HashDataStructers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Algorithm.Map
{
    public class GameMap : IEnumerable<HashDataStructers.KeyValuePair<Vector2Int, MapRoom>>
    {
        //direction vectors
        private static readonly int[] Xdir = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private static readonly int[] Ydir = { 1, 1, 0, -1, -1, -1, 0, 1 };

        private LayerMask _enemiesLayer;
        private GameHashMap<Vector2Int, MapRoom> _activeChunkDictionary;
        public int roomWidthInTiles;
        public int roomHeightInTiles;
        public GameMap(Vector2Int startingRoomIndex, int roomWidth, int roomHeight, Tilemap Obstacles,Tilemap floor, Tilemap water, LayerMask _enemiesLayer)
        {
            this._activeChunkDictionary = new GameHashMap<Vector2Int, MapRoom>();
            this.roomHeightInTiles = roomHeight;
            this.roomWidthInTiles = roomWidth;
            this._enemiesLayer = _enemiesLayer;
            UpdateActiveRooms(startingRoomIndex, Obstacles,floor,water);
        }

        public void UpdateActiveRooms(Vector2Int centerRoom, Tilemap obstacleMap,Tilemap floorMap, Tilemap waterMap)
        {
            GameHashSet<Vector2Int> roomsToKeep = new GameHashSet<Vector2Int>();

            // Load the room the player is in, and all neighboring rooms in different directions.
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int neighborIndex = new Vector2Int(centerRoom.x + x, centerRoom.y + y);
                    roomsToKeep.Add(neighborIndex);

                    if (!this._activeChunkDictionary.ContainsKey(neighborIndex))
                    {
                        LoadRoom(neighborIndex, obstacleMap,floorMap, waterMap,_enemiesLayer);

                    }
                }
            }

            List<Vector2Int> roomsToUnload = new List<Vector2Int>();
            foreach (var kvp in this._activeChunkDictionary)
            {
                if (!roomsToKeep.Contains(kvp.Key))
                {
                    roomsToUnload.Add(kvp.Key);
                }
            }

            foreach (var oldRoomIndex in roomsToUnload)
            {
                UnloadRoom(oldRoomIndex,_enemiesLayer);
            }
        }
        private void LoadRoom(Vector2Int index, Tilemap obstacleMap,Tilemap floorMap, Tilemap waterMap, LayerMask enemiesMask)
        {
            MapRoom newRoom = new MapRoom(index, roomWidthInTiles, roomHeightInTiles);
            newRoom.InitializeRoom(obstacleMap,floorMap,waterMap, 1);

            List<EnemyManager> enemies=newRoom.DetectEnemiesInRoom(enemiesMask, 1);
            foreach (EnemyManager enemy in enemies)
            {
                if (enemy != null)
                {
                    GameBlackboard.Instance.ActivateEnemy(enemy);
                }
            }

            this._activeChunkDictionary[index] = newRoom;
        }
        private void UnloadRoom(Vector2Int index, LayerMask enemiesMask)
        {
            MapRoom room = this._activeChunkDictionary[index];
            List<EnemyManager> enemies = room.DetectEnemiesInRoom(enemiesMask, 1);
            foreach (EnemyManager enemy in enemies)
            {
                if (enemy != null)
                {
                    GameBlackboard.Instance.KickEnemy(enemy);
                }
            
            }
            this._activeChunkDictionary.Remove(index);

        }
        public bool Contains(Vector2Int index)
        {
            return this._activeChunkDictionary.ContainsKey(index);
        }

        public IEnumerator<HashDataStructers.KeyValuePair<Vector2Int, MapRoom>> GetEnumerator()
        {
            return this._activeChunkDictionary.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public List<MapNode> GetNeighbors(Vector2Int GlobalIndex)
        {
            List<MapNode> neighbors = new List<MapNode>(8);

            for (int i = 0; i < 8; i++)
            {
                int neighborGlobalX = GlobalIndex.x + Xdir[i];
                int neighborGlobalY = GlobalIndex.y + Ydir[i];
                Vector2Int neighborGlobalIndex = new Vector2Int(neighborGlobalX, neighborGlobalY);

                bool isValidNeighbor = true;

                MapNode node = GetNodeDataByGlobalIndex(neighborGlobalIndex);

                if (node == null || !node.IsWalkable) isValidNeighbor = false;

                bool isDiagonal = (Xdir[i] != 0 && Ydir[i] != 0);

                if (isValidNeighbor && isDiagonal)// cannot walk diagonally when blocked from atleast one side.
                {
                    Vector2Int ortho1 = new Vector2Int(GlobalIndex.x + Xdir[i], GlobalIndex.y);
                    Vector2Int ortho2 = new Vector2Int(GlobalIndex.x, GlobalIndex.y + Ydir[i]);

                    MapNode node1 = GetNodeDataByGlobalIndex(ortho1);
                    MapNode node2 = GetNodeDataByGlobalIndex(ortho2);


                    if (node1 == null || node2 == null || !node1.IsWalkable || !node2.IsWalkable)
                    {
                        isValidNeighbor = false;
                    }
                }
                if (isValidNeighbor)
                {
                    neighbors.Add(node);
                }
            }

            return neighbors;
        }
        public MapNode GetNodeDataByGlobalIndex(Vector2Int GlobalIndex)
        {
            int roomX = Mathf.FloorToInt((float)GlobalIndex.x / this.roomWidthInTiles);
            int roomY = Mathf.FloorToInt((float)GlobalIndex.y / this.roomHeightInTiles);
            Vector2Int roomIndex = new Vector2Int(roomX, roomY);


            if (this._activeChunkDictionary.TryGetValue(roomIndex, out MapRoom room))
            {
                return room.GetNode(GlobalIndex);
            }

            return null;
        }

        public Vector2Int GetGlobalIndexFromWorldPosition(Vector3 worldPos)
        {
            float tileSize = GameManager.Instance.tileSize;
            int x = Mathf.FloorToInt(worldPos.x / tileSize);
            int y = Mathf.FloorToInt(worldPos.y / tileSize);
            return new Vector2Int(x, y);
        }
        public Vector2 GetWorldPositionFromGlobalIndex(Vector2Int globalIndex)
        {
            float tileSize = GameManager.Instance.tileSize;

            float worldX = (globalIndex.x * tileSize) + (tileSize / 2f);
            float worldY = (globalIndex.y * tileSize) + (tileSize / 2f);

            return new Vector3(worldX, worldY, 0f);
        }
        public bool IsTileEmpty(Vector2 worldPos)
        {
            Vector2Int index=GetGlobalIndexFromWorldPosition(worldPos);
            MapNode node= GetNodeDataByGlobalIndex(index);
            return node != null && node.IsWalkable;
        }
    }
}
