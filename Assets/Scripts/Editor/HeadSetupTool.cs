#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Authors the default floating-sphere HEAD as a swappable BodyPart_SO (Head-as-part-type, Slice 1).
    /// Idempotent. Creates Head_Sphere (slot=Head, Unity sphere primitive mesh), adds it to every
    /// PartCatalog_SO as a STARTER (so every profile owns it and RandomUnitGenerator/UnitFactory can
    /// resolve it by id), and adds a Head PartAssignment to every UnitDefinition_SO that lacks one so
    /// definition-driven units render a head. v1 is a hand-authored primitive — Meshy/Grok head
    /// generation is the Slice 2 hook.
    /// </summary>
    public static class HeadSetupTool
    {
        private const string HeadDir = "Assets/Data/Units/Parts";
        private const string HeadPath = HeadDir + "/Head_Sphere.asset";
        private const string HeadId = "head_sphere";

        [MenuItem("Tools/Build-To-Spec/Author Default Head")]
        public static void Author()
        {
            var head = CreateOrLoadHead();
            int catalogs = AddToCatalogs(head);
            int defs = AddToDefinitions(head);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[HeadSetupTool] '{HeadId}' ready at {HeadPath} (slot=Head, sphere primitive mesh); " +
                      $"ensured starter in {catalogs} catalog(s); ensured Head assignment in {defs} UnitDefinition(s). " +
                      "Prefab Mount_Head_Sphere + SlotMount + socket re-anchor are wired separately on the unit prefab.");
        }

        private static BodyPart_SO CreateOrLoadHead()
        {
            if (!AssetDatabase.IsValidFolder(HeadDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data/Units"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
                    AssetDatabase.CreateFolder("Assets/Data", "Units");
                }
                AssetDatabase.CreateFolder("Assets/Data/Units", "Parts");
            }

            var head = AssetDatabase.LoadAssetAtPath<BodyPart_SO>(HeadPath);
            if (head == null)
            {
                head = ScriptableObject.CreateInstance<BodyPart_SO>();
                AssetDatabase.CreateAsset(head, HeadPath);
            }
            SetField(head, "partId", HeadId);
            SetField(head, "nameTermKey", "Part.HeadSphere.Name");
            SetField(head, "slot", PartSlot.Head);
            SetField(head, "mesh", BuiltinSphereMesh());
            EditorUtility.SetDirty(head);
            return head;
        }

        // Unity's built-in Sphere mesh (from the default resources) — a stable shared reference.
        private static Mesh BuiltinSphereMesh()
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);
            return mesh;
        }

        private static int AddToCatalogs(BodyPart_SO head)
        {
            int touched = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:PartCatalog_SO"))
            {
                var cat = AssetDatabase.LoadAssetAtPath<PartCatalog_SO>(AssetDatabase.GUIDToAssetPath(guid));
                if (cat == null) continue;
                var parts = GetField<List<PartCatalog_SO.PartEntry>>(cat, "parts");
                if (parts == null) continue;

                var existing = parts.Find(pe => pe != null && pe.part == head);
                if (existing == null)
                {
                    parts.Add(new PartCatalog_SO.PartEntry { part = head, cost = 0, starter = true });
                    EditorUtility.SetDirty(cat);
                    touched++;
                }
                else if (!existing.starter)
                {
                    existing.starter = true;   // the default head must be owned by all
                    EditorUtility.SetDirty(cat);
                    touched++;
                }
            }
            return touched;
        }

        private static int AddToDefinitions(BodyPart_SO head)
        {
            int touched = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:UnitDefinition_SO"))
            {
                var def = AssetDatabase.LoadAssetAtPath<UnitDefinition_SO>(AssetDatabase.GUIDToAssetPath(guid));
                if (def == null) continue;
                var parts = GetField<List<PartAssignment>>(def, "parts");
                if (parts == null) continue;

                if (!parts.Exists(pa => pa.slot == PartSlot.Head))
                {
                    parts.Add(new PartAssignment { slot = PartSlot.Head, part = head });
                    EditorUtility.SetDirty(def);
                    touched++;
                }
            }
            return touched;
        }

        private static void SetField(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(target, value);
        }

        private static T GetField<T>(object target, string field) where T : class
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            return f?.GetValue(target) as T;
        }
    }
}
#endif
