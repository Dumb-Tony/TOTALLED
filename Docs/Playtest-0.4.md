# Crash Lab 0.4 — two deformable cars

The teal car is now another SacrificialSedan with exactly the same node/beam, wheel, panel and mechanical simulation as the player car. It starts without throttle or brakes. The fixed compound colliders have been removed.

VehicleWorld advances every car on shared 2 ms substeps. Twice per substep, bidirectional node-to-triangle contacts distribute position corrections to both structures according to inverse mass and barycentric contact weights. Both graphs then retain their own plastic deformation, broken mounts and component consequences. Broad-phase bounds reject distant pairs. Surface patches stop participating after severe tearing. The world solver also includes user-spawned cars and their existing node-backed detached parts.

5 launches the current car into the other car's flank; 6 starts a sideways collision. The launch positions follow the other car's current position rather than its original parking bay. Neither shortcut repairs the cars. E switches which car you drive, so either damaged sedan can be tested under power. R recovers only the selected car without healing it.

## Validation

41 regression checks cover the prior handling/damage suite plus dynamic two-car contacts. Both T-bone and player-side impacts push the unpowered car over half a metre and cause permanent deformation in both cars. Repeated hits preserve target damage. An isolated free-space collision conserves combined linear momentum within 2% after accounting for existing air damping, and does not increase total kinetic energy. See Verification-Summary.json for measurements.

## Remaining approximations

This is an initial two-way deformable contact solver. It uses node spheres against current triangular surfaces, not complete triangle-to-triangle continuous collision. Fast grazing edges, coplanar sheets and small debris can still miss contact. Wheels do not yet roll on other vehicles, and self-collision within one car is not implemented. Tangential car-to-car friction, contact manifold persistence and large wreck-pile performance remain further work. There is no AI or automatic throttle on the second car.
