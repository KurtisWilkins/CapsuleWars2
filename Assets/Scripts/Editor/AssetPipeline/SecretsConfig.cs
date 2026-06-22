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
    }
}
