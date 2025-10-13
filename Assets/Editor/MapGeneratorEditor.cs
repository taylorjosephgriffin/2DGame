using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator mg = (MapGenerator)target;

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Water Debug Tools", EditorStyles.boldLabel);
        if (GUILayout.Button("Regenerate Water"))
        {
            // Support edit mode execution
            if (!Application.isPlaying)
            {
                Undo.RecordObject(mg, "Regenerate Decorative Water");
            }
            mg.RegenerateDecorativeWater();
            // Mark scene dirty so changes persist
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(mg);
                if (mg.gameObject.scene.isLoaded)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mg.gameObject.scene);
            }
        }
        if (GUILayout.Button("Clear Water"))
        {
            if (mg.waterTilemap != null)
            {
                // Re-render base map to restore walls/floors/shadows, then clear decorative water
                Undo.RecordObject(mg, "Regenerate Base Map");
                mg.RegenerateBaseMap();
                Undo.RecordObject(mg.waterTilemap, "Clear Water Tilemap");
                mg.waterTilemap.ClearAllTiles();
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(mg.waterTilemap);
                    if (mg.gameObject.scene.isLoaded)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mg.gameObject.scene);
                }
            }
            else
            {
                Debug.LogWarning("MapGeneratorEditor: waterTilemap is not assigned.");
            }
        }
            GUILayout.Space(6);
    }
}
