using UnityEngine;

namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// Draws the deployment grid + player deploy zone in the Scene view via
    /// OnDrawGizmos, so the 7x9 board is visible while authoring even before the
    /// runtime UI is wired. Reads the live <see cref="DeploymentManager"/>'s config
    /// when present (so it always matches what combat uses), otherwise its own
    /// <see cref="overrideConfig"/>. Pure editor visualization — no runtime effect.
    /// </summary>
    [DisallowMultipleComponent]
    public class DeploymentGizmos : MonoBehaviour
    {
        [Tooltip("If set, the grid drawn matches this manager's config exactly. Falls back to overrideConfig.")]
        [SerializeField] private DeploymentManager manager;
        [Tooltip("Used only when no manager is assigned (e.g. previewing a layout).")]
        [SerializeField] private DeploymentGridConfig overrideConfig = new DeploymentGridConfig();

        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color zoneColor = new Color(0.2f, 0.8f, 0.3f, 0.25f);
        [SerializeField] private Color enemyZoneColor = new Color(0.9f, 0.35f, 0.2f, 0.25f);
        [SerializeField] private bool onlyWhenSelected = false;

        private DeploymentGridConfig Config => manager != null ? manager.Config : overrideConfig;

        private void OnDrawGizmos()
        {
            if (!onlyWhenSelected) Draw();
        }

        private void OnDrawGizmosSelected()
        {
            if (onlyWhenSelected) Draw();
        }

        private void Draw()
        {
            var cfg = Config;
            if (cfg == null) return;

            for (int row = 0; row < cfg.rows; row++)
            {
                for (int col = 0; col < cfg.columns; col++)
                {
                    var coord = new GridCoord(col, row);
                    Vector3 center = cfg.CellToWorld(coord);

                    if (cfg.InPlayerZone(coord))
                    {
                        Gizmos.color = zoneColor;
                        Gizmos.DrawCube(center, new Vector3(cfg.cellSize * 0.95f, 0.02f, cfg.cellSize * 0.95f));
                    }
                    else if (cfg.InEnemyZone(coord))
                    {
                        Gizmos.color = enemyZoneColor;
                        Gizmos.DrawCube(center, new Vector3(cfg.cellSize * 0.95f, 0.02f, cfg.cellSize * 0.95f));
                    }

                    Gizmos.color = gridColor;
                    Gizmos.DrawWireCube(center, new Vector3(cfg.cellSize, 0f, cfg.cellSize));
                }
            }
        }
    }
}
