using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Edit the optional generation API keys (Grok / Meshy / Anthropic) and save them to
    /// the protected <c>Tools/Editor/SecretsConfig.json</c>. That file is OUTSIDE Assets/
    /// (so it's never included in a player build), is matched by .gitignore (never
    /// committed), and is only read by the Editor-only <c>CapsuleWars.Editor</c> assembly
    /// (not compiled into builds). When a key is present the Asset Pipeline window shows
    /// "Generate" buttons; otherwise the pipeline stays assisted-manual.
    /// </summary>
    public class GenerationKeysWindow : EditorWindow
    {
        private string _grok = "";
        private string _meshy = "";
        private string _anthropic = "";
        private string _grokModel = "";
        private string _meshyModel = "";
        private string _anthropicModel = "";
        private bool _reveal;
        private bool _showAdvanced;
        private bool _loaded;

        [MenuItem("Tools/CapsuleWars/Generation API Keys")]
        public static void Open()
        {
            var w = GetWindow<GenerationKeysWindow>(true, "Generation API Keys");
            w.minSize = new Vector2(460, 360);
            w.maxSize = new Vector2(700, 420);
            w.LoadFromDisk();
        }

        private void OnEnable() { if (!_loaded) LoadFromDisk(); }

        private void LoadFromDisk()
        {
            var cfg = SecretsConfig.LoadFromFileOnly();
            _grok = cfg.grokApiKey ?? "";
            _meshy = cfg.meshyApiKey ?? "";
            _anthropic = cfg.anthropicApiKey ?? "";
            _grokModel = cfg.grokModel ?? "";
            _meshyModel = cfg.meshyAiModel ?? "";
            _anthropicModel = cfg.anthropicModel ?? "";
            _loaded = true;
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Keys are saved to a protected file and never shipped:\n" +
                $"• Stored at  {SecretsConfig.RelativePath}  — OUTSIDE Assets/, so it's never in a player build.\n" +
                "• Matched by .gitignore (SecretsConfig.json) — never committed.\n" +
                "• Only the Editor-only assembly reads it — not compiled into builds.\n" +
                "Plaintext, local to your machine (like a .env file). Leave a field blank to disable that service.",
                MessageType.Info);

            EditorGUILayout.Space(4);
            _reveal = EditorGUILayout.ToggleLeft("Show keys", _reveal);
            EditorGUILayout.Space(2);

            _grok = KeyField("Grok / xAI image key", _grok, GenerationServices.ImageGenAvailable);
            _meshy = KeyField("Meshy 3D key", _meshy, GenerationServices.ModelGenAvailable);
            _anthropic = KeyField("Anthropic key (descriptions)", _anthropic, GenerationServices.DescriptionGenAvailable);

            EditorGUILayout.Space(4);
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced — model overrides (optional)");
            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;
                _grokModel = EditorGUILayout.TextField(new GUIContent("Grok model", $"Blank = {GrokImageService.DefaultModel}"), _grokModel);
                _meshyModel = EditorGUILayout.TextField(new GUIContent("Meshy AI model", "Blank = Meshy server default (recommended). Set e.g. latest or meshy-6 only if needed."), _meshyModel);
                _anthropicModel = EditorGUILayout.TextField(new GUIContent("Anthropic model", $"Blank = {AnthropicDescriptionService.DefaultModel}"), _anthropicModel);
                EditorGUILayout.LabelField("Leave blank to use the default. Endpoints can be overridden in the JSON file directly.", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField(
                SecretsConfig.FileExists ? "Status: secrets file exists." : "Status: no secrets file yet (using env vars if set).",
                EditorStyles.miniLabel);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                var cfg = SecretsConfig.LoadFromFileOnly();   // keep any endpoint overrides already in the file
                cfg.grokApiKey = _grok.Trim();
                cfg.meshyApiKey = _meshy.Trim();
                cfg.anthropicApiKey = _anthropic.Trim();
                cfg.grokModel = _grokModel.Trim();
                cfg.meshyAiModel = _meshyModel.Trim();
                cfg.anthropicModel = _anthropicModel.Trim();
                cfg.Save();
                GenerationServices.Reload();
                ShowNotification(new GUIContent("Keys saved"));
                Repaint();
            }
            if (GUILayout.Button("Reload from file")) { LoadFromDisk(); GenerationServices.Reload(); }
            using (new EditorGUI.DisabledScope(!SecretsConfig.FileExists))
            {
                if (GUILayout.Button("Reveal file")) EditorUtility.RevealInFinder(SecretsConfig.AbsolutePath);
                if (GUILayout.Button("Delete file"))
                {
                    if (EditorUtility.DisplayDialog("Delete secrets file",
                            $"Delete {SecretsConfig.RelativePath}? This clears all saved keys.", "Delete", "Cancel"))
                    {
                        SecretsConfig.DeleteFile();
                        _grok = _meshy = _anthropic = "";
                        _grokModel = _meshyModel = _anthropicModel = "";
                        GenerationServices.Reload();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private string KeyField(string label, string value, bool active)
        {
            EditorGUILayout.BeginHorizontal();
            var content = new GUIContent(label, active ? "Configured — Generate enabled." : "Empty — assisted-manual.");
            string result = _reveal
                ? EditorGUILayout.TextField(content, value)
                : EditorGUILayout.PasswordField(content, value);
            GUILayout.Label(string.IsNullOrEmpty(value) ? "—" : "●", GUILayout.Width(16));
            EditorGUILayout.EndHorizontal();
            return result;
        }
    }
}
