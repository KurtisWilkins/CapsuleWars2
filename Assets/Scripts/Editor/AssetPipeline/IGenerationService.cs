namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Seam for optional external generation (image, 3D model, description text).
    /// Concrete HTTP implementations are future work; until one exists and a key is
    /// configured, the Asset Pipeline window only offers "Copy prompt" (assisted-manual).
    /// When <see cref="IsConfigured"/> is true the window also shows a "Generate" button.
    /// </summary>
    public interface IGenerationService
    {
        string DisplayName { get; }
        bool IsConfigured { get; }
    }

    /// <summary>
    /// Facade reporting which generation services are available based on
    /// <see cref="SecretsConfig"/>. The window reads these flags; no keys → all false
    /// → assisted-manual everywhere. Call <see cref="Reload"/> after editing secrets.
    /// </summary>
    public static class GenerationServices
    {
        private static SecretsConfig _secrets;
        public static SecretsConfig Secrets => _secrets ??= SecretsConfig.Load();
        public static void Reload() => _secrets = SecretsConfig.Load();

        /// <summary>Grok (or similar) image generation for the concept image.</summary>
        public static bool ImageGenAvailable => Secrets.HasGrok;

        /// <summary>Meshy image-to-3D generation.</summary>
        public static bool ModelGenAvailable => Secrets.HasMeshy;

        /// <summary>Anthropic API for in-editor description generation (optional; Q4b).</summary>
        public static bool DescriptionGenAvailable => Secrets.HasAnthropic;

        public static bool AnyAvailable => ImageGenAvailable || ModelGenAvailable || DescriptionGenAvailable;
    }
}
