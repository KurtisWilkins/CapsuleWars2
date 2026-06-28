#if UNITY_EDITOR
using CapsuleWars.Units.Customization;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Editor preview for the legacy per-unit ramp tint applier — preview a unit's tint in the Scene view without
    /// entering play mode. The TintPreset save/apply buttons were removed in the region-tint pivot (ADR-040): the new
    /// TintPreset is 3 region colors + a mask consumed by the (pending) region-tint shader, authored as data, not via
    /// this per-unit ramp applier.
    /// </summary>
    [CustomEditor(typeof(UnitTintApplier))]
    public class UnitTintApplierEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();   // primaryTint + accents → OnValidate live-previews as you edit

            var applier = (UnitTintApplier)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Preview tint (no play mode)"))
            {
                applier.Apply();
                SceneView.RepaintAll();
            }
        }
    }
}
#endif
