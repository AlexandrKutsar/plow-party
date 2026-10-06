# Snow Grid as packed 4-bit Depth in one host-written networked array

The Snow Grid is a 0.5 m grid over the Arena whose Cells hold a Depth of 0–15 in 4 bits: 0 is cleared, 3 is fully covered, 4–15 is a Snow Pile stacked above full snow. Eight Cells pack into one `int` of a `[Networked] NetworkArray<int>` (450 words for a 30 × 30 m Arena) written only by the Host; Fusion's per-word delta compression sends only the words that changed, which gives chunked sync without a protocol of our own. Clients do not predict the grid; their View clears Cells under the local Vehicle's Blade cosmetically until the Host's state arrives. Snow speaks in Depth steps, never in Load; Bucket owns the conversion.

## Considered Options

- One bit per Cell (the GDD's bitset): cheapest, but Regrowth could only flip a Cell from cleared to full at once, partially regrown snow could not be worth less than fresh snow, and Snow Piles would need a separate store.
- Per-Cell regrowth timers: smooth by construction, but several times the state and traffic; a seeded per-Cell chance to gain one step keeps the grid the only state.
- Hand-made chunks with dirty flags over RPC or reliable data: duplicates what Fusion's delta compression already does and needs its own late-join handling.
- Replaying "Vehicle passed here" events on every peer: tiny traffic, but a late joiner cannot rebuild the grid and any divergence is permanent.
- Client prediction of the grid with resimulation: only correct if Bucket Load is predicted too; Simulation is pure and deterministic, so switching to it later does not touch the rules.

The snow surface gets its own material instead of `M_Palette`, as `Art/CLAUDE.md` requires a reason for: its colour comes from a texture the View writes from the grid at runtime, one texel per Cell, which a palette UV cannot express.

## Consequences

A finer Cell or a larger Arena grows the array linearly; at 0.25 m the same Arena needs 1800 words, so a cell-size change is a bandwidth decision, not just a config tweak. Regrowth touches scattered Cells each regrowth step, so it dirties many words at once; its interval is a bandwidth knob as well as a balance one.
