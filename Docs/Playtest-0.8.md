# Crash Lab 0.8 — Staged crumple structure and sedan refinement

Play: https://dumb-tony.github.io/TOTALLED/

- Front and rear now each contain an additional physical cross section, splitting the old long end cells into two independently deformable stages. Outer sections yield first; inner rails and the transition into the cabin are stronger. Side/floor collision faces and attached detail follow these added nodes.
- Twelve added nodes redistribute existing mass: the sedan remains 1272 kg. Short-beam yield capacity is normalized by length so subdivision does not introduce idle damage. The stronger passenger cage, permanent plastic deformation and broken mounts remain the source of truth.
- Longer cabin, more believable front/rear glass slopes, shorter deck proportions, rounded body corners and a narrower physical wheel track that tucks the tires into the fenders.
- Finer edge wear replaces large paint blotches. Original sky/yard reflections, pressed bonnet lines, cowl vents, recessed lamps, resting wipers, a readable plate and a spare wheel in the boot add detail. Existing engine bay and interior remain deformable visuals.
- V enables structural debug; Tab cycles through stress, plastic deformation, broken connections and the new crumple-zone view. Orange is the outer section, yellow the inner section, blue the transition and green the cabin. Gray marks panels/attachments.

The new stages deform through physics; they are not scripted wreck states. Calibration remains a prototype, not measured automotive material data. Reflections approximate the yard rather than rendering it live. Cosmetic corners, trim, engine, spare wheel and cabin do not have separate physical collision bodies. Full self-collision and detailed pneumatic tire behavior remain unfinished.
