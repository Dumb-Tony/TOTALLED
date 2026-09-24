# TOTALLED — Crash Lab

Local Unity 6.6 technical prototype for persistent, deformable demolition-derby cars. Project: `C:\Dev\Unity\TOTALLED`. Unity version: **6000.6.0f1**. No purchased assets or external code dependencies.

## Play

Open the project in Unity, open `Assets/TOTALLED/Scenes/CrashLab.unity`, and press Play. If the scene is absent, use **TOTALLED → Create Crash Lab scene**. A standalone Windows development build can be generated with **TOTALLED → Build Windows prototype** and is placed in `Builds/Windows`.

| Control | Action |
|---|---|
| WASD / arrow keys | Throttle, reverse, steer |
| Space / left Ctrl | Rear handbrake / all-wheel brake |
| 1 / 2 / 3 / 4 | Launch same damaged specimen into front / rear / side wall / offset pole |
| [ / ] | Decrease / increase laboratory launch speed |
| R | Upright and relocate connected structure; preserve all damage |
| N | Spawn a new specimen; keep old wreck and debris |
| P / period / T | Pause / single step while paused / slow motion |
| B / V / C | Toggle body / node-beam overlay / component markers |
| Tab | Stress → cumulative plastic travel → broken-beam emphasis |
| Right mouse drag / wheel | Orbit / zoom |

Impact shortcuts are explicit laboratory launches, not normal vehicle driving. Debris that has detached is left where it fell when recovering a specimen. There is no global vehicle health or automatic wreck deletion.

## Verify

From PowerShell (close this project's editor first):

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Dev\Unity\TOTALLED' -executeMethod CrashLabBuild.Verify -logFile 'C:\Dev\Unity\TOTALLED\verification.log'
```

The verifier exits Unity when finished. Inspect `Artifacts/verification.json` and the nine PNG captures. `-nographics` allows numerical checks but intentionally skips captures. Use the **TOTALLED** editor menu for an interactive run; it replaces the open scene, so save your edits first.

Launch the Windows player with `-crashlab-smoke` for a ten-second unattended rendered check. It writes `Artifacts/runtime-smoke.json` and two screenshots, then exits. This option is only for validation; ordinary launches are fully interactive.

## Structure

- `Assets/TOTALLED/Runtime/SoftStructure.cs`: nodes, beams, plasticity, fracture, world collision.
- `SacrificialSedan.cs`: structural graph, suspension/contact forces, localized mechanics.
- `SedanView.cs`: node-driven mesh, wheels, structural debug overlays.
- `CrashLab.cs`: lab geometry, controls, time, repeat impacts and telemetry.
- `Assets/TOTALLED/Editor/CrashLabBuild.cs`: scene generation, regression captures and Windows build.
- `Docs/Design.md`: design intent, acceptance gate and precise limitations.

**Current collision limit:** cars and their detached parts collide with the static lab, but not with each other. The full derby and physically interacting wreck pile are not implemented. See the design contract before interpreting this as a finished BeamNG-style system.
