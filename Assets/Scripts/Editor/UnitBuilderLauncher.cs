#if UNITY_EDITOR
using CapsuleWars.Data.Units;
using CapsuleWars.UI.Customization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// One-click launcher for the standalone Unit Builder showroom: makes a throwaway empty scene with a
    /// single <see cref="UnitBuilderSandbox"/> (wired to the felid prefab + part catalog) and enters Play.
    /// No run/party needed — the sandbox is self-contained. Editor-only.
    /// </summary>
    public static class UnitBuilderLauncher
    {
        private const string PreviewPrefab = "Assets/Prefabs/Unit_Sample_Prefab.prefab";
        private const string Catalog = "Assets/Data/Units/PartCatalog.asset";
        private const string PatternMat = "Assets/Art/Shaders/Mat_ProceduralPattern.mat";

        [MenuItem("Tools/CapsuleWars/Open Unit Builder (Play)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Unit Builder: exit Play mode first, then re-open.");
                return;
            }

            // Don't silently discard unsaved work in the current scene.
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (!EditorUtility.DisplayDialog("Open Unit Builder",
                        "This opens a fresh sandbox scene. Save the current scene first?",
                        "Save & Open", "Cancel"))
                    return;
                EditorSceneManager.SaveOpenScenes();
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PreviewPrefab);
            var catalog = AssetDatabase.LoadMainAssetAtPath(Catalog);
            if (prefab == null) { Debug.LogError($"Unit Builder: preview prefab not found at {PreviewPrefab}."); return; }
            if (catalog == null) { Debug.LogError($"Unit Builder: part catalog not found at {Catalog}."); return; }

            var go = new GameObject("UnitBuilderSandbox");
            var sandbox = go.AddComponent<UnitBuilderSandbox>();
            var so = new SerializedObject(sandbox);
            so.FindProperty("previewPrefab").objectReferenceValue = prefab;
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.FindProperty("patternMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(PatternMat);
            var presetGuids = AssetDatabase.FindAssets("t:CoatPreset_SO");
            var presetsProp = so.FindProperty("presets");
            presetsProp.arraySize = presetGuids.Length;
            for (int i = 0; i < presetGuids.Length; i++)
                presetsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<CoatPreset_SO>(AssetDatabase.GUIDToAssetPath(presetGuids[i]));
            so.ApplyModifiedProperties();

            Selection.activeObject = go;
            EditorApplication.isPlaying = true;
            Debug.Log("Unit Builder: entering Play — pick parts on the left, colors at the bottom.");
        }
    }
}
#endif
