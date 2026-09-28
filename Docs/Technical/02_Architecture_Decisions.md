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

## Deferred design dependencies — not the current task

**Owner scope clarification:** this task is technical specification only. Do not solicit or define new gameplay or PvP rules. The following are future feature-design dependencies, not questions the owner must answer to complete the architecture. Represent their boundaries in contracts; do not enable dependent features with guessed rules.

1. **Arena starting state:** permanent build composition versus temporary battle buffs/stacks at reset; full HP and fresh timers versus current combat condition. The full PvE combat save and arena build snapshot are distinct concepts.
2. **Arena ending rules:** PvE normally revives indefinitely. Define PvP victory, eligibility of revive Powers and stalemate/timeout behavior explicitly.
3. **Arena snapshot replacement:** automatically replace with the latest reset build, or retain/select older snapshots? Only capture at reset is approved so far.
4. **Transition to server validation:** decide whether legacy local saves may enter competitive play, and under what migration/validation rules. A client-created save is not proof of legitimately earned progress.
5. **Offline inventory overflow:** the farm formula, eligibility, target fallback, cap and reward semantics are now defined in game design. Inventory overflow handling for large reward batches remains open.

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

## Resolved implementation questions — owner, 2026-09-28

- **D03 — fatal prevention:** apply the proposed HP loss first, then restore HP if a fatal-prevention effect succeeds. The action's committed damage and notification fields must reflect the final post-prevention outcome; saving is allowed only after the full action is resolved.
- **D06 — downed hero:** while the hero is downed and regenerating, enemy attack timers, Boss phase timers and effects on enemies are paused. Enemy HP remains unchanged except for already-resolving actions; resume those clocks when the hero revives.

## Design ambiguities to resolve before dependent implementation

These are findings, not corrections or newly locked rules.

| ID | Sources / issue | Decision or clarification required |
|---|---|---|
| D01 | Run Progression cadence table places choices at encounters 43 and 72; Location 01 distinguishes unlocks at 43/72 from choices at 44/73 | Establish one authoritative milestone table, including later runs and Stage Compression |
| D02 | Combat places conversions before derived multipliers; Eagle Eye uses Accuracy ×5 and Accuracy-to-Min-Damage conversion; Bulwark uses current Block | Define which resolved value each conversion reads and how temporary primary modifiers propagate into derived stats |
| D04 | Powers use Attack Speed ×0.5 while Combat distinguishes rating from attacks/sec | Specify the target quantity for every speed modifier and behavior at very small rates |
| D05 | Reactive Powers refer to a percentage of damage without defining the damage basis | Specify pre/post-mitigation damage, actual HP lost, overkill and reactive-damage eligibility |
| D07 | Temporary combat stacks can support momentum over several minutes, but encounter-state lifetime is scoped to one encounter | Define carryover between encounters and clearing on stage/location changes |
| D08 | Changing Vitality changes Max HP; timed enemy Vitality reduction already exists | Define current-HP adjustment on gain, loss and expiration; also equipment changes |
| D09 | Power choices are progression rewards, but waiting behavior is unspecified | Define whether combat pauses, choices queue, offers persist, and what happens with an exhausted eligible pool |
| D10 | Normalized reference economy has a per-reference-enemy hypothesis and a 100-encounter location | Define exact encounter EXP allocation, repeat-farming level and Stage Compression reward behavior |
| D11 | Gear explicitly leaves inventory capacity, overflow, disposal and named-item affixes open | Define acquisition when full, lock versus reset retention, and safe auto-equip comparison |
| D12 | Repeated boss farming and abandoning/switching fights are not fully specified | Define access, preserved fight state and repeat reward eligibility |
| D13 | Stat achievement unlocks are named without exact thresholds or measurement semantics | Define observed values, temporary-buff eligibility and unlock timing |
| D14 | Crit, DoT, AoE, summons, Collections, Prestige, Mastery and Challenges have direction but incomplete rules | Describe extension contracts now; require approved feature rules before implementing each extension |

This document is the single implementation-question register. Add every newly found gameplay ambiguity here with source, affected feature and the specific decision needed. Do not scatter new questions across code comments or new question lists; game design will resolve this register later. Until resolution, dependent production content remains disabled rather than receiving a guessed rule.

## Editorial issues

- Combat still mentions `Run Level` among values retained on death, although Run Progression explicitly says there is no XP-driven Run Level.
- Combat's later Block subsection says the formula is deferred, while the baseline section supplies an implementation-default formula.
- Economy's multi-enemy normalization table is interrupted by the encounter-owned EXP section and does not form a valid contiguous Markdown table.

These should be reconciled with the owner-approved rules rather than treated as permission to redesign gameplay.

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

The initial review was documentation-only. Subsequent implementation added the T02–T08 foundations summarized in `01_Engineering_Handoff.md`; dependent production rules remain pending in this register. See the current Git status and latest handoff for verification and changed-file state.
