# Termite 1.4 recovery and web-port status

Status: source recovery complete for the exported component graphs; standalone web application not yet implemented.

## Source
Recovered from FORCHATGPT_DECODED.txt, blob 2e8f928f69445bc42f9c548913ac1b508c66cde7 in CBC88/gpt.
Author metadata identifies Julian Jauk, Institute of Architecture and Media, Graz University of Technology, and states MIT License. Original attribution is retained here; the full license text is not included in the uploaded metadata inspected so far.

## Findings
- 20 top-level component clusters decoded.
- 4978 serialized objects across those clusters, including groups, panels and other interface objects; this is not a count of unique algorithms.
- 50 embedded script occurrences (duplicates retained), in Python, VB.NET and C#.
- Main G-code Generator contains 1884 serialized objects, 31 exposed inputs and 5 outputs.
- Most generation logic is represented by connected Grasshopper components rather than a single script.
- Decoding errors concern binary ON_Data payloads in Display Simulation. Their contents have not been recovered. The fallback GH_LooseChunk constructor in the supplied PowerShell script failed; these errors do not establish corruption or absence of geometry.
- Code snippets remain Grasshopper-hosted source fragments. They are not standalone programs and have not been executed.
- No source scripts were executed during recovery.

## Contents
- scripts/: exact decoded source text; filenames identify parent component and original serialized object index. File extensions reflect source syntax, not a language conversion.
- component-catalog.json: exposed input/output names, descriptions, object counts and component-type frequencies.
- decoding-errors.txt: errors preserved from the supplied export.
- The original decoded export on this branch remains the authoritative record of connections, data-tree settings, expressions and defaults.

## Port requirements
1. Trace the generator's active output dependencies, including list matching, data-tree paths, grafting, flattening, tolerances, curve discretization and layer names.
2. Implement independent curve/surface geometry operations and replace Rhino document and Windows UI dependencies.
3. Reproduce printer profiles, path sequencing, extrusion and speed rules, crossing treatment, priming, safe start/end, retraction and M0 pauses.
4. Implement geometry import, path editing, preview and simulation without Grasshopper.
5. Compare toolpaths and G-code against outputs from the original examples with identical settings. Exact equivalence is not established by source extraction.

## Component inventory

| Component | Serialized objects | Script occurrences |
|---|---:|---:|
| Termite Create Alternated Layers | 111 | 0 |
| Termite Create Base | 295 | 8 |
| Termite Create Shifted Ends | 131 | 0 |
| Termite Create Shifted Seams | 33 | 0 |
| Termite Create Sling Contours | 607 | 1 |
| Termite Create Spiral from Curve | 194 | 0 |
| Termite Create Spiral from Stacked Layers | 232 | 1 |
| Termite Create Spiral from Surface | 257 | 2 |
| Termite Create Stair Contours | 345 | 2 |
| Termite Create Wall | 183 | 5 |
| Termite Display Clipping Planes | 32 | 1 |
| Termite Display G-code | 58 | 1 |
| Termite Display Simulation | 180 | 1 |
| Termite Main G-code Generator | 1884 | 25 |
| Termite Sort Along Axis | 167 | 0 |
| Termite Sort Along Curve | 48 | 0 |
| Termite Sort by Distance | 50 | 0 |
| Termite Sort by Length | 36 | 0 |
| Termite Sort by Travel Path | 114 | 2 |
| Termite Sort Out by Length | 21 | 1 |
