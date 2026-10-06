# Snow surface as a GPU-displaced, smoothed height field

The Snow Grid is drawn as a mesh displaced in the vertex shader by a height texture, not as a flat quad coloured one texel per Cell. The View keeps an R8 texture with one texel per Cell holding the Cell's shown height, eased toward the height of its Depth so that scrapes and Regrowth steps never pop. A static grid mesh with `VerticesPerCell` vertices per Cell edge (2 by default) is built once. `SH_SnowSurface` (hand-written URP HLSL) samples the texture with a 4-tap cubic B-spline in the vertex stage, displaces Y, takes normals from central differences of the same smoothed field, and colours by height. The B-spline hides the Cell grid at gameplay distances, gives tracks soft banks, and gives Snow Piles real volume (Depth → metres from config).

## Considered Options

- Flat quad, bilinear colour texture (the first version): cheapest, but the Cell grid shows at the follow camera, tracks have hard pixel steps, and a Snow Pile has no volume.
- Rebuild a displaced mesh on the CPU each frame: no vertex texture fetch, but tens of thousands of vertices rewritten and uploaded per frame on a phone, and smoothing would cost CPU too.
- Shader Graph with vertex displacement: the project convention for shaders, but a B-spline filter, finite-difference normals, and a custom lighting ramp are long node graphs that diff badly and cannot be authored from the CLI. A `.shader` file is reviewable text; the `SH_` prefix was added to `Art/CLAUDE.md` for it.
- Per-pixel normals from the height texture: crisper banks, but every floor pixel would run four B-spline samples, and the floor covers most of the screen. Vertex normals at 0.25 m spacing read well from the follow camera.
- Catmull-Rom instead of B-spline: keeps exact Cell heights but overshoots at track edges (rings); the B-spline never overshoots and is softer, which suits snow.

## Consequences

Per frame the CPU does only easing for Cells that changed and one 6.4 KB texture upload when a texel moved (80 × 80 Cells). The GPU does 20 texture taps per vertex: 161 × 161 vertices for a 40 × 40 m Arena at the default density is about 0.5 M taps per frame, and `VerticesPerCell` 1 quarters that on weak devices. The surface needs vertex texture fetch (OpenGL ES 3 or Vulkan, already the Android minimum for URP). The B-spline lowers narrow features: a one-Blade-wide track shows about two thirds of its full depth, and a Pile's peak is lower than its config height. Default heights in `SnowConfig` (0.2 m snow, 1 m Pile) are set above the wanted look for that reason. The surface does not cast shadows; it receives the main light's, and its DepthOnly and DepthNormals passes displace the same way, so depth prepasses and SSAO see the real surface.
