# Public browser playtest

- Play: https://dumb-tony.github.io/TOTALLED/
- Source: https://github.com/Dumb-Tony/TOTALLED
- Windows download: https://github.com/Dumb-Tony/TOTALLED/releases/latest
- Bug reports: https://github.com/Dumb-Tony/TOTALLED/issues/new

The Unity WebGL release build is hosted on the repository's `gh-pages` branch. The latest Windows ZIP is published under release `v0.6.0`.

## Live checks — 0.2 (historical)

Tested the actual public Pages URL in a desktop Chromium-based browser:

- Unity downloads, starts, and displays the sedan, lab and instrumentation.
- The 1 key launches a real frontal wall collision.
- The first observed collision yielded 124 beams, broke 12 connections, and accumulated approximately 5.605 metres of summed permanent beam rest-length travel (not vehicle shortening).
- R recovers the vehicle without clearing that damage.
- V displays the node/beam overlay, and P pauses the simulation.
- No new runtime errors appeared after the corrected, versioned build was loaded.

## Web-specific fixes

Unity's IL2CPP stripping did not retain collider types constructed implicitly by CreatePrimitive. `Assets/TOTALLED/link.xml` preserves those types. The build also uses content-hashed filenames, so old cached asset data cannot be mixed with a newer WebAssembly runtime. The observed mixed-cache startup failure was resolved by loading the versioned build.

This verifies browser startup and basic lab interactions. It does not establish mobile support, multiplayer, long-session performance, or the full driving/fun acceptance gate. The collision and simulation limitations documented in Design.md still apply.


## Live checks — 0.3 (historical)

Published build loads with warm lighting, textured surfaces, node-bound trim, real-time shadows, the parked target, and compact HUD. H opens full telemetry. The 5 target collision produced 120 yielded beams and 9 broken connections in the live browser. The target stayed fixed. The 6 shortcut also starts the sideways test. No browser console errors were observed. Windows launch/crash smoke verification passed with two rendered captures and no errors.


## Live checks — 0.4 (historical)

The public browser build loads and completes a two-way impact using 5. The teal sedan moves out of its bay. Switching into it with E shows retained damage: 62 yielded beams and 0.763 m cumulative plastic travel in the observed run. The original player has 105 yielded beams and 3 broken connections. No console errors were observed. Windows smoke verification also passes.


## Live checks — 0.5

The public build loads with wheel openings, shaded windows, rear plates, corrected skid arcs, brick buildings and loading doors. No startup console errors were observed. Automated powered-driving checks confirm the actual tire and rim transforms rotate while preserving axle alignment. All 43 checks pass; Windows startup/crash smoke verification passes with no errors.


## Build checks — 0.6

All 45 numerical checks pass. Windows startup/crash smoke verification passes without errors. Intact and exposed-engine captures were inspected. The browser build targets standard WebAssembly because the bundled wasm23 engine archive failed with an invalid function relocation in Unity's serialization backend. Engine stripping remains enabled. The build script explicitly selects runtime-speed optimization.

Live Pages 0.6 verification: updated silhouette and transparent cabin render correctly; a frontal wall impact yielded 127 beams, broke 12 connections and retained 5.625 m of cumulative plastic travel in the observed run. No browser console errors were recorded. Compact HUD shows 0.6; the expanded telemetry header retains the older 0.5 label (cosmetic).

