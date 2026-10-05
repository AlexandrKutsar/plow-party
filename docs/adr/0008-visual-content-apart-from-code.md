# Visual content lives apart from code, in one Art folder with visual prefabs

All visual content lives in `client/Assets/_Project/Art/`, grouped by kind of content (`Vehicles/`, `Critters/`, `Environment/`, `VFX/`, `UI/`), one folder per asset, with type prefixes (`SM_`, `T_`, `M_`, `V_`, ...). Art holds no code. Each asset gets a visual prefab `V_<Asset>` with only Unity components; the feature's gameplay prefab in `<Feature>/Prefabs/` carries scripts and networking and nests the visual prefab as `Model`. Editable sources stay outside `client/` under `art/`; import settings are enforced by a postprocessor in `_Project/Editor/`. Rules: `client/Assets/_Project/Art/CLAUDE.md`.

## Considered Options

- Art inside each feature (`Gameplay/Vehicle/Art/`): content is shared across features (a Critter is in the Match and in Meta screens), and people look for content by kind, not by code module.
- Gameplay prefabs nesting the model file directly: works, but every look change then edits a networked prefab; a script-free visual prefab keeps the art boundary clean and swappable.
- No prefixes, folders only: `Vehicle.prefab`, `Vehicle.fbx`, and `Vehicle.cs` collide in search and in agent output; prefixes make the type explicit.

## Consequences

An asset rename touches its prefix and folder through the Editor (or `AssetDatabase.MoveAsset`) so GUIDs survive. Generated assets keep their output paths in sync with `art/blender/`.
