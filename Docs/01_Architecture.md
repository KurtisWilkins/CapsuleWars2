# Architecture

## Folder layout
```
Assets/Scripts/
  Core/                  CapsuleWars.Core           constants, enums, interfaces, primitive types
  Data/                  CapsuleWars.Data           ScriptableObject definitions only — no runtime logic
    Units/
    Abilities/
    Equipment/
    Elements/
    Classes/
    StatusEffects/
    Weapons/
    Map/
  Units/                 CapsuleWars.Units          runtime: controllers, AI, animation
    Controllers/         Health, Attack, Movement, Status, Animation, LevelUp
    AI/                  3D-ported targeting strategies
    Customization/       part swapping, palette application
  Combat/                CapsuleWars.Combat         battle orchestration
    State/               BattleStateManager (replaces Sprite Wars god class)
    Stats/               BattleStatsAggregator + per-run stat sink
    Rewards/             gold/equipment/recruit logic
    Deployment/
  Abilities/             CapsuleWars.Abilities      runtime ability code
    Effects/
    Triggers/
    Targeting/
    Filters/
  Run/                   CapsuleWars.Run            roguelike run loop
    Map/
    Economy/
    Recruitment/
  Legacy/                CapsuleWars.Legacy         persistent roster mode
    Roster/
    Drafting/
  Persistence/           CapsuleWars.Persistence    Newtonsoft save/load
    Dto/
  UI/                    CapsuleWars.UI             dumb views, data-event driven
  Audio/                 CapsuleWars.Audio          AudioCueSO + manager
  Generation/            CapsuleWars.Generation     POST-MVP: Meshy + Grok editor tools
  Editor/                CapsuleWars.Editor         editor utilities, inspectors
  Tests/
    EditMode/            CapsuleWars.Tests.EditMode
    PlayMode/            CapsuleWars.Tests.PlayMode
Docs/                    living design specs
```

## Assembly definitions
One `.asmdef` per top-level folder. Allowed reference graph:

```
Core
  ↓
Data ──────────────┐
  ↓                │
Units → Abilities  │
  ↓        ↓       │
Combat ←───┘       │
  ↓                │
Run ← Combat       │
Legacy ← Persistence
  ↓
UI ← {everything above}
Audio ← Combat (event sub)
Editor ← {everything above}
Tests.EditMode → {everything except UI}
Tests.PlayMode → {everything}
```

Rule of thumb: `Data` never references runtime; `UI` never references back into itself; `Combat` doesn't know about `Run`/`Legacy` (those layer on top).

## Namespacing
- Root: `CapsuleWars`
- Sub-namespace per top-level folder: `CapsuleWars.Combat`, `CapsuleWars.Units.AI`, etc.
- Test namespaces: `CapsuleWars.Tests.EditMode.Combat`, etc.

## Coding conventions
- C# 9+ (records where useful for DTOs).
- Public APIs get XML doc comments. Internal helpers do not.
- Prefer composition over inheritance for runtime; SO inheritance is fine for `EffectStrategy` etc.
- `using` ordering: System → Unity → third-party → CapsuleWars.
- Async work uses `UniTask` only if we add it later; until then, coroutines or DOTween's awaitable.
- No `FindObjectOfType` in hot paths. Inject via Awake-time wiring or a thin `BattleContext` passed at battle start.

## Event surface
Combat exposes a strongly typed event bus (`Combat.Stats.BattleEventBus`). UI / audio / stats / VFX subscribe. See `11_StatsTracking.md` for the full event list.

## I2 Localization
- One `LanguageSource` asset under `Assets/Resources/Localization/`.
- Term keys grouped by system: `UI.MainMenu.Play`, `Ability.Backstab.Name`, `Ability.Backstab.Description`, `Status.Stunned.Name`, `Element.Flame.Name`, etc.
- Every public-facing field that holds text on an SO holds a **term key**, not the literal string.

## DOTween
- One project-level `DOTweenSettings` asset, recycled tweens enabled.
- Always `.SetLink(gameObject)` on tweens that bind to objects that can be destroyed.
- Sequences over chained callbacks where possible.

## Source control
- Git LFS routes binary assets (already configured).
- Branch convention: `feature/<name>`, `fix/<name>`. Claude works on `claude/*` branches.
- Commit messages: imperative, scoped. `combat: split BattleController into state + stats + rewards`.
