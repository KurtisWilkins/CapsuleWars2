using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Owns a unit's stat block. M2 holds simple base stats only; M3 layers
    /// in status effect modifiers, M5 adds element multipliers, M6 adds
    /// equipment + class synergy contributions. Public getters always
    /// return modified values so consumers don't need to know the source.
    /// </summary>
    public class UnitStatusController : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField, Min(1)] private int baseMaxHp = 100;
        [SerializeField, Min(0)] private int baseAtk = 20;
        [SerializeField, Min(0)] private int baseDef = 5;
        [SerializeField, Min(0f)] private float baseSpeed = 3.5f;

        public int MaxHp => baseMaxHp;
        public int Atk => baseAtk;
        public int Def => baseDef;
        public float Speed => baseSpeed;
    }
}
