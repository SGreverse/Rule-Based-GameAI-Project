using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace Assets.Algorithm.Map
{
    public class MapRoom
    {
        public Vector2Int RoomIndex { get; private set; }

        public MapNode[,] NodeMatrix { get; private set; }


        public MapRoom(Vector2Int index, int roomWidthInTiles, int roomHeightInTiles)
        {
            RoomIndex = index;

            NodeMatrix = new MapNode[roomWidthInTiles, roomHeightInTiles];
        }


        public void InitializeRoom(Tilemap obstacleMap,Tilemap floorMap, Tilemap waterMap, float tileSize)
        {
            int width = NodeMatrix.GetLength(0);
            int height = NodeMatrix.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //his global index
                    Vector2Int worldGridPos = new Vector2Int(
                        (RoomIndex.x * width) + x,
                        (RoomIndex.y * height) + y
                    );

                    //coordinates of the middle of the tile in the actual world 
                    Vector3 trueWorldPos = new Vector3(
                        (worldGridPos.x * tileSize) + (tileSize / 2f),
                        (worldGridPos.y * tileSize) + (tileSize / 2f),
                        0f
                    );

                    // find the index of the world position in one of the layers
                    Vector3Int cellPosition = obstacleMap.WorldToCell(trueWorldPos);
                    bool isWalkable = !obstacleMap.HasTile(cellPosition) && floorMap.HasTile(cellPosition);
                    float weight = 1.0f;

                    if (waterMap != null && waterMap.HasTile(cellPosition))
                    {
                        weight = 2.0f; // Double the pathfinding cost,because its half as fast to walk on the floor then of the 
                    }
                    NodeMatrix[x, y] = new MapNode(isWalkable, weight, worldGridPos);// walk weight will be changes
                }
            }
        }

        //get node data from the global index
        public MapNode GetNode(Vector2Int Globalindex)
        {
            Vector2Int Offsetindex = GetLocalOffset(Globalindex,this.RoomIndex);
            if(Offsetindex.x<0 || Offsetindex.y < 0 || Offsetindex.x>= NodeMatrix.GetLength(0) || Offsetindex.y>= NodeMatrix.GetLength(1))
            {
                return null;
            }
            return this.NodeMatrix[Offsetindex.x,Offsetindex.y];
        }

        /// Converts a Global Grid Index into a Local offset for the Room's NodeMatrix.
        /// the formula used is L=G-(R*W\H)
        /// where:
        /// L-local index,G-Global index,R-room index,W\H-room Width\Height
        public Vector2Int GetLocalOffset(Vector2Int globalIndex, Vector2Int roomIndex)
        {
            int localX = globalIndex.x - (roomIndex.x * NodeMatrix.GetLength(0));
            int localY = globalIndex.y - (roomIndex.y * NodeMatrix.GetLength(1));

            return new Vector2Int(localX, localY);
        }
        public List<EnemyManager> DetectEnemiesInRoom(LayerMask enemyLayerMask, float tileSize)
        {
            List<EnemyManager> result = new List<EnemyManager>();
            int width = NodeMatrix.GetLength(0);
            int height = NodeMatrix.GetLength(1);

            Vector2 bottomLeft = new Vector2(RoomIndex.x * width * tileSize, RoomIndex.y * height * tileSize);
            Vector2 topRight = new Vector2((RoomIndex.x + 1) * width * tileSize, (RoomIndex.y + 1) * height * tileSize);

            Collider2D[] hitColliders = Physics2D.OverlapAreaAll(bottomLeft, topRight, enemyLayerMask);

            foreach (Collider2D hit in hitColliders)
            {
                EnemyManager enemy = hit.GetComponent<EnemyManager>();

                result.Add(enemy);
             
            }
            return result;
        }
    }
}
