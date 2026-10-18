# Validation

2026-10-18: requested module layout prepared locally.
- .NET SDK 10.0.402, Krafs.Rimworld.Ref 1.7.4979-alpha.
- Release build: 0 warnings, 0 errors.
- Construction behavior tests: passed.
- Project XML, About.xml and workflow YAML checked.
- ZIP verified: only our two DLLs, no game/reference DLLs.
- Not tested inside RimWorld; ZIP is a development startup scaffold.
- GitHub App installation completed; Contents write verified on 2026-10-18.
- First successful remote configuration commit: 9fee653d694716ec5291d410d779691ec8c0abd4.
- Initial CI run will follow the project initialization commit; consult Actions for its live status.
- No in-game validation has been performed.

2026-10-18: M1 dev-mode validation commands (branch `claude/m1-city-registry`, stacked on `feature/m1-city-persistence`).
- Local: .NET SDK 10.0.112 (Ubuntu package); Core build and all behaviour tests passed. The game adapter could not be built locally (nuget.org unreachable from the agent sandbox).
- CI run 39152445343 on dd7277c: build against Krafs.Rimworld.Ref 1.7.4979-alpha with warnings as errors, all tests, packaging and artifact upload passed.
- Artifact contents were not inspected by the agent (artifact download host unreachable from the sandbox); packaging script only copies our two DLLs.
- Not tested inside RimWorld. In-game save/reload procedure: docs/CITY_PERSISTENCE.md.
