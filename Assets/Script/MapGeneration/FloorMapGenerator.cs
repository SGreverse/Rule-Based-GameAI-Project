using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MapGeneration
{

    public class FloorMapGenerator : MonoBehaviour
    {
        [Header("Tilemap")]
        public Tilemap floorMap;
        public Tilemap decorationMap;

        [Header("Decorations")]
        public TileBase[] grassTufts; // NEW: Array to hold your different grass sprites
        [Range(0f, 1f)]
        public float tuftDensity = 0.15f; // NEW: 15% chance to spawn a tuft on a grass tile

        [Header("Grass Tiles (from TX Tileset Grass)")]
        [Tooltip("TX Tileset Grass — brightest green base tile (Row 1, Col 1-2)")]
        public TileBase grassLight;

        [Tooltip("TX Tileset Grass — darker green variant (Row 1, Col 3-4)")]
        public TileBase grassDark;

        [Tooltip("TX Tileset Grass — brown dirt/path tile (Row 3-4)")]
        public TileBase dirtPath;

        [Header("Stone Tiles (from TX Tileset Stone Ground)")]
        [Tooltip("TX Tileset Stone Ground — clean grey flagstone (Row 1, Col 1-2)")]
        public TileBase stoneClean;

        [Tooltip("TX Tileset Stone Ground — stone with moss growing through (Row 2-3)")]
        public TileBase stoneMossy;

        [Header("Generation Settings")]
        public int seed = 42;
        [Range(0.02f, 0.15f)]
        public float noiseScale = 0.1f;

        private System.Random _rng;

        public void Generate(MapLayoutData layoutData)
        {
            _rng = new System.Random(seed);
            floorMap.ClearAllTiles();
            if (decorationMap != null) decorationMap.ClearAllTiles();

            float ox = (float)_rng.NextDouble() * 1000f;
            float oy = (float)_rng.NextDouble() * 1000f;

            int w = layoutData.mapSize.width;
            int h = layoutData.mapSize.height;
            int border = layoutData.borderThickness;

            // STEP 1: Draw the base floor
            for (int x = border; x < w - border; x++)
            {
                for (int y = border; y < h - border; y++)
                {
                    ZoneData zone = FindZone(x, y, layoutData);
                    if (zone == null) continue;

                    float noise = Mathf.PerlinNoise((x + ox) * noiseScale, (y + oy) * noiseScale);
                    float rand = (float)_rng.NextDouble();
                    float blend = noise * 0.55f + rand * 0.45f;

                    TileBase tile = PickFloorTile(zone.floorWeights, blend);
                    floorMap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }

            // STEP 2: Draw the dirt paths (This will overwrite the grass where the paths go)
            PlaceDirtPaths(layoutData);

            // STEP 3: Draw the grass tufts on the decoration layer
            if (decorationMap != null && grassTufts != null && grassTufts.Length > 0)
            {
                for (int x = border; x < w - border; x++)
                {
                    for (int y = border; y < h - border; y++)
                    {
                        // Look at what tile is ACTUALLY on the floor right now (so we don't put grass on dirt paths)
                        TileBase currentFloorTile = floorMap.GetTile(new Vector3Int(x, y, 0));

                        // Only spawn tufts on top of grass tiles!
                        if (currentFloorTile == grassLight || currentFloorTile == grassDark)
                        {
                            if (_rng.NextDouble() < tuftDensity) // Roll the dice
                            {
                                // Pick a random grass tuft from your array
                                TileBase randomTuft = grassTufts[_rng.Next(grassTufts.Length)];
                                decorationMap.SetTile(new Vector3Int(x, y, 0), randomTuft);
                            }
                        }
                    }
                }
            }
        }

        private ZoneData FindZone(int x, int y, MapLayoutData data)
        {
            // Arena zones override parent zones
            foreach (var z in data.zones)
            {
                if (!z.id.Contains("arena") && !z.id.Contains("gate")) continue;
                if (InBounds(x, y, z.bounds)) return z;
            }
            foreach (var z in data.zones)
            {
                if (z.id.Contains("arena") || z.id.Contains("gate")) continue;
                if (InBounds(x, y, z.bounds)) return z;
            }
            return null;
        }

        private bool InBounds(int x, int y, Bounds b)
        {
            return x >= b.xMin && x <= b.xMax && y >= b.yMin && y <= b.yMax;
        }

        private float EdgeDistance(int x, int y, Bounds b)
        {
            float dx = Mathf.Min(x - b.xMin, b.xMax - x);
            float dy = Mathf.Min(y - b.yMin, b.yMax - y);
            return Mathf.Min(dx, dy);
        }

        private TileBase PickFloorTile(FloorWeights w, float v)
        {
            float t = 0f;
            t += w.grassLight; if (v < t) return grassLight ?? grassDark;
            t += w.grassDark; if (v < t) return grassDark ?? grassLight;
            t += w.dirtPath; if (v < t) return dirtPath ?? grassLight;
            t += w.stoneClean; if (v < t) return stoneClean ?? stoneMossy;
            t += w.stoneMossy; if (v < t) return stoneMossy ?? stoneClean;
            return grassLight;
        }

        /// <summary>
        /// Lays natural-looking dirt paths connecting key landmarks.
        /// Uses Bresenham-style line with random width wobble.
        /// </summary>
        private void PlaceDirtPaths(MapLayoutData data)
        {
            if (dirtPath == null) return;
            var rng = new System.Random(seed + 99);

            var waypoints = new List<Vector2Int>
            {
                new Vector2Int(data.fixedElements.spawnPoint.x, data.fixedElements.spawnPoint.y),
                new Vector2Int(50, 20), // south approach
                new Vector2Int(data.fixedElements.keys[1].x, data.fixedElements.keys[1].y), // south key
                new Vector2Int(50, 20),
                new Vector2Int(20, 35), // west approach
                new Vector2Int(data.fixedElements.keys[0].x, data.fixedElements.keys[0].y), // west key
                new Vector2Int(20, 50),
                new Vector2Int(50, 50), // center
                new Vector2Int(80, 50), // east approach
                new Vector2Int(data.fixedElements.keys[2].x, data.fixedElements.keys[2].y), // east key
                new Vector2Int(80, 50),
                new Vector2Int(50, 50),
                new Vector2Int(50, 80), // north approach
                new Vector2Int(data.fixedElements.bossDoor.x, data.fixedElements.bossDoor.y),
            };

            for (int i = 0; i < waypoints.Count - 1; i++)
                DrawDirtPath(waypoints[i], waypoints[i + 1], rng);
        }

        private void DrawDirtPath(Vector2Int a, Vector2Int b, System.Random rng)
        {
            int steps = Mathf.Max(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
            if (steps == 0) return;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int cx = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, t));
                int cy = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, t));
                int width = rng.Next(1, 3); // 1-2 tile wide path

                for (int dx = -width; dx <= width; dx++)
                    for (int dy = -width; dy <= width; dy++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) > width) continue;
                        floorMap.SetTile(new Vector3Int(cx + dx, cy + dy, 0), dirtPath);
                    }
            }
        }
    }

    // ─── Data model ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class MapLayoutData
    {
        public MapSize mapSize;
        public int borderThickness;
        public FixedElements fixedElements;
        public List<ZoneData> zones;
        public RiverData river;
        public List<BridgeData> bridges;
        public List<ArenaData> arenas;
        public List<ChokePointData> chokePoints;
        public CentralCoverData centralCover;
        public NorthernApproachCoverData northernApproachCover;
        public List<Position> scatteredTrees;
        public EncounterData encounters;
    }

    [System.Serializable] public class MapSize { public int width; public int height; }
    [System.Serializable] public class Position { public int x; public int y; }
    [System.Serializable] public class Bounds { public int xMin; public int xMax; public int yMin; public int yMax; }
    [System.Serializable] public class FixedElements { public Position spawnPoint; public List<KeyData> keys; public Position bossDoor; }
    [System.Serializable] public class KeyData { public string id; public int x; public int y; public string label; }

    [System.Serializable]
    public class ZoneData
    {
        public string id;
        public string name;
        public Bounds bounds;
        public FloorWeights floorWeights;
        public string character;
    }

    [System.Serializable]
    public class FloorWeights
    {
        public float grassLight;
        public float grassDark;
        public float dirtPath;
        public float stoneClean;
        public float stoneMossy;
    }

    [System.Serializable]
    public class RiverData
    {
        public ChannelData mainChannel;
        public BranchData branch;
        public List<PondData> ponds;
    }

    [System.Serializable]
    public class ChannelData
    {
        public string id;
        public string name;
        public List<ChannelSegment> segments;
    }

    [System.Serializable]
    public class ChannelSegment
    {
        public int xMin; public int xMax; public int yMin; public int yMax;
        public string depth;
    }

    [System.Serializable]
    public class BranchData
    {
        public string id; public string type;
        public Position start; public Position end;
        public int width; public string depth;
    }

    [System.Serializable]
    public class PondData
    {
        public string id;
        public int xMin; public int xMax; public int yMin; public int yMax;
        public string depth;
    }

    [System.Serializable]
    public class BridgeData
    {
        public string id; public string name;
        public int xMin; public int xMax; public int yMin; public int yMax;
        public int passableWidth; public string character;
    }

    [System.Serializable]
    public class ArenaData
    {
        public string id; public string name;
        public Bounds outerBoundary;
        public List<EntranceData> entrances;
    }

    [System.Serializable]
    public class EntranceData
    {
        public string side;
        public int xMin; public int xMax; public int yMin; public int yMax;
        public int x; public int y;
        public bool Issealed;
    }

    [System.Serializable]
    public class ChokePointData
    {
        public string id;
        public Bounds location;
        public List<ObstacleData> obstacles;
        public int passageWidth;
    }

    [System.Serializable]
    public class ObstacleData
    {
        public int xMin; public int xMax; public int yMin; public int yMax;
        public string type;
    }

    [System.Serializable]
    public class CentralCoverData
    {
        public List<Position> ruinPillars;
        public List<Bounds> rockClusters;
        public List<Bounds> ruinWallFragments;
    }

    [System.Serializable]
    public class NorthernApproachCoverData
    {
        public List<CoverRow> rows;
    }

    [System.Serializable]
    public class CoverRow
    {
        public int y;
        public List<Bounds> barriers;
    }

    [System.Serializable]
    public class EncounterData
    {
        public List<RestZone> restZones;
        public List<RewardData> rewards;
    }

    [System.Serializable]
    public class RestZone
    {
        public string id;
        public int xMin; public int xMax; public int yMin; public int yMax;
    }

    [System.Serializable]
    public class RewardData
    {
        public string id; public int x; public int y; public string type;
    }
}
