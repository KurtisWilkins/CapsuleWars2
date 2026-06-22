using System.IO;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.Units;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Turns a finished <see cref="AssetRequest"/> (imported model + category + slot)
    /// into a real item the game uses: a prefab under
    /// <c>Assets/Generated/Meshy/{slot}/</c>, then an <c>Equipment_SO</c> (Weapon/Armor)
    /// or <c>BodyPart_SO</c> (BodyPart) with the mesh/prefab assigned, added to the
    /// matching catalog (<c>EquipmentCatalog_SO</c> / <c>PartCatalog_SO</c>). Private
    /// SO fields are written via <see cref="SerializedObject"/> (the SOs expose getters
    /// only). Editor-only; reuses the existing item + socket + catalog patterns.
    /// </summary>
    public static class AssetPipelineImporter
    {
        private const string GeneratedRoot = "Assets/Generated";
        private const string MeshyRoot = "Assets/Generated/Meshy";
        private const string ItemsRoot = "Assets/Generated/Items";

        public struct Result
        {
            public bool ok;
            public string message;
            public Object createdItem;
            public GameObject prefab;
        }

        public static Result CreateAndWire(AssetRequest req)
        {
            if (req == null) return Fail("No request.");
            if (req.category == AssetCategory.Undecided) return Fail("Set a category (Weapon / EquipmentArmor / BodyPart) first.");
            if (string.IsNullOrEmpty(req.id)) return Fail("Request needs an id.");

            string slotName = SlotFolderName(req);

            // 1. Prefab from the imported model (optional — allows wiring before the model arrives).
            GameObject prefab = null;
            Mesh mesh = null;
            if (req.importedModel != null)
            {
                string meshFolder = EnsureFolder($"{MeshyRoot}/{slotName}");
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{meshFolder}/{req.id}.prefab");
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(req.importedModel);
                if (instance == null) instance = Object.Instantiate(req.importedModel);
                prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);

                var mf = req.importedModel.GetComponentInChildren<MeshFilter>();
                mesh = mf != null ? mf.sharedMesh : null;
            }

            // 2. Create the SO + add to its catalog.
            Object item;
            string msg;
            if (req.category == AssetCategory.BodyPart)
            {
                if (mesh == null) return Fail("BodyPart needs a mesh — assign an Imported Model with a MeshFilter first.");
                item = CreateBodyPart(req, mesh, slotName, out msg);
            }
            else
            {
                item = CreateEquipment(req, prefab, mesh, slotName, out msg);
            }

            if (item == null) return Fail(msg);

            req.generatedPrefab = prefab;
            req.createdItem = item;
            EditorUtility.SetDirty(req);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new Result { ok = true, message = msg, createdItem = item, prefab = prefab };
        }

        private static Object CreateBodyPart(AssetRequest req, Mesh mesh, string slotName, out string msg)
        {
            string folder = EnsureFolder($"{ItemsRoot}/BodyParts");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{req.id}.asset");

            var part = ScriptableObject.CreateInstance<BodyPart_SO>();
            AssetDatabase.CreateAsset(part, path);

            var so = new SerializedObject(part);
            so.FindProperty("partId").stringValue = req.id;
            so.FindProperty("nameTermKey").stringValue = $"Part.{req.id}.Name";
            so.FindProperty("slot").enumValueIndex = req.targetSlot;   // PartSlot (contiguous from 0)
            so.FindProperty("mesh").objectReferenceValue = mesh;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(part);

            bool added = AddToPartCatalog(part);
            msg = $"Created BodyPart_SO '{req.id}' (slot {(CapsuleWars.Core.PartSlot)req.targetSlot}) at {path}." +
                  (added ? " Added to PartCatalog." : " ⚠ No PartCatalog found — add it manually.");
            return part;
        }

        private static Object CreateEquipment(AssetRequest req, GameObject prefab, Mesh mesh, string slotName, out string msg)
        {
            string folder = EnsureFolder($"{ItemsRoot}/Equipment");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{req.id}.asset");

            var eq = ScriptableObject.CreateInstance<Equipment_SO>();
            AssetDatabase.CreateAsset(eq, path);

            var so = new SerializedObject(eq);
            so.FindProperty("equipmentId").stringValue = req.id;
            so.FindProperty("nameTermKey").stringValue = $"Equipment.{req.id}.Name";
            so.FindProperty("descTermKey").stringValue = $"Equipment.{req.id}.Desc";
            so.FindProperty("slot").enumValueIndex = req.targetSlot;   // EquipmentSlot (contiguous from 0)
            so.FindProperty("attachSocketName").stringValue = req.attachSocketName;
            if (prefab != null) so.FindProperty("visualPrefab").objectReferenceValue = prefab;
            else if (mesh != null) so.FindProperty("visualMesh").objectReferenceValue = mesh;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(eq);

            bool added = AddToEquipmentCatalog(eq);
            string slotDesc = ((CapsuleWars.Core.EquipmentSlot)req.targetSlot).ToString();
            msg = $"Created Equipment_SO '{req.id}' (slot {slotDesc}) at {path}." +
                  (added ? " Added to EquipmentCatalog." : " ⚠ No EquipmentCatalog found — add it manually.") +
                  (req.category == AssetCategory.Weapon ? " Assign a WeaponClass_SO + stat buffs on the asset." : " Assign rarity + stat buffs on the asset.");
            return eq;
        }

        private static bool AddToEquipmentCatalog(Equipment_SO item)
        {
            var catalog = FindFirst<EquipmentCatalog_SO>();
            if (catalog == null) return false;
            var so = new SerializedObject(catalog);
            var arr = so.FindProperty("items");
            int i = arr.arraySize;
            arr.arraySize = i + 1;
            arr.GetArrayElementAtIndex(i).objectReferenceValue = item;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return true;
        }

        private static bool AddToPartCatalog(BodyPart_SO part)
        {
            var catalog = FindFirst<PartCatalog_SO>();
            if (catalog == null) return false;
            var so = new SerializedObject(catalog);
            var arr = so.FindProperty("parts");
            int i = arr.arraySize;
            arr.arraySize = i + 1;
            var el = arr.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("part").objectReferenceValue = part;
            el.FindPropertyRelative("cost").intValue = 2;
            el.FindPropertyRelative("starter").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return true;
        }

        private static string SlotFolderName(AssetRequest req)
        {
            return req.category == AssetCategory.BodyPart
                ? ((CapsuleWars.Core.PartSlot)req.targetSlot).ToString()
                : ((CapsuleWars.Core.EquipmentSlot)req.targetSlot).ToString();
        }

        private static T FindFirst<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>Create a nested folder chain under Assets/ and return its path.</summary>
        public static string EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return assetPath;
            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
            return assetPath;
        }

        private static Result Fail(string message) => new Result { ok = false, message = message };
    }
}
