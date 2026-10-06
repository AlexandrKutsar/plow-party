## Summary

Winter farm art: 6 Critters, the farm Environment (with Drop-Off Zone and Drift), 3 Props. Only `SM_` models and script-free `V_` prefabs; no gameplay code.

```diff
 art/blender/plow_art/
+├── critter.py      # shared seated-Critter kit
+└── imported.py     # CC0 glTF from art/sources/ -> palette colors, one mesh, footprint size
 client/Assets/_Project/Art/
+├── Critters/       # Fox, Bear, Rabbit, Raccoon, Penguin, Beaver
+├── Environment/    # Barn, Shed, ChickenCoop, House, Fence, Well, WoodPile, HayBale, Cart, Pine, DropOffZone, Drift
+├── Props/          # Loot, Loot_Gadget, Snowball
+└── VFX/Steam/      # FX_Steam on the cauldron
```

Palette grows to 8×8 (`T_Palette`, `SM_Vehicle` rebuilt, same look). Footprints for Obstacles are in `art/CLAUDE.md`; sources are credited in `art/CREDITS.md`. ADR-0008/0009 amended; GLOSSARY + Critter Species, Snowball; `*.glb` in LFS.

## Evidence

- **Before:** `Art/` had only the Vehicle.
  **After:** `build.py` exports 23 FBX headless, 0 errors; rebuilds are geometry-identical. Unity: 23 `V_` prefabs, all on `M_Palette`, no scripts or colliders, 0 console errors.

## Merge Danger

**Door:** two-way

**Blast Radius:** art-only

Nothing uses the new prefabs yet.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
