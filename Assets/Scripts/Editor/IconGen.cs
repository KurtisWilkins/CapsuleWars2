#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CapsuleWars.Data.Classes;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.Units;
using CapsuleWars.Editor.AssetPipeline;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// Generates flat-emblem UI ICONS for content SOs via the existing editor-side Grok pipeline
    /// (<see cref="GrokImageService"/>; key stays in SecretsConfig, never leaves the editor) and assigns each
    /// PNG to the SO's <c>icon</c> field. Covers UnitClass / Equipment / BodyPart today; Ability (moves) joins
    /// once BTS-F authors the move assets (and the editor asmdef references the Abilities assembly).
    ///
    /// Each image is a PAID xAI call, so generation is sequential (one at a time), idempotent (skips SOs that
    /// already have an icon unless you regenerate), and writes one stable PNG per asset
    /// (Assets/Generated/Icons/&lt;category&gt;/&lt;assetName&gt;.png). The PNG is imported as a **Sprite**
    /// (textureType=Sprite) — the one step the AssetRequest image path lacked — so it's UI-ready.
    /// Curate results in the editor; re-run a category to fill in any you delete.
    /// </summary>
    public static class IconGen
    {
        private const string IconsRoot = "Assets/Generated/Icons";

        // Shared flat-emblem style anchor (the user's chosen direction) — appended to every prompt for consistency.
        private const string Style =
            "professional mobile game UI icon, flat emblem style, single centered subject, bold simple silhouette, " +
            "vibrant flat colors with subtle inner shading, clean vector shapes, dark slate radial-gradient background, " +
            "crisp and readable at small size, centered composition, no text, no words, no letters, no border, no frame";

        // -------- menu entry points --------

        [MenuItem("Tools/Icons/Generate ONE Test Class Icon (Grok)")]
        public static void GenerateOneTest()
        {
            var c = LoadAll<UnitClass_SO>().FirstOrDefault();
            if (c == null) { Debug.LogWarning("[IconGen] No UnitClass_SO assets found."); return; }
            _queue.Enqueue(new Job { prompt = ClassPrompt(c), category = "Classes", id = c.name, so = c });
            Debug.Log($"[IconGen] TEST: generating ONE icon for {c.name} to validate the pipeline…");
            Pump();
        }

        [MenuItem("Tools/Icons/Generate Class Icons (Grok)")]
        public static void GenerateClassIcons() => Enqueue<UnitClass_SO>("Classes", ClassPrompt);

        [MenuItem("Tools/Icons/Generate Equipment Icons (Grok)")]
        public static void GenerateEquipmentIcons() => Enqueue<Equipment_SO>("Equipment", EquipmentPrompt);

        [MenuItem("Tools/Icons/Generate Body Part Icons (Grok)")]
        public static void GenerateBodyPartIcons() => Enqueue<BodyPart_SO>("BodyParts", BodyPartPrompt);

        [MenuItem("Tools/Icons/Generate ALL Item Icons (Grok)")]
        public static void GenerateAll()
        {
            GenerateClassIcons();
            GenerateEquipmentIcons();
            GenerateBodyPartIcons();
        }

        // -------- prompts --------

        // Per-class imagery for stronger emblems (the class name alone is weak). Falls back to a humanized id.
        private static readonly Dictionary<string, string> ClassDesc = new()
        {
            { "Barbarian",   "a raging barbarian swinging a massive two-handed battle axe" },
            { "Fighter",     "a nimble dual-wielding swordsman in light armor" },
            { "Archer",      "a hooded archer drawing a longbow" },
            { "Spearman",    "a phalanx soldier bracing a long spear behind a shield" },
            { "Heavy",       "an armored knight behind a huge tower shield" },
            { "Wizard",      "a wizard channeling glowing arcane energy through a staff" },
            { "Javelin",     "a skirmisher hurling a javelin" },
            { "Alchemist",   "an alchemist throwing a bubbling green potion" },
            { "Cleric",      "a cleric raising a holy staff radiating light" },
            { "Ambrosian",   "an apothecary tossing a glowing golden healing elixir" },
            { "Assassin",    "a cloaked assassin wielding twin daggers" },
            { "Monk",        "a martial-arts monk in a focused fighting stance" },
            { "Crossbow",    "a marksman aiming a heavy crossbow" },
            { "HandGunner",  "a musketeer firing a flintlock gun with a puff of smoke" },
            { "Siegebreaker","a demolisher throwing a lit round bomb" },
            { "Paladin",     "a holy paladin with a blessed sword and shield" },
        };

        private static string ClassPrompt(UnitClass_SO c)
        {
            string id = !string.IsNullOrEmpty(c.ClassId) ? c.ClassId : c.name;
            string desc = ClassDesc.TryGetValue(id, out var d) ? d : $"a fantasy {Humanize(id)} class hero";
            return $"{desc}, fantasy RPG class emblem. {Style}";
        }

        private static string EquipmentPrompt(Equipment_SO e) =>
            $"a {Humanize(e.name)} ({e.Slot} equipment), fantasy RPG item. {Style}";

        private static string BodyPartPrompt(BodyPart_SO b) =>
            $"a {Humanize(b.name)} cosmetic part for a stylized capsule character ({b.Slot}). {Style}";

        // -------- generation queue (sequential; mirrors GenerationActions) --------

        private struct Job { public string prompt; public string category; public string id; public ScriptableObject so; }

        private static readonly Queue<Job> _queue = new();
        private static bool _busy;
        private static int _ok, _fail;

        private static void Enqueue<T>(string category, Func<T, string> prompt) where T : ScriptableObject
        {
            var assets = LoadAll<T>();
            int added = 0;
            foreach (var a in assets)
            {
                if (HasIcon(a)) continue;   // idempotent — keep existing icons; delete one to regenerate it
                _queue.Enqueue(new Job { prompt = prompt(a), category = category, id = a.name, so = a });
                added++;
            }
            Debug.Log($"[IconGen] {category}: {assets.Count} asset(s), {added} need icons → queued. Generating sequentially " +
                      "(one paid Grok call each)…");
            Pump();
        }

        private static void Pump()
        {
            if (_busy) return;
            if (_queue.Count == 0)
            {
                if (_ok + _fail > 0) { Debug.Log($"[IconGen] Batch done — {_ok} ok, {_fail} failed."); _ok = _fail = 0; }
                return;
            }

            var job = _queue.Dequeue();
            _busy = true;

            string key = GenerationServices.Secrets != null ? GenerationServices.Secrets.grokApiKey : null;
            string model = GenerationServices.GrokModel;
            string endpoint = GenerationServices.GrokEndpoint;

            Task.Run(() => GrokImageService.GenerateAsync(job.prompt, key, model, endpoint, "1:1", "1k"))
                .ContinueWith(t => GenerationHttp.OnMainThread(() =>
                {
                    try
                    {
                        if (t.IsFaulted) throw GenerationHttp.Unwrap(t.Exception);

                        string folder = AssetPipelineImporter.EnsureFolder($"{IconsRoot}/{job.category}");
                        string path = $"{folder}/{job.id}.png";
                        File.WriteAllBytes(path, t.Result);

                        // Import as a SPRITE so it's assignable to UI Image.sprite (the step the AssetRequest path
                        // lacked). Set the importer, then FORCE a synchronous reimport so the Sprite sub-asset
                        // actually exists before we load it — a plain SaveAndReimport can defer, leaving
                        // LoadAssetAtPath<Sprite> null and the icon silently unassigned.
                        AssetDatabase.ImportAsset(path);
                        if (AssetImporter.GetAtPath(path) is TextureImporter imp)
                        {
                            imp.textureType = TextureImporterType.Sprite;
                            imp.spriteImportMode = SpriteImportMode.Single;
                            imp.alphaIsTransparency = true;
                            imp.SaveAndReimport();
                        }
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sprite == null) throw new Exception("Sprite import produced no Sprite sub-asset.");

                        var so = new SerializedObject(job.so);
                        var ip = so.FindProperty("icon");
                        if (ip == null) throw new Exception("target SO has no serialized 'icon' field.");
                        ip.objectReferenceValue = sprite;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(job.so);
                        AssetDatabase.SaveAssets();

                        _ok++;
                        Debug.Log($"[IconGen] ✓ {job.category}/{job.id}  ({_queue.Count} left)");
                    }
                    catch (Exception e)
                    {
                        _fail++;
                        Debug.LogError($"[IconGen] ✗ {job.category}/{job.id}: {e.Message}");
                    }
                    finally
                    {
                        _busy = false;
                        Pump();   // keep the batch moving even if one fails
                    }
                }));
        }

        // -------- helpers --------

        private static List<T> LoadAll<T>() where T : ScriptableObject
        {
            var list = new List<T>();
            foreach (var g in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var a = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g));
                if (a != null) list.Add(a);
            }
            return list;
        }

        private static bool HasIcon(ScriptableObject so)
        {
            var p = new SerializedObject(so).FindProperty("icon");
            return p != null && p.objectReferenceValue != null;
        }

        private static string Humanize(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "item";
            string s = raw;
            foreach (var pre in new[] { "Eq_", "Equip_", "WC_", "Class_", "Body_", "Part_" })
                if (s.StartsWith(pre)) { s = s.Substring(pre.Length); break; }
            s = Regex.Replace(s, "([a-z0-9])([A-Z])", "$1 $2");   // camelCase → spaced
            s = s.Replace('_', ' ').Replace('-', ' ');
            return s.Trim().ToLowerInvariant();
        }
    }
}
#endif
