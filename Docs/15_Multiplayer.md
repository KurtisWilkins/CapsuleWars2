# Multiplayer

## Scope
**Out of MVP scope** for shipping; architectural placeholder only. Single-player auto-battler must work fully on a plane with no signal — that is the core experience.

## Eventual model
- **Peer-to-peer** (no dedicated servers).
- **Optional matchmaking** lobby (lightweight; not a goal-state).
- **Async** support is a possible later mode (submit a team, opponent's device simulates against it offline).

## Architecture preparation
At MVP, do not write netcode. But:
- Wrap all combat input through a `BattleInput` boundary (deployment, bench swap, surrender). Today these come from local UI; tomorrow they could come from a remote peer.
- `BattleStateManager` is deterministic given a seed + input stream. Avoid `Random.Range` directly; use a `BattleRng` instance with an explicit seed stored in `BattleContext`.
- `Time.fixedDeltaTime`-driven simulation (already standard).

If those two boundaries hold, we can layer on lockstep P2P later without rewriting combat.

## Determinism notes
- Use `BattleRng.NextInt(min, max)` everywhere instead of `Random.Range`.
- Avoid `Time.realtimeSinceStartup` in gameplay logic — only use it for UI animation (DOTween).
- Floating-point determinism across platforms is risky; if P2P is required, audit for fixed-point or run a desync detector.

## P2P transport (future)
Likely choices:
- Unity Netcode for GameObjects with a relay (still server-mediated for NAT punch-through).
- Mirror with a custom transport.
- Steam P2P if shipping on Steam.

Pick at the time. None of them affect MVP code.

## Async mode (future)
Submit a `TeamSnapshotDTO` (your roster + composition for that match). Opponent's device plays it out locally vs. their team. Result reported back. Cheating risk acceptable for casual; mitigations later.

## What NOT to do at MVP
- Don't add a network layer "just in case."
- Don't make systems "framework-ready" for netcode unless the abstraction also helps single-player (the input boundary does; an RPC wrapper does not).

## Open items
- Final transport choice.
- Whether matchmaking is in-house or platform-provided.
- Whether spectate is a feature.
