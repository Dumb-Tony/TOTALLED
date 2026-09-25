# TOTALLED — Crash Lab

Work only on making repeated impacts physically convincing until the acceptance gate in Docs/Design.md passes. No derby AI, menus, progression or vehicle catalog yet.

Unity 6000.6.0f1, built-in renderer, C#. Open Assets/TOTALLED/Scenes/CrashLab.unity. All runtime geometry is original procedural placeholder art.

The node graph is the source of truth. Never heal node positions or beam rest lengths during recovery. Never substitute global vehicle HP or canned wreck stages. Wheel traction must follow the current attachment geometry. Debris persists until an explicit new experiment.

Keep solver, sedan definition, rendering, lab UI and editor verification separate. Document approximations honestly. Run CrashLabBuild.Verify in batch mode after physics changes, inspect Artifacts/verification.json, and visually inspect the capture set when rendering changes. Passing numerical checks does not prove the handling feels good.

User delivery preference: publish playtests on GitHub and give the shareable playable GitHub Pages URL as the primary link, not local launchers. Source: https://github.com/Dumb-Tony/TOTALLED . Play: https://dumb-tony.github.io/TOTALLED/ . Build browser output with CrashLabPublish.BuildWeb, publish it on gh-pages, verify the live page, and keep Windows downloads under GitHub Releases. The user's publication request authorizes this workflow.
