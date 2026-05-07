using System.Collections.Generic;
using Assets.Algorithm.BehaviorTree.Sequences;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class UtilityAIDebuggerWindow : EditorWindow {
    private EnemyManager _selectedEnemy;
    private int _selectedLayer = 0; // 0 = Layer 1 (Main), 1 = Layer 2 (Attacks)
    private string[] _layerNames = new string[] { "Layer 1: Main Tactics", "Layer 2: Attack Types" };
    private Vector2 _scrollPosition;

    // Layout constants for table columns
    private readonly GUILayoutOption _colName = GUILayout.Width(160);
    private readonly GUILayoutOption _colRaw = GUILayout.Width(80);
    private readonly GUILayoutOption _colCurve = GUILayout.Width(80);
    private readonly GUILayoutOption _colWeight = GUILayout.Width(60);
    private readonly GUILayoutOption _colScore = GUILayout.Width(80);

    [MenuItem("Window/AI Utility Table Debugger")]
    public static void ShowWindow()
    {
        GetWindow<UtilityAIDebuggerWindow>("AI Utility Table");
    }

    private void OnInspectorUpdate()
    {
        if (Application.isPlaying) Repaint(); // Update table numbers live
    }

    private void OnGUI()
    {
        if(!GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected))
        GUILayout.Label("Hierarchical Utility AI Profiler", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _selectedEnemy = (EnemyManager)EditorGUILayout.ObjectField("Target Enemy", _selectedEnemy, typeof(EnemyManager), true);

        if (_selectedEnemy == null || !Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Press Play and assign an EnemyManager to view the live utility tables.", MessageType.Info);
            return;
        }

        // --- LAYER SWITCHER ---
        EditorGUILayout.Space();
        _selectedLayer = GUILayout.Toolbar(_selectedLayer, _layerNames);
        EditorGUILayout.Space();

        // Fetch the nodes based on the selected layer
        List<Node> nodesToDisplay = GetNodesForLayer(_selectedLayer);

        if (nodesToDisplay == null || nodesToDisplay.Count == 0)
        {
            GUILayout.Label("No utility nodes found for this layer.", EditorStyles.helpBox);
            return;
        }
        if (!GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected)) return;
            // --- START SCROLL VIEW HERE ---
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        // Draw a table for every node in the selected layer
        foreach (Node node in nodesToDisplay)
        {
            DrawNodeTable(node);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        // --- END SCROLL VIEW HERE ---
        GUILayout.EndScrollView();
    }

    private void DrawNodeTable(Node node)
    {
        // 1. Node Header
        EditorGUILayout.BeginVertical("window"); // Creates a nice bounding box

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(node.GetType().Name, EditorStyles.boldLabel);

        // Calculate Priority Live
        float rawPriority = 1f;
        float finalPriority = 1f;
        if (node.PriorityCurve != null && node.PriorityFetcher != null)
        {
            rawPriority = node.PriorityFetcher.Invoke(_selectedEnemy);
            finalPriority = node.PriorityCurve.Plot(rawPriority);
        }

        // The total utility combines the factors, priority, and inertia
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Priority Mod: {finalPriority:F2}", EditorStyles.boldLabel);
        GUILayout.Label($"TOTAL UTILITY: {node.CurrentUtility:F3}", EditorStyles.whiteLargeLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 2. Table Header Row
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("Factor Name", EditorStyles.miniBoldLabel, _colName);
        GUILayout.Label("Raw Input (x)", EditorStyles.miniBoldLabel, _colRaw);
        GUILayout.Label("Curve ( f(x) )", EditorStyles.miniBoldLabel, _colCurve);
        GUILayout.Label("Weight", EditorStyles.miniBoldLabel, _colWeight);
        GUILayout.Label("Final Score", EditorStyles.miniBoldLabel, _colScore);
        EditorGUILayout.EndHorizontal();

        // 3. Table Data Rows
        float baseUtilitySum = 0f;

        if (node.UtilityFactors != null)
        {
            foreach (UtilityFactor factor in node.UtilityFactors)
            {
                // Live Math
                float rawValue = factor.ParameterFetcher != null ? factor.ParameterFetcher.Invoke(_selectedEnemy) : 0f;
                float curveValue = factor.Curve != null ? factor.Curve.Plot(rawValue) : 0f;
                float score = curveValue * factor.Weight;
                baseUtilitySum += score;

                // Draw Row
                EditorGUILayout.BeginHorizontal();
                // Fallback to "Unnamed Factor" if you forgot to add a name
                GUILayout.Label(string.IsNullOrEmpty(factor.Name) ? "Unnamed Factor" : factor.Name, _colName);
                GUILayout.Label(rawValue.ToString("F2"), _colRaw);
                GUILayout.Label(curveValue.ToString("F2"), _colCurve);
                GUILayout.Label(factor.Weight.ToString("F2"), _colWeight);
                GUILayout.Label(score.ToString("F2"), EditorStyles.boldLabel, _colScore);
                EditorGUILayout.EndHorizontal();
            }
        }

        // 4. Footer (Math Summary)
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Sum: {baseUtilitySum:F2}  * Priority: {finalPriority:F2}  =  Final Base: {(baseUtilitySum * finalPriority):F2}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private List<Node> GetNodesForLayer(int layerIndex)
    {
        List<Node> nodes = new List<Node>();
        if (_selectedEnemy == null || _selectedEnemy.GetBrain() == null) return nodes;

        Node root = _selectedEnemy.GetBrain().GetRootNode();

        // 1. Dig through the tree to find the Main Utility Selector
        UtilitySelector mainUtilitySelector = FindFirstUtilitySelector(root);

        if (mainUtilitySelector == null)
        {
            return nodes; // Still didn't find it!
        }

        if (layerIndex == 0) // Layer 1: Main Tactics (Heal, Flee, AttackSelector, etc.)
        {
            foreach (Node child in mainUtilitySelector.GetChildren())
            {
                if (child.UtilityFactors != null && child.UtilityFactors.Count > 0)
                {
                    nodes.Add(child);
                }
            }
        }
        else if (layerIndex == 1) // Layer 2: Attack Types (Charge, Shoot, Flank)
        {
            // Find the AttackSelector inside the main UtilitySelector
            AttackSelector attackSelector = null;
            foreach (Node child in mainUtilitySelector.GetChildren())
            {
                if (child is AttackSelector)
                {
                    attackSelector = (AttackSelector)child;
                    break;
                }
            }

            if (attackSelector != null)
            {
                foreach (Node child in attackSelector.GetChildren())
                {
                    if (child.UtilityFactors != null && child.UtilityFactors.Count > 0)
                    {
                        nodes.Add(child);
                    }
                }
            }
        }

        return nodes;
    }
    private UtilitySelector FindFirstUtilitySelector(Node node)
    {
        if (node == null) return null;

        // If we found a UtilitySelector (and it's NOT the AttackSelector), that's our Layer 1!
        if (node is UtilitySelector us && !(node is AttackSelector))
            return us;

        // Otherwise, keep digging down the tree
        if (node is Assets.Algorithm.MainAlgorithm.Selector selector)
        {
            foreach (var child in selector.GetChildren())
            {
                var found = FindFirstUtilitySelector(child);
                if (found != null) return found;
            }
        }
        else if (node is Assets.Algorithm.MainAlgorithm.Sequence sequence)
        {
            foreach (var child in sequence.GetChildren())
            {
                var found = FindFirstUtilitySelector(child);
                if (found != null) return found;
            }
        }

        return null;
    }
}
