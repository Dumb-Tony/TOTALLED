# Crash Lab 0.2 — stable panels, progressive crumpling, rear handbrake

The starter sedan keeps its existing engine force. Intact panels now have a braced surface and triangulated mount groups, so suspension motion does not make undamaged doors and lids flap. Impacts can crease the subdivided surfaces, fail the latch, and then break the surviving hinges. The front and rear crumple zones yield more readily; bounded plastic flow avoids adding energy by moving rest lengths beyond actual deformation. Darker rough surfaces identify regions that have permanently yielded.

Space progressively locks the rear tires, cutting rear cornering grip while leaving the front tires available for steering. Left Shift applies the four-wheel service brake; left Ctrl remains an alias. Releasing Space restores rear grip gradually. There is no artificial yaw kick and no power increase. A long handbrake hold can spin the car; use a brief pull while turning, then release and countersteer.

Recovery and the camera now reference the cabin rather than detached end nodes. Overstretched skin patches tear permanently instead of drawing long ribbons between separated nodes. This is a rendering approximation: those individual torn patches do not become new physical sheet-metal fragments. Whole detached panels and wheel nodes still persist.

## Verification

Unity 6000.6.0f1: all 31 regression checks pass. See `Verification-Summary.json` and `Revision-Checks.json` for the recorded values. The checks cover settled panels, undamaged driving/braking, matched brake maneuvers, normal throttle-driven wall impact, repeated directional impacts, damaged propulsion, irreversible recovery, wheel misalignment, and partial-to-full panel detachment.

- Settled panel motion stays below 0.1 mm in an eight-second idle experiment.
- Normal throttle-driven wall impact shortens the nose by about 8.5 cm without using laboratory launch commands.
- In a matched 16 m/s, 1.2-second straight-line test, the foot brake reduces speed to about 0.9 m/s; the handbrake leaves about 7.9 m/s.
- In the matched steering test, a continuously held handbrake produces about 65 degrees of lateral slip, compared with less than 1 degree on the foot brake. This establishes a clear difference, not a claim of realistic tire calibration or an ideal drift angle.
- The damaged sedan travels over 7 m under its own power after the first laboratory impact. Subsequent impacts accumulate deformation and broken connections.

Rendered captures were inspected for intact panel shape, first-impact deformation, and severe separation. Numerical checks do not replace player feedback on handling. Static-world collision only, no self/car/debris interaction, and the remaining limitations in `Design.md` still apply.

Future vehicle variety should include progressively more capable vehicles. The current slow sedan remains the baseline; no progression system or vehicle catalog was added in this revision.
