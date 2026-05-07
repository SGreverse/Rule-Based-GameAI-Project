using System;
using Assets.Algorithm.MainAlgorithm;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyManager), true)]
public class BehaviorTreeVisualizer : Editor
{
    // MENTOR NOTE: We use a timer to "throttle" our repaints. 
    // Redrawing the editor 5 times a second (0.2f) is highly efficient 
    // and still looks perfectly real-time for debugging AI states.
    private float _lastRepaintTime = 0f;
    private const float RepaintInterval = 0.2f;

    private void OnEnable()
    {
        // Subscribe to the editor's global update loop when this inspector is active
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        // CRITICAL: Always unsubscribe from global delegates when the object is disabled,
        // otherwise you will create memory leaks in the Unity Editor!
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        // Only bother repainting if the game is running
        if (!Application.isPlaying) return;

        // Check if enough time has passed since the last repaint
        if (Time.realtimeSinceStartup - _lastRepaintTime > RepaintInterval)
        {
            Repaint(); // Safely request a redraw from OUTSIDE the GUI loop
            _lastRepaintTime = Time.realtimeSinceStartup;
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw the normal variables you have in your manager script
        DrawDefaultInspector();

        EnemyManager manager = (EnemyManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Behavior Tree Debugger", EditorStyles.boldLabel);

        if (Application.isPlaying && manager.GetBrain() != null && manager.GetBrain().GetRootNode() != null)
        {
            DrawNodeRecursive(manager.GetBrain().GetRootNode(), 0);

            // REMOVED: Repaint() is no longer here! The infinite loop is gone.
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to visualize the Behavior Tree.", MessageType.Info);
        }
    }

    private void DrawNodeRecursive(Node node, int indentLevel)
    {
        if (node == null) return;

        Color nodeColor = Color.grey;
        switch (node.CurrentState)
        {
            case NodeState.Success:
                nodeColor = Color.green;
                break;
            case NodeState.Failure:
                nodeColor = Color.red;
                break;
            case NodeState.Running:
                nodeColor = new Color(1f, 0.64f, 0f); // Orange
                break;
        }

        Color originalColor = GUI.color;
        GUI.color = nodeColor;
        EditorGUI.indentLevel = indentLevel;

        // MENTOR NOTE: String interpolation $"..." generates garbage (GC Alloc) every time it runs.
        // Because this runs recursively for every node, it can trigger the Garbage Collector, causing stutter.
        // For a final exam project, this is acceptable, but in AAA games, we cache node names in the Node class!
        EditorGUILayout.LabelField($"{node.GetType().Name} [{node.CurrentState}],{node.CurrentUtility}", EditorStyles.helpBox);

        GUI.color = originalColor;

        if (node is Composite composite)
        {
            foreach (Node child in composite.GetChildren())
            {
                DrawNodeRecursive(child, indentLevel + 1);
            }
        }
    }
}