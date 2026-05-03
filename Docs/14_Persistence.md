# Persistence

## Backend
**Newtonsoft.Json.** Reasons:
- Polymorphism (e.g. `EffectStrategy` subclasses, status effect variants) without manual surrogate types.
- Reference handling for shared objects.
- Already a transitive dependency of multiple Unity packages we use.

`JsonUtility` is reserved for trivial settings (PlayerPrefs replacements) where polymorphism isn't needed.

## File layout
All saves live in `Application.persistentDataPath`:

```
{persistentDataPath}/
├── legacy/
│   └── profile.json              // LegacyProfileDTO (roster + player profile)
├── runs/
│   ├── current.json              // RunStateDTO — in-progress run, autosaved
│   └── archive/
│       └── {runId}.json          // completed runs (last 10)
├── settings.json                 // graphics, audio, input bindings
└── version.txt                   // current save schema version
```

## Save versioning
Every DTO has `int saveVersion`. On load:
1. Read `saveVersion`.
2. If older than current, run migrations in order (`v1 → v2 → v3 …`).
3. Migration registry lives in `Persistence.Migrations`.

MVP ships with `saveVersion = 1`. We write migrations only when we break schema.

## Polymorphism in JSON
Newtonsoft handles via `TypeNameHandling.Auto` for known polymorphic types. Settings:
```csharp
new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto,
    SerializationBinder = new CapsuleWarsTypeBinder(),  // restricts to allowed types
    NullValueHandling = NullValueHandling.Ignore,
    Formatting = Formatting.Indented   // dev builds only
}
```

`CapsuleWarsTypeBinder` whitelists DTO types — protects against deserialization gadgets if save files are ever shared.

## SO references in DTOs
DTOs never embed SO data — only the SO's stable string ID. At load time, `Database.GetById<T>(id)` resolves the runtime SO. This means SO changes can be made freely without breaking saves.

## Saving cadence
- **Legacy profile**: save on roster change, unit promotion, customization unlock. Debounced 1s.
- **Run state**: save on every map node transition. Save on app suspend (Android/iOS lifecycle hook).
- **Settings**: save on change.

All saves write atomically — write to `{file}.tmp`, then rename to `{file}`. Prevents corruption on crash mid-write.

## Crash recovery
- App startup checks for `.tmp` files; if found, treat as orphaned and delete.
- Run state file checked for valid JSON; if corrupt, prompt user to start fresh (don't lose legacy roster).

## DTO inventory
| DTO | Purpose | File |
|---|---|---|
| `LegacyProfileDTO` | Player + roster | `legacy/profile.json` |
| `LegacyUnitDTO` | One persistent unit | (inside `LegacyProfileDTO`) |
| `LifetimeStatsDTO` | Cumulative stats per legacy unit | (inside `LegacyUnitDTO`) |
| `UnitDTO` | Identity + parts + abilities (shared by legacy + run) | (inside many) |
| `EquipmentDTO` | Equipment instance | (inside `UnitDTO` and `RunStateDTO`) |
| `RunStateDTO` | In-progress run | `runs/current.json` |
| `RunSummaryDTO` | Completed run summary | `runs/archive/{runId}.json` |
| `MapStateDTO` | Run map graph + progress | (inside `RunStateDTO`) |
| `RunStatsTrackerDTO` | Per-run stats | (inside `RunStateDTO`) |
| `PlayerProfileDTO` | Unlocks, settings, meta | (inside `LegacyProfileDTO`) |
| `SettingsDTO` | Graphics/audio/input | `settings.json` |

## Cloud save
Out of scope for MVP. If added later, `LegacyProfileDTO` is the only file that needs syncing — runs and settings are device-local.

## Open items
- Whether to keep an archive of completed runs (last 10 or all). Default: last 10, FIFO.
- Whether to encrypt save files (anti-tamper). Default: no — single-player, low value to cheat.
