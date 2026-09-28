# Engineering baseline and chat handoff

**Status:** initial technical baseline, 2026-09-28. This describes the repository as observed; proposed structure is not yet implemented.

## Where to work

- Git root: this repository (`Idle-Build-Master/`). The parent directory is not a Git repository. Start new Codex chats with this Git root as the project/work directory so `AGENTS.md` applies.
- Unity project: `Unity Project [Idle build master]/IdleRPGBuildMaster/`.
- Editor version pinned by `ProjectSettings/ProjectVersion.txt`: **6000.6.0f1**. Open this exact project folder in Unity Hub.
- Current implementation: a Unity 2D template with `SampleScene` and its welcome asset. Gameplay systems, gameplay tests and assembly definitions for game code have not been added yet.
- Commit Unity source in `Assets/` with every matching `.meta`, `Packages/manifest.json`, `Packages/packages-lock.json` and `ProjectSettings/`. Generated caches and IDE files are excluded by the root `.gitignore`.

## Sources and boundaries

- `Docs/README.md` maps the design and records its decision policy. Game design and content live under `Docs/GameDesign/`, `Docs/Content/` and `Docs/Economy/`.
- `Docs/Technical/00_Architecture_Requirements.md` is the authoritative technical requirement set. Refer to its section numbers when making architecture decisions. It states capabilities, not a prescribed class diagram or package layout.
- Technical tasks implement agreed rules. If a rule is missing or contradictory, record the assumption or question explicitly; do not promote it to a design decision in code.
- The first complete integration target is the Location 1 loop in architecture requirement **§47**. Future directions in **§48** are compatibility constraints, not current feature requests; **§49** lists systems not to build yet.

## Implementation constraints to carry into each relevant task

1. Simulation and presentation are separate (§1, §34). Combat outcomes cannot depend on frame rate, animation, a scene or a GameObject.
2. Account, run and encounter state have distinct lifetimes and a single authority (§2). UI requests actions but does not own game state (§46).
3. Ordinary content is authored as data; persistent references use stable IDs (§3–4). The first Location must use reusable rules (§47).
4. Stat calculation order, conversions and modifiers follow §5. Combat supports several independent enemies, event ordering, seeded RNG and abstract time (§7–14).
5. Saves need explicit versions and safe handling of unknown content (§31–32). Offline and accelerated progress must remain possible (§33–36).
6. Do not add speculative frameworks. Build only the capabilities needed by the current vertical slice while respecting §48–49.

## Unity and verification workflow

- Keep **Visible Meta Files** and **Force Text** serialization enabled; both were already set when this baseline was written. Commit each asset and its `.meta` together.
- Keep normal domain and scene reload on entering Play Mode until gameplay code explicitly resets static state. This makes repeated editor runs comparable.
- Search by path and topic (from the Unity project folder: `rg --files Assets`, then scoped `rg`) and read the relevant design section. Avoid dumping all design documents or generated Unity directories into a chat.
- Verify C# compilation in Unity after code changes. Use focused Edit Mode tests for simulation rules and Play Mode tests for Unity integration when those tests exist. Record the Unity version, command or editor action, and result in the task handoff. Do not claim an editor check when Unity could not run.
- Before committing, inspect `git status --short --untracked-files=all` and confirm that Unity caches are absent while new `Assets/*.meta` and project configuration are included.

## Copyable brief for a new chat

```text
Task: <one concrete technical outcome>
Starting point: <branch/commit and any existing uncommitted work>
Scope: <systems/files allowed to change>
Behavior and acceptance: <observable result, edge cases, tests>
Design sources: <specific Docs paths/sections only>
Constraints: follow AGENTS.md and Docs/Technical/00_Architecture_Requirements.md where relevant; do not change game design.
Verification: <Unity compile / named tests / manual steps expected>
```

At the end of each task, hand over: changed files, implementation decisions, verification actually run, remaining issues, and the next concrete step. Put lasting decisions in this file or a focused technical document; leave transient progress in the chat/PR summary.
