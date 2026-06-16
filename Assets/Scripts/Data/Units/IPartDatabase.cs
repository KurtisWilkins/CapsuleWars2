namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Resolves customization assets by their stable ids — the parts/palette
    /// counterpart of <see cref="IUnitDefinitionDatabase"/>. Used by UnitFactory
    /// to rebuild a generated/customized unit's visuals from a UnitDTO's part ids.
    /// <see cref="PartCatalog_SO"/> implements this.
    /// </summary>
    public interface IPartDatabase
    {
        /// <summary>The part with <paramref name="partId"/>, or null if unknown.</summary>
        BodyPart_SO GetPart(string partId);

        /// <summary>The palette with <paramref name="paletteId"/>, or null if unknown.</summary>
        Palette_SO GetPalette(string paletteId);
    }
}
