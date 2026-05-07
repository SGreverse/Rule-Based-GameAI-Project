#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MapGeneration.Editor
{
    [CustomEditor(typeof(LevelBuilder))]
    public class LevelBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var builder = (LevelBuilder)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Map Generation", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.35f, 0.85f, 0.35f);
            if (GUILayout.Button("Build Map", GUILayout.Height(40)))
            {
                Undo.RegisterCompleteObjectUndo(builder.gameObject, "Build Map");
                builder.BuildMap();
                EditorUtility.SetDirty(builder);
            }

            GUI.backgroundColor = new Color(0.9f, 0.35f, 0.35f);
            if (GUILayout.Button("Clear Map", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Clear Map",
                    "Remove all tiles from all three layers?", "Clear", "Cancel"))
                {
                    Undo.RegisterCompleteObjectUndo(builder.gameObject, "Clear Map");
                    builder.ClearMap();
                    EditorUtility.SetDirty(builder);
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Quick Setup:\n" +
                "1. Assign MapLayoutData.json to 'Map Data Json'\n" +
                "2. Fill tile references on each generator (see TILE MAPPING comments in each script)\n" +
                "3. Click Build Map\n\n" +
                "Grid children: FloorMap, WaterMap, WallMap\n" +
                "Map size: 100x100 tiles @ 32px each",
                MessageType.Info
            );
        }
    }
}
#endif
