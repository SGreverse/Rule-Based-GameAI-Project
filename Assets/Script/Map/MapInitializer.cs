using UnityEngine;
using UnityEngine.Tilemaps;

public class MapInitializer : MonoBehaviour
{
    [Header("Room Maps")]
    public Tilemap ObstacleMap;
    public Tilemap FloorMap;

    private void Start()
    {
        // Overwrite the stale, destroyed references with the new Room maps
        Initalize();

    }
    public void Initalize()
    {
        GameManager.Instance.obstacleTilemap = ObstacleMap;
        GameManager.Instance.floorTilemap = FloorMap;
    }
}
