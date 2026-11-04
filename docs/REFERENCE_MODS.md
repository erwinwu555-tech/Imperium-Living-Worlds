# Similar-mod reference notes

Reviewed 2026-11-04. This is design research, not an implementation or
compatibility claim. No third-party source or assets have been copied.

| Project | Reviewed material | Useful pattern | Boundary for Imperium |
|---|---|---|---|
| [RimCities](https://github.com/rvanasa/rimworld-cities) | `Source/World/City.cs`, `Source/Util/GenCity.cs`; tree `5a1520e469427b8cca8b903af5ee59f5750f0802` | Settlement-derived city world object, explicit map entry/generation, faction-owned inhabitants | Generating a populated map does not provide persistent background population or autonomous construction. |
| [Colony Manager Redux](https://github.com/ilyvion/colony-manager-redux) | `Source/ColonyManagerRedux/Core/Manager.cs`, `Source/ColonyManagerRedux/ManagerJobs/ManagerJob.cs`; tree `88817d3a9751d350fe001651071c572979810347` | Map-scoped manager; separate gather/execute phases; interval-based jobs; suspend a failing job | Use a small proposal/approval boundary first. Do not import its full framework or assume 1.7 compatibility. |
| [Empire Refactored](https://github.com/matathias/Empire-1_6-Continued) | Repository README | Self-governing settlements, faction events, military direction | README explicitly targets 1.6. Reference for later empire scope, not a drop-in 1.7 dependency. |
| [Routes & Resources](https://github.com/matathias/Empire-Routes_and_Resources) | README and `1.6/Source/Core/Routes/PendingDelivery.cs`; tree `e7d3e30357c2a31073d5570c860addde32d57116` | Optional per-settlement stocks; an in-transit delivery captures dispatch data and persists references | Our logistics must still define reservations, arrival credit, interruption and replay protection. |
| [Faction Manager](https://steamcommunity.com/sharedfiles/filedetails/?id=2878135150) | Author's Workshop description only | Unload/reload colonies to reduce active-map work | Freezing a colony is not continuing its economy in the background. Source not reviewed. |

## License observations

- RimCities repository identifies MIT.
- Colony Manager Redux README identifies MIT for software/documentation and
  separate content/asset terms. Do not assume textures share the code license.
- Empire Refactored repository identifies GPL-3.0.
- No license file was found in the inspected Routes & Resources tree. That does
  not establish permission to reuse its implementation.
- Faction Manager source/license has not been checked in this review.

Recheck actual files and applicable notices before any reuse. These observations
do not select a license for Imperium.

## Development implication

Finish M1 real-game save/reload validation, then build the M2 minimum loop:
city needs -> proposal -> ordinary/major permission decision -> approval UI.
Keep proposal evaluation separate from mutation. Actual building placement and
resource consumption remain M3. Do not add logistics or background simulation
as part of this M1 hardening patch.
