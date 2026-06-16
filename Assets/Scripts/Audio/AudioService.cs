using UnityEngine;

namespace CapsuleWars.Audio
{
    /// <summary>
    /// One-shot SFX player (Docs/17 M10 audio). Persistent singleton: drop one on
    /// a GameObject (with an AudioSource) and call <see cref="PlayCue"/> from
    /// anywhere to play an <see cref="AudioCueSO"/>. Master volume scales all cues
    /// (driven by the settings layer).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        private AudioSource source;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetMasterVolume(float v) => masterVolume = Mathf.Clamp01(v);

        public void Play(AudioCueSO cue)
        {
            if (cue == null || !cue.HasClips || source == null) return;
            var clip = cue.PickClip();
            if (clip == null) return;
            source.pitch = cue.PickPitch();
            source.PlayOneShot(clip, Mathf.Clamp01(cue.Volume) * masterVolume);
        }

        /// <summary>Play a cue via the active instance, if any. Safe no-op when absent.</summary>
        public static void PlayCue(AudioCueSO cue)
        {
            if (Instance != null) Instance.Play(cue);
        }
    }
}
