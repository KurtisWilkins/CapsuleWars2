using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CapsuleWars.Core;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Orchestrates the "Generate" actions: builds the prompt, runs the network call on
    /// the thread pool, then marshals the result back to the main thread to write the
    /// asset, assign it on the <see cref="AssetRequest"/>, advance the stage, and save.
    /// One generation runs at a time. The window listens to <see cref="Changed"/> to
    /// repaint and reads <see cref="Busy"/>/<see cref="Status"/> for feedback.
    /// </summary>
    public static class GenerationActions
    {
        public static bool Busy { get; private set; }
        public static string Status { get; private set; } = "";
        public static event Action Changed;

        private const string ImagesFolder = "Assets/Generated/Images";

        // --- public entry points (called from the window on the main thread) ---

        private static readonly Queue<AssetRequest> _imageBatch = new Queue<AssetRequest>();

        public static void GenerateImage(AssetRequest r)
        {
            if (!Begin("Grok image")) return;
            StartImage(r);
        }

        /// <summary>Queue several requests for image generation; they run sequentially (one at a time).</summary>
        public static void GenerateImagesBatch(IEnumerable<AssetRequest> requests)
        {
            foreach (var r in requests) if (r != null) _imageBatch.Enqueue(r);
            PumpImageBatch();
        }

        private static void PumpImageBatch()
        {
            if (Busy || _imageBatch.Count == 0) return;
            var r = _imageBatch.Dequeue();
            if (r == null) { PumpImageBatch(); return; }
            if (!Begin($"Grok image (batch, {_imageBatch.Count} left)")) return;
            StartImage(r);
        }

        // Composes the prompt from the shared StyleProfile + part template, runs the right Grok
        // path (text→image, or reference-image edit if the profile opts in), saves the PNG, sets
        // the Meshy prompt, and advances the stage. Assumes Begin() was already called.
        private static void StartImage(AssetRequest r)
        {
            string prompt = StyleComposer.ComposeImagePrompt(r);
            r.grokImagePrompt = prompt;

            string key = GenerationServices.Secrets.grokApiKey;
            string model = GenerationServices.GrokModel;
            string endpoint = GenerationServices.GrokEndpoint;
            string id = SafeId(r);

            var profile = StyleComposer.ResolveProfile();
            string aspect = profile != null ? profile.aspectRatio : "1:1";
            string resolution = profile != null ? profile.resolution : "1k";
            bool useRef = profile != null && profile.useReferenceImage && profile.referenceImage != null;
            string refPath = useRef ? AssetDatabase.GetAssetPath(profile.referenceImage) : null;

            Task.Run(async () =>
            {
                if (useRef)
                {
                    byte[] refBytes = File.ReadAllBytes(refPath);
                    return await GrokImageService.EditAsync(prompt, refBytes, key, model, "", aspect, resolution);
                }
                return await GrokImageService.GenerateAsync(prompt, key, model, endpoint, aspect, resolution);
            }).ContinueWith(t => GenerationHttp.OnMainThread(() =>
            {
                try
                {
                    if (t.IsFaulted) throw GenerationHttp.Unwrap(t.Exception);
                    string folder = AssetPipelineImporter.EnsureFolder(ImagesFolder);
                    string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{id}.png");
                    File.WriteAllBytes(path, t.Result);
                    AssetDatabase.ImportAsset(path);
                    r.chosenImage = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    r.imagePath = path;
                    r.meshyPrompt = StyleComposer.ComposeMeshyPrompt(r);   // ready for the next stage
                    if (r.stage < PipelineStage.ImageChosen) r.stage = PipelineStage.ImageChosen;
                    Persist(r);
                    Done($"Image saved: {path}");
                }
                catch (Exception e) { Fail("Grok image", e); }
            }));
        }

        public static void GenerateModel(AssetRequest r)
        {
            // Reject-gate: never build a 3D model from a request the human rejected in review.
            // Non-modal (log, not dialog) so bridge/automation calls can't hang on a popup.
            if (r.lifecycle == Lifecycle.Rejected)
            {
                Debug.LogWarning($"[AssetPipeline] '{r.id}' is marked Rejected — skipping Meshy. Restore it to Active (or approve it) first.");
                return;
            }
            if (r.chosenImage == null)
            {
                EditorUtility.DisplayDialog("Need an image first",
                    "Set a Chosen image (generate one with Grok, or drag one in) before generating the 3D model.", "OK");
                return;
            }
            if (!Begin("Meshy 3D")) return;

            string imgPath = AssetDatabase.GetAssetPath(r.chosenImage);
            string key = GenerationServices.Secrets.meshyApiKey;
            string aiModel = GenerationServices.MeshyAiModel;
            string endpoint = GenerationServices.MeshyEndpoint;
            string slotName = SlotFolderName(r);
            string id = SafeId(r);

            Task.Run(async () =>
            {
                byte[] png = File.ReadAllBytes(imgPath);
                return await MeshyModelService.GenerateAsync(png, key, aiModel, endpoint, Progress);
            }).ContinueWith(t => GenerationHttp.OnMainThread(() =>
            {
                try
                {
                    if (t.IsFaulted) throw GenerationHttp.Unwrap(t.Exception);
                    var result = t.Result;
                    string folder = AssetPipelineImporter.EnsureFolder($"Assets/Generated/Meshy/{slotName}");
                    string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{id}.{result.ext}");
                    File.WriteAllBytes(path, result.data);
                    AssetDatabase.ImportAsset(path);
                    r.importedModel = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (result.textureData != null) AssignBaseColor(path, result.textureData);
                    if (r.stage < PipelineStage.ModelImported) r.stage = PipelineStage.ModelImported;
                    Persist(r);
                    Done($"Model imported: {path}");
                }
                catch (Exception e) { Fail("Meshy 3D", e); }
            }));
        }

        public static void GenerateDescription(AssetRequest r)
        {
            if (!Begin("Description")) return;
            string prompt = PromptTemplates.DescriptionBrief(r);
            string key = GenerationServices.Secrets.anthropicApiKey;
            string model = GenerationServices.AnthropicModel;
            string endpoint = GenerationServices.AnthropicEndpoint;

            Task.Run(() => AnthropicDescriptionService.GenerateAsync(prompt, key, model, endpoint))
                .ContinueWith(t => GenerationHttp.OnMainThread(() =>
                {
                    try
                    {
                        if (t.IsFaulted) throw GenerationHttp.Unwrap(t.Exception);
                        r.description = (t.Result ?? "").Trim();
                        if (r.stage < PipelineStage.Described) r.stage = PipelineStage.Described;
                        Persist(r);
                        Done("Description written.");
                    }
                    catch (Exception e) { Fail("Description", e); }
                }));
        }

        // --- helpers ---

        private static string SafeId(AssetRequest r) => string.IsNullOrEmpty(r.id) ? "asset" : r.id;

        private static string SlotFolderName(AssetRequest r) =>
            r.category == AssetCategory.BodyPart
                ? ((PartSlot)r.targetSlot).ToString()
                : ((EquipmentSlot)r.targetSlot).ToString();

        // Save Meshy's baked grayscale base-color texture next to the model and assign it to the model's material
        // (Meshy ships the FBX textureless — the map is a separate URL). The grayscale is the part's source of truth
        // + the region-tint shader's luminance. Remaps the FBX's embedded material to a new Lit material that
        // references the texture, so the staged mesh renders with its grayscale instead of bare white.
        private static void AssignBaseColor(string modelPath, byte[] texPng)
        {
            try
            {
                string dir = Path.GetDirectoryName(modelPath).Replace('\\', '/');
                string baseName = Path.GetFileNameWithoutExtension(modelPath);
                string texPath = $"{dir}/{baseName}_BaseColor.png";
                File.WriteAllBytes(texPath, texPng);
                AssetDatabase.ImportAsset(texPath);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex == null) return;

                string matName = null;
                foreach (var a in AssetDatabase.LoadAllAssetsAtPath(modelPath))
                    if (a is Material m) { matName = m.name; break; }
                if (string.IsNullOrEmpty(matName)) return;

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetTexture("_BaseMap", tex);
                string matPath = $"{dir}/{baseName}_Mat.mat";
                AssetDatabase.CreateAsset(mat, matPath);

                var imp = (ModelImporter)AssetImporter.GetAtPath(modelPath);
                imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), matName), mat);
                imp.SaveAndReimport();
                Debug.Log($"[AssetPipeline] base-color texture grabbed + assigned: {texPath}");
            }
            catch (Exception e) { Debug.LogWarning("[AssetPipeline] base-color assign failed: " + e.Message); }
        }

        private static void Persist(AssetRequest r)
        {
            EditorUtility.SetDirty(r);
            AssetDatabase.SaveAssets();
        }

        private static bool Begin(string what)
        {
            if (Busy)
            {
                EditorUtility.DisplayDialog("Busy", "A generation is already running — wait for it to finish.", "OK");
                return false;
            }
            Busy = true;
            Status = $"{what}: starting…";
            Changed?.Invoke();
            return true;
        }

        private static void Progress(string s) => GenerationHttp.OnMainThread(() =>
        {
            Status = s;
            Changed?.Invoke();
        });

        private static void Done(string s)
        {
            Busy = false;
            Status = s;
            Debug.Log("[AssetPipeline] " + s);
            Changed?.Invoke();
            PumpImageBatch();
        }

        private static void Fail(string what, Exception e)
        {
            Busy = false;
            Status = what + " failed.";
            Debug.LogError($"[AssetPipeline] {what} failed: {e.Message}");
            EditorUtility.DisplayDialog(what + " failed", e.Message, "OK");
            Changed?.Invoke();
            PumpImageBatch();   // keep a batch going even if one item fails
        }
    }
}
