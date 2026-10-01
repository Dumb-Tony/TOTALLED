# TOTALLED — Motor Works

[Play Level 01](https://dumb-tony.github.io/TOTALLED/) · [Windows download](https://github.com/Dumb-Tony/TOTALLED/releases/latest) · [Report a bug](https://github.com/Dumb-Tony/TOTALLED/issues/new)

A three-lap banger race with three AI rivals and persistent node-and-beam vehicle damage. First across the line wins. Ramming is allowed; wrecks stay on the circuit.

## Controls

Enter starts or races again. WASD / arrows drive; Space applies the handbrake; Shift / Ctrl brakes. Escape pauses. R restarts the whole event. F returns your car to the track without repairing it. M mutes audio.

See [Playtest 0.9](Docs/Playtest-0.9.md) for scope and limitations.

## Unity development

Unity **6000.6.0f1**. Local project: C:\Dev\Unity\TOTALLED.

Open Assets/TOTALLED/Scenes/MotorWorks.unity for the race. Assets/TOTALLED/Scenes/CrashLab.unity retains the technical test yard and its debug tools. Cars, track geometry and textures are original procedural prototype assets.

The runtime separates the structural solver, shared vehicle contacts, sedan definition, rendering, race rules and AI controls. Damage comes from structural deformation and broken connections, not a global vehicle health bar.

## Validation and publishing

- CrashLabBuild.Verify: crash, steering, braking, elastic/plastic deformation, two-car collisions and repeated-impact regressions.
- DerbyRaceBuild.Verify: full autonomous four-car event, directional lap gates, finish rules and damage-preserving recovery.
- DestructionBenchmark.Run: matched-speed first and repeated wall impacts.
- CrashLabBuild.BuildWindows: packaged Motor Works event.
- CrashLabPublish.BuildWeb: browser build in Builds/WebGL.
- Launch the Windows build with -race-smoke for startup/countdown/driving/restart checks.

Publish browser output on gh-pages and Windows ZIPs under GitHub Releases. Numerical checks do not certify handling quality, BeamNG fidelity or browser frame rate. Results and limitations are in Docs/Browser-Playtest.md.
