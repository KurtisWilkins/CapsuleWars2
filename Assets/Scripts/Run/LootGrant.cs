using CapsuleWars.Data.Equipment;
using CapsuleWars.Persistence.Dto;

namespace CapsuleWars.Run
{
    /// <summary>
    /// Rolls a <see cref="LootTable_SO"/> and grants the dropped items into a run's loose inventory (BTS-G).
    /// The Run-layer bridge between the pure Data-layer <see cref="LootRoller"/> and <see cref="RunState"/>
    /// storage: rolls deterministically from <paramref name="seed"/>, converts each rolled
    /// <c>EquipmentInstance</c> to its save-shaped <see cref="UnitEquipmentDTO"/>, and adds it. The reward hook
    /// (BattleNodeReturn on a win, EventPanel on a treasure) calls this and then <c>RunSession.Save()</c> —
    /// the win path does not otherwise persist before the scene load. Returns the number of items granted.
    /// </summary>
    public static class LootGrant
    {
        public static int GrantTo(RunState state, LootTable_SO table, int seed)
        {
            if (state == null || table == null) return 0;

            var drops = LootRoller.Roll(table, seed);
            int granted = 0;
            for (int i = 0; i < drops.Count; i++)
            {
                var inst = drops[i];
                if (inst == null) continue;
                state.AddItem(UnitEquipmentDTO.From(inst.Slot, inst));
                granted++;
            }
            return granted;
        }
    }
}
