# Art: winter farm content (Critters, Environment, Props)

Status: ready-for-agent

Branch `feature/art-farm`. Change only `art/` and `client/Assets/_Project/Art/`; shared files (`GLOSSARY.md`, `docs/architecture.md`) get additive edits only.

## Setting

Winter farmyard. Critters melt snow into water for the farm. Cartoon low poly, flat shading, warm colors (red barn, wood, hay) on white snow. Readability from the top-down camera beats detail. The Vehicle exists: a tractor with a front Bucket and an open seat, anchor `CritterSeat`.

## Scope, one commit each

1. Critters: 4–6 animals seated on `CritterSeat`, under ~1500 triangles, recognizable from above by silhouette and color. New names go to `GLOSSARY.md` first.
2. Environment, modular farm set:
   - perimeter buildings: barn, chicken coop, shed or sauna, house;
   - field obstacles: round hay bale, fence sections, woodpile, well, cart, pine. Footprints are boxes or circles (the simulation knows only those);
   - Drop-Off Zone: big cauldron over a fire with steam, center of the map;
   - Drift: snowed-over haystack.
3. Props: Loot gift, Gadget pickup, snowball.

## Sources (every asset approved by the user before download or build)

- A: free poly.pizza asset (CC0 or CC-BY), B: paid asset (user buys), C: Blender script per `art/CLAUDE.md`, D: rework of A/B to the style.
- Third-party models are brought to the project style: one `M_Palette` material, palette colors only (new colors go to `art/blender/plow_art/palette.py`), flat shading, triangle budget. If impossible, ask and record an exception in ADR-0009.
- Licenses and authors in `art/CREDITS.md`: asset, author, link, license (mandatory for CC-BY).

## Constraints

- Final exports headless from the worktree: `blender --background --factory-startup --python art/blender/build.py -- <model>`.
- Visual prefabs `V_<Asset>` per `client/Assets/_Project/Art/CLAUDE.md`: no scripts, colliders, or NetworkObject; anchors named with glossary terms.
- No comments in code, Python included. Docs and commits in English.

## Done when

- Models build headless, import without warnings, each has a `V_` prefab.
- Asset tables updated in `art/CLAUDE.md` and `client/Assets/_Project/Art/CLAUDE.md`, plus `art/CREDITS.md`.
- The user has seen renders of every asset (Blender `look` or Unity render).
- PR written with `/pr`.
