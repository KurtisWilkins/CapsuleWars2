using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Instantiates equipped items' visuals as child objects on named sockets and
    /// keeps them in sync with the unit's equipment. Listens to
    /// <see cref="UnitStatusController.OnStatsChanged"/> and diff-rebuilds (only the
    /// slots that actually changed), so it serves both the live customization preview
    /// AND combat units — <c>UnitFactory</c> applies equipment via
    /// <c>UnitStatusController.Equip</c>, which fires the same event, so no spawner
    /// wiring is needed.
    ///
    /// Each item targets a socket by name (<see cref="Equipment_SO.AttachSocketName"/>)
    /// and supplies a <see cref="Equipment_SO.VisualPrefab"/> (preferred) or
    /// <see cref="Equipment_SO.VisualMesh"/>. Wire one socket Transform per name on the
    /// unit prefab — typically empties under the hand/head/chest bones.
    /// </summary>
    [RequireComponent(typeof(UnitStatusController))]
    [DisallowMultipleComponent]
    public class UnitEquipmentVisuals : MonoBehaviour
    {
        [Serializable]
        public class NamedSocket
        {
            [Tooltip("Socket name items reference via Equipment_SO.AttachSocketName (e.g. RightHand).")]
            public string name;
            [Tooltip("Transform the item visual is parented to (usually an empty under a bone).")]
            public Transform socket;
        }

        [Tooltip("Named attach points on this unit. Items pick one by name.")]
        [SerializeField] private List<NamedSocket> sockets = new();

        private UnitStatusController status;
        private readonly Dictionary<EquipmentSlot, GameObject> spawned = new();
        private readonly Dictionary<EquipmentSlot, Equipment_SO> current = new();

        private void Awake() => status = GetComponent<UnitStatusController>();

        private void OnEnable()
        {
            if (status == null) status = GetComponent<UnitStatusController>();
            if (status != null) status.OnStatsChanged += Rebuild;
        }

        private void Start() => Rebuild();   // catch equipment applied before we subscribed

        private void OnDisable()
        {
            if (status != null) status.OnStatsChanged -= Rebuild;
        }

        private void OnDestroy() => ClearAll();

        /// <summary>Re-sync attached visuals to the unit's current equipment (add new, remove gone, swap changed).</summary>
        public void Rebuild()
        {
            if (status == null) return;

            // Desired slot -> item, for equipped items that actually have a visual.
            var desired = new Dictionary<EquipmentSlot, Equipment_SO>();
            foreach (var eq in status.Equipment)
            {
                if (eq.item == null) continue;
                if (eq.item.VisualPrefab == null && eq.item.VisualMesh == null) continue;
                desired[eq.slot] = eq.item;
            }

            // Remove instances whose slot is now empty or whose item changed.
            var toRemove = new List<EquipmentSlot>();
            foreach (var kv in current)
                if (!desired.TryGetValue(kv.Key, out var want) || want != kv.Value)
                    toRemove.Add(kv.Key);
            for (int i = 0; i < toRemove.Count; i++) Despawn(toRemove[i]);

            // Add instances for desired slots not currently shown.
            foreach (var kv in desired)
                if (!spawned.ContainsKey(kv.Key))
                    Spawn(kv.Key, kv.Value);
        }

        private void Spawn(EquipmentSlot slot, Equipment_SO item)
        {
            var socket = FindSocket(item.AttachSocketName);
            if (socket == null) return;   // no matching socket → nothing to attach

            GameObject go;
            if (item.VisualPrefab != null)
            {
                go = Instantiate(item.VisualPrefab, socket);
            }
            else
            {
                go = new GameObject("Equip_" + item.EquipmentId);
                go.transform.SetParent(socket, false);
                go.AddComponent<MeshFilter>().sharedMesh = item.VisualMesh;
                var mr = go.AddComponent<MeshRenderer>();
                if (item.VisualMaterials != null && item.VisualMaterials.Count > 0)
                {
                    var mats = new Material[item.VisualMaterials.Count];
                    for (int i = 0; i < mats.Length; i++) mats[i] = item.VisualMaterials[i];
                    mr.sharedMaterials = mats;
                }
            }
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            spawned[slot] = go;
            current[slot] = item;
        }

        private void Despawn(EquipmentSlot slot)
        {
            if (spawned.TryGetValue(slot, out var go) && go != null) Destroy(go);
            spawned.Remove(slot);
            current.Remove(slot);
        }

        private void ClearAll()
        {
            foreach (var kv in spawned) if (kv.Value != null) Destroy(kv.Value);
            spawned.Clear();
            current.Clear();
        }

        private Transform FindSocket(string socketName)
        {
            if (string.IsNullOrEmpty(socketName)) return null;
            for (int i = 0; i < sockets.Count; i++)
                if (sockets[i] != null && sockets[i].name == socketName) return sockets[i].socket;
            return null;
        }
    }
}
