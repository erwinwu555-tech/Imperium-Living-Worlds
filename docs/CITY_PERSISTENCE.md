# M1 — city registry and save/load adapter

**Status:** first implementation submitted for code review; unverified in a real game.

## Implemented

- `Imperium.Core.CityRecord`: stable GUID-based city identity, world tile, name, founding tick.
- `Imperium.Core.CityRegistry`: create/register/query with unique IDs and one city per tile.
- `CityRegistry.Restore`: explicit schema version, empty pre-M1 save migration, reject unsupported future versions and duplicate data.
- `Imperium.RimWorld.ImperiumWorldComponent`: world-level `ExposeData` adapter using `Scribe_Values` and `Scribe_Collections` deep records.
- Behaviour tests covering registry invariants, restoration and invalid data.

## Scope and safety

This stage does **not** create cities automatically, place buildings, mutate maps or add a UI. It adds an internal API, not a feature players can activate in the game. The main-menu startup log does not prove `WorldComponent` save/load works.

If loading city records fails, the component keeps its registry read-only and logs a warning. **Unknown future-save fields cannot be guaranteed to round-trip through older Scribe code.** Exit without saving and preserve the original save. Do not test on important colonies.

Existing saves without `imperiumSchemaVersion` migrate only when no city records are present. Saving with version 1 serializes a snapshot of the current registry; actual game write/read behaviour needs a game integration test.

## Required in-game verification

1. Compile the workflow branch successfully.
2. Launch RimWorld 1.7 with Core + Imperium only, using a new test world.
3. Verify there are no mod exceptions during new-world creation, save, reload or exit.
4. Add a development-only diagnostic command to create a city (next step), then save/reload and verify the same GUID, tile, name and founding tick.
5. Verify missing and future versions fail safely with backup test saves.
6. Only merge after CI passes and major regressions have been ruled out.

## Next iteration

- In-game debug-only city create/query command.
- World tile bounds and faction ownership validation.
- Version migration fixtures and historical save compatibility.
- Auditable save-state integrity checks and diagnostic export.
