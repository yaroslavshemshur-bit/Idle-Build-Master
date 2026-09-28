# Engineering instructions

- Work on technical implementation in the Unity project at `Unity Project [Idle build master]/IdleRPGBuildMaster/`. Treat `Docs/` as the source of truth for existing game design. Do not invent or silently change gameplay rules.
- Read only the documents relevant to the current task. `Docs/README.md` is the design map; `Docs/Technical/00_Architecture_Requirements.md` defines architecture constraints; `Docs/Technical/01_Engineering_Handoff.md` is the short engineering baseline.
- Keep simulation rules independent of scenes, GameObjects, animation timing and UI. Give account, run and encounter state explicit owners. Use stable IDs for saved content and controllable RNG/time where relevant.
- Edit Unity source under `Assets/`, `Packages/` and `ProjectSettings/`; retain matching `.meta` files. Do not edit or inspect generated `Library/`, `Temp/`, `Obj/` or IDE project files unless a specific diagnostic requires it.
- For each implementation task, make the smallest complete change, verify the affected behavior when the required tools are available, and report exact verification and any remaining blocker. Add durable technical decisions to the engineering handoff only when they are established.
