# Termite Web Workbench v0.1

A standalone, offline polyline workbench. **This is a partial development build, not a complete replacement for Termite 1.4. G-code export is not implemented.**

## Open
1. Open `web/termite-web.html` in GitHub.
2. Choose **Download raw file** from the file toolbar.
3. Double-click the downloaded HTML file to open it in Chrome, Edge or Firefox.
No server, dependencies, Rhino or Grasshopper are needed for the implemented features. Opening GitHub's source page does not launch the app.

## Implemented
- JSON and simple CSV path imports; millimetres, up to 200,000 points.
- Orbit/pan/zoom 3D canvas preview, travel lines and selected-path highlighting.
- Path-sequence playback (not a physical printer simulation).
- Polyline length sorting and strict minimum-length filtering.
- Vertex-centroid axis/distance sorting (approximations).
- Port of the recovered clustered C# travel-ordering algorithm, using the legacy System.Random(1) sequence. Preserves original algorithm behaviour including its unusual 2-opt cost comparison and no-flip candidate selection. This is not the full travel-sort cluster; original graph selection/subdivision wiring is not yet reproduced. Runtime limited to 250 paths / 45 seconds; uses a browser worker.
- Path reversal, alternating layer directions, fixed-fraction closed seam shifts.
- Stacked copies and an approximate spiral interpolation utility.
- Eight undo states, reset, operation history, JSON/CSV export and project round trips.
- In-app coverage table covering all 20 original components.

## Not yet implemented
- Rhino NURBS and surface geometry, STL slicing, .3dm import.
- Original base, wall, sling and stair generators.
- Attractor/random seam shifting; original shifted-end semantics.
- Grasshopper data-tree behaviour and full sorting parity.
- Termite main G-code generator: printer profiles, extrusion rules, priming, first-layer handling, safe start/end, retraction, intersection treatment, pause points, multi-layer joins, numerical formatting.
- Physical extrusion display, clipping planes and original printer geometry.
- Original-vs-web output comparisons.

## Tests
`node web/core.test.cjs` runs dependency-free tests.
18 core tests passed in a V8 JavaScript runtime, including length calculations, interpolation, seam closure/length conservation, source immutability, CSV round trips, and the known legacy random sequence.
Inline HTML scripts passed JavaScript syntax checks.
No browser visual/interaction tests or Rhino parity tests were available in this session. See test-results.json.

## Source provenance
Recovered source: ../recovery/. Original Termite author: Julian Jauk.
The travel-ordering function corresponds to recovery/scripts/termite-sort-by-travel-path/02-object-80.cs. Other operations are explicitly identified as polyline adaptations or utilities. Source metadata states MIT License; see recovery/README.md.
The HTML embeds core.js to remain a single offline file. When editing core.js, update the embedded engine as well.
