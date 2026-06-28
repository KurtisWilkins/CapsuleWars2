#if UNITY_EDITOR
using CapsuleWars.Data.Equipment;
using CapsuleWars.Units.Customization;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Tint milestone editor authoring: preview a unit's tint in the Scene view WITHOUT entering play mode, SAVE the
    /// previewed tint to a reusable <see cref="TintPreset"/> asset (new or existing), and APPLY a preset back onto the
    /// unit. The applier's serialized tint is what persists the painted color (MaterialPropertyBlock values do not);
    /// the preset asset is what makes it reusable across units (and referenceable by ThemeProfile next milestone).
    /// </summary>
    [CustomEditor(typeof(UnitTintApplier))]
    public class UnitTintApplierEditor : UnityEditor.Editor
    {
        private TintPreset presetTarget;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();   // primaryTint + accents → OnValidate live-previews as you edit

            var applier = (UnitTintApplier)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tint preview / preset", EditorStyles.boldLabel);

            if (GUILayout.Button("Preview tint (no play mode)"))
            {
                applier.Apply();
                SceneView.RepaintAll();
            }

            presetTarget = (TintPreset)EditorGUILayout.ObjectField("Preset", presetTarget, typeof(TintPreset), false);

            using (new EditorGUI.DisabledScope(presetTarget == null))
            {
                if (GUILayout.Button("Apply preset → unit"))
                {
                    Undo.RegisterCompleteObjectUndo(applier, "Apply Tint Preset");
                    applier.LoadPreset(presetTarget);
                    EditorUtility.SetDirty(applier);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Save current tint → this preset"))
                {
                    applier.SaveToPreset(presetTarget);
                    EditorUtility.SetDirty(presetTarget);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[Tint] saved current tint to {presetTarget.name}", presetTarget);
                }
            }

            if (GUILayout.Button("Save current tint → NEW preset asset…"))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Tint Preset", "TintPreset", "asset",
                    "Save the previewed tint as a reusable preset asset.");
                if (!string.IsNullOrEmpty(path))
                {
                    var preset = ScriptableObject.CreateInstance<TintPreset>();
                    applier.SaveToPreset(preset);
                    AssetDatabase.CreateAsset(preset, path);
                    AssetDatabase.SaveAssets();
                    presetTarget = preset;
                    EditorGUIUtility.PingObject(preset);
                }
            }
        }
    }
}
#endif
