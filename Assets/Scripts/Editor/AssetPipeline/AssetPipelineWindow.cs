using System.Collections.Generic;
using System.Linq;
using System.Text;
using CapsuleWars.Core;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// The Asset Pipeline queue (Tools ▸ CapsuleWars ▸ Asset Pipeline). Lists every
    /// <see cref="AssetRequest"/> grouped by <see cref="PipelineStage"/> so you can see
    /// the whole queue and what each item is waiting on. Per item you can add/advance/
    /// rollback, copy the Grok/Meshy prompts to the clipboard, paste back the chosen
    /// image + imported model, set the category/slot, create + wire the item, and
    /// read/edit the description. Generate buttons appear only when an API key is
    /// configured (see <see cref="GenerationServices"/>); otherwise it's assisted-manual.
    /// </summary>
    public class AssetPipelineWindow : EditorWindow
    {
        private const string RequestsFolder = "Assets/Editor/AssetPipeline/Requests";

        private readonly List<AssetRequest> _requests = new List<AssetRequest>();
        private readonly Dictionary<PipelineStage, bool> _stageFoldout = new Dictionary<PipelineStage, bool>();
        private readonly HashSet<AssetRequest> _expanded = new HashSet<AssetRequest>();
        private Vector2 _scroll;

        // New-request form state.
        private bool _showNew;
        private string _newTitle = "";
        private string _newText = "";
        private AssetCategory _newCategory = AssetCategory.Undecided;
        private bool _bulkMode;
        private string _bulkText = "";

        [MenuItem("Tools/CapsuleWars/Asset Pipeline")]
        public static void Open()
        {
            var w = GetWindow<AssetPipelineWindow>("Asset Pipeline");
            w.minSize = new Vector2(420, 480);
            w.Refresh();
        }

        private void OnEnable() => Refresh();
        private void OnFocus() => Refresh();

        private void Refresh()
        {
            _requests.Clear();
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(AssetRequest)}"))
            {
                var r = AssetDatabase.LoadAssetAtPath<AssetRequest>(AssetDatabase.GUIDToAssetPath(guid));
                if (r != null) _requests.Add(r);
            }
            _requests.Sort((a, b) => string.Compare(a.title, b.title, System.StringComparison.OrdinalIgnoreCase));
            GenerationServices.Reload();
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_showNew) DrawNewRequestForm();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_requests.Count == 0)
            {
                EditorGUILayout.HelpBox("No requests yet. Click \"+ New request\" to add one.", MessageType.Info);
            }
            else
            {
                foreach (PipelineStage stage in System.Enum.GetValues(typeof(PipelineStage)))
                    DrawStageSection(stage);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(_showNew ? "− New request" : "+ New request", EditorStyles.toolbarButton, GUILayout.Width(110)))
                _showNew = !_showNew;
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70))) Refresh();
            if (GUILayout.Button("API Keys…", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                GenerationKeysWindow.Open();
            }
            GUILayout.FlexibleSpace();
            string mode = GenerationServices.AnyAvailable ? "API: configured" : "Mode: assisted-manual";
            GUILayout.Label(mode, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNewRequestForm()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_bulkMode ? "New requests (bulk)" : "New request", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            _bulkMode = GUILayout.Toggle(_bulkMode, "Bulk (one per line)", EditorStyles.miniButton, GUILayout.Width(130));
            EditorGUILayout.EndHorizontal();

            _newCategory = (AssetCategory)EditorGUILayout.EnumPopup("Category (applied to all)", _newCategory);

            if (_bulkMode)
            {
                EditorGUILayout.LabelField("One request per line.  Optional details after a pipe:  Title | request text",
                    EditorStyles.miniLabel);
                _bulkText = EditorGUILayout.TextArea(_bulkText, GUILayout.MinHeight(110));

                int count = CountBulkLines(_bulkText);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(count == 0))
                {
                    if (GUILayout.Button(count > 0 ? $"Create {count} request{(count == 1 ? "" : "s")}" : "Create requests", GUILayout.Width(160)))
                        CreateBulk();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                _newTitle = EditorGUILayout.TextField("Title", _newTitle);
                EditorGUILayout.LabelField("What do you want to build?");
                _newText = EditorGUILayout.TextArea(_newText, GUILayout.MinHeight(48));

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newTitle)))
                {
                    if (GUILayout.Button("Create request", GUILayout.Width(120))) CreateRequest();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private static int CountBulkLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            int n = 0;
            foreach (var line in text.Split('\n'))
                if (!string.IsNullOrWhiteSpace(line)) n++;
            return n;
        }

        private void DrawStageSection(PipelineStage stage)
        {
            var items = _requests.Where(r => r.stage == stage).ToList();
            if (items.Count == 0) return;

            if (!_stageFoldout.ContainsKey(stage)) _stageFoldout[stage] = true;
            _stageFoldout[stage] = EditorGUILayout.Foldout(_stageFoldout[stage], $"{stage}  ({items.Count})", true, EditorStyles.foldoutHeader);
            if (!_stageFoldout[stage]) return;

            EditorGUI.indentLevel++;
            foreach (var r in items) DrawRequest(r);
            EditorGUI.indentLevel--;
        }

        private void DrawRequest(AssetRequest r)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();

            // Header row.
            EditorGUILayout.BeginHorizontal();
            bool open = _expanded.Contains(r);
            bool newOpen = EditorGUILayout.Foldout(open, $"{r.title}   [{r.category}]", true);
            if (newOpen != open) { if (newOpen) _expanded.Add(r); else _expanded.Remove(r); }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("◄", GUILayout.Width(24))) Rollback(r);
            if (GUILayout.Button("►", GUILayout.Width(24))) Advance(r);
            if (GUILayout.Button("Ping", GUILayout.Width(40))) EditorGUIUtility.PingObject(r);
            EditorGUILayout.EndHorizontal();

            if (newOpen)
            {
                EditorGUILayout.LabelField("Request", EditorStyles.miniBoldLabel);
                r.requestText = EditorGUILayout.TextArea(r.requestText, GUILayout.MinHeight(36));
                r.category = (AssetCategory)EditorGUILayout.EnumPopup("Category", r.category);
                DrawSlotField(r);

                DrawConcepts(r);
                DrawPrompts(r);
                DrawImportFields(r);
                DrawDescription(r);

                EditorGUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create / Wire item")) CreateAndWire(r);
                using (new EditorGUI.DisabledScope(r.createdItem == null))
                {
                    if (GUILayout.Button("Open item", GUILayout.Width(80))) EditorGUIUtility.PingObject(r.createdItem);
                }
                if (GUILayout.Button("Delete", GUILayout.Width(60))) { DeleteRequest(r); GUIUtility.ExitGUI(); }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(r);
            EditorGUILayout.EndVertical();
        }

        private void DrawSlotField(AssetRequest r)
        {
            if (r.category == AssetCategory.BodyPart)
            {
                r.targetSlot = (int)(PartSlot)EditorGUILayout.EnumPopup("Slot", (PartSlot)Mathf.Clamp(r.targetSlot, 0, 5));
            }
            else if (r.category != AssetCategory.Undecided)
            {
                r.targetSlot = (int)(EquipmentSlot)EditorGUILayout.EnumPopup("Slot", (EquipmentSlot)Mathf.Clamp(r.targetSlot, 0, 7));
                r.attachSocketName = EditorGUILayout.TextField("Attach socket", r.attachSocketName);
            }
        }

        private void DrawConcepts(AssetRequest r)
        {
            if (r.concepts == null || r.concepts.Count == 0) return;
            EditorGUILayout.LabelField("Concepts (pick one)", EditorStyles.miniBoldLabel);
            for (int i = 0; i < r.concepts.Count; i++)
            {
                var c = r.concepts[i];
                EditorGUILayout.BeginHorizontal();
                bool chosen = r.chosenConceptIndex == i;
                bool toggled = GUILayout.Toggle(chosen, "", GUILayout.Width(18));
                if (toggled && !chosen) r.chosenConceptIndex = i;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"{i + 1}. {c.name}", EditorStyles.boldLabel);
                if (!string.IsNullOrEmpty(c.visualDesc)) EditorGUILayout.LabelField(c.visualDesc, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPrompts(AssetRequest r)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Prompts", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Grok prompt"))
            {
                r.grokImagePrompt = PromptTemplates.GrokImagePrompt(r);
                EditorGUIUtility.systemCopyBuffer = r.grokImagePrompt;
                ShowNotification(new GUIContent("Grok prompt copied"));
            }
            if (GenerationServices.ImageGenAvailable && GUILayout.Button("Generate", GUILayout.Width(80)))
                NotImplemented("Grok image generation");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Meshy prompt"))
            {
                r.meshyPrompt = PromptTemplates.MeshyPrompt(r);
                EditorGUIUtility.systemCopyBuffer = r.meshyPrompt;
                ShowNotification(new GUIContent("Meshy prompt copied"));
            }
            if (GenerationServices.ModelGenAvailable && GUILayout.Button("Generate", GUILayout.Width(80)))
                NotImplemented("Meshy 3D generation");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawImportFields(AssetRequest r)
        {
            EditorGUILayout.Space(2);
            r.chosenImage = (Texture2D)EditorGUILayout.ObjectField("Chosen image", r.chosenImage, typeof(Texture2D), false);
            r.importedModel = (GameObject)EditorGUILayout.ObjectField("Imported model", r.importedModel, typeof(GameObject), false);
            if (r.generatedPrefab != null)
                EditorGUILayout.ObjectField("Generated prefab", r.generatedPrefab, typeof(GameObject), false);
        }

        private void DrawDescription(AssetRequest r)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description", EditorStyles.miniBoldLabel);
            if (GenerationServices.DescriptionGenAvailable && GUILayout.Button("Generate", GUILayout.Width(80)))
                NotImplemented("Anthropic description generation");
            EditorGUILayout.EndHorizontal();
            r.description = EditorGUILayout.TextArea(r.description, GUILayout.MinHeight(48));
        }

        // --- actions ---

        private AssetRequest CreateRequestAsset(string title, string text, AssetCategory category, string folder)
        {
            var r = CreateInstance<AssetRequest>();
            r.id = Slugify(title);
            r.title = title.Trim();
            r.requestText = text;
            r.category = category;
            r.stage = PipelineStage.Requested;
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{(string.IsNullOrEmpty(r.id) ? "request" : r.id)}.asset");
            AssetDatabase.CreateAsset(r, path);   // registered immediately so the next GenerateUniqueAssetPath dedupes
            return r;
        }

        private void CreateRequest()
        {
            string folder = AssetPipelineImporter.EnsureFolder(RequestsFolder);
            var r = CreateRequestAsset(_newTitle, _newText, _newCategory, folder);
            AssetDatabase.SaveAssets();

            _newTitle = ""; _newText = ""; _newCategory = AssetCategory.Undecided; _showNew = false;
            Refresh();
            _expanded.Add(r);
        }

        private void CreateBulk()
        {
            string folder = AssetPipelineImporter.EnsureFolder(RequestsFolder);
            int created = 0;
            foreach (var raw in _bulkText.Split('\n'))
            {
                string line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string title = line, text = "";
                int bar = line.IndexOf('|');
                if (bar >= 0) { title = line.Substring(0, bar).Trim(); text = line.Substring(bar + 1).Trim(); }
                if (string.IsNullOrEmpty(title)) continue;

                CreateRequestAsset(title, text, _newCategory, folder);
                created++;
            }
            AssetDatabase.SaveAssets();

            _bulkText = ""; _bulkMode = false; _newCategory = AssetCategory.Undecided; _showNew = false;
            Refresh();
            ShowNotification(new GUIContent($"Created {created} request{(created == 1 ? "" : "s")}"));
        }

        private void Advance(AssetRequest r)
        {
            var values = (PipelineStage[])System.Enum.GetValues(typeof(PipelineStage));
            int i = System.Array.IndexOf(values, r.stage);
            if (i < values.Length - 1) { r.stage = values[i + 1]; EditorUtility.SetDirty(r); }
        }

        private void Rollback(AssetRequest r)
        {
            var values = (PipelineStage[])System.Enum.GetValues(typeof(PipelineStage));
            int i = System.Array.IndexOf(values, r.stage);
            if (i > 0) { r.stage = values[i - 1]; EditorUtility.SetDirty(r); }
        }

        private void CreateAndWire(AssetRequest r)
        {
            var result = AssetPipelineImporter.CreateAndWire(r);
            if (result.ok)
            {
                if (r.stage < PipelineStage.Categorized) r.stage = PipelineStage.Categorized;
                EditorUtility.SetDirty(r);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Asset Pipeline", result.message, "OK");
                if (result.createdItem != null) EditorGUIUtility.PingObject(result.createdItem);
                Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("Asset Pipeline — can't create yet", result.message, "OK");
            }
        }

        private void DeleteRequest(AssetRequest r)
        {
            if (!EditorUtility.DisplayDialog("Delete request",
                    $"Delete the pipeline request \"{r.title}\"?\n\n(This removes only the request record, not any created item/prefab.)",
                    "Delete", "Cancel"))
                return;
            string path = AssetDatabase.GetAssetPath(r);
            _expanded.Remove(r);
            AssetDatabase.DeleteAsset(path);
            Refresh();
        }

        private static void NotImplemented(string what) =>
            EditorUtility.DisplayDialog("Not implemented yet",
                $"{what} isn't wired up yet. Use \"Copy prompt\" and run it manually for now.\n" +
                "Add an IGenerationService implementation to enable this button.", "OK");

        private static string Slugify(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var sb = new StringBuilder();
            bool capNext = true;
            foreach (char ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(capNext ? char.ToUpperInvariant(ch) : ch);
                    capNext = false;
                }
                else capNext = true;
            }
            return sb.ToString();
        }
    }
}
