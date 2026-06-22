using System;
using System.IO;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Optional API keys for the generation services. Loaded from
    /// <c>Tools/Editor/SecretsConfig.json</c> (relative to the project root,
    /// git-ignored) with environment-variable fallback. Absent by default →
    /// the pipeline runs assisted-manual (copy prompts; run Grok/Meshy yourself).
    /// Per Docs/16_AssetGeneration.md.
    /// </summary>
    [Serializable]
    public class SecretsConfig
    {
        public string grokApiKey;
        public string meshyApiKey;
        public string anthropicApiKey;

        // Optional overrides (blank = use the code defaults in each service). Lets you fix a
        // changed model name or endpoint without editing code.
        public string grokModel;
        public string meshyAiModel;
        public string anthropicModel;
        public string grokEndpoint;
        public string meshyEndpoint;
        public string anthropicEndpoint;

        /// <summary>Path relative to the project root (the folder above Assets/).</summary>
        public const string RelativePath = "Tools/Editor/SecretsConfig.json";

        public bool HasGrok => !string.IsNullOrEmpty(grokApiKey);
        public bool HasMeshy => !string.IsNullOrEmpty(meshyApiKey);
        public bool HasAnthropic => !string.IsNullOrEmpty(anthropicApiKey);

        public static string AbsolutePath
        {
            get
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(projectRoot, RelativePath);
            }
        }

        public static SecretsConfig Load()
        {
            var cfg = new SecretsConfig();
            try
            {
                string path = AbsolutePath;
                if (File.Exists(path))
                    cfg = JsonUtility.FromJson<SecretsConfig>(File.ReadAllText(path)) ?? new SecretsConfig();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AssetPipeline] Could not read {RelativePath}: {e.Message}");
            }

            cfg.grokApiKey = Fallback(cfg.grokApiKey, "GROK_API_KEY", "XAI_API_KEY");
            cfg.meshyApiKey = Fallback(cfg.meshyApiKey, "MESHY_API_KEY");
            cfg.anthropicApiKey = Fallback(cfg.anthropicApiKey, "ANTHROPIC_API_KEY");
            return cfg;
        }

        private static string Fallback(string current, params string[] envNames)
        {
            if (!string.IsNullOrEmpty(current)) return current;
            foreach (var n in envNames)
            {
                var v = Environment.GetEnvironmentVariable(n);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return current;
        }

        public static bool FileExists => File.Exists(AbsolutePath);

        /// <summary>
        /// Read only what's stored in the file (no environment fallback). Use this
        /// for the editor UI so env-supplied keys are never written back to disk.
        /// </summary>
        public static SecretsConfig LoadFromFileOnly()
        {
            try
            {
                if (File.Exists(AbsolutePath))
                    return JsonUtility.FromJson<SecretsConfig>(File.ReadAllText(AbsolutePath)) ?? new SecretsConfig();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AssetPipeline] Could not read {RelativePath}: {e.Message}");
            }
            return new SecretsConfig();
        }

        /// <summary>
        /// Write the keys to <c>Tools/Editor/SecretsConfig.json</c> (created if needed).
        /// The path is OUTSIDE Assets/ (never included in a player build) and matched by
        /// .gitignore (never committed). Plaintext, local-only — like a .env file.
        /// </summary>
        public void Save()
        {
            string path = AbsolutePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(this, true));
            Debug.Log($"[AssetPipeline] Saved API keys to {RelativePath} (git-ignored, outside Assets — not shipped).");
        }

        public static void DeleteFile()
        {
            try
            {
                if (File.Exists(AbsolutePath)) File.Delete(AbsolutePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AssetPipeline] Could not delete {RelativePath}: {e.Message}");
            }
        }
    }
}
