# TOTALLED — Crash Lab

Work only on making repeated impacts physically convincing until the acceptance gate in Docs/Design.md passes. No derby AI, menus, progression or vehicle catalog yet.

Unity 6000.6.0f1, built-in renderer, C#. Open Assets/TOTALLED/Scenes/CrashLab.unity. All runtime geometry is original procedural placeholder art.

The node graph is the source of truth. Never heal node positions or beam rest lengths during recovery. Never substitute global vehicle HP or canned wreck stages. Wheel traction must follow the current attachment geometry. Debris persists until an explicit new experiment.

Keep solver, sedan definition, rendering, lab UI and editor verification separate. Document approximations honestly. Run CrashLabBuild.Verify in batch mode after physics changes, inspect Artifacts/verification.json, and visually inspect the capture set when rendering changes. Passing numerical checks does not prove the handling feels good.

