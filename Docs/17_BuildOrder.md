# MVP Build Order

## Principle
Working game at the end of every milestone. No half-finished piles. Each milestone has an explicit "demo state" — a thing you can show.

## M0 — Project plumbing
**Demo state:** empty scene compiles, tests run green, all conventions enforced.

- [x] Create `Assets/Scripts/` folder layout per `01_Architecture.md`.
- [x] `.asmdef` per top-level folder with the dependency graph from `01_Architecture.md`.
- [x] `CapsuleWars.*` namespaces wired.
- [x] `Tests/EditMode` and `Tests/PlayMode` folders + asmdefs.
- [x] Newtonsoft.Json package added.
- [ ] I2 LanguageSource asset created at `Assets/Resources/Localization/CapsuleWarsTerms.asset` with empty term groups. *(deferred to first I2 use in M3)*
- [x] DOTween settings configured (recycled tweens on, capacity tuned). *(asset existed from package import; will revisit when first DOTween call is added)*
- [x] One trivial EditMode test green (`AssemblyLoadTest`).
- [ ] CI-equivalent local script: open project headless, run tests, exit code. *(deferred — manual Test Runner runs cover this until we automate)*

## M1 — One unit standing in an arena
**Demo state:** capsule unit visible in a 3D scene with idle animation.

- [x] `BodyPart_SO`, `Palette_SO` definitions.
- [x] `UnitCustomization` — applies parts and palette to a 3D capsule.
- [x] One sample `UnitDefinition_SO` with 1 body, 2 hands, 2 feet, 1 palette.
- [x] Animator with ExplosiveLLC unarmed sub-state machine; `WeaponType` parameter. *(Using ExplosiveLLC's stock controller for now — humanoid skeleton hidden, our floating-limb meshes mounted on bones. Parameter-name conformance to `03_AnimationController.md` deferred to M2 when code first drives animation.)*
- [x] Idle + Walk states wired. *(Reachable via ExplosiveLLC controller.)*
- [x] Test arena scene `Test_M1_Idle.unity` with one unit visible.

**Rig approach decided:** Option A (humanoid rig under the hood). ExplosiveLLC humanoid skeleton drives floating limb mesh transforms. Body capsule and limb meshes are visually disconnected; the rig is hidden.

## M2 — Two units fight
**Demo state:** two capsule units approach each other, attack, one is downed.

- [ ] `UnitHealthController` with `OnHealthChanged`, `OnDowned` events.
- [ ] `UnitMovementController` on NavMesh.
- [ ] `UnitAttackController` with cooldown + attack animation trigger.
- [ ] `UnitAnimationController` with sub-SM routing.
- [ ] One `WeaponClass_SO` (1H Sword) wired to its sub-SM.
- [ ] Damage event flows; KO state visible (downed pose).
- [ ] EditMode test: damage math without scene load.

## M3 — Battle scene
**Demo state:** play a battle from a "start" button to a "victory/defeat" screen, with stats.

- [ ] `BattleStateManager` (PreBattle / Active / Resolved).
- [ ] `BattleContext` injected into spawner + controllers.
- [ ] Win/loss check + 90s sudden-death timer.
- [ ] `-50% HP next battle` rule (driven by `Downed` flag carried into next `BattleContext`).
- [ ] `BattleEventBus` with all events from `11_StatsTracking.md`.
- [ ] `BattleStatsAggregator` writes per-battle counters.
- [ ] End-of-battle leaderboard UI (top damage / top kills) — text-only is fine.
- [ ] PlayMode test: scripted 1v1 battle resolves deterministically with a seeded RNG.

## M4 — Abilities
**Demo state:** units with abilities cast them mid-battle; visible effects.

- [ ] `Ability_SO` + 4 strategy bases as SOs.
- [ ] Triggers: `TimeBasedTrigger`, `OnHitTrigger`.
- [ ] Targeting: `GetEnemyTargets`, `GetSelfTarget`.
- [ ] Filters: `ClosestN`, `LowestHpN`, `InRange`.
- [ ] Effects: `DamageEffect`, `KnockBackEffect` (3D-ported), `HealEffect`.
- [ ] Weapon-class gating in `UnitAttackController`.
- [ ] 3 sample abilities authored (one each: damage, heal, AoE).

## M5 — Elements & status effects
**Demo state:** elemental matchups visibly modify damage; statuses apply with VFX.

- [ ] `ElementType_SO` × 15.
- [ ] `ElementChart_SO` with 5×5 family matrix.
- [ ] `StatCalculator.GetElementMultiplier` integrated into damage pipeline.
- [ ] `StatusEffect_SO` × 24 (data + behavior).
- [ ] `UnitStatusController.ApplyStatus` with resistance roll, duration tick, stacking modes.
- [ ] Animator integrations: Stunned sub-SM, Frozen shader.
- [ ] VFX prefabs for buff/debuff/control/DoT/HoT (placeholder OK).

## M6 — Equipment & class synergies
**Demo state:** equip items pre-battle; synergies bar updates as you place units.

- [ ] `Equipment_SO`, `Rarity_SO`, `Rune_SO`.
- [ ] 8-slot equip on `UnitCustomization` (mesh swap).
- [ ] `StatCalculator` includes equipment + rarity multipliers.
- [ ] `UnitClass_SO` with `ClassSynergyTier[]`.
- [ ] `SynergyResolver` recomputes on battle start + KO + revive.
- [ ] Pre-battle deployment UI shows synergy preview.

## M7 — Run loop (roguelike)
**Demo state:** play a 5-floor mini-run from start to boss with map, shop, drops.

- [ ] `MapGenerator` — node graph with all 7 node types.
- [ ] Map UI screen with branching paths.
- [ ] Combat / Elite / Treasure node implementations.
- [ ] Shop node: 4 equipment + 1 unit slot expansion + heal.
- [ ] Event node: 3 sample events with stat outcomes.
- [ ] Rest node: heal % across roster.
- [ ] Boss node: hand-authored fight.
- [ ] `RunStateDTO` save/resume on every node transition.
- [ ] Run summary screen on win/loss.

## M8 — Legacy mode
**Demo state:** save a roster, draft into a new run, finish, see lifetime stats grow.

- [ ] `LegacyProfileDTO` + Newtonsoft serialization.
- [ ] Legacy roster screen (grid view, filters).
- [ ] Draft-into-run flow.
- [ ] Level-resets-to-1 enforced at run start.
- [ ] `LegacyStatWriter` merges per-run → lifetime at run end.
- [ ] End-of-run recruit prompt for surviving roguelike-only units.
- [ ] Roster cap with release prompt.
- [ ] Per-unit detail screen with lifetime stats.

## M9 — Customization unlocks
**Demo state:** earn unlock points from a run, spend them in customization screen, see new options in random gen.

- [ ] `PlayerProfileDTO.unlockPoints` + earn/spend.
- [ ] Customization screen UI.
- [ ] Unlock-aware `UnitCustomization` (only shows unlocked parts).
- [ ] Random unit generator (`12_RoguelikeRun.md`) draws from unlocked pool.
- [ ] Asymmetric mixing UI controls.

## M10 — Polish & MVP candidate
**Demo state:** shippable single-player loop.

- [ ] Audio: `AudioCueSO`, hit/heal/KO/UI cues, basic music.
- [ ] DOTween polish on UI transitions, damage popups, camera shake.
- [ ] Tutorial stub: 3-screen popup intro on first launch.
- [ ] Multi-arena: 4 hand-crafted arenas with prop variant placement.
- [ ] Balance pass: enemy stat curve, gold economy, ability cooldowns.
- [ ] Settings screen: graphics, audio sliders, input rebinds.
- [ ] Localization sweep: ensure no hardcoded strings.
- [ ] Performance pass: target 60fps mid-tier device, no GC spikes mid-battle.

## Post-MVP
- Meshy + image-gen editor tools (`16_AssetGeneration.md`).
- P2P multiplayer (`15_Multiplayer.md`).
- Ascensions / heat levels.
- Cloud save.
- Set bonuses on equipment.
- Multi-class units.
- Cloud cosmetic milestones.

## Working agreement with Claude
- One milestone in flight at a time.
- Each milestone closed with a demo and a passing test.
- No code beyond the milestone's scope without an explicit ask.
- Docs updated whenever a decision changes — these files are living specs.
