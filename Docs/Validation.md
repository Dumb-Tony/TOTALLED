# Crash Lab validation — 24 September 2026

Unity 6000.6.0f1. Tested in the real Unity physics environment, using procedural static walls, barriers and ground. Source is committed locally; no remote repository or publishing is configured.

## Numerical regression

**21 checks passed.** The sequence includes a five-second settle, powered driving, repeated front/rear/side collisions, ten-hit endurance, six further high-speed collisions including the offset pole, and recovery with detached debris.

- No yielding or broken connections from resting under gravity.
- Sub-yield elastic strain recovers without permanent deformation.
- Pristine ground-contact drive forces move the car.
- First real wall collision permanently changes beam rest lengths.
- Recovery preserves every rest length and broken flag exactly.
- After the first collision, the damaged car travels approximately **7.48 metres under power in two seconds** after a settling interval.
- Subsequent collisions increase accumulated plastic deformation without resetting prior damage.
- The sixteen-hit sequence produces **11 broken connections**, at least one fully detached panel, and approximately **49.7 degrees maximum wheel alignment deviation** relative to the deformed chassis forward vector.
- Position values remain finite across the impact sequence. Detached panel nodes remain where they were when the connected car is recovered.

The detailed machine-readable report and nine rendered checkpoints are in `Artifacts/`. Captures include pristine, first impact, repeated impact, multi-direction damage, plasticity, broken beams, ten hits, severe damage, and the recovered severe wreck. Plastic travel in the JSON is a sum over beam rest-length changes, not the distance the whole car shortened.

## What these checks do not establish

The experiment is not yet a convincing BeamNG replacement or a finished game. There has not been a human 20-minute handling/fun acceptance session. The body is deliberately low-resolution placeholder geometry. Numerical checks do not validate material realism, detailed tire/suspension dynamics, car-to-car collision, debris-to-car collision, self-collision, or multi-car performance.

The rendered Windows smoke test **passed with no runtime errors**. It uses a hidden player with two explicit offscreen camera captures and verifies permanent damage after two impacts. It does not measure an interactive frame rate or capture the screen-space HUD. See `Artifacts/runtime-smoke.json` for that result.

## Engineering findings fixed during this iteration

1. Swept ground contacts originally discarded remaining tangential motion. Keeping that motion fixed artificial braking and allowed actual driving and wall impacts.
2. Shared vertices on forward/reverse body triangles cancelled normals. Separate reverse-side vertices fixed the body shading.
3. An aggressive force-only structural fracture setting destroyed the chassis on the first hit. Structural beams now fracture from excessive geometric strain or accumulated plastic travel; component mounts retain force-sensitive failure. This preserves yielding before disintegration.
4. Runtime-only `Shader.Find` references were stripped from the player. Explicit material assets under Resources now preserve the required surface/debug shaders.
5. The initial follow camera could pass through the impact wall. A swept camera obstruction check and rear-quarter starting view keep the car visible; the lab labels were also resized after inspecting the player capture.

Next work should focus on deformable surface/self/body/debris collision, collision energy and material calibration, richer suspension/tire behavior, and an interactive driving evaluation. Keep gameplay expansion behind the acceptance gate in Design.md.
