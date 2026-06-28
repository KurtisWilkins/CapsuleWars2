#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Generates a flat GRAYSCALE region MASK via Grok, BYPASSING the 3D-part StyleProfile framing — a mask is a 2D
    /// pattern layout (white = secondary/marking region, black = primary/base), not an image-to-3D object, so the
    /// "single isolated object suitable for image-to-3D" base prompt would fight it. Region-tint model (ADR-040): each
    /// patterned race's marking layout is a mask the (pending) region-tint shader consumes. Staged under
    /// Assets/Generated/Masks/, not imported. Async like GenerationActions — poll <see cref="Busy"/> / the file.
    /// </summary>
    public static class MaskGen
    {
        public static bool Busy { get; private set; }
        public static string Status { get; private set; } = "";
        private const string Folder = "Assets/Generated/Masks";

        public static void GenerateMask(string id, string subject)
        {
            if (Busy) { Debug.LogWarning("[MaskGen] busy — one at a time"); return; }
            Busy = true;
            Status = "mask: " + id;

            string prompt =
                "A FLAT 2D GRAYSCALE PATTERN MASK: " + subject + ". " +
                "Pure solid BLACK background with the markings painted in solid WHITE. A flat top-down graphic texture " +
                "map — NO 3D object, no shading, no gradients, no perspective, no depth. High contrast, crisp white " +
                "shapes on black, even edge-to-edge coverage. No color, no outlines, no text, no border, no watermark.";

            string key = GenerationServices.Secrets.grokApiKey;
            string model = GenerationServices.GrokModel;
            string endpoint = GenerationServices.GrokEndpoint;

            Task.Run(async () => await GrokImageService.GenerateAsync(prompt, key, model, endpoint, "1:1", "1k"))
                .ContinueWith(t => GenerationHttp.OnMainThread(() =>
                {
                    try
                    {
                        if (t.IsFaulted) throw GenerationHttp.Unwrap(t.Exception);
                        string folder = AssetPipelineImporter.EnsureFolder(Folder);
                        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{id}.png");
                        File.WriteAllBytes(path, t.Result);
                        AssetDatabase.ImportAsset(path);
                        Busy = false;
                        Status = "mask saved: " + path;
                        Debug.Log("[MaskGen] " + path);
                    }
                    catch (Exception e)
                    {
                        Busy = false;
                        Status = "mask failed";
                        Debug.LogError("[MaskGen] " + id + " failed: " + e.Message);
                    }
                }));
        }
    }
}
#endif
