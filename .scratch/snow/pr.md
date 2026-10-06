## Summary

Snow Grid for the Match: Vehicles scrape Snow with their Blade, cleared Cells regrow, Blizzard waves re-cover the Arena and leave Snow Piles, and a Spill drops a Snow Pile anyone can collect. Design and sync decision: ADR-0012; spec: `.scratch/snow/spec.md`.

```text
Host, every Fusion tick (SnowGridDriver.FixedUpdateNetwork)
  for each Vehicle in Slot order
    SnowGrid.Scrape(SnowBlade.Ahead(vehicle), room)    # Depth steps taken
  SnowGrid.Tick(elapsed Playing time)
    Regrowth step: +1 Depth by seeded chance, up to full
    Blizzard front: passed Cells -> full, drops seeded Snow Piles
  copy 450 packed words -> [Networked] NetworkArray<int>   # Fusion sends changed words only
  raise Scraped(Vehicle, steps)                        # Bucket will turn steps into Load

Client, every frame (SnowGridView)
  copy words -> local SnowGrid
  scrape local Player's recent Blades                  # cosmetic pre-clear, no wait for Host
  repaint changed words into a 60x60 texture on the floor
```

Cell: 0.5 m, Depth 4 bits (0 cleared, 3 full, 4–15 Snow Pile), 8 Cells per `int`.

```diff
 client/Assets/_Project/
 ├── Gameplay/
+│   ├── Snow/                      # new module: Simulation, Network, View, Config, Tests
 │   └── Vehicle/Simulation/
+│       └── VehicleArena.Contains  # point-in-obstacle, used to mask Cells
 ├── Bootstrap/                     # SnowConfig in root scope, driver + view in MatchScope
 ├── Configs/
+│   └── SnowConfig.asset
 ├── Art/Environment/
+│   └── SnowGrid/                  # V_SnowGrid quad, M_SnowGrid (runtime base map)
 └── Scenes/Match.unity             # SnowGrid scene NetworkObject
```

Also: `AssembliesToWeave` gains `PlowParty.Gameplay.Snow`; GLOSSARY gains Cell, Depth, Blade and refines Snow Pile, Drift, Blizzard; GDD 4.4 records Blizzard piles, GDD 6 records Drift piles for later.

## Evidence

- **Before:** no Snow; the floor is bare, no `PlowParty.Gameplay.Snow.Tests` suite.
  **After:** all EditMode suites green, 94/94 (Snow 49, Vehicle 45 incl. 5 new `VehicleArena.Contains` tests):

```text
Scrape_RoomSmallerThanSnow_StopsAfterRoomInRowMajorOrder          Passed
Spill_MoreThanOneCellHolds_OverflowsIntoSurroundingRingRowByRow   Passed
Tick_SameTimeReachedInSmallSteps_GivesSameGridAsOneStep           Passed
Tick_CellClearedBehindFront_StaysClearedUntilNextWave             Passed
Tick_HalfwayThroughWave_DropsPilesOnlyWhereFrontHasPassed         Passed
Tick_SameSeedAndInputs_ProducesIdenticalWords                     Passed
```

- Play Mode, Host: track left behind the Blade, regrowing; Spill from `eval` leaves a Snow Pile; first Blizzard at 45 s leaves 3 Piles (450 steps) behind its front.
- Multiplayer Play Mode (Host + Client): Client receives the grid and sees the Host's track; local track appears immediately via pre-clear. Checked by hand.

## Merge Danger

**Door:** two-way

New module plus additive wiring; reverting the PR removes it cleanly. The sync format (4-bit Depth in a `NetworkArray<int>`) is the costly part to change later, recorded in ADR-0012.

**Blast Radius:** Match scene

Every Match now spawns the `SnowGrid` scene object: ~450 words of networked state per session and one runtime texture. Bucket, DropOff, Drifts, and Bots will build on `Scraped`, `Spill`, and the Depth-step unit. Regrowth/Blizzard progress and the seed live only on the Host, so host migration (MVP+) must handle them.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
