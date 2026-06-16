using CapsuleWars.Persistence.Dto;
using UnityEngine;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Applies a <see cref="SettingsDTO"/> to the running game: master volume via
    /// the global AudioListener, graphics quality, and fullscreen. Per-channel
    /// (sfx/music) routing lands when an AudioMixer is added; for now master
    /// volume is the global control.
    /// </summary>
    public static class SettingsService
    {
        public static void Apply(SettingsDTO s)
        {
            if (s == null) return;
            s.Clamp();

            AudioListener.volume = s.MasterVolume;

            if (s.QualityLevel >= 0 && s.QualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(s.QualityLevel, applyExpensiveChanges: true);

            Screen.fullScreen = s.Fullscreen;
        }
    }
}
