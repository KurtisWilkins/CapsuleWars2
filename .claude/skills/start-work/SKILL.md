---
name: start-work
description: Resume CapsuleWars2 work by loading current state and continuing the top task. Use when the user says /start-work, "continue", "go", "next", or starts a session without a specific task.
---

# Start Work

Execute the SESSION START PROTOCOL defined in the project's root `CLAUDE.md`:

1. Read `Docs/PROJECT_STATE.md` and `Docs/TASKS.md`.
2. If you need recent history, read only the last 1–2 entries of
   `Docs/SESSION_LOG.md`.
3. Verify the CoplayDev Unity MCP bridge is connected with a lightweight read
   before editing (e.g. `manage_editor telemetry_status`). If it's not
   connected, say so and stop.
4. Summarize in 3–5 lines: current state + the single next task you'll start
   (the top unblocked item in `TASKS.md`).
5. Begin that task unless it's destructive or ambiguous.
