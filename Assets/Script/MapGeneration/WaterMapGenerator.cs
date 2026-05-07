using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MapGeneration
{
    public class WaterMapGenerator : MonoBehaviour
    {
        [Header("Tilemap Reference")]
        public Tilemap waterMap;

        [Header("Water Tiles")]
        public TileBase deepWater;
        public TileBase shallowWater;

        [Header("Organic Settings")]
        public int seed = 42;
        [Tooltip("How much the straight river edges erode")]
        [Range(0.1f, 0.6f)] public float edgeErosion = 0.35f;

        public void Generate(MapLayoutData layoutData)
        {
            waterMap.ClearAllTiles();

            // Keep generation random but consistent per-seed
            UnityEngine.Random.InitState(seed);

            var river = layoutData.river;
            if (river == null) return;

            PlaceMainChannels(river.mainChannel);
            PlaceBranch(river.branch);
            PlacePonds(river.ponds);
            PlaceShoreEdges(layoutData);
            CarveBridges(layoutData.bridges);
        }

        private void PlaceMainChannels(ChannelData channel)
        {
            if (channel == null) return;
            foreach (var seg in channel.segments)
            {
                TileBase tile = seg.depth == "shallow" ? shallowWater : deepWater;
                for (int x = seg.xMin; x <= seg.xMax; x++)
                    for (int y = seg.yMin; y <= seg.yMax; y++)
                    {
                        // Calculate how close we are to the bank
                        float distToEdgeX = Mathf.Min(x - seg.xMin, seg.xMax - x);
                        float distToEdgeY = Mathf.Min(y - seg.yMin, seg.yMax - y);

                        // Only erode the outer boundary of the river
                        if (distToEdgeX <= 1 || distToEdgeY <= 1)
                        {
                            float noise = Mathf.PerlinNoise(x * 0.4f, y * 0.4f);
                            // Skip placing this tile to create jagged, eroded banks
                            if (noise < edgeErosion) continue;
                        }

                        waterMap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
            }
        }

        private void PlaceBranch(BranchData branch)
        {
            if (branch == null) return;
            TileBase tile = branch.depth == "shallow" ? shallowWater : deepWater;

            int steps = Mathf.Max(
                Mathf.Abs(branch.end.x - branch.start.x),
                Mathf.Abs(branch.end.y - branch.start.y)
            );
            if (steps == 0) return;

            float baseRadius = branch.width / 2f;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int cx = Mathf.RoundToInt(Mathf.Lerp(branch.start.x, branch.end.x, t));
                int cy = Mathf.RoundToInt(Mathf.Lerp(branch.start.y, branch.end.y, t));

                // Add a slight wobble to the stream width
                float wobbleRadius = baseRadius + UnityEngine.Random.Range(-0.5f, 1f);
                int brushSize = Mathf.CeilToInt(wobbleRadius);

                // Paint a circular brush
                for (int dx = -brushSize; dx <= brushSize; dx++)
                    for (int dy = -brushSize; dy <= brushSize; dy++)
                    {
                        if (dx * dx + dy * dy > wobbleRadius * wobbleRadius) continue;
                        waterMap.SetTile(new Vector3Int(cx + dx, cy + dy, 0), tile);
                    }
            }
        }

        private void PlacePonds(List<PondData> ponds)
        {
            if (ponds == null) return;
            foreach (var p in ponds)
            {
                TileBase tile = p.depth == "shallow" ? shallowWater : deepWater;

                // Find center and radii to draw an ellipse instead of a rectangle
                float cx = (p.xMin + p.xMax) / 2f;
                float cy = (p.yMin + p.yMax) / 2f;
                float rx = (p.xMax - p.xMin) / 2f;
                float ry = (p.yMax - p.yMin) / 2f;

                for (int x = p.xMin; x <= p.xMax; x++)
                    for (int y = p.yMin; y <= p.yMax; y++)
                    {
                        // Distance formula for an ellipse
                        float dx = (x - cx) / rx;
                        float dy = (y - cy) / ry;
                        float dist = (dx * dx) + (dy * dy);

                        // Add Perlin noise to the edges to make it look like a natural swamp pool
                        float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.4f;

                        if (dist < 1f + noise) // Fill only inside the organic shape
                        {
                            waterMap.SetTile(new Vector3Int(x, y, 0), tile);
                        }
                    }
            }
        }

        private void PlaceShoreEdges(MapLayoutData data)
        {
            if (shallowWater == null) return;

            int w = data.mapSize.width;
            int h = data.mapSize.height;
            var shorePositions = new List<Vector3Int>();

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (waterMap.GetTile(pos) != null) continue;

                    bool adjacentToDeepWater = false;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            var neighbor = new Vector3Int(x + dx, y + dy, 0);

                            // Only add shores around deep water
                            if (waterMap.GetTile(neighbor) == deepWater)
                            {
                                adjacentToDeepWater = true;
                                break;
                            }
                        }

                    // If it touches deep water, add shallow water. 
                    // We add noise so the shore has tiny gaps and looks like a real riverbank!
                    if (adjacentToDeepWater && Mathf.PerlinNoise(x * 0.35f, y * 0.35f) > 0.2f)
                    {
                        shorePositions.Add(pos);
                    }
                }

            foreach (var pos in shorePositions)
                waterMap.SetTile(pos, shallowWater);
        }

        private void CarveBridges(List<BridgeData> bridges)
        {
            if (bridges == null) return;
            foreach (var b in bridges)
            {
                for (int x = b.xMin; x <= b.xMax; x++)
                    for (int y = b.yMin; y <= b.yMax; y++)
                        waterMap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }
}
