using CapsuleWars.Core;
using UnityEngine;
using UnityEngine.AI;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Drives target acquisition and NavMesh movement. Each frame:
    ///   - if no target or target downed → re-acquire closest enemy
    ///   - if outside attack range → SetDestination(target)
    ///   - if in range → halt, face target, ask UnitAttackController to swing
    /// Animation Speed parameter is updated as a side-effect (0=idle, 0.5=run).
    /// Reactive retargeting (carry-forward from Sprite Wars) lands in M3+.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitMovementController : MonoBehaviour
    {
        [Tooltip("Buffer multiplier on attack range so the unit doesn't oscillate at the boundary.")]
        [SerializeField, Min(0.5f)] private float stoppingDistanceMultiplier = 0.9f;

        [Tooltip("Degrees per second for facing the target while attacking.")]
        [SerializeField, Min(0f)] private float facingTurnSpeed = 720f;

        private NavMeshAgent agent;
        private UnitRoot root;
        private IUnitRef currentTarget;

        public IUnitRef CurrentTarget => currentTarget;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            root = GetComponentInParent<UnitRoot>();
        }

        private void Start()
        {
            if (root != null && root.Status != null)
            {
                agent.speed = root.Status.Speed;
            }
        }

        private void OnEnable()
        {
            CombatServices.Registry?.Register(root);
        }

        private void OnDisable()
        {
            CombatServices.Registry?.Unregister(root);
        }

        public void StopMoving()
        {
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            currentTarget = null;
        }

        private void Update()
        {
            // Gate by battle phase: PreBattle = idle, Resolved = frozen.
            if (CombatServices.Phase != BattlePhase.Active)
            {
                if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
                root?.Animation?.SetSpeed(0f);
                return;
            }

            if (root == null || root.Health == null || root.Health.IsDowned)
            {
                StopMoving();
                root?.Animation?.SetSpeed(0f);
                return;
            }

            // Status effects can immobilize: Stunned/Frozen halt and idle.
            // Doc convention is Animator Speed=1 → Stunned state, but
            // until you author that state we fall back to Speed=0 (idle pose).
            // Once a Stunned state exists in your AnimatorController, swap
            // the SetSpeed(0f) call below to SetSpeed(1f).
            if (root.Status != null && (root.Status.CannotMove || root.Status.CannotAct))
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                root.Animation?.SetSpeed(0f);
                return;
            }

            // Units are spawned (Awake-time spawners + deployment placement) BEFORE ArenaBuilder bakes the runtime
            // NavMesh (its Start), and Unity does NOT auto-attach an agent once a navmesh later appears. Snap any
            // off-mesh agent onto the baked mesh the first frame it needs to act — otherwise the approach branch
            // below short-circuits at `!agent.isOnNavMesh` before driving the Animator's Speed param, and the unit
            // freezes in Idle (never moves, never attacks). Cheap: only runs while an agent is still off-mesh.
            if (agent != null && !agent.isOnNavMesh &&
                NavMesh.SamplePosition(transform.position, out var navHit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(navHit.position);
            }

            if (currentTarget == null || currentTarget.IsDowned)
                currentTarget = AcquireTarget();

            if (currentTarget == null)
            {
                if (agent.isOnNavMesh) agent.isStopped = true;
                root.Animation?.SetSpeed(0f);
                return;
            }

            float range = root.Attack != null ? root.Attack.AttackRange : 2f;
            float dist = Vector3.Distance(transform.position, currentTarget.Transform.position);

            if (dist > range)
            {
                if (!agent.isOnNavMesh) return;
                agent.stoppingDistance = range * stoppingDistanceMultiplier;
                agent.isStopped = false;
                agent.SetDestination(currentTarget.Transform.position);
                root.Animation?.SetSpeed(0.5f);
            }
            else
            {
                if (agent.isOnNavMesh) agent.isStopped = true;
                FaceTarget(currentTarget.Transform);
                root.Animation?.SetSpeed(0f);
                root.Attack?.TryAttack(currentTarget);
            }
        }

        private IUnitRef AcquireTarget()
        {
            var registry = CombatServices.Registry;
            if (registry == null) return null;

            IUnitRef best = null;
            float bestSqr = float.MaxValue;
            var pos = transform.position;

            var list = registry.Units;
            for (int i = 0; i < list.Count; i++)
            {
                var u = list[i];
                if (u == null) continue;
                if (ReferenceEquals(u, root)) continue;
                if (u.Team == root.Team) continue;
                if (u.IsDowned) continue;

                float d = (u.Transform.position - pos).sqrMagnitude;
                if (d < bestSqr)
                {
                    bestSqr = d;
                    best = u;
                }
            }
            return best;
        }

        private void FaceTarget(Transform target)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            var goal = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, facingTurnSpeed * Time.deltaTime);
        }
    }
}
