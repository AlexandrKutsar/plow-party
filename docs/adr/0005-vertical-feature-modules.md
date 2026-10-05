# Vertical feature modules split into Gameplay and Meta

Code is organized by feature, not by technical layer: each feature folder under `_Project/Gameplay/` or `_Project/Meta/` owns its Simulation, Network, View, Config, and Tests, has one asmdef, and carries a `CLAUDE.md` that agents load automatically when working there. Gameplay (inside a Match) never references Meta (menus, account, backend); they meet only through Session starting a Match and `MatchResult` coming out. Asmdef references enforce both rules at compile time.

## Consequences

The "Simulation has no Fusion types" rule is not compiler-enforced, because a feature has one asmdef. Code review checks it; a feature that keeps violating it gets split into `.Simulation` and main asmdefs.
