using System;
using System.IO;
using CapsuleWars.Persistence.Dto;
using Newtonsoft.Json;
using UnityEngine;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Loads + saves player settings to <c>Application.persistentDataPath/settings.json</c>,
    /// atomically (tmp + rename), mirroring <see cref="LegacyStore"/>. Volumes are
    /// clamped on load.
    /// </summary>
    public static class SettingsStore
    {
        private const string FileName = "settings.json";
        private static SettingsDTO current;

        public static SettingsDTO Current
        {
            get
            {
                if (current == null) current = Load();
                return current;
            }
        }

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static SettingsDTO Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    var fresh = new SettingsDTO();
                    SaveTo(fresh, FilePath);
                    return fresh;
                }

                var json = File.ReadAllText(FilePath);
                var settings = JsonConvert.DeserializeObject<SettingsDTO>(json) ?? new SettingsDTO();
                settings.Clamp();
                return settings;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsStore] Failed to load settings from {FilePath}: {ex.Message}");
                return new SettingsDTO();
            }
        }

        public static void Save()
        {
            if (current == null) return;
            current.Clamp();
            SaveTo(current, FilePath);
        }

        private static void SaveTo(SettingsDTO settings, string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsStore] Failed to save settings to {path}: {ex.Message}");
            }
        }

        /// <summary>Reset the cached settings so the next Current access reloads. For tests.</summary>
        public static void Clear() => current = null;
    }
}
