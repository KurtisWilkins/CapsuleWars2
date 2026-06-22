using System;
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

        public static void GenerateImage(AssetRequest r)
        {
            if (!Begin("Grok image")) return;
            string prompt = PromptTemplates.GrokImagePrompt(r);
            if (string.IsNullOrEmpty(r.grokImagePrompt)) r.grokImagePrompt = prompt;
            string key = GenerationServices.Secrets.grokApiKey;
            string model = GenerationServices.GrokModel;
            string endpoint = GenerationServices.GrokEndpoint;
            string id = SafeId(r);

            Task.Run(() => GrokImageService.GenerateAsync(prompt, key, model, endpoint))
                .ContinueWith(t => GenerationHttp.OnMainThread(() =>
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
                        if (r.stage < PipelineStage.ImageChosen) r.stage = PipelineStage.ImageChosen;
                        Persist(r);
                        Done($"Image saved: {path}");
                    }
                    catch (Exception e) { Fail("Grok image", e); }
                }));
        }

        public static void GenerateModel(AssetRequest r)
        {
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
        }

        private static void Fail(string what, Exception e)
        {
            Busy = false;
            Status = what + " failed.";
            Debug.LogError($"[AssetPipeline] {what} failed: {e.Message}");
            EditorUtility.DisplayDialog(what + " failed", e.Message, "OK");
            Changed?.Invoke();
        }
    }
}
