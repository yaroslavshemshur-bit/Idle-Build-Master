# Engineering baseline and chat handoff

**Status:** initial technical baseline, 2026-09-28. This describes the repository as observed; proposed structure is not yet implemented.

## Where to work

- Git root: this repository (`Idle-Build-Master/`). The parent directory is not a Git repository. Prefer this Git root as the project/work directory for new Codex chats; its tracked `AGENTS.md` then applies directly. The local parent workspace also has a short `AGENTS.md` pointer.
- Unity project: `Unity Project [Idle build master]/IdleRPGBuildMaster/`.
- Editor version pinned by `ProjectSettings/ProjectVersion.txt`: **6000.6.0f1**. Open this exact project folder in Unity Hub.
- Current implementation: a Unity 2D template with `SampleScene` and its welcome asset. Gameplay systems, gameplay tests and assembly definitions for game code have not been added yet.
- Commit Unity source in `Assets/` with every matching `.meta`, `Packages/manifest.json`, `Packages/packages-lock.json` and `ProjectSettings/`. Generated caches and IDE files are excluded by the root `.gitignore`.

## Sources and boundaries

- `Docs/README.md` maps the design and records its decision policy. Game design and content live under `Docs/GameDesign/`, `Docs/Content/` and `Docs/Economy/`.
- `Docs/Technical/00_Architecture_Requirements.md` is the authoritative technical requirement set. Refer to its section numbers when making architecture decisions. It states capabilities, not a prescribed class diagram or package layout.
- Technical specification v0.2 is in `03_System_Architecture.md`, `04_Simulation_Contracts.md`, `05_Persistence_and_Services.md` and `06_Implementation_and_Verification.md` in this directory. These define the implementation baseline; they are not implemented code. `02_Architecture_Decisions.md` records owner constraints and unresolved design dependencies. Large numbers are required immediately; active combat preserves mathematical results; saved fights may rebase to new balance; offline efficiency is analytical and its formula belongs to game design; all current computation is client-side. Follow the staged implementation plan; do not redesign gameplay or PvP to fill missing feature rules.
- Technical tasks implement agreed rules. If a rule is missing or contradictory, record the assumption or question explicitly; do not promote it to a design decision in code.
- If the task lacks a material outcome, behavior, scope or acceptance condition, ask one focused clarification before broad document searches or coding. If design rules are needed but the prompt gives neither behavior nor a source, ask which rule to implement. Do not use a repository-wide read to guess unstated requirements.
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

## T01 foundation implementation — 2026-09-28

- Scope: `Assets/Project/Scripts/Domain`, `Scripts/Application`, and `Tests/EditMode` in the Unity project. The template scene and gameplay design were not changed.
- Assemblies: `IBM.Domain` and `IBM.Application` have no Unity engine references; Application depends on Domain. The Edit Mode test assembly depends on both.
- Stable references: ordinal `ContentId`, typed `DefinitionId<TKind>` for mechanical definitions, and a durable counter-based `InstanceIdSequence` for runtime instances. No display names or Unity GUIDs are save keys.
- Numeric format v1: `GameNumber` stores a signed `BigInteger` coefficient rounded to 34 decimal digits (ties to even) and a checked `long` exponent. Zero is `(0,0)`; invariant coefficient/exponent strings are the intended DTO representation. Arithmetic does not convert the full value to `double`. Input is range checked at bounded conversion and simulation time boundaries.
- RNG v1: PCG32 with explicit state and increment; seed derivation v1 uses FNV-1a 64 over little-endian root-seed bytes followed by the UTF-8 stream ID. Streams are created separately by their future owning systems. Version stamps keep save schema, simulation rules, numeric, RNG and content revisions distinct.
- Time: signed microsecond `SimTime`, nonnegative `SimDuration`, midpoint-away-from-zero duration conversion and retained fractional microseconds for a real-time driver. Scheduling and gameplay policies belong to T04.
- Verification: standalone .NET 9 compilation of all new Domain/Application C# files passed; a temporary smoke harness passed PCG32 reference outputs, large-number boundary checks, time checks and 160,801 small-integer arithmetic pairs. `git diff --check` passed. Unity CLI's installation registry did not contain the Editor, but the owner supplied its actual path: `W:/UNity Editor/6000.6.0f1/Editor/Unity.exe`. A first sandboxed batch run failed while starting the licensing client (WMI access denied and IPC conflict). Repeating `unity --non-interactive test <project> --mode EditMode --editor-path <actual Unity.exe> --output <results.xml> --timeout 600` outside the sandbox passed all 8 Edit Mode tests. The report was moved to the system temp directory to keep the Unity project clean.
- Next increment: T02 content catalog and validation. T03–T06 must still be implemented before the first Location is playable. Design dependencies D01–D14 in `02_Architecture_Decisions.md` remain unresolved where they affect production rules.

## Tuning authoring foundation — 2026-09-28

- `BalanceTuningAsset` and `InterfaceTuningAsset` are editable SO profiles under `Assets/Project/Content/Authoring/Tuning/`. They are intentionally empty: no unapproved gameplay coefficients or UI measurements have been invented. The editor menu `Idle Build Master/Tuning/Create Missing Profiles` recreates missing profiles without replacing existing authored data.
- A balance profile compiles stable keyed, typed values into an engine-free immutable `BalanceCatalog`, with duplicate, type, probability, duration and numeric validation. The canonical mechanical SHA-256 hash is independent of authoring order. Future content SOs must place their own balance quantities in authored fields and join the same catalog/validation pipeline; mechanics must request required keys, not carry production fallback literals.
- An interface profile stores keyed measurements with units and colors, rejects invalid/missing values, and stays outside the mechanical hash. UI implementation must reference this profile for tunable layout, animation and display measurements.
- Unity Editor is installed at `W:/UNity Editor/6000.6.0f1/Editor/Unity.exe` even though `unity editors list` is empty. For batch checks, pass it with `--editor-path` and run the editor outside the sandbox: the sandboxed attempt caused a licensing-client WMI/IPC failure. The normal client had a valid entitlement; the outside-sandbox Unity Edit Mode run passed 8/8 foundation tests. No license reset or reinstall was needed.
- This is the tuning authoring layer of T02, not the full content compiler: effect schemas, typed registries, source-path diagnostics, graph validation and content-wide manifests remain later T02 work. No active gameplay or UI consumer exists yet.
- Final verification: Unity 6000.6.0f1 `unity test` with the explicit editor path passed 12/12 Edit Mode tests after the tuning assets and hash test were added; `git diff --check` found no whitespace errors. The SO profiles and matching `.meta` files are present. Changes are uncommitted on `main` (base `352f6e1`).

## T02–T08 implementation status — 2026-09-28

- T02 foundation: `GameContentAsset` compiles typed locations, stages, encounters, enemies, effects, Powers, items and loot into a validated immutable `ContentCatalog` with a deterministic mechanical hash. Balance and interface profiles are ScriptableObjects; approved combat and offline coefficients are authored in `BalanceTuning.asset`. The production `GameContent.asset` is intentionally empty pending the gameplay decisions in `02_Architecture_Decisions.md`.
- T03 foundation: `GameSession` owns account, run and combat state, applies revisioned commands via a draft transaction, and exposes a read-only projection. Its in-memory operation deduplication is bounded; durable command-idempotency still needs implementation.
- T04 foundation: a deterministic event scheduler and bounded combat reaction queue preserve ordering across yields; approved baseline combat formulas use balance keys. The full stat graph, actions, effects, downed lifecycle and Boss behavior are not implemented.
- T05 foundation: full currently modeled state is serialized to a checksummed JSON generation, with three-generation file recovery and safe-point capture. Migration, current-balance rebase and continuation-equivalence fixtures remain outstanding.
- T06: no production Location 1 scenarios, rewards or gear rules have been activated. D01, D02, D04, D05 and D07–D13 affect their results; questions are consolidated in `02_Architecture_Decisions.md` for game design.
- T07: Unity presentation and interactive game bootstrap are still outstanding. The current catalog has no playable stage.
- T08 foundation: the separate analytical offline formula, residual accounting result and eligible completed-normal-Stage target selector are implemented. Reward application, durable idempotency, device lifecycle and profiling remain outstanding.
- Verification: Unity 6000.6.0f1 Edit Mode tests passed 19/19 with the explicit editor path and outside-sandbox license access. This verifies the foundation and synthetic fixtures, not the outstanding production features above. The working tree contains uncommitted T02–T08 foundation changes and the open-decision updates.

## PvE decision implementation continuation — 2026-09-28

- Starting point: `cb6c012` with a clean working tree. `02_Architecture_Decisions.md` now resolves D01–D14 for current PvE; earlier pending-decision statements above describe the previous state, not the current blocker list.
- T04: `CombatStatResolver` resolves approved baseline dependencies, flats, conversions from fully resolved source stats, then target multipliers. It rejects cycles and applies Attack Speed multipliers to rating before the logarithmic conversion. `CombatMath` now exposes percent-preserving MaxHP rebasing, separate `ResolvedDamage` / `ActualHpLost`, fatal-prevention replacement within the damage transaction, and authored downed regeneration multiplier.
- T02/T06: locations may author normalized progress units and ordered milestone definitions in `GameContentAsset`. `LocationProgression` uses exact rational comparisons, so Stage Compression can cross multiple milestones in ascending order without shifting their positions. Unlock-before-choice ordering and first-run-only choices are represented. Production first-location data and reward execution remain to be integrated.
- T03/T05: recent command operation IDs are now copied with session state and serialized in snapshots; reloading a save preserves the bounded deduplication window. Older compatible snapshots without this additive field load with an empty history.
- Tuning: `combat.death_regen.multiplier` is authored in `BalanceTuning.asset` and recreated by the editor profile command when missing.
- Verification: Unity 6000.6.0f1 Edit Mode passed 20/20 tests with the explicit editor path outside the sandbox. `git diff --check` found no whitespace errors. Changes remain uncommitted. The next concrete step is wiring encounter completion and rewards to the authored progression milestones, then producing the first Location content and a playable Unity bootstrap.
- Newly identified D15 in `02_Architecture_Decisions.md` needs the compressed reward-slot mapping. Milestone crossing and uncompressed/final-slot rewards can be implemented without it; compressed EXP/loot selection should wait for the game-design answer.
- T04 follow-up: `BasicAttackResolver` now performs the ordinary hit roll, consumes a damage roll only on hit, applies Block, and returns the full HP/damage transaction. It accepts an injected RNG and keeps target selection, Crit, outgoing modifiers and triggered effects outside this baseline primitive. This is not yet an encounter driver: scheduled attacks, continuous regeneration, downed clock pauses and reward transitions must still be wired into the session.
- T07/T05 follow-up: `GameRoot` compiles the content, balance and interface SO profiles, restores a save (or creates a seeded local session), routes commands to the single writer, and saves on pause/quit. It requires an authored starting Stage and is not attached to the template scene while the production catalog is empty. The catalog's mechanical hash now includes the compiled balance hash and starting Stage; a save from different coefficients requires an explicit compatibility handler rather than silently restoring under new rules.
- Save schema and content-authoring schema use separate version numbers in the bootstrap. `GameSnapshotDto.CurrentSchemaVersion` identifies the local save format; `GameContentAsset.schemaVersion` validates authoring data only.
- Verification after this follow-up: Unity 6000.6.0f1 Edit Mode passed 21/21 tests, including RNG consumption on hit/miss and save incompatibility after a balance edit. `git diff --check` found no whitespace errors. Working tree remains uncommitted from `cb6c012`.

## Scheduled ordinary-attack slice — 2026-09-28

- `CombatTimeline` starts attack clocks in stable actor-ID order and advances ordinary attacks through the durable `(due, priority, sequence)` scheduler. Target selection and stat lookup are injected policies; no undocumented production targeting rule is hard-coded. Each attack uses `BasicAttackResolver`, updates HP and the dedicated combat RNG, and schedules the next attack only while the actor remains active.
- Regeneration is evaluated from a saved segment anchor (`RegenAnchorAt`, `RegenAnchorHp`) rather than incrementally per frame. These anchors are part of the combat save DTO. An older compatible save without anchor fields initializes them from its saved HP and combat time.
- `GameSession.AdvanceCombat` commits a complete copied combat step; a failed step leaves the session state unchanged. The current ordinary-attack slice explicitly rejects a lethal hero hit until the D03/D06 downed/revival policy is wired. Enemy deaths stop attacks, but reward and encounter-transition execution is still outstanding.
- Edit Mode fixture covers one large advance, three smaller advances, a yielded/resumed work budget, save/load continuation and the single-writer session path. Unity 6000.6.0f1 passed 22/22 tests after one compile repair. Changes are uncommitted from `cb6c012`.

## Downed recovery, encounter clear and Power-choice continuation — 2026-09-28

- A lethal ordinary hit now leaves the hero downed at zero HP. Enemy attacks, enemy-owned timed events and Boss transition events move to a durable paused scheduler list. Enemy regeneration anchors stop advancing. Hero Death Regeneration continues and a scheduled revive restores full HP; paused clocks shift by the exact downtime. The hero's own attack-clock rule is injected as `HeroDownedAttackClockPolicy` pending D16, and multi-enemy target selection remains injected pending D17.
- `GameSession.ResolveEncounterClear` applies an encounter's base EXP once, records required normal-Stage clears, crosses authored Location milestones in order, unlocks Powers and queues Power-choice credits, then AutoPushes after Stage completion. Completed normal Stages use the final encounter profile for repeat rewards; a required Boss pays once per run. Starting a fresh encounter validates its Stage profile and enemy identities. An active encounter is abandoned when a deliberate Stage navigation changes the selected Stage.
- `ProgressStageCount` on authored Locations keeps the Boss Stage outside the normalized 100-point progress denominator. Fixed Power options can be authored on a choice milestone. `GenerateNextPowerOffer` materializes only the first pending credit, stores its options and source in run/save state, and consumes the Offers RNG only for an injected normal-offer policy. Reopening a generated offer cannot reroll it. `ChoosePowerCommand` takes one offered Power and consumes only that credit. An empty eligible pool leaves its credit pending without blocking combat.
- Save DTOs now include required clear counts, completed order, triggered milestones, pending credits, offer source/options, downed start/policy, paused events and account completed-run count. The current schema accepts older additive fields with defaults; a content or balance hash mismatch still requires explicit compatibility handling.
- This is still a partial vertical slice. Loot tables, Power reward modifiers, effect/Boss handlers, normal offer weighting/size, compressed encounter-slot rewards (D15), production Location content and scene wiring remain outstanding. The ordinary-attack timeline rejects encounters requiring missing handlers. The two injected policies await game-design resolution in D16–D17.
- Verification for this batch: one final Unity 6000.6.0f1 Edit Mode run with the explicit Editor path passed 25/25 tests; `git diff --check` found no whitespace errors. Working tree is uncommitted from `cb6c012`.

## Reward transaction seam — 2026-09-28

- The previous foundation was committed as `77b4b0d` (`Build deterministic combat and progression foundation`).
- `IEncounterRewardPolicy` now receives a copied state snapshot and the dedicated Loot RNG. It returns final EXP and item drafts for encounters with a loot table or owned Powers. `GameSession` validates the plan, assigns durable item instance IDs, grants EXP and inventory items, advances the Loot RNG and marks the encounter claimed in one draft commit. A rejected plan changes none of these values. Empty-content encounters still use the authored base EXP without a policy.
- Run/save state includes `NextItemSequence`, with compatibility initialization from existing inventory IDs for older saves. Save validation checks item identities, definitions, level, equipment references and sequence monotonicity. The reward policy must supply approved loot rolls, item levels and Power reward modifiers; the current content schema does not specify roll count or drop probability, so no production lottery algorithm is activated here. Procedural affixes remain unsupported in reward plans until their definitions and rules exist.
- Verification for this follow-up: one final Unity 6000.6.0f1 Edit Mode run passed 26/26 tests; `git diff --check` found no whitespace errors. New work after `77b4b0d` is ready to commit.

## Inventory commands — 2026-09-28

- Starting point: clean `main` at `6e625c8`. `SetItemLockCommand`, `EquipItemCommand` and `UnequipItemCommand` now validate ownership and run through the session's revisioned, deduplicated command transaction. Manual equip uses the authored item slot; lock remains independent from equipment and reset retention.
- A live equipment change requires an injected `IEquipmentTransitionPolicy` to update combat stats, HP and clocks under approved rules. Without it the command is rejected without changing state. Save validation checks equipped item identity and slot compatibility.
- The owner approved `n²` for the Nth STR/VIT/AGI/DEX purchase and allows purchases during active combat. `PrimaryStatProgression` reads per-stat starting values and cost scales plus the curve power from Balance SO; `BuyStatUpgradeCommand` spends EXP and increments the purchased count atomically. Live purchases require an injected `IPrimaryStatTransitionPolicy` so combat HP and clocks cannot remain stale. D18 records the unresolved attack-clock rule for mid-combat Attack Speed changes.
- Verification: Unity 6000.6.0f1 Edit Mode passed 28/28 tests after one compile repair in the inventory fixture. This increment follows `6e625c8` and is ready to commit.
