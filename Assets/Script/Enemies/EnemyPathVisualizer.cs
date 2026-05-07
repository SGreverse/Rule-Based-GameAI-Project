using System.Collections.Generic;
using Assets.Algorithm.PathFinding;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EnemyPathVisualizer : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    // Configuration variables that can be tweaked in the Unity Inspector
    [Header("Line Settings")]
    [SerializeField] private float _lineWidth = 0.05f;
    [SerializeField] private Color _lineColor = Color.red;
    [SerializeField] private float _zOffset = -0.1f; // Slight offset so the line doesn't clip into the ground sprite

    private void Awake()
    {
        // Initialize the LineRenderer component
        _lineRenderer = GetComponent<LineRenderer>();

        // Setup line renderer properties for a thin red line
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;

        // Using a basic unlit sprite material so the line is purely red and unaffected by lighting
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;

        // Ensure the line is drawn on top of the ground but under the characters
        _lineRenderer.sortingOrder = 5;
    }


    public void DrawPath(Vector2 currentEnemyPosition, Stack<PathWaypoint> remainingPath)
    {
        // If there is no path, clear the line and exit
        if (remainingPath == null || remainingPath.Count == 0)
        {
            ClearPath();
            return;
        }

        // We need an array size of remaining waypoints + 1 (for the enemy's current position)
        _lineRenderer.positionCount = remainingPath.Count + 1;

        // Point 0 is exactly where the enemy is standing right now
        _lineRenderer.SetPosition(0, new Vector3(currentEnemyPosition.x, currentEnemyPosition.y, _zOffset));

        // Iterate through the Stack. 
        // Note: Foreach on a Stack reads from top to bottom without popping the elements!
        // This is perfect because we don't want to destroy the movement logic's data.
        int index = 1;
        foreach (PathWaypoint waypoint in remainingPath)
        {
            // Convert Vector2 to Vector3 for the LineRenderer
            _lineRenderer.SetPosition(index, new Vector3(waypoint.Position.x, waypoint.Position.y, _zOffset));
            index++;
        }
    }
    public void ClearPath()
    {
        _lineRenderer.positionCount = 0;
    }
}

