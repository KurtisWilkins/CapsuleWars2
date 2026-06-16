using System;
using System.IO;
using CapsuleWars.Persistence.Dto;
using Newtonsoft.Json;
using UnityEngine;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Persists the in-progress run (<see cref="RunStateDTO"/>) to
    /// <c>Application.persistentDataPath/run.json</c>. Writes atomically
    /// (tmp file + rename) like <see cref="LegacyStore"/> so a crash mid-save
    /// can't corrupt the file. Pure DTO persistence — the DTO &lt;-&gt; runtime
    /// mapping lives in the Run assembly (RunState.ToDTO/FromDTO).
    /// </summary>
    public static class RunStore
    {
        private const string FileName = "run.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool Exists() => File.Exists(FilePath);

        public static RunStateDTO Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;
                var json = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<RunStateDTO>(json, GetSettings());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunStore] Failed to load run from {FilePath}: {ex.Message}");
                return null;
            }
        }

        public static void Save(RunStateDTO dto)
        {
            if (dto == null) return;
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(dto, GetSettings());
                var tmp = FilePath + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tmp, FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunStore] Failed to save run to {FilePath}: {ex.Message}");
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunStore] Failed to delete run at {FilePath}: {ex.Message}");
            }
        }

        private static JsonSerializerSettings GetSettings() => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };
    }
}
