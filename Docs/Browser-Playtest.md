# Public browser playtest

- Play: https://dumb-tony.github.io/TOTALLED/
- Source: https://github.com/Dumb-Tony/TOTALLED
- Windows download: https://github.com/Dumb-Tony/TOTALLED/releases/latest
- Bug reports: https://github.com/Dumb-Tony/TOTALLED/issues/new

The Unity WebGL release build is hosted on the repository's `gh-pages` branch. The Windows ZIP is published under release `v0.1.0-playtest`.

## Live checks

Tested the actual public Pages URL in a desktop Chromium-based browser:

- Unity downloads, starts, and displays the sedan, lab and instrumentation.
- The 1 key launches a real frontal wall collision.
- The first observed collision yielded 108 beams with approximately 1.246 metres of summed permanent beam rest-length travel (not vehicle shortening).
- R recovers the vehicle without clearing that damage.
- V displays the node/beam overlay, and P pauses the simulation.
- No new runtime errors appeared after the corrected, versioned build was loaded.

## Web-specific fixes

Unity's IL2CPP stripping did not retain collider types constructed implicitly by CreatePrimitive. `Assets/TOTALLED/link.xml` preserves those types. The build also uses content-hashed filenames, so old cached asset data cannot be mixed with a newer WebAssembly runtime. The observed mixed-cache startup failure was resolved by loading the versioned build.

This verifies browser startup and basic lab interactions. It does not establish mobile support, multiplayer, long-session performance, or the full driving/fun acceptance gate. The collision and simulation limitations documented in Design.md still apply.
