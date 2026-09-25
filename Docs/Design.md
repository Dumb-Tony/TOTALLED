# TOTALLED: Crash Lab design contract

## What we are making

A simple, pick-up-and-play demolition derby about **driving destroyed cars**. WASD/controller, handbrake, readable weight and traction; sophisticated damage beneath forgiving controls. A round should turn a clean arena into a scrapyard. A vehicle remains in contention as long as its remaining mechanical parts and contact patches can physically propel it. No overall HP meter, automatic explosion at a threshold, canned destroyed mesh or automatic disappearance of wrecks.

The immediate product is a technical experiment: one Sacrificial Sedan, flat test slab, walls, offset barrier, narrow pole and ramp. No AI, progression, menus, vehicle catalog or full derby until repeated collisions alone are convincing and entertaining. BeamNG is the simulation inspiration, not a claim of equivalent fidelity or copied implementation.

## Non-negotiable damage behavior

- Structure: nodes carry mass and velocity; beams carry elastic loads, yield plastically and break. Every impact begins from the previously deformed structure. Damage has direction and location.
- Skin: body surfaces follow structural nodes. No separate cosmetic damage state that can contradict the physical skeleton.
- Wheels: independent suspension attachment geometry controls alignment and the directions of actual tire forces. Distorted mount geometry changes handling. A wheel can lose one link, hang from surviving links, then detach.
- Panels: independent hinges and latches; partial attachments permit flapping/dangling before final separation. Detached parts remain physical objects in the experiment.
- Tires/rims: local tire failure, loss of rubber radius/grip, exposed rims and bent-wheel behavior. Later add abrasion, shredding and wheel-well interference.
- Mechanical components: spatially attached engine, radiator/cooling, transmission/drivetrain and fuel system. Crushing a front compartment must not magically damage a rear component. Cooling loss causes progressive overheating. Mechanical incapacity, not global health, ends propulsion.
- Persistent aftermath: intact wrecks and major debris remain obstacles. Eventually cars collide with cars, push wrecks and become trapped against them.
- Feedback: later add collision audio, dirt, sparks, glass, steam, warning sounds and restrained camera/controller feedback. Debug percentages in Crash Lab are instrumentation, not the intended derby HUD.

## Implemented experiment

`SoftStructure` is a world-space extended position-based dynamics (XPBD) solver, using 10 substeps and 8 alternating constraint sweeps per 50 Hz tick. It handles mass-weighted distance constraints with compliance, constraint-load-driven plastic flow, cumulative plastic travel, rest-length bounds, irreversible breakage, axial velocity damping, swept spherical nodes and static collider penetration correction. There is no rigidbody standing in for the chassis and no shape-matching spring that restores the pristine shape.

`SacrificialSedan` defines a triangulated 42-node lower lattice, four roof nodes, four ten-node panels, and four wheel hub nodes. The dense cabin is stronger than the ends. Each panel has a braced 3×3 surface and one internal support node. Two stronger hinge mounts and a weaker latch each use a triangulated group of constraints that fail together. Surface subdivisions permit localized creases. Yielded beam impulses are bounded and plastic rest lengths flow toward actual deformation without overshooting. Wheel hubs have four suspension links, with a more compliant spring link. Rear hubs receive drive force. The service brake acts on all four wheels; the handbrake progressively locks the rear pair and reduces rear lateral grip, preserving front steering traction. Current mount vectors determine steering and tire force axes. Component condition records maximum local span distortion; cooling temperature and fuel leakage evolve in time.

`SedanView` renders surfaces directly from current node positions, with original procedural placeholder geometry. Panels are their own deforming surfaces. Debug modes show nodes, stress, accumulated plastic travel, broken connections and recent contact impulses. `CrashLab` owns the environment, repeat impact launch commands, recovery, time controls, follow/orbit camera, telemetry and new specimens.

## Honest limits and next engineering work

This is an early feasibility prototype. It is not yet a validated vehicle simulator or a completed damage system.

1. **Collision scope:** nodes collide with static Unity colliders, including the teal parked car prop added in 0.3. That target is fixed and does not deform. There is no vehicle-to-vehicle, node-to-triangle, panel self-collision, debris-to-car or wreck-to-car response yet. Detached panels and wheels persist and collide with the ground/environment, but can pass through the car. New specimens preserve old wrecks visually and numerically, but those wrecks are not yet obstacles to another car. This is the next structural collision milestone.
2. **Surface coverage:** collision uses spheres at structural nodes. Thin obstacles can pass between nodes; swept nodes reduce tunneling but do not constitute continuous surface collision.
3. **Calibration:** plastic yield maps XPBD constraint force through a tunable effective compliance. Thresholds are prototype tuning values, not automotive material data. Beam rest-length bounds avoid inversion extremes but do not enforce volume or sheet-metal buckling. No claim of energy-conserving crash reconstruction.
4. **Suspension/tire fidelity:** linked hub plus bounded contact forces approximates suspension/traction. No angular wheel dynamics, differential, slip-ratio tire model or true pneumatic tire simulation. Tire failure currently derives from local attachment distortion; rim wobble is a visual approximation. Low-speed braking and support on inclined surfaces need more driving evaluation.
5. **Mechanicals:** component span deformation is a local crush proxy. Cooling/power/fuel consequences work, but there are no physical engine mounts, detailed gears, driveshaft collisions, fire or exposed-component collision meshes yet.
6. **Visuals:** low-resolution procedural body with node-bound opaque glass, trim and lamp lenses; no proper wheel arches, dashboard, glass shards, sounds or particles. Make deformation convincing before producing art.
7. **Persistence:** damage and debris persist for the current session. Disk save/load of vehicle states is not implemented.
8. **Performance:** managed solver and dynamic debug meshes are intentionally inspectable. No jobs/Burst, sleeping islands, broad-phase car collision or multi-car scaling claim. Benchmark before expanding car counts.

## Acceptance gate

Numerical regression: stationary sedan supports its weight without yielding; sub-yield strain recovers; wheel forces produce motion; actual wall collisions produce permanent changes; recovery does not alter any beam rest length or broken state; later hits add damage; damaged propulsion remains possible; repeated front/rear/side collisions stay finite. Record failures honestly and preserve test outputs.

Visual/interactive gate: spend at least 20 minutes driving and repeatedly wrecking the same sedan. Inspect front/rear/offset/side impacts at several speeds, rollover/ramp landings, a wheel hanging by one link, a door hanging on a hinge, exposed rim driving, progressive cooling failure and a badly deformed specimen limping away. Inspect both body and skeleton. Ten different impact sequences should produce visibly distinct shapes. Numerical pass alone does not satisfy this gate.

Then: robust surface/self/car/debris collision; wheel and panel constraints; mechanical exposure; sound and impact feedback; performance measurements. Only after these gates add the simplest Last Car Running arena and derby AI. Later possibilities retained from the concept: different vehicle identities and progressively more capable vehicles while retaining this slow starter sedan, figure eight/team/survival modes, physical reinforcement/weight tradeoffs, emergent damage-driven tactics. They are deliberately outside current implementation scope.



