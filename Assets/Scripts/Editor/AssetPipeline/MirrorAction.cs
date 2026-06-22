using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// "Mirror to opposite side": horizontally flips a sided part's approved image and
    /// finds-or-creates a linked AssetRequest for the opposite side, ready for Meshy.
    /// Flipping the 2D image (not negative-scaling the mesh) keeps the 3D result's normals
    /// correct. Idempotent (deterministic mirror id → re-runs update, never pile up) and the
    /// original is never overwritten. Editor-only.
    /// </summary>
    public static class MirrorAction
    {
        public struct Result { public bool ok; public string message; public AssetRequest mirror; }

        private const string ImagesFolder = "Assets/Generated/Images";
        private const string RequestsFolder = "Assets/Editor/AssetPipeline/Requests";

        /// <param name="interactive">true = show the modal symmetry-warning dialog (window button);
        /// false = non-modal (logs the warning; refuses if the part is flagged asymmetric) — used by the
        /// MenuItem / automation so a bridge call can't hang on a dialog.</param>
        public static Result MirrorRequest(AssetRequest src, bool interactive)
        {
            if (src == null) return Fail("No request.");
            if (!MirrorUtil.TryGetOpposite(src.category, src.targetSlot, out int oppSlot, out string sideWord, out string oppWord))
                return Fail($"'{src.title}' isn't a sided part — only right/left hands and feet can be mirrored.");
            if (src.chosenImage == null || string.IsNullOrEmpty(src.imagePath) || !File.Exists(src.imagePath))
                return Fail("This request has no saved Chosen image to mirror. Generate or assign an image first.");

            // Warn: a plain horizontal mirror assumes symmetry.
            string warn = "Mirroring assumes the part is symmetric.\nCancel if it has text, a logo, or one-sided detail." +
                          (src.asymmetric ? "\n\n⚠ This request is flagged ASYMMETRIC." : "");
            if (interactive)
            {
                if (!EditorUtility.DisplayDialog($"Mirror {sideWord} → {oppWord}?", warn, "Mirror it", "Cancel"))
                    return Fail("Cancelled.");
            }
            else
            {
                if (src.asymmetric)
                    return Fail($"'{src.title}' is flagged asymmetric — mirror it from the window button (with confirmation), not the menu.");
                Debug.LogWarning($"[AssetPipeline] Mirroring '{src.title}' {sideWord}→{oppWord}: {warn}");
            }

            string mirrorId = $"{src.id}_{oppWord}";

            // 1. Flip the image (never touches the original file).
            byte[] flipped;
            try { flipped = FlipHorizontalPng(File.ReadAllBytes(src.imagePath)); }
            catch (Exception e) { return Fail("Flip failed: " + e.Message); }

            string imgPath = $"{AssetPipelineImporter.EnsureFolder(ImagesFolder)}/{mirrorId}.png";
            File.WriteAllBytes(imgPath, flipped);
            AssetDatabase.ImportAsset(imgPath);

            // 2. Find-or-create the mirror request (idempotent: deterministic id, else an existing mirrorOf==src).
            string reqPath = $"{AssetPipelineImporter.EnsureFolder(RequestsFolder)}/{mirrorId}.asset";
            var mirror = AssetDatabase.LoadAssetAtPath<AssetRequest>(reqPath) ?? FindExistingMirror(src);
            bool created = mirror == null;
            if (created)
            {
                mirror = ScriptableObject.CreateInstance<AssetRequest>();
                AssetDatabase.CreateAsset(mirror, reqPath);
            }

            mirror.id = mirrorId;
            mirror.title = SwapSide(src.title, sideWord, oppWord);
            mirror.requestText = SwapSide(src.requestText, sideWord, oppWord);
            mirror.category = src.category;
            mirror.targetSlot = oppSlot;
            mirror.attachSocketName = SwapSide(src.attachSocketName, sideWord, oppWord);
            mirror.asymmetric = src.asymmetric;
            mirror.mirrorOf = src;
            mirror.chosenImage = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
            mirror.imagePath = imgPath;
            mirror.grokImagePrompt = "";   // the image comes from the flip; no Grok regen for the mirror
            mirror.stage = PipelineStage.ImageChosen;
            mirror.meshyPrompt = StyleComposer.ComposeMeshyPrompt(mirror);

            EditorUtility.SetDirty(mirror);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = mirror;
            EditorGUIUtility.PingObject(mirror);

            return new Result
            {
                ok = true,
                mirror = mirror,
                message = $"{(created ? "Created" : "Updated")} mirror '{mirrorId}' (opposite of '{src.id}'): " +
                          $"flipped image {imgPath}, slot {oppWord}, Meshy prompt ready."
            };
        }

        // --- image flip ---

        /// <summary>Pure horizontal mirror: decode → reverse each row's columns → re-encode PNG.
        /// Preserves resolution, grayscale, and background; no vertical flip or rotation.</summary>
        private static byte[] FlipHorizontalPng(byte[] srcPng)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!tex.LoadImage(srcPng)) throw new Exception("could not decode the source PNG.");
                int w = tex.width, h = tex.height;
                var src = tex.GetPixels32();
                var dst = new Color32[src.Length];
                for (int y = 0; y < h; y++)
                {
                    int row = y * w;
                    for (int x = 0; x < w; x++)
                        dst[row + (w - 1 - x)] = src[row + x];
                }
                tex.SetPixels32(dst);
                tex.Apply();
                return tex.EncodeToPNG();
            }
            finally { UnityEngine.Object.DestroyImmediate(tex); }
        }

        // --- helpers ---

        private static AssetRequest FindExistingMirror(AssetRequest src)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(AssetRequest)}"))
            {
                var r = AssetDatabase.LoadAssetAtPath<AssetRequest>(AssetDatabase.GUIDToAssetPath(guid));
                if (r != null && r.mirrorOf == src) return r;
            }
            return null;
        }

        // Whole-word, case-preserving side swap (avoids hitting substrings like "bright").
        private static string SwapSide(string s, string fromWord, string toWord)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = Regex.Replace(s, $@"\b{fromWord}\b", toWord);
            s = Regex.Replace(s, $@"\b{fromWord.ToLowerInvariant()}\b", toWord.ToLowerInvariant());
            return s;
        }

        private static Result Fail(string message) => new Result { ok = false, message = message };

        // --- MenuItem (bridge-triggerable, non-modal) ---

        [MenuItem("Tools/CapsuleWars/Mirror Selected Request")]
        public static void MirrorSelected()
        {
            var r = Selection.activeObject as AssetRequest ?? FindSingleEligible();
            if (r == null)
            {
                Debug.LogWarning("[AssetPipeline] Mirror: select a sided AssetRequest (right/left hand or foot) with a Chosen image first.");
                return;
            }
            // Non-modal on purpose (no DisplayDialog) so this menu path is safe to trigger headlessly.
            var res = MirrorRequest(r, interactive: false);
            if (res.ok) Debug.Log("[AssetPipeline] " + res.message);
            else Debug.LogWarning("[AssetPipeline] Mirror skipped: " + res.message);
        }

        // The single sided, image-having, non-mirror request (for menu/automation when nothing is selected).
        private static AssetRequest FindSingleEligible()
        {
            AssetRequest found = null;
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(AssetRequest)}"))
            {
                var r = AssetDatabase.LoadAssetAtPath<AssetRequest>(AssetDatabase.GUIDToAssetPath(guid));
                if (r != null && r.mirrorOf == null && r.chosenImage != null && MirrorUtil.IsSided(r))
                {
                    if (found != null) return null;   // ambiguous — require an explicit selection
                    found = r;
                }
            }
            return found;
        }
    }
}
