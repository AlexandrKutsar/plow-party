# Shared

Plain data types that cross the Gameplay/Meta boundary, such as `MatchResult` and `PlayerId`. References nothing, so both sides can use it.

## Rules

- Only immutable data types and value objects; behaviour belongs to the module that owns the rule.
- A type goes here only when both Gameplay and Meta need it; otherwise it stays in its feature.

The asmdef sets `overrideReferences` with no precompiled DLLs, so Fusion types cannot leak in.

Empty until the first cross-boundary type exists.
