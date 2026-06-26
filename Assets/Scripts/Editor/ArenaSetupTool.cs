#if UNITY_EDITOR
using System.IO;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Data.Arena;
using CapsuleWars.UI.Arena;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// One-shot editor setup for the runtime arena builder (Slice B). Creates two placeholder themes
    /// (grass + volcanic) using tinted primitive blocks — no paid assets — and wires an
    /// <see cref="ArenaBuilder"/> into the open battle scene (DeploymentManager + NavMeshSurface + the
    /// ground Plane's renderer). Idempotent: re-running reuses the existing assets/component. Editor-only.
    ///
    /// To author real blocks later: assign prefabs to a theme's <see cref="ThemeBlockSet"/> — no code change.
    /// </summary>
    public static class ArenaSetupTool
    {
        private const string ArenaDir = "Assets/Settings/Arena";

        [MenuItem("Tools/Arena/Create Placeholder Themes + Wire Test Scene")]
        public static void Setup()
        {
            EnsureFolder(ArenaDir);

            // Tinted materials (URP Lit, fallback Standard).
            var grassA = MakeMaterial("Arena_Grass_A", new Color(0.36f, 0.55f, 0.30f));
            var grassB = MakeMaterial("Arena_Grass_B", new Color(0.26f, 0.43f, 0.22f));
            var grassObstacle = MakeMaterial("Arena_Grass_Obstacle", new Color(0.42f, 0.38f, 0.32f));
            var hazardMat = MakeMaterial("Arena_Hazard", new Color(0.95f, 0.45f, 0.10f));

            var volcanicA = MakeMaterial("Arena_Volcanic_A", new Color(0.30f, 0.17f, 0.15f));
            var volcanicB = MakeMaterial("Arena_Volcanic_B", new Color(0.18f, 0.10f, 0.10f));
            var volcanicObstacle = MakeMaterial("Arena_Volcanic_Obstacle", new Color(0.12f, 0.10f, 0.10f));
            var lavaMat = MakeMaterial("Arena_Lava", new Color(0.95f, 0.30f, 0.05f));

            var grassSet = MakeBlockSet("ThemeBlockSet_Grass", grassA, grassB, grassObstacle, hazardMat);
            var volcanicSet = MakeBlockSet("ThemeBlockSet_Volcanic", volcanicA, volcanicB, volcanicObstacle, lavaMat);

            var grassTheme = MakeTheme("EncounterTheme_Grass", "grass", "Grasslands", grassSet);
            MakeTheme("EncounterTheme_Volcanic", "volcanic", "Volcanic", volcanicSet);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireOpenScene(grassTheme);
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path = $"{ArenaDir}/{name}.mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            mat.color = color;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static ThemeBlockSet MakeBlockSet(string name, Material a, Material b, Material obstacle, Material hazard)
        {
            string path = $"{ArenaDir}/{name}.asset";
            var set = AssetDatabase.LoadAssetAtPath<ThemeBlockSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ThemeBlockSet>();
                AssetDatabase.CreateAsset(set, path);
            }
            var so = new SerializedObject(set);
            SetMaterial(so, "floorA", a);
            SetMaterial(so, "floorB", b);
            SetMaterial(so, "obstacle", obstacle);
            SetMaterial(so, "hazardMarker", hazard);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void SetMaterial(SerializedObject so, string defName, Material mat)
        {
            var def = so.FindProperty(defName);
            if (def == null) return;
            var matProp = def.FindPropertyRelative("material");
            if (matProp != null) matProp.objectReferenceValue = mat;
        }

        private static EncounterTheme MakeTheme(string name, string id, string display, ThemeBlockSet set)
        {
            string path = $"{ArenaDir}/{name}.asset";
            var theme = AssetDatabase.LoadAssetAtPath<EncounterTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<EncounterTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }
            var so = new SerializedObject(theme);
            so.FindProperty("themeId").stringValue = id;
            so.FindProperty("displayName").stringValue = display;
            so.FindProperty("blockSet").objectReferenceValue = set;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            return theme;
        }

        private static void WireOpenScene(EncounterTheme theme)
        {
            var manager = Object.FindFirstObjectByType<DeploymentManager>(FindObjectsInactive.Include);
            if (manager == null)
            {
                Debug.LogWarning("[ArenaSetupTool] Themes created, but no DeploymentManager in the open scene — " +
                                 "open Test_M3_Battle and re-run to wire the ArenaBuilder.");
                return;
            }
            var surface = Object.FindFirstObjectByType<NavMeshSurface>(FindObjectsInactive.Include);
            Renderer planeRenderer = surface != null ? surface.GetComponent<Renderer>() : null;

            var builder = Object.FindFirstObjectByType<ArenaBuilder>(FindObjectsInactive.Include);
            if (builder == null)
            {
                var go = new GameObject("ArenaBuilder");
                builder = go.AddComponent<ArenaBuilder>();
                Undo.RegisterCreatedObjectUndo(go, "Create ArenaBuilder");
            }

            var so = new SerializedObject(builder);
            so.FindProperty("manager").objectReferenceValue = manager;
            so.FindProperty("theme").objectReferenceValue = theme;
            so.FindProperty("navMeshSurface").objectReferenceValue = surface;
            so.FindProperty("groundPlaneRenderer").objectReferenceValue = planeRenderer;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(builder);

            var scene = builder.gameObject.scene;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[ArenaSetupTool] Wired ArenaBuilder in '{scene.name}': manager={manager.name}, " +
                      $"surface={(surface != null ? surface.name : "NONE")}, planeRenderer={(planeRenderer != null ? planeRenderer.name : "NONE")}.");
        }

        [MenuItem("Tools/Arena/Author Demo Terrain (open scene)")]
        public static void AuthorDemoTerrain()
        {
            var manager = Object.FindFirstObjectByType<DeploymentManager>(FindObjectsInactive.Include);
            if (manager == null) { Debug.LogWarning("[ArenaSetupTool] No DeploymentManager in the open scene."); return; }

            // A few obstacles + a hazard in the NEUTRAL middle rows (3-5) so the player (0-2) and enemy (6-8)
            // deploy zones stay clear. Placeholder for Slice C's generated layout.
            var so = new SerializedObject(manager);
            var cells = so.FindProperty("terrainLayout").FindPropertyRelative("cells");
            cells.ClearArray();
            AddTerrainCell(cells, 2, 4, 1);   // Impassable
            AddTerrainCell(cells, 4, 4, 1);   // Impassable
            AddTerrainCell(cells, 3, 5, 1);   // Impassable
            AddTerrainCell(cells, 3, 3, 2);   // Hazard
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            EditorSceneManager.SaveScene(manager.gameObject.scene);
            Debug.Log("[ArenaSetupTool] Authored demo terrain: Impassable (2,4)(4,4)(3,5) + Hazard (3,3) in the neutral rows.");
        }

        private static void AddTerrainCell(SerializedProperty cells, int col, int row, int terrainTypeIndex)
        {
            int i = cells.arraySize;
            cells.InsertArrayElementAtIndex(i);
            var el = cells.GetArrayElementAtIndex(i);
            var coord = el.FindPropertyRelative("coord");
            coord.FindPropertyRelative("col").intValue = col;
            coord.FindPropertyRelative("row").intValue = row;
            el.FindPropertyRelative("type").enumValueIndex = terrainTypeIndex;
        }

        [MenuItem("Tools/Arena/Build Preview (open scene)")]
        public static void BuildPreview()
        {
            var builder = Object.FindFirstObjectByType<ArenaBuilder>(FindObjectsInactive.Include);
            if (builder == null) { Debug.LogWarning("[ArenaSetupTool] No ArenaBuilder in the open scene; run the setup item first."); return; }
            builder.BuildGeometry();   // geometry only — does not touch the baked NavMesh asset
            SceneView.RepaintAll();    // reflect the ground-plane hide immediately (renderer toggles don't auto-repaint)
            Debug.Log("[ArenaSetupTool] Built arena preview geometry (no NavMesh bake). Use 'Clear Preview' before saving the scene.");
        }

        [MenuItem("Tools/Arena/Clear Preview (open scene)")]
        public static void ClearPreview()
        {
            var builder = Object.FindFirstObjectByType<ArenaBuilder>(FindObjectsInactive.Include);
            if (builder == null) return;
            builder.Teardown();
            SceneView.RepaintAll();
            Debug.Log("[ArenaSetupTool] Cleared arena preview.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
