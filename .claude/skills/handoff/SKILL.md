---
name: handoff
description: Save a full session handoff so the next Claude Code session can resume CapsuleWars2 with zero context loss. Use when the user says /handoff, "checkpoint", "save state", "wrap up", or when context is getting full.
---

# Handoff

Execute the HANDOFF PROTOCOL defined in the project's root `CLAUDE.md`. Do all of it:

1. Append a new dated entry to the TOP of `Docs/SESSION_LOG.md` using the template
   at the bottom of that file. Be specific: files touched, what changed and why,
   whether it compiled, what needs human verification, and the exact next step.
2. Overwrite `Docs/PROJECT_STATE.md` to reflect reality right now (it's a
   snapshot — replace stale lines, don't append).
3. Update `Docs/TASKS.md`: check off completed items, add anything new you
   discovered, and ensure the top item is the precise next action.
4. If an architectural decision was made, add an ADR entry to `Docs/DECISIONS.md`.
5. Reply with one line confirming the handoff is saved and what the next session
   will start with.

Write everything for a fresh Claude with no memory of this session.
