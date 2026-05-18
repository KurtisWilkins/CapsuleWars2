using System.IO;
using CapsuleWars.Core;
using CapsuleWars.Data.Elements;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// One-click generator for the canonical 15 ElementType_SO assets and
    /// the ElementChart_SO. Writes to Assets/Data/Elements/. Re-running
    /// is safe: existing assets are not overwritten.
    /// </summary>
    public static class ElementSetupTool
    {
        private const string ElementsFolder = "Assets/Data/Elements";

        [MenuItem("Tools/CapsuleWars/Create Default Elements")]
        public static void CreateDefaultElements()
        {
            EnsureFolder(ElementsFolder);
            EnsureFolder($"{ElementsFolder}/Types");

            // Fire family
            CreateElement("flame", "Element.Flame.Name", "Element.Flame.Desc", ElementFamily.Fire, new Color(1f, 0.45f, 0.2f));
            CreateElement("magma", "Element.Magma.Name", "Element.Magma.Desc", ElementFamily.Fire, new Color(0.85f, 0.2f, 0.05f));
            CreateElement("solar", "Element.Solar.Name", "Element.Solar.Desc", ElementFamily.Fire, new Color(1f, 0.85f, 0.3f));

            // Water family
            CreateElement("tide", "Element.Tide.Name", "Element.Tide.Desc", ElementFamily.Water, new Color(0.25f, 0.55f, 0.95f));
            CreateElement("frost", "Element.Frost.Name", "Element.Frost.Desc", ElementFamily.Water, new Color(0.6f, 0.85f, 1f));
            CreateElement("mist", "Element.Mist.Name", "Element.Mist.Desc", ElementFamily.Water, new Color(0.75f, 0.85f, 0.9f));

            // Earth family
            CreateElement("stone", "Element.Stone.Name", "Element.Stone.Desc", ElementFamily.Earth, new Color(0.6f, 0.5f, 0.4f));
            CreateElement("crystal", "Element.Crystal.Name", "Element.Crystal.Desc", ElementFamily.Earth, new Color(0.8f, 0.7f, 1f));
            CreateElement("sand", "Element.Sand.Name", "Element.Sand.Desc", ElementFamily.Earth, new Color(0.95f, 0.85f, 0.55f));

            // Spirit family
            CreateElement("holy", "Element.Holy.Name", "Element.Holy.Desc", ElementFamily.Spirit, new Color(1f, 0.95f, 0.7f));
            CreateElement("shadow", "Element.Shadow.Name", "Element.Shadow.Desc", ElementFamily.Spirit, new Color(0.4f, 0.2f, 0.5f));
            CreateElement("soul", "Element.Soul.Name", "Element.Soul.Desc", ElementFamily.Spirit, new Color(0.6f, 0.85f, 0.8f));

            // Air family
            CreateElement("wind", "Element.Wind.Name", "Element.Wind.Desc", ElementFamily.Air, new Color(0.85f, 0.95f, 0.95f));
            CreateElement("lightning", "Element.Lightning.Name", "Element.Lightning.Desc", ElementFamily.Air, new Color(1f, 1f, 0.5f));
            CreateElement("sound", "Element.Sound.Name", "Element.Sound.Desc", ElementFamily.Air, new Color(0.9f, 0.7f, 0.95f));

            // Chart
            string chartPath = $"{ElementsFolder}/ElementChart.asset";
            if (AssetDatabase.LoadAssetAtPath<ElementChart_SO>(chartPath) == null)
            {
                var chart = ScriptableObject.CreateInstance<ElementChart_SO>();
                AssetDatabase.CreateAsset(chart, chartPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ElementSetupTool] Created/verified 15 elements + chart at {ElementsFolder}/");
        }

        private static void CreateElement(string id, string nameKey, string descKey, ElementFamily family, Color color)
        {
            string path = $"{ElementsFolder}/Types/{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<ElementType_SO>(path) != null) return;

            var element = ScriptableObject.CreateInstance<ElementType_SO>();
            SetPrivate(element, "elementId", id);
            SetPrivate(element, "nameTermKey", nameKey);
            SetPrivate(element, "descTermKey", descKey);
            SetPrivate(element, "family", family);
            SetPrivate(element, "uiColor", color);
            AssetDatabase.CreateAsset(element, path);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
