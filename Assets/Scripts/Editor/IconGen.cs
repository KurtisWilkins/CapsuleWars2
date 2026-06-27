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
    /// Generates flat-emblem UI ICONS for content SOs via the editor-side Grok pipeline
    /// (<see cref="GrokImageService"/>; key stays in SecretsConfig, never leaves the editor) and assigns each
    /// imported Sprite to the SO's <c>icon</c> field.
    ///
    /// Two paths (ADR-033/035):
    ///  • CLASSES (abstract, no mesh): text→image — a per-class imagery prompt + the shared flat-emblem style.
    ///  • EQUIPMENT / BODY PARTS (have geometry): MESH-STYLIZED — render the item's visualPrefab/visualMesh/Mesh
    ///    to a reference image (PreviewRenderUtility), then Grok image-EDIT it into a flat-emblem icon that
    ///    RESEMBLES the actual mesh while matching the class style. (Name-only prompts can't match a specific mesh.)
    ///
    /// Each image is a PAID xAI call → sequential, one stable PNG per asset
    /// (Assets/Generated/Icons/&lt;category&gt;/&lt;assetName&gt;.png), imported as a Sprite (forced synchronous so
    /// the ref actually persists). Mesh-stylized menus OVERWRITE existing icons (they redo the name-only ones).
    /// Reference renders are saved under Icons/_refs so the render itself can be eyeballed.
    /// </summary>
    public static class IconGen
    {
        private const string IconsRoot = "Assets/Generated/Icons";
        private const string RefsRoot = "Assets/Generated/Icons/_refs";

        private const string Style =
            "professional mobile game UI icon, flat emblem style, single centered subject, bold simple silhouette, " +
            "vibrant flat colors with subtle inner shading, clean vector shapes, dark slate radial-gradient background, " +
            "crisp and readable at small size, centered composition, no text, no words, no letters, no border, no frame";

        private const string EditStyle =
            "Restyle this 3D item render as a flat emblem mobile game UI icon. PRESERVE the overall shape, silhouette " +
            "and proportions of the item shown. Bold clean vector shapes, vibrant flat colors with subtle inner " +
            "shading, dark slate radial-gradient background, crisp and readable at small size, centered, no text, no border.";

        // ---------------- menus: classes (text → image) ----------------

        [MenuItem("Tools/Icons/Generate ONE Test Class Icon (Grok)")]
        public static void GenerateOneTestClass()
        {
            var c = LoadAll<UnitClass_SO>().FirstOrDefault();
            if (c == null) { Debug.LogWarning("[IconGen] No UnitClass_SO assets."); return; }
            _queue.Enqueue(new Job { prompt = ClassPrompt(c), category = "Classes", id = c.name, so = c });
            Debug.Log($"[IconGen] TEST: one class icon for {c.name}…");
            Pump();
        }

        [MenuItem("Tools/Icons/Generate Class Icons (Grok)")]
        public static void GenerateClassIcons()
        {
            var assets = LoadAll<UnitClass_SO>();
            int added = 0;
            foreach (var a in assets)
            {
                if (HasIcon(a)) continue;
                _queue.Enqueue(new Job { prompt = ClassPrompt(a), category = "Classes", id = a.name, so = a });
                added++;
            }
            Debug.Log($"[IconGen] Classes: {assets.Count} asset(s), {added} need icons → queued (one paid Grok call each)…");
            Pump();
        }

        // ---------------- menus: equipment & body parts (mesh render → Grok stylize) ----------------

        [MenuItem("Tools/Icons/Generate ONE Test Equipment Icon (mesh-stylized)")]
        public static void GenerateOneTestEquipment()
        {
            var e = LoadAll<Equipment_SO>().FirstOrDefault(x => HasGeometry(x));
            if (e == null) { Debug.LogWarning("[IconGen] No Equipment_SO with a visualPrefab/visualMesh."); return; }
            var refPng = RenderReference(e, save: true);
            if (refPng == null) { Debug.LogWarning($"[IconGen] {e.name}: render produced nothing."); return; }
            _queue.Enqueue(new Job { prompt = $"A {Humanize(e.name)}. {EditStyle}", category = "Equipment", id = e.name, so = e, referencePng = refPng });
            Debug.Log($"[IconGen] TEST: mesh-stylized equipment icon for {e.name} (ref {refPng.Length} bytes saved under _refs)…");
            Pump();
        }

        [MenuItem("Tools/Icons/Generate Equipment Icons (mesh-stylized)")]
        public static void GenerateEquipmentIcons() =>
            EnqueueMeshStylized<Equipment_SO>("Equipment", e => $"A {Humanize(e.name)}. {EditStyle}");

        [MenuItem("Tools/Icons/Generate Body Part Icons (mesh-stylized)")]
        public static void GenerateBodyPartIcons() =>
            EnqueueMeshStylized<BodyPart_SO>("BodyParts", b => $"A {Humanize(b.name)} character part. {EditStyle}");

        // ---------------- menus: equipment & body parts (DIRECT mesh render — no Grok) ----------------
        // Reliable fallback / primary path: the rendered mesh thumbnail IS the icon, so it always matches the
        // actual geometry. Free + synchronous (no paid call), overwrites existing icons.

        [MenuItem("Tools/Icons/Generate Equipment Icons (mesh render, direct)")]
        public static void GenerateEquipmentIconsDirect() => RenderDirect<Equipment_SO>("Equipment");

        [MenuItem("Tools/Icons/Generate Body Part Icons (mesh render, direct)")]
        public static void GenerateBodyPartIconsDirect() => RenderDirect<BodyPart_SO>("BodyParts");

        private static void RenderDirect<T>(string category) where T : ScriptableObject
        {
            var assets = LoadAll<T>();
            int ok = 0, skip = 0;
            foreach (var a in assets)
            {
                var png = RenderReference(a, save: false);
                if (png == null) { skip++; Debug.LogWarning($"[IconGen] {category}/{a.name}: no renderable mesh — skipped."); continue; }

                string folder = AssetPipelineImporter.EnsureFolder($"{IconsRoot}/{category}");
                string path = $"{folder}/{a.name}.png";
                File.WriteAllBytes(path, png);
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
                if (sprite == null) { skip++; Debug.LogWarning($"[IconGen] {category}/{a.name}: Sprite import failed."); continue; }

                var so = new SerializedObject(a);
                var ip = so.FindProperty("icon");
                if (ip != null) { ip.objectReferenceValue = sprite; so.ApplyModifiedPropertiesWithoutUndo(); }
                EditorUtility.SetDirty(a);
                ok++;
                Debug.Log($"[IconGen] ✓ {category}/{a.name} (direct render)");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[IconGen] {category} (direct mesh render): {ok} icons assigned, {skip} skipped.");
        }

        private static void EnqueueMeshStylized<T>(string category, Func<T, string> prompt) where T : ScriptableObject
        {
            var assets = LoadAll<T>();
            int added = 0, skipped = 0;
            foreach (var a in assets)
            {
                var refPng = RenderReference(a, save: true);
                if (refPng == null) { skipped++; Debug.LogWarning($"[IconGen] {category}/{a.name}: no renderable mesh — skipped."); continue; }
                _queue.Enqueue(new Job { prompt = prompt(a), category = category, id = a.name, so = a, referencePng = refPng });
                added++;
            }
            Debug.Log($"[IconGen] {category} (mesh-stylized): {assets.Count} asset(s) → {added} queued, {skipped} skipped " +
                      "(overwrites existing icons; one paid Grok edit call each)…");
            Pump();
        }

        // ---------------- prompts ----------------

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

        // ---------------- queue / pump ----------------

        private struct Job { public string prompt; public string category; public string id; public ScriptableObject so; public byte[] referencePng; }

        private static readonly Queue<Job> _queue = new();
        private static bool _busy;
        private static int _ok, _fail;

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
            byte[] reference = job.referencePng;

            Task.Run(() => reference != null
                    ? GrokImageService.EditAsync(job.prompt, reference, key, model, "", "1:1", "1k")
                    : GrokImageService.GenerateAsync(job.prompt, key, model, endpoint, "1:1", "1k"))
                .ContinueWith(t => GenerationHttp.OnMainThread(() =>
                {
                    try
                    {
                        if (t.IsFaulted) throw GenerationHttp.Unwrap(t.Exception);

                        string folder = AssetPipelineImporter.EnsureFolder($"{IconsRoot}/{job.category}");
                        string path = $"{folder}/{job.id}.png";
                        File.WriteAllBytes(path, t.Result);

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
                        Pump();
                    }
                }));
        }

        // ---------------- mesh render (reference for the stylize edit) ----------------

        private static bool HasGeometry(Equipment_SO e) => e != null && (e.VisualPrefab != null || e.VisualMesh != null);

        private static byte[] RenderReference(ScriptableObject so, bool save)
        {
            GameObject prefab = null; Mesh mesh = null; Material[] mats = null;
            if (so is Equipment_SO e) { prefab = e.VisualPrefab; mesh = e.VisualMesh; mats = e.VisualMaterials?.ToArray(); }
            else if (so is BodyPart_SO b) { mesh = b.Mesh; mats = b.DefaultMaterials?.ToArray(); }
            if (prefab == null && mesh == null) return null;

            byte[] png = RenderToPng(prefab, mesh, mats, so.name, 512);
            if (save && png != null)
            {
                string folder = AssetPipelineImporter.EnsureFolder(RefsRoot);
                string refPath = $"{folder}/{so.name}_ref.png";
                File.WriteAllBytes(refPath, png);
                AssetDatabase.ImportAsset(refPath);
            }
            return png;
        }

        // Per-item orientation fixes — some meshes' "front" isn't toward the default 3/4 camera (e.g. a helmet
        // rendered facing away). Euler degrees applied to the item before framing; tune per item as needed.
        private static readonly Dictionary<string, Vector3> RotOverride = new()
        {
            { "Equip_StarterHelm", new Vector3(0f, 180f, 0f) },
            { "TestHelmet",        new Vector3(0f, 180f, 0f) },
        };

        // Long flat items (swords) auto-oriented: blade laid on the screen diagonal (tip → top-right,
        // hilt → bottom-left), broad face toward the camera — computed from the mesh's long/thin axes.
        private static readonly HashSet<string> DiagonalBlade = new() { "Eq_IronSword", "Equip_StarterSword" };
        // If a diagonal-blade item renders tip-down (bottom-left), add it here to flip tip ↔ hilt.
        private static readonly HashSet<string> DiagonalBladeFlip = new();

        // Per-item framing margin (smaller = zoom in / fill more of the frame); default 1.3.
        private static readonly Dictionary<string, float> ZoomOverride = new()
        {
            { "MikeyMouseHands",    0.167f },   // ~3× closer than 0.5
            { "Eq_IronSword",       1.05f },
            { "Equip_StarterSword", 1.05f },
        };

        private static Quaternion RotationFor(string name) =>
            RotOverride.TryGetValue(name, out var e) ? Quaternion.Euler(e) : Quaternion.identity;

        private static float ZoomFor(string name) =>
            ZoomOverride.TryGetValue(name, out var z) ? z : 1.3f;

        private static Material _fallbackMat;
        private static Material FallbackMat =>
            _fallbackMat != null ? _fallbackMat :
            (_fallbackMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")));

        private static byte[] RenderToPng(GameObject prefab, Mesh mesh, Material[] mats, string name, int size)
        {
            var pru = new PreviewRenderUtility();
            GameObject temp = null;
            try
            {
                var cam = pru.camera;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.55f, 0.57f, 0.62f, 1f);
                cam.fieldOfView = 30f;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 1000f;
                pru.lights[0].intensity = 1.4f;
                pru.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                if (pru.lights.Length > 1) { pru.lights[1].intensity = 1.0f; pru.lights[1].transform.rotation = Quaternion.Euler(-25f, -40f, 0f); }

                Material mat = null;
                if (mats != null) foreach (var m in mats) if (m != null) { mat = m; break; }
                if (mat == null) mat = FallbackMat;

                Vector3 viewDir = (Quaternion.Euler(18f, -28f, 0f) * Vector3.forward).normalized;

                // Measure the item at IDENTITY → axes (for auto-orient) + the framing center/size.
                Bounds local;
                if (prefab != null)
                {
                    temp = UnityEngine.Object.Instantiate(prefab);
                    temp.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    local = CalcRendererBounds(temp);
                }
                else
                {
                    local = mesh.bounds;
                }

                Quaternion rot = DiagonalBlade.Contains(name)
                    ? DiagonalBladeRotation(local, viewDir, DiagonalBladeFlip.Contains(name))
                    : RotationFor(name);

                if (prefab != null) { temp.transform.rotation = rot; pru.AddSingleGO(temp); }

                // Frame on the ROTATED center (the item is rotated about the origin); per-item zoom margin.
                Vector3 center = rot * local.center;
                float radius = Mathf.Max(local.extents.magnitude, 0.1f);
                float dist = radius / Mathf.Sin(Mathf.Deg2Rad * cam.fieldOfView * 0.5f) * ZoomFor(name);
                cam.transform.position = center - viewDir * dist;
                cam.transform.rotation = Quaternion.LookRotation(viewDir, Vector3.up);

                pru.BeginStaticPreview(new Rect(0, 0, size, size));
                if (prefab == null && mesh != null)
                    pru.DrawMesh(mesh, Matrix4x4.TRS(Vector3.zero, rot, Vector3.one), mat, 0);
                cam.Render();
                var tex = pru.EndStaticPreview();
                return tex != null ? tex.EncodeToPNG() : null;
            }
            catch (Exception ex) { Debug.LogError($"[IconGen] render failed: {ex.Message}"); return null; }
            finally
            {
                if (temp != null) UnityEngine.Object.DestroyImmediate(temp);
                pru.Cleanup();
            }
        }

        // Auto-orient a long flat item so its long axis (blade) lies along the screen bottom-left→top-right
        // diagonal with the broad face toward the camera. flip = swap which end (tip/hilt) sits at top-right.
        private static Quaternion DiagonalBladeRotation(Bounds local, Vector3 viewDir, bool flip)
        {
            Vector3 ext = local.extents;
            Vector3 blade = (ext.x >= ext.y && ext.x >= ext.z) ? Vector3.right : (ext.y >= ext.z ? Vector3.up : Vector3.forward);
            Vector3 broad = (ext.x <= ext.y && ext.x <= ext.z) ? Vector3.right : (ext.y <= ext.z ? Vector3.up : Vector3.forward);
            if (blade == broad) broad = (blade == Vector3.forward) ? Vector3.up : Vector3.forward;   // degenerate guard

            var camRot = Quaternion.LookRotation(viewDir, Vector3.up);
            Vector3 diagonal = (camRot * Vector3.right + camRot * Vector3.up).normalized;   // screen bottom-left → top-right
            if (flip) diagonal = -diagonal;

            var localFix = Quaternion.Inverse(Quaternion.LookRotation(broad, blade));   // broad→+Z, blade→+Y
            var world = Quaternion.LookRotation(-viewDir, diagonal);                    // +Z→toward camera, +Y→diagonal
            return world * localFix;
        }

        private static Bounds CalcRendererBounds(GameObject go)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }

        // ---------------- helpers ----------------

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
            s = Regex.Replace(s, "([a-z0-9])([A-Z])", "$1 $2");
            s = s.Replace('_', ' ').Replace('-', ' ');
            return s.Trim().ToLowerInvariant();
        }
    }
}
#endif
