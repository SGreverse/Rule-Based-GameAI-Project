using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MapGeneration
{

    public class WallMapGenerator : MonoBehaviour
    {
        [Header("Tilemap")]
        public Tilemap wallMap;
        private Tilemap _waterMapRef; // NEW: A reference to the actual water map

        [Header("Wall Tiles (from TX Tileset Wall Top)")]
        [Tooltip("TX Tileset Wall Top — grey stone wall tile (rows 1-4)")]
        public TileBase ruinWall;

        [Tooltip("TX Tileset Wall Top — clean intact wall variant")]
        public TileBase wallClean;

        [Header("Nature Obstacles (from TX Props)")]
        [Tooltip("TX Props — large boulder sprite")]
        public TileBase rockLarge;

        [Tooltip("TX Props — small rock/pebble sprite")]
        public TileBase rockSmall;

        [Tooltip("TX Props — green bush sprite")]
        public TileBase bush;

        [Header("Trees")]
        [Tooltip("Tree trunk sprite (any of the 3 Cainos trees)")]
        public TileBase treeTrunk;

        [Header("Pillars / Standing Stones")]
        [Tooltip("Single wall-top tile used as ruin pillar, or tall rock prop")]
        public TileBase pillar;


        // ... Keep your TileBase variables exactly as they are ...

        public void Generate(MapLayoutData layoutData)
        {
            // NEW: Automatically find the WaterMap so we can check it
            var waterObj = GameObject.Find("WaterMap");
            if (waterObj != null) _waterMapRef = waterObj.GetComponent<Tilemap>();

            wallMap.ClearAllTiles();
            PlaceBorderWalls(layoutData);
            PlaceArenas(layoutData);
            PlaceChokePoints(layoutData);
            PlaceCentralCover(layoutData);
            PlaceNorthernApproachCover(layoutData);
            PlaceScatteredTrees(layoutData);
        }

        // ── Border ────────────────────────────────────────────────────────

        private void PlaceBorderWalls(MapLayoutData d)
        {
            int w = d.mapSize.width, h = d.mapSize.height, t = d.borderThickness;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (x >= t && x < w - t && y >= t && y < h - t) continue;
                // Leave gaps where river exits the map
                if (IsRiverBorderGap(x, y, d)) continue;
                wallMap.SetTile(new Vector3Int(x, y, 0), ruinWall);
            }
        }

        private bool IsRiverBorderGap(int x, int y, MapLayoutData d)
        {
            int t = d.borderThickness, w = d.mapSize.width, h = d.mapSize.height;
            bool onBorder = x < t || x >= w - t || y < t || y >= h - t;
            if (!onBorder) return false;

            // Leave gaps at the top and bottom of the map for the new Twin Rivers
            if (x >= 25 && x <= 29 && (y < t || y >= h - t)) return true;
            if (x >= 71 && x <= 75 && (y < t || y >= h - t)) return true;

            return false;
        }

        // ── Arenas ────────────────────────────────────────────────────────

        private void PlaceArenas(MapLayoutData d)
        {
            foreach (var arena in d.arenas)
            {
                PlaceArenaOuterWall(arena);

                switch (arena.id)
                {
                    case "west_ancient_grove":  PlaceAncientGrove(d);  break;
                    case "south_sunken_garden": PlaceSunkenGarden(d); break;
                    case "east_stone_circle":   PlaceStoneCircle(d);  break;
                }
            }
        }

        private void PlaceArenaOuterWall(ArenaData arena)
        {
            var skip = BuildEntranceSet(arena.entrances);
            var b = arena.outerBoundary;

            // Use bush/rock mix for natural arena boundaries
            TileBase borderTile = ruinWall;

            for (int x = b.xMin; x <= b.xMax; x++)
            {
                TryPlace(x, b.yMin, borderTile, skip);
                TryPlace(x, b.yMax, borderTile, skip);
            }
            for (int y = b.yMin; y <= b.yMax; y++)
            {
                TryPlace(b.xMin, y, borderTile, skip);
                TryPlace(b.xMax, y, borderTile, skip);
            }
        }

        // ── West Arena: The Ancient Grove ─────────────────────────────────

        private void PlaceAncientGrove(MapLayoutData d)
        {
            // Massive center tree
            Place(10, 50, treeTrunk ?? rockLarge);
            Place(10, 51, treeTrunk ?? rockLarge);
            Place(11, 50, treeTrunk ?? rockLarge);
            Place(11, 51, treeTrunk ?? rockLarge);

            // Stone ring around tree (semi-circle of rocks)
            int cx = 10, cy = 50, r = 4;
            for (int angle = 0; angle < 360; angle += 45)
            {
                float rad = angle * Mathf.Deg2Rad;
                int rx = cx + Mathf.RoundToInt(Mathf.Cos(rad) * r);
                int ry = cy + Mathf.RoundToInt(Mathf.Sin(rad) * r);
                Place(rx, ry, rockSmall ?? rockLarge);
            }

            // Scattered rocks for cover
            int[][] rocks = { new[]{5,44}, new[]{14,44}, new[]{5,56}, new[]{14,56}, new[]{4,50}, new[]{16,50} };
            foreach (var p in rocks)
                Place(p[0], p[1], rockLarge ?? ruinWall);
        }

        // ── South Arena: The Sunken Garden ────────────────────────────────

        private void PlaceSunkenGarden(MapLayoutData d)
        {
            // 6 ruin pillars in 2×3 grid
            int[][] pillars = { new[]{44,7}, new[]{44,13}, new[]{50,7}, new[]{50,13}, new[]{56,7}, new[]{56,13} };
            foreach (var p in pillars)
                Place(p[0], p[1], pillar ?? ruinWall);

            // Broken wall fragments between pillars for cover
            Fill(45, 49, 10, 10, ruinWall);
            Fill(51, 55, 10, 10, ruinWall);
        }

        // ── East Arena: The Stone Circle ──────────────────────────────────

        private void PlaceStoneCircle(MapLayoutData d)
        {
            // Standing stones arranged in a circle
            int[][] stones = { new[]{88,45}, new[]{95,45}, new[]{87,50}, new[]{96,50}, new[]{88,55}, new[]{95,55} };
            foreach (var p in stones)
            {
                Place(p[0], p[1], pillar ?? rockLarge);
                // Add a small rock next to each standing stone for cover
                Place(p[0] + 1, p[1], rockSmall ?? bush);
            }

            // Center altar (ruins)
            Fill(90, 93, 49, 51, ruinWall);
        }

        // ── Choke Points ──────────────────────────────────────────────────

        private void PlaceChokePoints(MapLayoutData d)
        {
            foreach (var choke in d.chokePoints)
            foreach (var obs in choke.obstacles)
            {
                TileBase tile = GetObstacleTile(obs.type);
                Fill(obs.xMin, obs.xMax, obs.yMin, obs.yMax, tile);
            }
        }

        private TileBase GetObstacleTile(string type)
        {
            switch (type)
            {
                case "rock":      return rockLarge ?? ruinWall;
                case "ruin_wall": return ruinWall ?? rockLarge;
                case "trees":     return treeTrunk ?? bush;
                case "pillar":    return pillar ?? ruinWall;
                default:          return ruinWall;
            }
        }

        // ── Central Cover ─────────────────────────────────────────────────

        private void PlaceCentralCover(MapLayoutData d)
        {
            if (d.centralCover == null) return;

            foreach (var p in d.centralCover.ruinPillars)
            {
                if (InWater(p.x, p.y, d)) continue;
                Place(p.x, p.y, pillar ?? ruinWall);
            }

            foreach (var c in d.centralCover.rockClusters)
                Fill(c.xMin, c.xMax, c.yMin, c.yMax, rockLarge ?? ruinWall);

            foreach (var w in d.centralCover.ruinWallFragments)
                Fill(w.xMin, w.xMax, w.yMin, w.yMax, ruinWall);
        }

        private bool InWater(int x, int y, MapLayoutData d)
        {
            // PERFECT WATER CHECK: Actually look at the Water Tilemap!
            if (_waterMapRef != null)
            {
                if (_waterMapRef.GetTile(new Vector3Int(x, y, 0)) != null)
                    return true;
            }
            return false;
        }

        // ── Northern Approach ─────────────────────────────────────────────

        private void PlaceNorthernApproachCover(MapLayoutData d)
        {
            if (d.northernApproachCover == null) return;
            foreach (var row in d.northernApproachCover.rows)
            foreach (var b in row.barriers)
            {
                // Alternate between ruin walls and rocks for visual variety
                TileBase tile = (row.y % 2 == 0) ? ruinWall : rockLarge ?? ruinWall;
                Fill(b.xMin, b.xMax, row.y, row.y + 1, tile);
            }
        }

        // ── Scattered Trees ───────────────────────────────────────────────

        private void PlaceScatteredTrees(MapLayoutData d)
        {
            if (d.scatteredTrees == null || treeTrunk == null) return;
            var rng = new System.Random(12345); // Keep it deterministic

            foreach (var t in d.scatteredTrees)
            {
                // Place the main tree
                TryPlaceTree(t.x, t.y, d);

                // Add 1 to 3 clustered trees around it for an organic forest look
                int clusterSize = rng.Next(1, 4);
                for (int i = 0; i < clusterSize; i++)
                {
                    int offsetX = rng.Next(-2, 3);
                    int offsetY = rng.Next(-2, 3);
                    TryPlaceTree(t.x + offsetX, t.y + offsetY, d);
                }
            }
        }

        private void TryPlaceTree(int x, int y, MapLayoutData d)
        {
            if (x <= 0 || x >= d.mapSize.width || y <= 0 || y >= d.mapSize.height) return;
            if (InWater(x, y, d)) return;
            if (wallMap.GetTile(new Vector3Int(x, y, 0)) != null) return;
            Place(x, y, treeTrunk);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private HashSet<Vector2Int> BuildEntranceSet(List<EntranceData> entrances)
        {
            var set = new HashSet<Vector2Int>();
            if (entrances == null) return set;
            foreach (var e in entrances)
            {
                if (e.Issealed) continue;
                for (int x = e.xMin; x <= e.xMax; x++)
                for (int y = e.yMin; y <= e.yMax; y++)
                    set.Add(new Vector2Int(x, y));
            }
            return set;
        }

        private void TryPlace(int x, int y, TileBase tile, HashSet<Vector2Int> skip)
        {
            if (!skip.Contains(new Vector2Int(x, y)))
                wallMap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        private void Place(int x, int y, TileBase tile)
        {
            if (tile != null)
                wallMap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        private void Fill(int xMin, int xMax, int yMin, int yMax, TileBase tile)
        {
            if (tile == null) return;
            for (int x = xMin; x <= xMax; x++)
            for (int y = yMin; y <= yMax; y++)
                wallMap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }
}
