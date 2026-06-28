#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Review gallery for generated assets — see each request's chosen image as it lands and Approve / Reject it
    /// with a note, writing straight to the <see cref="AssetRequest"/> lifecycle (Active/Archived/Rejected) + notes.
    /// This IS the human archive/reject gate the generation contract ends on, in one scrollable window with live
    /// auto-refresh. Open via <b>Tools ▸ CapsuleWars ▸ Asset Review</b>.
    /// </summary>
    public class AssetReviewWindow : EditorWindow
    {
        [MenuItem("Tools/CapsuleWars/Asset Review")]
        public static void Open() => GetWindow<AssetReviewWindow>("Asset Review");

        private static readonly string[] FilterLabels = { "All", "Pending", "Approved", "Rejected" };

        private Vector2 scroll;
        private int filter;
        private bool autoRefresh = true;
        private string search = "";
        private List<AssetRequest> requests = new List<AssetRequest>();
        private double lastRefresh;

        private void OnEnable() => Refresh();
        private void OnFocus() => Refresh();

        private void Update()
        {
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefresh > 2.0)
            {
                Refresh();
                Repaint();
            }
        }

        private void Refresh()
        {
            requests = AssetDatabase.FindAssets("t:AssetRequest")
                .Select(g => AssetDatabase.LoadAssetAtPath<AssetRequest>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(r => r != null && r.chosenImage != null)
                .OrderBy(r => r.id)
                .ToList();
            lastRefresh = EditorApplication.timeSinceStartup;
        }

        private bool Passes(AssetRequest r)
        {
            if (!string.IsNullOrEmpty(search) &&
                (r.id ?? "").IndexOf(search, System.StringComparison.OrdinalIgnoreCase) < 0 &&
                (r.title ?? "").IndexOf(search, System.StringComparison.OrdinalIgnoreCase) < 0) return false;
            switch (filter)
            {
                case 1: return r.lifecycle == Lifecycle.Active;
                case 2: return r.lifecycle == Lifecycle.Archived;
                case 3: return r.lifecycle == Lifecycle.Rejected;
                default: return true;
            }
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                filter = GUILayout.Toolbar(filter, FilterLabels, EditorStyles.toolbarButton, GUILayout.Width(300));
                GUILayout.Space(8);
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(160));
                GUILayout.FlexibleSpace();
                autoRefresh = GUILayout.Toggle(autoRefresh, "Auto", EditorStyles.toolbarButton);
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) Refresh();
            }

            var shown = requests.Where(Passes).ToList();
            int approved = requests.Count(r => r.lifecycle == Lifecycle.Archived);
            int rejected = requests.Count(r => r.lifecycle == Lifecycle.Rejected);
            int pending = requests.Count(r => r.lifecycle == Lifecycle.Active);
            EditorGUILayout.LabelField($"{shown.Count} shown   ·   {pending} pending, {approved} approved, {rejected} rejected", EditorStyles.miniLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (shown.Count == 0)
                EditorGUILayout.HelpBox("No generated images yet. Generate some, or clear the filter/search.", MessageType.Info);
            foreach (var r in shown) DrawItem(r);
            EditorGUILayout.EndScrollView();
        }

        private void DrawItem(AssetRequest r)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                Rect thumb = GUILayoutUtility.GetRect(150, 150, GUILayout.Width(150), GUILayout.Height(150));
                if (r.chosenImage != null) GUI.DrawTexture(thumb, r.chosenImage, ScaleMode.ScaleToFit);

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(r.title) ? r.id : r.title, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"id: {r.id}    ·    status: {StatusLabel(r.lifecycle)}", EditorStyles.miniLabel);

                    EditorGUILayout.LabelField("Notes / feedback:", EditorStyles.miniLabel);
                    string newNotes = EditorGUILayout.TextArea(r.notes ?? "", GUILayout.Height(38));
                    if (newNotes != r.notes) { r.notes = newNotes; EditorUtility.SetDirty(r); }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUI.backgroundColor = new Color(0.55f, 0.9f, 0.55f);
                        if (GUILayout.Button("✓ Approve")) SetLifecycle(r, Lifecycle.Archived);
                        GUI.backgroundColor = new Color(0.95f, 0.55f, 0.55f);
                        if (GUILayout.Button("✗ Reject")) SetLifecycle(r, Lifecycle.Rejected);
                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button("↺ Reset")) SetLifecycle(r, Lifecycle.Active);
                        if (GUILayout.Button("Ping")) EditorGUIUtility.PingObject(r.chosenImage != null ? (Object)r.chosenImage : r);
                    }
                }
            }
        }

        private static string StatusLabel(Lifecycle l) =>
            l == Lifecycle.Archived ? "APPROVED" : l == Lifecycle.Rejected ? "REJECTED" : "pending";

        private void SetLifecycle(AssetRequest r, Lifecycle to)
        {
            r.lifecycle = to;
            r.lifecycleDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            if (!string.IsNullOrEmpty(r.notes)) r.lifecycleReason = r.notes;
            EditorUtility.SetDirty(r);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
