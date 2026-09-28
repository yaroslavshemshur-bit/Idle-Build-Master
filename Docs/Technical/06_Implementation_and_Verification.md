# 06 — Implementation sequence and verification

**Status:** technical specification v0.2, 2026-09-28. No game implementation performed by this documentation task.

## 1. Working rules for the implementing agent

1. Read repository AGENTS, this increment's contracts and its named design sources.
2. Inspect current Git state and preserve unrelated changes. Do not assume the technical specification is already implemented.
3. Implement one complete increment, including relevant metadata, diagnostics and verification.
4. When a gameplay field is absent or contradictory, cite its dependency ID from document 02. Do not invent the value, silently choose a source or design PvP. Continue independent technical work with synthetic test fixtures where possible.
5. Implement future contracts when a current dependency needs them; do not create empty frameworks for every row of the expansion matrix.
6. Document the actual files changed, decisions, exact checks and result, remaining dependencies, Git state and next increment.

## 2. Increment plan

| Increment | Deliverable | Required acceptance |
|---|---|---|
| T01 Foundation | Domain/Application assembly boundaries, identifiers, large-number implementation, version types, RNG/time interfaces and core test harness | Domain/Application compile with no Unity engine reference; RNG vectors, numbers beyond double range and numeric boundary tests pass |
| T02 Content | Authoring assets, plain catalog compiler, manifest, typed registries and validator; balance and interface quantities authored in SO assets | Duplicate/missing IDs, unsupported effects, invalid probabilities and graph cycles fail with source paths; same inputs produce same mechanical hash; missing UI keys and invalid UI units fail validation |
| T03 Session | Single writer, command envelope/results, state ownership, revision and transaction boundaries | Rejected commands leave state/RNG unchanged; duplicate commands do not double-apply; no UI needed |
| T04 Simulation mechanics | Scheduler, stat graph, action/effect pipeline, actor lifecycle and serializable boss behavior | Partition invariance and same-time ordering tests; reaction continuations yield safely; synthetic effects exercise dependencies and stacks |
| T05 Persistence foundation | Full snapshot DTO/codec/store, generations, migrations, current-balance rebase and safe-point capture | Crash/corruption/restore fixtures; identical continuation on unchanged rules/content; atomic validated transformation after updates |
| T06 Initial content integration | Approved first-location rules, 25-Power primitives as needed, rewards, gear and progression | Architecture Requirements §47 scenarios, with traceable approved rules and no Location-specific combat controller |
| T07 Unity presentation | Bootstrap, combat scene, stat purchases, Power choice, inventory/comparison, map/farm views | Views may close/reopen without state changes; commands and errors round-trip; no animation determines outcomes |
| T08 Offline/mobile | Analytical formula evaluator supplied by game design, idempotent reward application, lifecycle handling and profiling | No combat simulations for offline efficiency; no advancement of frozen fight during absence; retry cannot duplicate grants; device pause/resume and save recovery verified |
| T09 Permanent progression/extensions | Collections, achievements, mastery, reset, retained gear, future combat families in separate feature increments | Approved feature-specific rules; each extension uses existing contracts and migration tests |
| T10 Server/account/content | Authentication/linking, validated authority, cloud saves, remote content and administration tools | Trusted reward operations, revision conflicts, old-client/content handling, staging/rollback and migration policy verified |
| T11 Async arena | Reset build snapshots and isolated matches under subsequently specified authority/validation contracts | No live-run mutation; retry-safe snapshot capture/results; test against subsequently supplied arena rules; server-side execution is not a current prerequisite |

T01–T05 can use synthetic test content without choosing unresolved game design. T06 production scenarios depend only on the relevant approved gameplay rules, not on complete future-system design. T05 starts early because serialization must test every growing state model. T08 offline economy depends on its configured policies; mobile lifecycle verification can proceed independently. T09 is a family of increments, not one large implementation task.

No application code is requested as part of the current documentation delivery.

For the current delivery, all computation stays on the client. T10–T11 are future phases, not dependencies of T01–T09. The mathematical offline formula is explicitly delegated to game design; implement its technical boundary with fixtures, but do not invent or simulate a production model.

## 3. First-location integration checklist

Implement and verify the existing Architecture Requirements §47 loop: fresh-run attributes, combat, EXP spending, Power offers/unlocks, variable enemy counts, procedural gear/affixes/comparison, Stage 8 targeted farming, Auto Push, Goblin Chieftain phases, death/revive with enemy HP preserved, location completion and save/load.

Additional regression cases:

- A six-enemy encounter grants its authored budget once, independent of kill order.
- Reload during Fortify preserves break counter, phase timing, HP, effects and RNG.
- A stored Power offer remains identical through reopen/reload.
- Named-item conversion reacts to source-stat changes according to its approved dependency semantics.
- Changing visible frame rate and disabling VFX does not change the simulated result.
- Closing inventory or switching views does not reset gameplay.
- A repeated completion/claim/reset command does not duplicate reward or reset effects.

Use approved gameplay examples as golden expectations. Use synthetic rules only for structural tests and clearly label them as fixtures, never as production design.

## 4. Test layers

| Layer | Coverage | Execution |
|---|---|---|
| Pure logic | Stats, RNG, time, effects, reward ledgers, commands | Focused Unity Edit Mode tests without scenes |
| Persistence | DTOs, migrations, corruption and resume continuity | Edit Mode plus platform-store integration |
| Content | Schemas, references, unsupported rules, deterministic compilation | Editor validator and batch validation |
| Unity integration | Bootstrap, subscriptions, screen reconstruction, lifecycle | Play Mode tests and focused manual checks |
| Device | Memory, sustained CPU, background/termination/relaunch | Android/iOS development builds |
| Future server | Authority, duplicate requests, conflicts, replay validation | Service tests plus client integration |

Run compilation and focused tests for each implementation increment. Broaden testing when dependency changes create new risk. Do not write tests that merely mirror a setter; verify behavior and invariants that could regress.

## 5. Performance and diagnostics

Do not claim a device performance guarantee before selecting and measuring representative Android/iOS hardware. During mobile integration, record device/OS/build, scene/content, build snapshot, simulation duration, average/p95 frame and simulation CPU, allocation rate, peak memory, save size/time, offline calculation time and backlog.

Required benchmark scenarios:

- Baseline first location, one and six enemies.
- Reactive stack-heavy build from the implemented Power set.
- Many timed effects with independent expirations.
- High-rate synthetic load, distinguishing exact supported simulation from an unsupported aggregation regime.
- Save/restore in an active boss fight.
- Large inventory and offline reward batch.
- Long sustained farming session to detect unbounded history/heap growth.

Budget settings are technical configuration: work per frame, trace ring size, autosave interval, presentation event sampling. None may silently alter damage, duration, rewards or proc count.

Keep a bounded diagnostic ring for command/revision, action lineage, RNG position, stat trace references, damage phases, reward operations and boss transitions. Full traces are opt-in development output. Production logs must avoid authentication secrets and uncontrolled save dumps.

## 6. Design dependency handling

The PvE questions D01–D14 in document 02 are now resolved for the current implementation baseline.

Implementation should use the approved rules and their owning design documents rather than synthetic production defaults.

Future Arena/PvP rules, advanced permanent progression, later combat families and future inventory caps are explicitly **Later** and do not block T01–T09 work that does not implement those features.

If a new ambiguity affects current approved content, add it to document 02 with:

- source documents;
- affected feature;
- exact missing rule;
- blocked tests/handlers.

Do not promote a Later feature into a current dependency merely because the architecture exposes an extension point for it.

## 7. Verification of this documentation delivery

The technical specification is a proposed implementation baseline, not a report of a working game. Verify Markdown links, IDs between documents, consistency with owner decisions and absence of accidental gameplay defaults. Unity compilation and gameplay tests do not apply until source implementation begins.

Record Git branch/commit and outstanding changes at handoff. Do not commit or claim tests merely because documentation describes them.

### Documentation handoff, 2026-09-28

- Base: branch `main`, commit `6662e37` (`Record model preference for architecture and implementation`).
- Working tree: uncommitted documentation edits to README, architecture requirements and engineering handoff; new decision register and specifications 03–06. No Unity source changed.
- Verified local Unity version and package manifest before selecting the technical baseline.
- `git diff --check`: passed; Git emitted only LF-to-CRLF conversion notices for tracked Markdown files.
- Local Markdown-link existence and fenced-code balance check across `Docs/README.md` and `Docs/Technical/*.md`: passed.
- Unity compilation, mobile profiling and gameplay tests: not run; no implementation was performed.
- Next implementation increment: T01. The specification supplies technical defaults; later gameplay-dependent increments must respect the unresolved design register rather than invent rules.
