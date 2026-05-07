using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapGeneration
{

    public class LevelBuilder : MonoBehaviour
    {
        [Header("Map Data")]
        public TextAsset mapDataJson;

        [Header("Layer Generators")]
        public FloorMapGenerator floorGenerator;
        public WaterMapGenerator waterGenerator;
        public WallMapGenerator  wallGenerator;

        [Header("Tilemaps (auto-detected if null)")]
        public Tilemap floorMap;
        public Tilemap waterMap;
        public Tilemap wallMap;

        private MapLayoutData _data;

        public void BuildMap()
        {
            if (!LoadData()) return;
            AutoDetect();
            Assign();

            Debug.Log("[LevelBuilder] Building Floor…");
            floorGenerator.Generate(_data);

            Debug.Log("[LevelBuilder] Building Water…");
            waterGenerator.Generate(_data);

            Debug.Log("[LevelBuilder] Building Walls…");
            wallGenerator.Generate(_data);

            Debug.Log($"[LevelBuilder] Done — {_data.mapSize.width}x{_data.mapSize.height} map.");
        }

        public void ClearMap()
        {
            AutoDetect();
            floorMap?.ClearAllTiles();
            waterMap?.ClearAllTiles();
            wallMap?.ClearAllTiles();
            Debug.Log("[LevelBuilder] Cleared.");
        }

        private bool LoadData()
        {
            if (mapDataJson == null) { Debug.LogError("[LevelBuilder] No JSON assigned."); return false; }
            _data = JsonUtility.FromJson<MapLayoutData>(mapDataJson.text);
            if (_data == null) { Debug.LogError("[LevelBuilder] JSON parse failed."); return false; }
            Debug.Log($"[LevelBuilder] Loaded: {_data.zones.Count} zones, {_data.bridges.Count} bridges, {_data.arenas.Count} arenas.");
            return true;
        }

        private void AutoDetect()
        {
            if (!floorMap) floorMap = FindChild("FloorMap");
            if (!waterMap) waterMap = FindChild("WaterMap");
            if (!wallMap)  wallMap  = FindChild("WallMap");
        }

        private Tilemap FindChild(string n)
        {
            var c = transform.Find(n);
            return c ? c.GetComponent<Tilemap>() : null;
        }

        private void Assign()
        {
            if (floorGenerator) floorGenerator.floorMap = floorMap;
            if (waterGenerator) waterGenerator.waterMap  = waterMap;
            if (wallGenerator)  wallGenerator.wallMap    = wallMap;
        }
    }
}
