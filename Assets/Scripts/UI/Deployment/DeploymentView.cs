using System.Collections.Generic;
using CapsuleWars.Combat.Deployment;
using CapsuleWars.Combat.State;
using CapsuleWars.Core;
using CapsuleWars.Run;
using CapsuleWars.UI.Inspection;
using CapsuleWars.Units.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CapsuleWars.UI.Deployment
{
    /// <summary>
    /// Drives the deployment screen during BattlePhase.PreBattle. Gathers the
    /// player's (already-spawned) units, applies the run's saved arrangement, and
    /// lets the player tap a unit to select+inspect it then tap a grid cell to
    /// place it (works for mouse and touch). Placement changes are pushed back to
    /// the run and saved so the arrangement persists between battles. The Start
    /// Battle button hands off to BattleStateManager.
    ///
    /// Spawn-then-arrange: BattlePartySpawner has already created the units; this
    /// only repositions them via DeploymentManager. Uses the new Input System
    /// low-level pointer APIs + Physics raycasts (set the layer masks in-scene).
    /// </summary>
    public class DeploymentView : MonoBehaviour
    {
        [SerializeField] private DeploymentManager manager;
        [SerializeField] private BattleStateManager stateManager;
        [SerializeField] private DeploymentGridRenderer gridRenderer;
        [SerializeField] private UnitInspectionPanel inspectionPanel;
        [SerializeField] private Button startBattleButton;

        [Header("Raycasting")]
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private LayerMask unitMask = ~0;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;

        private UnitRoot selected;

        private void Awake()
        {
            if (manager == null) manager = FindAnyObjectByType<DeploymentManager>();
            if (stateManager == null) stateManager = FindAnyObjectByType<BattleStateManager>();
            if (gridRenderer == null) gridRenderer = GetComponent<DeploymentGridRenderer>();
            if (inspectionPanel == null) inspectionPanel = FindAnyObjectByType<UnitInspectionPanel>(FindObjectsInactive.Include);
            if (raycastCamera == null) raycastCamera = Camera.main;
            if (startBattleButton != null) startBattleButton.onClick.AddListener(OnStartBattle);
        }

        private void OnEnable()
        {
            if (manager != null) manager.OnPlacementsChanged += HandlePlacementsChanged;
        }

        private void OnDisable()
        {
            if (manager != null) manager.OnPlacementsChanged -= HandlePlacementsChanged;
        }

        private void Start()
        {
            if (manager == null) return;

            // Register the player's spawned units with the placement authority.
            var roots = FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);
            for (int i = 0; i < roots.Length; i++)
                if (roots[i] != null && roots[i].Team == Team.Player) manager.RegisterUnit(roots[i]);

            // Restore the saved arrangement, else seed from current spawn positions.
            if (RunSession.IsActive && RunSession.Current.Placements.Count > 0)
                manager.ApplyPlacements(RunSession.Current.Placements);
            else
                manager.SeedFromCurrentPositions();

            if (gridRenderer != null) gridRenderer.Build(manager);
        }

        private void Update()
        {
            if (CombatServices.Phase != BattlePhase.PreBattle) return;
            if (manager == null || raycastCamera == null) return;
            if (!TryGetPress(out Vector2 screenPos)) return;

            Ray ray = raycastCamera.ScreenPointToRay(screenPos);

            // 1) Tapping a player unit selects + inspects it.
            if (Physics.Raycast(ray, out RaycastHit unitHit, maxRayDistance, unitMask))
            {
                var root = unitHit.collider.GetComponentInParent<UnitRoot>();
                if (root != null && root.Team == Team.Player)
                {
                    selected = root;
                    if (inspectionPanel != null) inspectionPanel.Show(root);
                    return;
                }
            }

            // 2) With a unit selected, tapping the ground places it on that cell.
            if (selected != null && Physics.Raycast(ray, out RaycastHit groundHit, maxRayDistance, groundMask))
            {
                var coord = manager.Config.WorldToCell(groundHit.point);
                manager.PlaceUnit(selected.UnitId, coord);   // no-op if the cell isn't deployable
            }
        }

        private void HandlePlacementsChanged(IReadOnlyDictionary<string, GridCoord> placements)
        {
            if (RunSession.IsActive)
            {
                RunSession.Current.ClearPlacements();
                foreach (var kv in placements) RunSession.Current.SetPlacement(kv.Key, kv.Value);
                RunSession.Save();
            }
            if (gridRenderer != null) gridRenderer.Refresh();
        }

        private void OnStartBattle()
        {
            if (stateManager != null) stateManager.StartBattle();
        }

        // True with the pointer's screen position when a press began this frame
        // (mouse left button or primary touch).
        private static bool TryGetPress(out Vector2 position)
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                position = mouse.position.ReadValue();
                return true;
            }

            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.wasPressedThisFrame)
            {
                position = ts.primaryTouch.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }
    }
}
