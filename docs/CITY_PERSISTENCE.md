# M1 — city registry and save/load adapter

**Status:** registry, save adapter and Dev Mode validation commands implemented; compiled and unit-tested in CI; **not yet verified in a real game**.

## Implemented

- `Imperium.Core.CityRecord`: stable GUID-based city identity, world tile, name, founding tick.
- `Imperium.Core.CityRegistry`: create/register/query with unique IDs and one city per tile; tile index for O(1) `FindByTile` (no scanning of all cities); stable `Describe` line used by the in-game test.
- `CityRegistry.Restore`: explicit schema version, empty pre-M1 save migration, reject unsupported future versions and duplicate data.
- `Imperium.RimWorld.ImperiumWorldComponent`: world-level `ExposeData` adapter using `Scribe_Values` and `Scribe_Collections` deep records; logs every city on load.
- `Imperium.RimWorld.ImperiumDebugActions` (Dev Mode only, debug actions menu, category **Imperium**):
  - **Register city on current map** — registers the current map's world tile as a city, named after the settlement. Refuses a second city on the same tile and refuses while the registry is locked.
  - **Log city registry** — writes the registry to the log.
- Behaviour tests covering registry invariants, restoration, invalid data, tile index, rollback of rejected registration, ID normalisation and the stable log line.

## Scope and safety

This stage does **not** create cities automatically, place buildings, mutate maps or add a player UI. The two commands exist only to validate persistence and are only visible with Dev Mode enabled.

If loading city records fails, the component keeps its registry read-only and logs an error. **Unknown future-save fields cannot be guaranteed to round-trip through older Scribe code.** Exit without saving and preserve the original save. Do not test on important colonies.

Existing saves without `imperiumSchemaVersion` migrate only when no city records are present. Saving with version 1 serializes a snapshot of the current registry.

## In-game verification (save/reload)

1. Launch RimWorld 1.7 with Core + Imperium only; enable Dev Mode in Options.
2. Start a new test colony. Open the debug actions menu (Dev Mode toolbar), category **Imperium**, run **Register city on current map**.
3. Expected: a green message and a log line `Registered city id=<32 hex> name="<settlement>" tile=<n> foundedTick=<n>`, then `City registry (after registration): schema=1 cities=1 locked=False`.
4. Run it again on the same map. Expected: refused with "already has a city"; still `cities=1`.
5. Save, return to the main menu, load the save.
6. Expected in the log: `City registry (loaded from save): schema=1 cities=1 locked=False` followed by a line **identical** to the one from step 3.
7. No red errors from Imperium at any step.

Not yet covered in game: missing/future schema handling with real save files (covered by unit tests only).

## Known limitations

- The world tile is stored as an integer. RimWorld 1.6+ planet layers (Odyssey orbit/space) mean a tile number may only be unique within one layer; layer-aware IDs must be added before cities can exist off the surface layer.
- Cities are not removed when a settlement is abandoned; lifecycle handling belongs to a later stage.

## Next iteration

- World tile bounds, planet layer and faction ownership validation.
- Version migration fixtures and historical save compatibility.
- Auditable save-state integrity checks and diagnostic export.
