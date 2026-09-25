# Crash Lab 0.5 — rolling wheels and visual refinement

Wheel meshes now apply the existing simulated spin angle around their actual suspension axle. Tire tread, steel rims and ventilation slots rotate together, including reverse motion and the existing rear-handbrake spin reduction. Previously the spin value was only used for bent-rim wobble, so healthy wheels looked stationary.

Visual updates include wheel openings interpolated from the existing deformable body nodes, shaded window textures, rear license plates, clearer layered trim, continuous curved skid marks, brick warehouse materials, loading shutters and yard fencing. These are original generated assets. The visual wheel openings do not change the collision surface; complete wheel-well collision remains future work.

The node graph, wheel forces, handling settings and two-car collision solver are unchanged. Both cars retain deformation and can be selected with E as before.

Validation adds a rendered-object check: drive a sedan with throttle, then verify both its tire and rim transforms change while the tire remains aligned to the suspension axle. This catches a disconnected visual spin value. All 43 regression checks pass. Captures are inspected for the car, yard, and crash deformation. Browser and Windows builds retain the same controls.
