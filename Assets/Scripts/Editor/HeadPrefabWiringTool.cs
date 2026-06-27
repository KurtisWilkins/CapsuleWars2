#if UNITY_EDITOR
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Units.Customization;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Structurally wires the floating sphere head into the unit prefab (Head-as-part-type, Slice 1):
    /// creates Mount_Head_Sphere under the B_Head bone (so it animates with the head), points a Head
    /// SlotMount at it (PaletteRole.Body, fallbackMesh = sphere so the unit is never headless), adds +
    /// wires a HeadPreviewTuner, and re-parents Socket_Helmet + Mount_Head (HeadProp) under B_Head so
    /// head gear rides the sphere. Idempotent.
    ///
    /// This does the STRUCTURE only. The exact sphere size / float gap and the helmet/HeadProp offsets
    /// are dialed by hand via the HeadPreviewTuner's "Apply Head Preview" context menu and confirmed in
    /// Play (the bridge can't read transforms / capture the viewport).
    /// </summary>
    public static class HeadPrefabWiringTool
    {
        private const string PrefabPath = "Assets/Prefabs/Unit_Sample_Prefab.prefab";
        private const string HeadPartPath = "Assets/Data/Units/Parts/Head_Sphere.asset";
        private const string HeadBone = "B_Head";
        private const string HeadMountName = "Mount_Head_Sphere";

        [MenuItem("Tools/Build-To-Spec/Wire Head Mount Into Prefab")]
        public static void Wire()
        {
            var head = AssetDatabase.LoadAssetAtPath<BodyPart_SO>(HeadPartPath);
            Mesh sphere = head != null ? head.Mesh : null;
            if (sphere == null) { Debug.LogError($"[HeadPrefabWiringTool] No sphere mesh on {HeadPartPath}. Run 'Author Default Head' first."); return; }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var bHead = FindDeep(root.transform, HeadBone);
                if (bHead == null) { Debug.LogError($"[HeadPrefabWiringTool] {HeadBone} bone not found."); return; }

                // 1. Mount_Head_Sphere under B_Head with MeshFilter/MeshRenderer.
                var mountT = bHead.Find(HeadMountName);
                var mount = mountT != null ? mountT.gameObject : new GameObject(HeadMountName);
                if (mountT == null) mount.transform.SetParent(bHead, false);
                mount.transform.localPosition = new Vector3(0f, 0.15f, 0f); // HeadPreviewTuner float-gap default
                mount.transform.localScale = Vector3.one * 0.6f;            // sphere-size default
                mount.transform.localRotation = Quaternion.identity;

                // NOTE: do NOT use `?? AddComponent` — GetComponent returns a Unity fake-null that ?? treats
                // as non-null, so AddComponent never runs. Use Unity-aware == null checks.
                var mf = mount.GetComponent<MeshFilter>();
                if (mf == null) mf = mount.AddComponent<MeshFilter>();
                var mr = mount.GetComponent<MeshRenderer>();
                if (mr == null) mr = mount.AddComponent<MeshRenderer>();
                mf.sharedMesh = sphere;
                // Reuse the body's material so the head shares the URP shader + is palette-tintable.
                var bodyMount = FindDeep(root.transform, "Mount_Body");
                var bodyMr = bodyMount != null ? bodyMount.GetComponent<MeshRenderer>() : null;
                if (bodyMr != null && bodyMr.sharedMaterial != null) mr.sharedMaterial = bodyMr.sharedMaterial;
                else Debug.LogWarning("[HeadPrefabWiringTool] Could not copy Mount_Body material — assign a material to Mount_Head_Sphere by hand.");

                // 2. Head SlotMount in UnitCustomization (PaletteRole.Body, fallback = sphere).
                var custom = root.GetComponentInChildren<UnitCustomization>(true);
                if (custom == null) { Debug.LogError("[HeadPrefabWiringTool] UnitCustomization not found."); return; }
                WriteHeadMount(custom, mf, mr, sphere);

                // 3. HeadPreviewTuner wired to the mount.
                var tuner = root.GetComponent<HeadPreviewTuner>();
                if (tuner == null) tuner = root.AddComponent<HeadPreviewTuner>();
                var tSo = new SerializedObject(tuner);
                tSo.FindProperty("headMount").objectReferenceValue = mount.transform;
                tSo.ApplyModifiedPropertiesWithoutUndo();

                // 4. Re-anchor head gear under B_Head, preserving world pose (no immediate jump; offsets dialed later).
                int reanchored = 0;
                reanchored += Reanchor(root.transform, bHead, "Socket_Helmet");
                reanchored += Reanchor(root.transform, bHead, "Mount_Head");

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[HeadPrefabWiringTool] Wired {HeadMountName} under {HeadBone} + Head SlotMount(palette=Body, fallback=sphere) + HeadPreviewTuner; re-anchored {reanchored} head-gear socket(s). " +
                          "Dial sphere size / float gap via HeadPreviewTuner > 'Apply Head Preview', re-seat helmets, and Play-verify.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Add or update the Head SlotMount. InsertArrayElementAtIndex copies the prior element, so every
        // field is set explicitly to avoid inheriting another slot's references.
        private static void WriteHeadMount(UnitCustomization custom, MeshFilter mf, MeshRenderer mr, Mesh fallback)
        {
            var so = new SerializedObject(custom);
            var mounts = so.FindProperty("mounts");

            SerializedProperty target = null;
            for (int i = 0; i < mounts.arraySize; i++)
            {
                var el = mounts.GetArrayElementAtIndex(i);
                if (el.FindPropertyRelative("slot").enumValueIndex == (int)PartSlot.Head) { target = el; break; }
            }
            if (target == null)
            {
                int idx = mounts.arraySize;
                mounts.InsertArrayElementAtIndex(idx);
                target = mounts.GetArrayElementAtIndex(idx);
            }

            target.FindPropertyRelative("slot").enumValueIndex = (int)PartSlot.Head;       // contiguous enum → index == value
            target.FindPropertyRelative("meshFilter").objectReferenceValue = mf;
            target.FindPropertyRelative("meshRenderer").objectReferenceValue = mr;
            target.FindPropertyRelative("paletteRole").enumValueIndex = (int)PaletteRole.Body;
            target.FindPropertyRelative("fallbackMesh").objectReferenceValue = fallback;
            target.FindPropertyRelative("fallbackMaterials").arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int Reanchor(Transform root, Transform newParent, string name)
        {
            var t = FindDeep(root, name);
            if (t == null || t.parent == newParent) return 0;
            t.SetParent(newParent, worldPositionStays: true);
            return 1;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
