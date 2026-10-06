# Snow Grid as packed 4-bit Depth in one host-written networked array

The Snow Grid is a 0.5 m grid over the Arena whose Cells hold a Depth of 0–15 in 4 bits: 0 is cleared, 3 is fully covered, 4–15 is a Snow Pile stacked above full snow. Eight Cells pack into one `int` of a `[Networked] NetworkArray<int>` (450 words for a 30 × 30 m Arena) written only by the Host; Fusion's per-word delta compression sends only the words that changed, which gives chunked sync without a protocol of our own. Clients do not predict the grid; their View clears Cells under the local Vehicle's Blade cosmetically until the Host's state arrives. Snow speaks in Depth steps, never in Load; Bucket owns the conversion.

## Considered Options

- One bit per Cell (the GDD's bitset): cheapest, but Regrowth could only flip a Cell from cleared to full at once, partially regrown snow could not be worth less than fresh snow, and Snow Piles would need a separate store.
- Per-Cell regrowth timers in networked state: smooth by construction, but several times the state and traffic. Superseded in part (Snow polish): the timers now exist host-only, outside networked state (see Regrowth below), so the networked grid is still the only synced state.
- Hand-made chunks with dirty flags over RPC or reliable data: duplicates what Fusion's delta compression already does and needs its own late-join handling.
- Replaying "Vehicle passed here" events on every peer: tiny traffic, but a late joiner cannot rebuild the grid and any divergence is permanent.
- Client prediction of the grid with resimulation: only correct if Bucket Load is predicted too; Simulation is pure and deterministic, so switching to it later does not touch the rules.

The snow surface gets its own material instead of `M_Palette`, as `Art/CLAUDE.md` requires a reason for: its look comes from a texture the View writes from the grid at runtime, one texel per Cell, which a palette UV cannot express. How that texture is drawn (a displaced, smoothed mesh) is ADR-0015.

## Regrowth (amended by Snow polish)

Regrowth was first a seeded per-Cell chance to gain one step every interval. Tracks then refilled as random speckles, not as a readable trail. It is now age-based and has no randomness: the Host keeps, per Cell, the time of its next Regrowth step; lowering a Cell below full sets it to now + `RegrowthDelay`, after which the Cell gains a step every `RegrowthStepInterval` up to full. The oldest part of a track refills first, so a track fades as a tail, and the result still depends only on elapsed time, never on tick slicing. The per-Cell times are host-only, like Blizzard progress.

## Consequences

A finer Cell or a larger Arena grows the array linearly; at 0.25 m the same Arena needs 1800 words, so a cell-size change is a bandwidth decision, not just a config tweak. The array holds 1024 words, enough for a 40 × 40 m Arena at 0.5 m (800). Regrowth times, Blizzard progress, and the seed live only on the Host, outside networked state; that is safe while a Host never migrates, and host migration (GDD MVP+) must network them or rebuild them from elapsed time. Age-based Regrowth dirties words where tracks were driven, spread over time as their delays expire, instead of scattered Cells across the whole grid at every interval.
