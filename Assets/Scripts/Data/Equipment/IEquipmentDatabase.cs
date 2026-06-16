namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// Resolves <see cref="Equipment_SO"/> by stable id — the equipment
    /// counterpart of <c>IUnitDefinitionDatabase</c> / <c>IPartDatabase</c>.
    /// Used by <c>UnitFactory</c> to rebuild a unit's equipment from a DTO.
    /// </summary>
    public interface IEquipmentDatabase
    {
        Equipment_SO GetEquipment(string equipmentId);
    }
}
