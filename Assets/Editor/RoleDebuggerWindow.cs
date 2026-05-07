using UnityEngine;
using UnityEditor;
using Assets.Algorithm.BlackBoard;
using System;
using System.Collections.Generic;

public class RoleDebuggerWindow : EditorWindow
{
    private RoleType _selectedRole = RoleType.Charging;
    private Vector2 _scrollPosition;

    // This creates a new menu item at the very top of the Unity Editor!
    [MenuItem("Tools/AI Role Debugger")]
    public static void ShowWindow()
    {
        // Opens the window and gives it a title
        RoleDebuggerWindow window = GetWindow<RoleDebuggerWindow>("AI Role Debugger");
        window.minSize = new Vector2(400, 300);
    }

    // This forces the window to refresh continuously so we get real-time utility updates
    private void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        // 1. Check if the game is running (Blackboard only exists in Play Mode)
        if (!Application.isPlaying || GameBlackboard.Instance == null)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to view real-time AI Roles.", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();

        // ==========================================
        // LEFT PANEL: Role Navigation Sidebar
        // ==========================================
        EditorGUILayout.BeginVertical("box", GUILayout.Width(150));
        EditorGUILayout.LabelField("Select Role", EditorStyles.boldLabel);

        foreach (RoleType role in Enum.GetValues(typeof(RoleType)))
        {
            // Highlight the currently selected button
            GUI.backgroundColor = (_selectedRole == role) ? Color.gray : Color.white;

            if (GUILayout.Button(role.ToString(), GUILayout.Height(25)))
            {
                _selectedRole = role;
            }
        }
        GUI.backgroundColor = Color.white; // Reset color
        EditorGUILayout.EndVertical();

        // ==========================================
        // RIGHT PANEL: Active Enemies & Utility Data
        // ==========================================
        EditorGUILayout.BeginVertical("box");

        RoleData currentRoleData = GameBlackboard.Instance.GetRoleData(_selectedRole);

        if (currentRoleData == null)
        {
            EditorGUILayout.HelpBox("Role Data not initialized yet.", MessageType.Warning);
        }
        else
        {
            // Title Header
            EditorGUILayout.LabelField($"{_selectedRole} Queue", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Capacity: {currentRoleData.Size} / {currentRoleData.MaxCapacity}");

            EditorGUILayout.Space(10);

            if (currentRoleData.Size == 0)
            {
                EditorGUILayout.HelpBox("No enemies are currently performing this role.", MessageType.None);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                // Table Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Agent ID (Name)", EditorStyles.boldLabel, GUILayout.Width(200));
                EditorGUILayout.LabelField("Current Utility", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                // Draw a line separator
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
                EditorGUILayout.Space(5);

                // Display all enemies currently in this role
                List<RoleClaim> claims = currentRoleData.GetSortedClaims();
                for (int i = 0; i < claims.Count; i++)
                {
                    RoleClaim claim = claims[i];
                    EditorGUILayout.BeginHorizontal();

                    // Mark the enemy at the bottom of the list (the one next on the chopping block)
                    string prefix = (i == claims.Count - 1 && currentRoleData.Size >= currentRoleData.MaxCapacity)
                                    ? "[WEAKEST] "
                                    : $"#{i + 1} ";

                    // Color code the weakest link in red for easy spotting
                    if (i == claims.Count - 1 && currentRoleData.Size >= currentRoleData.MaxCapacity)
                    {
                        GUI.contentColor = new Color(1f, 0.4f, 0.4f);
                    }

                    EditorGUILayout.LabelField(prefix + claim.agentID, GUILayout.Width(200));
                    EditorGUILayout.LabelField(claim.UtilityScore.ToString("F3")); // Show up to 3 decimals

                    GUI.contentColor = Color.white; // Reset text color
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
}