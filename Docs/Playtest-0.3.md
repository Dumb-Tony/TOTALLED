# Crash Lab 0.3 — Motor Works yard

Visual direction: original low-poly industrial scenery with the restrained detail and weathered color palette of earlier open-world driving games. Warm directional light, cool ambient fill, soft real-time shadows, a procedural sky and distance haze replace the flat test presentation. Asphalt aggregate, concrete joints, oxidized enamel and tire tread are generated original textures. Warehouses, shipping containers, yard lights, lane paint and skid marks give the course a recognizable setting.

The sedan has node-bound windshield, rear and side glass, lamp lenses, grille bars, bumpers, door handles and rubbing strips, plus detailed steel wheels. Trim follows the deforming structure and stops bridging severely separated nodes. Glass is an opaque visual approximation; shattering and glass collision are not implemented. Car geometry remains deliberately simple and is not the final vehicle art.

The default HUD is compact. H toggles full telemetry. Existing driving forces, handbrake behavior and deformation tuning are unchanged.

## Parked collision target

A teal sedan sits in the marked bay at (12, 0, 9), broadside to the approach. It is a fixed prop with compound body, cabin and wheel colliders. Drive into it freely, press 5 to launch into its flank, or press 6 to slide the player's side into it. These shortcuts reuse the current damaged car and retain damage.

The target does not move or deform. These are deformable-player-versus-fixed-car-shaped-obstacle tests, not a two-way soft-body collision solver. User-spawned specimens and debris still cannot collide with each other. Future moving cars need mutual momentum exchange and robust surface contact.

## Validation

All 35 checks pass, including the previous 31 handling and damage checks plus two target collision/damage checks and two fixed-target persistence checks. Target T-bone and player-side impacts produce permanent deformation without invalid node states. Rendered yard, pristine-car, damaged-car and target-impact captures are generated in Artifacts. See Verification-Summary.json for measured results. Visual and numerical validation do not establish the full long-session driving acceptance gate.
