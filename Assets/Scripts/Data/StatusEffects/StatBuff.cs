using System;
using CapsuleWars.Core;

namespace CapsuleWars.Data.StatusEffects
{
    /// <summary>
    /// One stat modifier on a status effect (or, later, on equipment).
    /// e.g. <c>{stat=Atk, modType=Percent, amount=25}</c> = +25% Atk.
    /// </summary>
    [Serializable]
    public struct StatBuff
    {
        public StatType stat;
        public StatBuffModType modType;
        public float amount;
    }
}
