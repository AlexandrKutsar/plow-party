# Custom deterministic kinematics with capsule shapes, not Fusion Physics

Vehicles move and collide in our own engine-free simulation (`VehicleWorld`), stepped once per Fusion tick, rather than in Unity physics driven by Fusion Physics. Each Vehicle is a capsule — a segment along its facing swept by a radius — sized to the model's footprint; obstacles are axis-aligned boxes and circles. GDD 5 named custom kinematics as the first choice and Fusion Physics as the fallback; the fallback is not needed.

## Considered Options

- Fusion Physics in host mode: every predicted tick resimulates the whole PhysX scene on the client, which is costly on a phone, and PhysX results drift between Host and client, so corrections show as jitter. Our step is plain C# over a handful of shapes, cheap to resimulate and identical on every peer for identical inputs.
- Circles: simplest, but the Vehicle is about 1.5 times longer than wide, so a circle either clips the nose and tail through walls or leaves a visible gap at the sides, and Ram direction is wrong on long hits.
- Oriented boxes: closest to the model, but corner contacts make the springy bounce snag; capsules slide along walls and give a clean contact normal.

## Consequences

Every contact rule is covered by EditMode tests without a scene. Map geometry must be expressible as axis-aligned boxes and circles; a rotated or mesh obstacle needs a new shape in the simulation first. The capsule is configured, not read from the mesh, so a model with a new footprint needs its config updated.
