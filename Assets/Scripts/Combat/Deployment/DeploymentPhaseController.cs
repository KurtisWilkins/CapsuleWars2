using System;
using CapsuleWars.Combat.State;
using UnityEngine;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// Owns the pre-combat Deployment Phase. Its presence marks the battle as
    /// requiring deployment: it sets <see cref="BattleStateManager.DeploymentRequired"/>
    /// so combat cannot reach Active until the player presses Assemble. The
    /// deployment UI calls <see cref="Confirm"/> to lock placement, which raises
    /// <see cref="OnConfirmed"/> (the party spawner spawns at the placed cells) and
    /// then starts the battle. If this controller is absent, combat starts as before.
    /// Thin: state + gate only; camera framing is handled by DeploymentCameraController.
    /// </summary>
    [DisallowMultipleComponent]
    public class DeploymentPhaseController : MonoBehaviour
    {
        [Tooltip("Battle state manager to gate/start. Auto-found if left empty.")]
        [SerializeField] private BattleStateManager stateManager;

        /// <summary>True once the player has pressed Assemble.</summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>Raised on Assemble, before combat starts — spawners listen to spawn the party.</summary>
        public event Action OnConfirmed;
        /// <summary>Raised when the layout is cleared (placements emptied).</summary>
        public event Action OnCleared;

        private void Awake()
        {
            if (stateManager == null) stateManager = FindAnyObjectByType<BattleStateManager>();
            if (stateManager != null)
            {
                stateManager.DeploymentRequired = true;     // block combat until Assemble
                stateManager.DeploymentConfirmed = false;
            }
        }

        /// <summary>Lock deployment, spawn the party, and start combat. Idempotent.</summary>
        public void Confirm()
        {
            if (IsConfirmed) return;
            IsConfirmed = true;

            OnConfirmed?.Invoke();                            // spawn party at placed cells

            if (stateManager != null)
            {
                stateManager.DeploymentConfirmed = true;
                stateManager.StartBattle();
            }
        }

        /// <summary>Notify listeners the layout was cleared (does not un-confirm a started battle).</summary>
        public void NotifyCleared() => OnCleared?.Invoke();
    }
}
