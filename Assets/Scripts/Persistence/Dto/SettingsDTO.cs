using System;
using UnityEngine;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// Player settings (Docs/14_Persistence.md: settings.json). Audio volumes,
    /// graphics quality, and display. Input rebinds are a later addition.
    /// </summary>
    [Serializable]
    public class SettingsDTO
    {
        public int SaveVersion = 1;

        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float SfxVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 1f;

        /// <summary>QualitySettings level index, or -1 to leave the platform default.</summary>
        public int QualityLevel = -1;

        public bool Fullscreen = true;

        /// <summary>Clamp volumes to [0,1] (call after loading or editing).</summary>
        public void Clamp()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
        }
    }
}
