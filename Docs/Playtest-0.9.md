# Motor Works — Level 01 / Playtest 0.9

A three-lap banger race for one player and three AI rivals. Win by crossing the finish line first after all ordered gates. A four-minute simulation-time limit ends stalled events. The complete loop is ready screen, three-second countdown, racing, results, and restart.

## Driving
- WASD / arrows: throttle, reverse and steering.
- Space: rear-wheel handbrake. Shift / Ctrl: service brake.
- F: return to the course behind the last earned gate, keeping all damage. Five-second cooldown.
- R: restart the event with a fresh field.
- Enter: start / race again. Escape: pause / resume. M: mute.

## Race and demolition
The opponents use the same sedan node graph, wheel forces, damage, tires and mechanical components as the player. They steer toward a lookahead point and choose a rival's lane for close-range contact; they attempt a short reverse when stuck. No position animation, invulnerability, repair or catch-up teleport is applied to AI.

Both sides of the oval have concrete walls. Cars and detached pieces remain physical for the event. Ordered directional gates reject wrong-way crossings and infield shortcuts. Finished AI continue circulating until the player reaches results.

## Current limits
Four racers are deliberately used to control physics cost. All are color variants of the current sedan; this release does not claim a vehicle catalog. AI is a first path-following and close-contact implementation, not an advanced tactical opponent. A badly wrecked opponent can stay stranded.

Damage remains the simplified Crash Lab node/beam model. The standalone race smoke check covers startup, countdown, driving, captures and scene restart. Editor full-race timings are not browser FPS. Browser interaction verification is reported in Browser-Playtest.md.

The original CrashLab scene and physics regression suite remain available in Unity.

## Destruction pass
Already-folded crumple members progressively lose up to half their yield strength. Pristine members keep their original threshold. Hood and trunk sheets buckle more readily and their mounts fail sooner, while door mounts remain strong enough to permit a visible dent before releasing.

Matched wall-impact measurements are recorded in Destruction-Before.json and Destruction-After.json. Plastic travel is the sum of permanent beam-length changes, not body shortening. Ordinary driving and idle-stability regressions remain mandatory.
