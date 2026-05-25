using System;
using System.IO;
using CapsuleWars.Persistence.Dto;
using Newtonsoft.Json;
using UnityEngine;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Loads + saves the player's legacy profile. Static singleton:
    /// <see cref="Current"/> lazy-loads on first access, <see cref="Save"/>
    /// writes atomically (tmp file + rename) so a crash mid-save doesn't
    /// corrupt the on-disk file.
    /// File lives at <c>Application.persistentDataPath/legacy.json</c>.
    /// </summary>
    public static class LegacyStore
    {
        private const string FileName = "legacy.json";
        private static LegacyProfileDTO current;

        public static LegacyProfileDTO Current
        {
            get
            {
                if (current == null) current = Load();
                return current;
            }
        }

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static LegacyProfileDTO Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    var fresh = new LegacyProfileDTO();
                    SaveTo(fresh, FilePath);
                    return fresh;
                }

                var json = File.ReadAllText(FilePath);
                var profile = JsonConvert.DeserializeObject<LegacyProfileDTO>(json, GetSettings())
                              ?? new LegacyProfileDTO();
                if (profile.Units == null) profile.Units = new System.Collections.Generic.List<LegacyUnitDTO>();
                return profile;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LegacyStore] Failed to load legacy profile from {FilePath}: {ex.Message}");
                return new LegacyProfileDTO();
            }
        }

        public static void Save()
        {
            if (current == null) return;
            SaveTo(current, FilePath);
        }

        private static void SaveTo(LegacyProfileDTO profile, string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(profile, GetSettings());
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LegacyStore] Failed to save legacy profile to {path}: {ex.Message}");
            }
        }

        /// <summary>Reset the cached profile so the next Current access reloads from disk. For tests.</summary>
        public static void Clear()
        {
            current = null;
        }

        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }
    }
}
