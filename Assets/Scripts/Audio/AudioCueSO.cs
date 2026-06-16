using UnityEngine;

namespace CapsuleWars.Audio
{
    /// <summary>
    /// A reusable sound cue (Docs/17 M10 audio): one or more interchangeable
    /// clips plus volume and a pitch range for variation. Authoring data only —
    /// <see cref="AudioService"/> plays it. Hit/heal/KO/UI cues are just separate
    /// AudioCue assets referenced where they're triggered.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioCue", menuName = "CapsuleWars/Audio Cue", order = 5)]
    public class AudioCueSO : ScriptableObject
    {
        [Tooltip("One or more clips; a random one is chosen per play for variation.")]
        [SerializeField] private AudioClip[] clips;

        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        [Tooltip("Random pitch is lerped between x and y per play (1,1 = no variation).")]
        [SerializeField] private Vector2 pitchRange = new Vector2(1f, 1f);

        public bool HasClips => clips != null && clips.Length > 0;
        public float Volume => volume;

        /// <summary>Choose a clip (random across variants). Pass a seeded RNG for determinism; null if empty.</summary>
        public AudioClip PickClip(System.Random rng = null)
        {
            if (!HasClips) return null;
            if (clips.Length == 1) return clips[0];
            int i = rng != null ? rng.Next(clips.Length) : UnityEngine.Random.Range(0, clips.Length);
            return clips[i];
        }

        /// <summary>A pitch within the configured range.</summary>
        public float PickPitch(System.Random rng = null)
        {
            float lo = Mathf.Min(pitchRange.x, pitchRange.y);
            float hi = Mathf.Max(pitchRange.x, pitchRange.y);
            float t = rng != null ? (float)rng.NextDouble() : UnityEngine.Random.value;
            return Mathf.Lerp(lo, hi, t);
        }
    }
}
