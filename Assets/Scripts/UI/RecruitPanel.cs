using System;
using System.Collections.Generic;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.UI
{
    /// <summary>
    /// End-of-run (win) recruit screen (Docs/13_LegacyMode.md §35-42). Lists the
    /// run's surviving roguelike-only units (<see cref="RunState.Recruits"/>),
    /// lets the player promote up to <see cref="maxRecruits"/> into the legacy
    /// roster, and — when the roster is at its cap — switches into a release
    /// sub-mode to free a slot before completing the promotion.
    ///
    /// Same fixed-slot pattern as DraftPanel/ShopPanel. When done (or skipped) it
    /// calls <see cref="RunController.FinishRecruiting"/> to show the run-end panel.
    /// </summary>
    public class RecruitPanel : MonoBehaviour
    {
        [Serializable]
        public class Slot
        {
            public Button button;
            public Text label;
        }

        [SerializeField] private Text headerText;
        [SerializeField] private Slot[] slots;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text confirmLabel;
        [SerializeField] private Button skipButton;

        [Tooltip("Max units recruitable per run (Docs/13_LegacyMode.md: K, default 3).")]
        [SerializeField, Min(1)] private int maxRecruits = 3;

        private enum Mode { Recruit, Release }
        private Mode mode = Mode.Recruit;

        private readonly List<UnitDTO> recruitable = new();   // run recruit pool (Recruit mode)
        private readonly List<LegacyUnitDTO> roster = new();  // current roster (Release mode)
        private readonly HashSet<int> selected = new();
        private readonly Queue<UnitDTO> pending = new();      // selected recruits awaiting room

        private void OnEnable()
        {
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    int idx = i;
                    if (slots[i]?.button != null)
                    {
                        slots[i].button.onClick.RemoveAllListeners();
                        slots[i].button.onClick.AddListener(() => OnSlotClicked(idx));
                    }
                }
            }
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirm);
            }
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(Finish);
            }

            EnterRecruitMode();
        }

        private void EnterRecruitMode()
        {
            mode = Mode.Recruit;
            selected.Clear();
            recruitable.Clear();
            var state = RunSession.Current;
            if (state?.Recruits != null) recruitable.AddRange(state.Recruits);
            Refresh();
        }

        private void EnterReleaseMode()
        {
            mode = Mode.Release;
            roster.Clear();
            var profile = LegacyStore.Current;
            if (profile?.Units != null) roster.AddRange(profile.Units);
            Refresh();
        }

        private void OnSlotClicked(int index)
        {
            if (mode == Mode.Recruit)
            {
                if (index < 0 || index >= recruitable.Count) return;
                if (!selected.Remove(index))
                {
                    if (selected.Count >= maxRecruits) return;
                    selected.Add(index);
                }
                Refresh();
            }
            else // Release: clicking a roster unit frees a slot, then resume.
            {
                if (index < 0 || index >= roster.Count) return;
                LegacyStore.Current.Release(roster[index].Id);
                mode = Mode.Recruit;
                ProcessQueue();
            }
        }

        private void OnConfirm()
        {
            if (mode != Mode.Recruit) return;
            pending.Clear();
            foreach (int idx in selected)
                if (idx >= 0 && idx < recruitable.Count) pending.Enqueue(recruitable[idx]);
            ProcessQueue();
        }

        /// <summary>Drain the pending-recruit queue, pausing into Release mode if the roster is full.</summary>
        private void ProcessQueue()
        {
            var profile = LegacyStore.Current;
            while (pending.Count > 0)
            {
                var unit = pending.Peek();
                if (profile.TryAdd(LegacyUnitDTO.FromUnit(unit)))
                {
                    RunSession.Current?.RemoveRecruit(unit);
                    pending.Dequeue();
                }
                else if (profile.IsAtCap)
                {
                    EnterReleaseMode();   // wait for the player to release one
                    return;
                }
                else
                {
                    pending.Dequeue();    // duplicate/invalid — skip
                }
            }

            LegacyStore.Save();
            Finish();
        }

        private void Refresh()
        {
            bool recruit = mode == Mode.Recruit;

            if (headerText != null)
            {
                headerText.text = recruit
                    ? $"RECRUIT  ({selected.Count}/{maxRecruits})"
                    : "ROSTER FULL — RELEASE A UNIT";
            }

            int count = recruit ? recruitable.Count : roster.Count;
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    if (slot == null) continue;
                    bool has = i < count;
                    if (slot.button != null)
                    {
                        slot.button.gameObject.SetActive(has);
                        slot.button.interactable = has && (!recruit || selected.Contains(i) || selected.Count < maxRecruits);
                    }
                    if (slot.label != null && has)
                    {
                        if (recruit)
                        {
                            var u = recruitable[i];
                            string mark = selected.Contains(i) ? "[x] " : "[ ] ";
                            slot.label.text = $"{mark}{u.DisplayName} [{u.Id}]";
                        }
                        else
                        {
                            var u = roster[i];
                            slot.label.text = $"Release  {u.DisplayName} [{u.Id}]";
                        }
                    }
                }
            }

            if (confirmButton != null) confirmButton.gameObject.SetActive(recruit);
            if (confirmLabel != null) confirmLabel.text = "Recruit";
            if (skipButton != null) skipButton.gameObject.SetActive(recruit);
        }

        private void Finish()
        {
            LegacyStore.Save();
            var controller = FindAnyObjectByType<RunController>();
            if (controller != null) controller.FinishRecruiting();
        }
    }
}
