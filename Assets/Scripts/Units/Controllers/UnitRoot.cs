using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// One per unit prefab. Aggregates references to the unit's controllers
    /// so other systems can fetch them through a single component, and
    /// implements <see cref="IUnitRef"/> for registry + event payloads.
    /// Wires cross-controller event hookups (e.g. Health.OnDowned routes
    /// to Animation.PlayDeath) so the controllers themselves stay decoupled.
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitRoot : MonoBehaviour, IUnitRef
    {
        [SerializeField] private Team team = Team.Player;

        [Header("Controllers")]
        [SerializeField] private UnitStatusController status;
        [SerializeField] private UnitHealthController health;
        [SerializeField] private UnitMovementController movement;
        [SerializeField] private UnitAttackController attack;
        [SerializeField] private UnitAnimationController animation;

        public Team Team => team;
        public UnitStatusController Status => status;
        public UnitHealthController Health => health;
        public UnitMovementController Movement => movement;
        public UnitAttackController Attack => attack;
        public UnitAnimationController Animation => animation;

        GameObject IUnitRef.GameObject => gameObject;
        Transform IUnitRef.Transform => transform;
        Team IUnitRef.Team => team;
        bool IUnitRef.IsDowned => health != null && health.IsDowned;

        private void Awake()
        {
            // Auto-fill missing references for convenience.
            if (status == null) status = GetComponentInChildren<UnitStatusController>();
            if (health == null) health = GetComponentInChildren<UnitHealthController>();
            if (movement == null) movement = GetComponentInChildren<UnitMovementController>();
            if (attack == null) attack = GetComponentInChildren<UnitAttackController>();
            if (animation == null) animation = GetComponentInChildren<UnitAnimationController>();
        }

        private void OnEnable()
        {
            if (health != null) health.OnDowned += OnUnitDowned;
        }

        private void OnDisable()
        {
            if (health != null) health.OnDowned -= OnUnitDowned;
        }

        private void OnUnitDowned(DownedEvent _)
        {
            if (animation != null) animation.PlayDeath();
            if (movement != null) movement.StopMoving();
        }
    }
}
