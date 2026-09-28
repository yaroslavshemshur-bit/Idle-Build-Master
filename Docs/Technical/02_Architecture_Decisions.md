# Architecture scope and open decisions

**Status:** product constraints and unresolved design dependencies; technical specification starts in `03_System_Architecture.md`.  
**Date:** 2026-09-28

## Agreed documentation scope

The technical specification covers the whole game and the expansion directions already described in the design documents. Implementation will proceed in separately verifiable increments.

Document the foundation and extension contracts together. This does not authorize implementing every future feature immediately or inventing missing gameplay rules. Architecture Requirements §48 describes expansion directions; §49 still excludes premature speculative frameworks.

Owner decisions below supplement the existing requirements, including mobile portrait presentation and separation of simulation from Unity presentation.

## Reviewed sources

- `Docs/README.md`.
- All six existing `Docs/GameDesign/00`–`05` documents.
- `Docs/Content/Powers.md`, `Items.md`, `Location_01_Mechanical_Baseline.md`.
- `Docs/Economy/00_Reference_Economy_Baseline.md`.
- `Docs/Technical/00_Architecture_Requirements.md` and `01_Engineering_Handoff.md`.

This review uses the repository's recorded design and reference snapshots. It does not independently verify the external reference game's values.

## Approved owner decisions — 2026-09-28

These product decisions are approved. Their implementation details remain to be specified.

| ID | Approved decision | Architectural consequence |
|---|---|---|
| A01 | Android and iOS with reasonable optimization. Web is desirable in the distant future | Mobile is the current target; isolate platform dependencies. Device tier and performance budgets are still open; no current Web delivery commitment |
| A02 | Current game works without a connection. Once the planned server is introduced, connection is required and the server validates progression for cheating | Specify distinct local and connected operating modes. Exact server authority, validation protocol and behavior during transient disconnects remain to be designed |
| A03 | Recovery after reinstall and cross-device progress arrive with accounts. Before accounts, save import is available only in development builds | No interim public save transfer feature. Separate local identity, persistence and future account linking/sync. Development imports must not silently become trusted competitive progress |
| A04 | Approximate offline farming is sufficient | Exact attack-by-attack offline replay is not required. Offline clear rate is now design-defined independently of combat composition or DPS. |
| A05 | Save the complete combat state | Specify restoration of HP, effects, timers, boss state, RNG and other authoritative combat state. A08 defines the frozen fight's relationship to offline farming |
| A06 | Initially ship content in application updates. Desired future is server-delivered content managed through an administration interface | Separate content loading from gameplay; plan versioned validated content delivery. No server or administration UI implementation at this stage |
| A07 | Future PvP is asynchronous against saved player builds. The final build of a run is saved for the arena at reset | Capture the build before reset clears run state. Arena matches must not mutate the saved build or the live run. This updates the former speculative status of PvP in Architecture Requirements §49; match rules and validation still need specification |
| A08 | Owner accepted the offline contract, including EXP | Freeze the active fight; calculate separate farming rewards for an eligible completed normal Stage; resume the saved fight on return. Do not advance the frozen fight's timers during absence. Baseline cap is 6h, efficiency 50%, minimum offline encounter time 60s. |
| A09 | Large numbers are required immediately | Implement a large-number representation in T01; do not start with a finite-double-only wrapper |
| A10 | Mathematical combat results take priority; presentation may omit visual events | Never omit attacks, proc rolls or reactions to meet a frame budget. Visual coalescing and mathematically equivalent optimizations are allowed |
| A11 | Saved fights may be recalculated using updated balance | Rebase the saved state onto current supported content/rules in one validated load transaction; do not require finishing the fight under its old balance |
| A12 | Offline farming must use a mathematical non-combat formula | Use the game-design formula from `Docs/Economy/01_Offline_Farming.md`. Do not estimate offline rate from combat DPS, enemy count, encounter composition, sample fights, Monte Carlo or battle replay. |
| A13 | For now, all computation runs on the client | Implement local adapters only. Future server validation remains a planned boundary, but hosting, authority split and backend cost decisions are deferred |

Do not ask the owner to choose class names, dependency-injection libraries or serialization packages before these product decisions are settled. Technical proposals should follow the required behavior.

## Later design dependencies — do not block current PvE implementation

The following are explicitly **Later**.

They must not block implementation of the current PvE game, first Location, saves, offline farming or core buildcraft.

1. **Arena / PvP starting state.**
2. **Arena / PvP ending and stalemate rules.**
3. **Arena snapshot replacement / history policy.**
4. **Competitive transition to server validation and treatment of legacy local saves.**
5. **Future inventory caps / overflow**, if a hard cap is introduced later. MVP has no hard inventory capacity.

Implement only the already-defined extension boundaries needed to avoid architectural lock-in. Do not build Arena gameplay now.

## Technical consequences to specify

- Offline farming means rewards for time away from the application. It does not imply continued disconnected gameplay once the server phase makes connection mandatory.
- Arena build capture and reset need a consistent operation: capture the final build before deleting run state, with a stable operation identity so retries do not create inconsistent results.
- Arena opponents are immutable build snapshots, instantiated into independent match state. Keep the PvE encounter lifecycle and arena victory rules separate while reusing applicable combat primitives.
- Future server validation must cover the provenance of arena builds and progression. Uploading a client-computed result or adding a checksum alone does not establish legitimate progression. The exact validation strategy remains a technical proposal to develop.
- A full combat snapshot must contain enough authoritative state to resume a fight without rerolling pending offers, attacks or rewards. Saving only account/run state will not meet A05.
- Snapshot consistency must prevent a reward from being granted twice after load. Exact snapshot boundaries and interrupted-write recovery will be specified in the persistence design.
- Offline farming is a separate calculation mode, but must reuse the applicable content, reward rules and account modifiers. Approximation does not permit duplicate rewards or automatic strategic choices.
- Account identity, save persistence, content delivery and time sources need explicit boundaries so local implementations can later coexist with server integrations.
- Content and save versions must be distinguishable. Resuming a saved fight after a balance update needs an explicit compatibility rule; do not silently assume old and new effect definitions are interchangeable.
- A11 selects rebasing to current balance. Rules for HP adjustment, modified effect durations and removed phase states are migration inputs; preserve the source save if an update supplies no valid transformation. This is not permission to reset or compensate the player arbitrarily.
- The future administration interface manages supported content schemas. Whether future executable gameplay changes require an application update remains a technical design decision; remote data delivery alone does not imply arbitrary code updates.

## Resolved gameplay implementation decisions — 2026-09-28

The following rules are now approved for current PvE implementation.

| ID | Resolution |
|---|---|
| D01 | Milestones use normalized Location Progress. First-Location authoritative points are 2, 14, 28, 43 unlock, 44 first-run-only choice, 57, 72 unlock, 73 normal choice, 86, 100. Stage Compression reduces physical fights but not milestone positions; crossed milestones resolve in ascending order. |
| D02 | Temporary/permanent is lifetime, not a math layer. Conversions read fully resolved source stats and are added before the receiving stat's own multiplier. Stat dependency cycles require explicit custom resolution or validation failure. |
| D03 | Fatal prevention resolves inside the action transaction after proposed damage; the final HP transition is committed only after prevention has resolved. |
| D04 | `Attack Speed ×X` multiplies Attack Speed Rating before the reference logarithmic attacks/sec conversion. Direct gear additions also modify rating before conversion. No project 30 APS hard cap. |
| D05 | Generic “% of damage” effects use post-mitigation `ResolvedDamage` before current-HP clamping/fatal prevention. `ActualHpLost` is separate. Reactive-origin damage does not recursively trigger generic reflect/counter effects unless explicitly allowed. |
| D06 | While hero is downed, enemy attack clocks, Boss phase clocks, enemy timed effects and enemy regeneration pause; hero Death Regeneration continues. |
| D07 | Hero temporary buffs/stacks persist across encounter/Stage/location transitions by default while their duration permits; death clears them. Enemy-bound state disappears with the enemy. Effects may explicitly declare EncounterBound. |
| D08 | MaxHP changes preserve current HP percentage. The rebase does not itself emit Damage/Heal/Death events. |
| D09 | Generated Power offers are stored durably, never rerolled by UI/save-load, and queue in order. A choice with eligible options blocks simulation; an empty-pool choice remains a non-blocking pending credit until something unlocks. |
| D10 | EXP uses encounter-owned rewards. Stage Compression skips rewards for skipped baseline encounters; no compensation is granted. Repeat farming uses the final/end-of-Stage reward profile. |
| D11 | MVP inventory has no hard capacity/overflow. Lock and reset retention are separate. Auto Equip only performs strict context-free dominance swaps; unique/triggered/conditional items require manual review. Named items do not roll procedural affixes unless explicitly authored to. |
| D12 | Required Location Boss reward is once per run. Deliberately navigating away from an unfinished encounter abandons it without rewards; returning starts a fresh encounter. Death/save-load/offline absence do not count as abandonment. |
| D13 | Reference-based achievement thresholds are copied for the current Power set; stat achievements observe fully resolved reached values and unlock permanently. Cumulative Damage Taken uses the project's resolved-damage basis. |
| D14 | Future Crit/DoT/AoE/Summon/Collections/Prestige/Mastery/Challenges are Later unless already required by current content. Each future mechanic must define event eligibility, targeting, stat/damage basis, stacking/lifetime, save state, offline policy and UI before production content uses it. |

This resolves the current PvE implementation-question register.

New ambiguities found during implementation should be added here only when they affect current approved content. Future-feature questions should be marked **Later** rather than blocking current work.

## New implementation question for game design

| ID | Source / issue | Decision needed |
|---|---|---|
| D15 | `Docs/Economy/00_Reference_Economy_Baseline.md` says compressed encounters sample the Stage's ten baseline reward slots in order and end at its final profile, but does not specify which slots are selected when only `R < 10` physical encounters remain. Different round-up/round-nearest/explicit tables yield different EXP and loot. | Specify the exact slot mapping for each compressed `R` (1–10), or approve a formula. The normalized milestone crossing implementation does not depend on this choice; compressed reward selection remains pending. |
| D16 | Combat defines paused enemy clocks and hero recovery while downed, but does not state whether the hero's own pending attack resumes with its remaining readiness or starts a fresh interval after revival. | Choose `PauseRemaining` or `RestartOnRevive`. The timeline requires this policy as an explicit input and stores the selected policy in downed saves; no production default is active. |
| D17 | Combat says targets are chosen automatically but does not define the ordinary multi-enemy target order or tie-break. | Define the baseline target-selection rule. The timeline accepts an injected target policy; the test-only first-opponent selector is not production behavior. |

## Editorial cleanup status

The previously noted stale `Run Level`, deferred Block-formula wording and malformed multi-enemy economy table are resolved in the current design-document pass.

## Intended specification deliverables

1. Module boundaries, dependency direction, authoritative state owners and lifetimes.
2. Simulation time, numeric representation, RNG, stat dependencies, event order and effect contracts.
3. Content schemas, stable identity, authoring, validation and custom mechanic extension points.
4. Progression, rewards, gear, unlocks, reset and future permanent-system integration.
5. Save/load, migration, offline progress and selected platform/service integration.
6. Unity composition, presentation, command handling and application lifecycle.
7. Implementation increments with source requirements, prerequisites, acceptance scenarios and verification.

Each specification must distinguish approved decisions, proposed defaults and unresolved questions. An implementation agent must be able to identify blocked behavior without guessing.

## Technical specification index

- `03_System_Architecture.md`: module structure, ownership, content and expansion contracts.
- `04_Simulation_Contracts.md`: commands, time, RNG, numeric operations, stats and effects.
- `05_Persistence_and_Services.md`: complete saves, offline calculation, lifecycle and future server boundaries.
- `06_Implementation_and_Verification.md`: staged implementation, acceptance checks and handoff.

## Review checkpoint

The initial review was documentation-only. Subsequent implementation added foundations summarized in `01_Engineering_Handoff.md`. The current PvE D01–D14 register is resolved; remaining Arena/PvP and future-feature questions are explicitly Later and do not block current PvE implementation. See the current Git status and latest handoff for verification and changed-file state.
