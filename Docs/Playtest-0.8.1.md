# Crash Lab 0.8.1 — Side impacts and collision performance

Side doors now take permanent local dents under a T-bone. Their reinforcement yields before the stronger floor and sills. Cabin-side members also deform, and the hidden duplicate collision skin behind each door opening has been removed. Forces still load both node graphs; the target is neither fixed nor assigned scripted damage.

Car contacts now query a refitted hierarchy of swept node spheres instead of scanning every node against every triangle. Corrected nodes update the hierarchy immediately; detached parts retain separate leaves. Static obstacle candidates are reused between constraint iterations with a conservative movement margin. Physics resolution remains 10 substeps and 8 solver sweeps.

Body skin buffers are reused, tiny per-patch arrays have been removed, and trim topology is uploaded once instead of recreated every frame. This reduces garbage collection pressure.

Validation:
- Both left and right T-bones at 18 m/s leave retained door dents measured relative to their own corner plane, not whole-door displacement.
- The spatial hierarchy is compared with exhaustive swept bounds across 200 incremental position updates.
- Existing momentum, energy, pristine stability, driving, wheel, front/rear crumple, recovery and repeated-impact tests remain required.
- See Verification-Summary.json and Collision-Performance.json for final results. Timings are editor physics ticks, not browser frame rate.
- Browser interaction automation remains unavailable after the host restart. Deployment asset verification and packaged Windows smoke testing are reported separately.

The sides remain a simplified node/beam model, with coarse door panels and no full self-contact solver. Wrecks persist, so spawning many cars still increases workload.
