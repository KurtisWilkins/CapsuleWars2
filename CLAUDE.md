# CapsuleWars2 — Claude Code Operating Manual

CapsuleWars2 is a Unity (C#) auto-battler / roguelite, developed solo by Kurtis.
Claude Code edits the project live through the **CoplayDev Unity MCP bridge**.

This file is loaded automatically at the start of every session. It is the
**roadmap** to the project, not the project itself. The detailed, always-current
state lives in the imported files below — keep this file short.

> Note: the docs folder on disk is `Docs/` (it also holds the 00–17 milestone
> design docs). Imports below use that exact casing so they resolve on both
> Windows and case-sensitive clones.

---

## Always-loaded context (imported)

@Docs/PROJECT_STATE.md
@Docs/TASKS.md
@Docs/ARCHITECTURE.md
@Docs/DECISIONS.md

> `Docs/SESSION_LOG.md` is **not** imported here on purpose — it grows over time
> and would eat the context window. Read only its **last entry or two** when you
> need recent history.

---

## SESSION START PROTOCOL

When a new session begins and the user says anything like "continue", "go",
"next", "what's next", or gives no specific task:

1. Read `Docs/PROJECT_STATE.md` and `Docs/TASKS.md` (already in context above).
2. If you need recent history, open `Docs/SESSION_LOG.md` and read only the
   **last 1–2 entries** — not the whole file.
3. Confirm the Unity MCP bridge is connected before attempting any edit
   (try a lightweight read, e.g. `manage_editor telemetry_status`, list scripts,
   or read one known file). If it is not connected, say so and stop — do not
   guess at file contents.
4. Give a 3–5 line summary: where the project stands, and the **single next
   task** you intend to start (the top unblocked item in `TASKS.md`).
5. Begin that task. Don't wait for further confirmation unless the task is
   destructive or ambiguous.

## WORK PROTOCOL

- Make focused, compile-safe edits through the Unity MCP bridge.
- After each meaningful change, let Unity recompile and check for errors
  (`refresh_unity` then `read_console`). Report any compile errors and fix them
  before moving on.
- You cannot see Play Mode results. When a change needs human verification in
  the editor (visual, gameplay, scene wiring), **flag it explicitly** and add it
  to the "Needs human verification" section of `PROJECT_STATE.md`.
- Prefer the established patterns in `ARCHITECTURE.md`. If you must deviate,
  record why in `DECISIONS.md`.
- Keep changes small enough that a single handoff entry can describe them.

## HANDOFF PROTOCOL  (this is what prevents context loss)

Treat the docs files as the project's real memory. Update them **incrementally**,
not just at the end:

- **After each completed task**: tick it off in `TASKS.md`, update the relevant
  lines in `PROJECT_STATE.md`.
- **When context starts getting tight** (you're roughly 75% full, the user runs
  `/context` and it's high, or you're about to be `/compact`-ed): proactively
  run the handoff *before* you lose detail. Don't wait to be asked.
- **When the user runs `/handoff`** or says "wrap up" / "checkpoint" / "save state".

A handoff means doing ALL of the following:

1. **Append** a new dated entry to the TOP of `Docs/SESSION_LOG.md` using the
   template at the bottom of that file. Keep it factual and specific: files
   touched, decisions made, what compiled, what's unverified, exact next step.
2. **Overwrite** `Docs/PROJECT_STATE.md` so it reflects reality *right now*
   (this file is a snapshot, not a log — replace stale lines).
3. **Update** `Docs/TASKS.md`: check off done items, add new ones discovered,
   and make sure the top item is the precise next thing to do.
4. If any architectural decision was made, add it to `Docs/DECISIONS.md`.
5. Tell the user in one line that the handoff is saved and what the next session
   will start with.

The golden rule: **another fresh Claude with zero memory should be able to read
PROJECT_STATE.md + TASKS.md + the last SESSION_LOG entry and pick up exactly
where you left off.** Write for that reader.

## GUARDRAILS

- Never delete or wholesale-rewrite a file without saying so first.
- Never assume a script's contents from memory — read it through the bridge.
- If the MCP bridge call fails, stop and report; do not fabricate edits.
- Don't expand scope mid-task; capture new ideas in `TASKS.md` instead.
- The bridge drops on every domain reload (recompile / entering Play) and cycles
  ports — just reconnect and continue. Game-view MCP screenshots come back blank;
  use `manage_camera capture_source=scene_view` for 3D, or computer-use on the
  Game/Simulator window for overlay UI.

## CONVENTIONS

- C# / Unity standard naming (PascalCase types/methods, camelCase locals,
  `_camelCase` private fields). Match surrounding code.
- Unit/stat data is ScriptableObject-driven — see `ARCHITECTURE.md`.
- Keep MonoBehaviours thin; logic lives in plain C# systems where practical.
- Assembly layering is enforced: lower layers must not reference UI/Run (see
  `ARCHITECTURE.md`). Setting object/component reference fields via the MCP uses
  the object form `{"instanceID": N}` / `{"path": "Assets/..."}` — a bare integer
  silently fails to bind.
