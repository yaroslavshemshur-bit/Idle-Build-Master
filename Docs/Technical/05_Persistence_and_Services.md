# 05 — Persistence, offline progression and service integration

**Status:** technical specification v0.2, 2026-09-28. Not implemented.  
**Sources:** Architecture Requirements §2, §4, §31–34, §38, §45; owner decisions A01–A13 in document 02.

## 1. Operating modes

| Mode | Authority | Required integrations |
|---|---|---|
| Initial local game | Local session and durable save | Bundled content, local storage, local time; no login/backend dependency |
| Development build | Same local contracts plus diagnostic tooling | Development-only save import with validation and provenance marking |
| Future connected game | Server validates accepted progression; exact protocol to be specified | Authentication, authoritative revisions/time, cloud persistence, content delivery |

Connected mode is not merely local mode with a login screen. Loss of connectivity must be an explicit application state; do not silently keep granting trusted progression locally. Handling brief disconnects, prediction and reconciliation belongs to the server implementation phase. The owner has not requested permanent offline play after server introduction.

Offline farming means rewards for time away from the game. It remains compatible with mandatory connection when the player returns.

Current delivery uses client-side computation for every implemented feature (A13). The future connected row is an integration direction only, not authorization to implement server combat now or a settled client/server authority split.

## 2. External ports

Define these interfaces in Application; implementations live in Infrastructure:

| Port | Contract |
|---|---|
| `ISaveStore` | Read candidate slots; commit immutable snapshot bytes; return durable commit result |
| `ISaveCodec` | Encode/decode typed DTOs with format/version limits, no arbitrary runtime types |
| `ISaveMigrator` | Upgrade a validated old DTO through ordered schema migrations |
| `IContentProvider` | Load a specific immutable catalog version; verify manifest and supported schema |
| `IWallClock` | UTC reading plus trust/source metadata; never simulation time |
| `IMonotonicClock` | In-session elapsed time, unaffected by wall-clock adjustment |
| `IProgressAuthority` | Submit intent/batch, obtain accepted revision/result; local adapter initially |
| `IAccountService` | Future authentication and local-profile linking; absent before account phase |
| `ITelemetrySink` | Bounded best-effort diagnostic/event export, no gameplay authority |

Ports use typed success/failure results, cancellation and explicit operation IDs where retries can duplicate effects. Storage/network operations do not run from inside the combat kernel. Do not block Unity's main thread on network I/O.

## 3. Snapshot envelope

```text
SaveEnvelope
  formatId
  schemaVersion
  gameBuildVersion
  simulationRulesVersion
  numericFormatVersion
  rngAlgorithmVersion
  contentVersion + mechanicalContentHash
  profileId + runId
  snapshotId + stateRevision + storageGeneration
  savedAtUtc + timeSource
  payloadLength + integrityChecksum
  payload
```

The checksum detects corruption; it is not anti-cheat evidence. JSON integers beyond common parser-safe numeric ranges and RNG machine words should be encoded as documented strings or fixed binary/text representations, not passed through lossy floating-point parsers.

Payload contains AccountState, RunState, CombatState, session metadata and offline accounting state. Store explicit discriminators for effect/behavior variants. Never store assembly-qualified type names, object-reference graphs, delegates, coroutines, Unity InstanceIDs or scene object paths.

## 4. Full combat restoration

The saved state must include:

- All actors: instance/definition IDs, faction/owner, HP, life state and relevant resources.
- Active encounter ID, composition, spawn counters, completion state and reward shares/claims.
- Hero combat continuity state, active targets and stable actor ordering.
- Attack readiness, clock origins/revisions and pending scheduled attacks.
- Buff/debuff instances, source identities, independent stack expirations and cooldowns.
- Boss behavior state, entered time, counters (including Fortify break progress) and pending transitions.
- Simulation time, scheduler sequence, pending durable scheduler entries and instance allocators.
- Every gameplay RNG stream's state and algorithm version.
- Pending Power offers and any generated-but-not-yet-presented results in their owning run state.

Do not save derived stat caches; rebuild them against the active catalog and compare invariants. On unchanged content/rules, restore exact continuation; on an update, first apply the rebase transaction in §6. Persist the long-lived scheduler as typed entries with IDs and stable ordering, or deterministically reconstruct it from stored state using the same keys. Choose one representation per event family; never save and independently regenerate duplicates.

Transient reaction queues must be drained to a safe transaction boundary before capture. If a work budget yielded mid-action, resume later or retain the previously committed durable snapshot. No partially applied HP change may be captured without its associated reward/trigger state.

On restore, validate actor references, unique instance IDs, effect support, nonnegative durations where required, scheduler time bounds, ledger consistency and state/content compatibility before activating the session. Recreate visuals from state; do not replay old VFX as gameplay actions.

## 5. Durable local commit

Use two independent save generations plus temporary write candidates. A platform store must guarantee recovery to at least one verified complete generation; do not assume file rename/flush semantics are identical on every target.

Commit procedure:

1. Capture an immutable snapshot at a safe boundary on the session writer.
2. Encode on a background task if supported; live mutable objects must not escape to the writer task.
3. Write the inactive generation through a temporary candidate; flush/close, then verify payload size/checksum and decode envelope.
4. Publish that generation with the platform-supported replacement operation.
5. Report durable completion. Retain the previous valid generation.

Load validates both generations and selects the newest valid compatible generation. It must survive an incomplete candidate, corrupt latest generation and interrupted publication. Newer unsupported schemas are reported as incompatible, not overwritten with a fresh profile. If neither generation is usable, preserve evidence and surface a recoverable load failure; do not silently erase progression.

Use a single serialized save writer, with monotonically ordered generations. Coalesce obsolete pending snapshots but never allow an older asynchronous completion to replace a newer committed generation.

Checkpoint after important committed actions such as a Power choice, equipment change, reset and reward claim; additionally use configurable periodic autosaves and best-effort pause/focus-loss checkpoints. The exact interval is a technical configuration chosen from measured write cost. Mobile termination may not provide a final callback; periodic checkpoints are mandatory. No architecture can promise preservation of actions that never reached durable storage before process termination.

## 6. Migrations and content compatibility

Migrations are pure ordered transformations `N -> N+1`, with fixtures for each supported version. Work on a copy, validate the result, then persist as a new generation. Keep the original until the migrated generation is verified.

Distinguish:

- Save schema change: structural DTO migration.
- Content change: IDs, balance coefficients and effect definitions.
- Simulation rules change: ordering, interpretation, RNG consumption or algorithm behavior.
- Numeric/RNG representation change: dedicated conversion or explicit compatibility handling.

The owner permits recalculation under updated balance (A11). Save the original content/rules version for provenance, then rebase the full fight to the currently supported version in one transaction. There is no requirement to finish an old fight under its old balance or ship old rule engines solely for that purpose.

Load/update sequence: decode original snapshot -> migrate structural DTOs -> resolve current IDs/definitions -> transform affected state through a versioned compatibility handler -> rebuild derived stats and scheduler -> validate -> commit a new save generation -> activate the session. Keep the original generation until the new one is verified. Never activate a mixture of old and new calculated state.

The compatibility handler explicitly accounts for HP/MaxHP relationships, changed attack rates, effect magnitudes/durations, boss phase variables and removed definitions. Current PvE game design preserves HP percentage when MaxHP changes. A future rules version may supply a different explicit transformation if needed. A release changing those semantics must supply its transformation. If no valid handler exists, preserve the save and report incompatibility instead of resetting the fight or compensating arbitrarily. Existing rolled items, offers and already-granted rewards are not rerolled by the load pipeline.

Missing presentation data may use a placeholder. Unknown mechanical IDs are quarantined with diagnostics; automatic deletion, item compensation and Power replacement require explicit migration rules. Unsupported required effects must not run as no-ops.

Remote content later uses download -> integrity/schema/reference validation -> staging -> explicit activation with the same atomic rebase boundary. Never mutate a catalog in place during a fight. Keep the last valid bundle if a candidate fails. Signatures/TLS and trusted manifests belong to the future delivery implementation; content authenticity does not validate player progression.

## 7. Offline calculation contract

Approved behavior: preserve the active fight without advancing its combat timers; calculate separate farm rewards from an eligible completed normal Stage; resume the saved fight after return. Do not spend EXP, choose Powers, equip gear or push new progression automatically.

Authoritative game-design source:

`Docs/Economy/01_Offline_Farming.md`

Baseline design constants:

```
MaxOfflineTime = 6 hours
OfflineEfficiency = 0.50
MinOfflineEncounterTime = 60 seconds
```

The offline model intentionally does **not** inspect or estimate:

- combat DPS;
- survivability;
- deaths;
- enemy count;
- encounter composition;
- Boss phases;
- proc frequency;
- AoE efficiency.

Offline value is driven by the selected completed Stage's normal reward profile. The evaluator only determines virtual clear entitlement.

Data contract:

```
OfflineRequest
  operationId, profileId, baseStateRevision
  absenceStart, absenceEnd, timeSource
  farmingAssignmentId, catalog/rulesVersion
  permanentModifierRevision
  offlineRngState
  residualClearProgress

OfflineClearResult
  eligible
  eligibleSeconds
  effectiveOfflineEfficiency
  virtualClears
  newResidualClearProgress
  modelVersion
  validityReason

OfflineRewardPlan
  operationId, baseStateRevision, accountedThrough
  rewardEntries, rolledItemInstances, progressFacts
  nextOfflineRngState, modelVersion, catalogVersion
```

The first implementation formula is:

```
EligibleSeconds =
min(max(AbsenceSeconds, 0), MaxOfflineTime)

EffectiveOfflineEfficiency =
clamp(BaseOfflineEfficiency × OfflineEfficiencyModifier, 0, 1)

ClearProgress =
PreviousResidual
+ EligibleSeconds
× EffectiveOfflineEfficiency
/ MinOfflineEncounterTime

OfflineClears = floor(ClearProgress)

NewResidual =
ClearProgress - OfflineClears
```

At baseline:

```
OfflineClears per 6h = 180
```

At future 100% efficiency:

```
OfflineClears per 6h = 360
```

The 60-second minimum therefore remains the maximum offline clear rate unless game design explicitly changes it.

Target eligibility/fallback:

1. explicit eligible completed normal Stage selected by the player;
2. current completed Stage when Auto Push is OFF;
3. highest completed normal Stage accessible in the current run;
4. otherwise no offline farming.

Bosses are never offline farm targets.

The planner reuses the Stage's normal reward definitions. It may resolve:

- EXP;
- procedural gear;
- rarity;
- named-item drops;
- future resources;
- future Collection progress;
- future passive achievement progress.

Use the dedicated offline RNG stream.

Do not create special reduced offline loot tables unless design explicitly introduces them later.

Reward modifiers that already existed when the absence began may apply according to their normal eligibility rules. Rewards earned during the absence do not modify later virtual clears from the same absence.

The evaluator must retain fractional clear progress so splitting one absence into several application sessions cannot repeatedly lose or duplicate entitlement.

Apply a reward plan only to the matching revision/assignment, atomically with its RNG state and `accountedThrough` cursor. Retrying the same operation returns the stored result rather than rolling again.

Acknowledging the Offline Rewards UI is separate from granting the rewards. Dismiss/reopen cannot grant twice.

If offline gains unlock permanent effects, those effects activate only after the offline reward transaction. They do not retroactively alter the same offline period or the frozen combat state.

Local elapsed-time handling clamps negative elapsed time to zero and records suspicious wall-clock changes diagnostically. No local algorithm can establish trusted elapsed time against a user-controlled device. In connected mode, use server-accepted time and an idempotent server reward operation.

## 8. Accounts, server and trust transition

Before accounts, keep a stable local profile ID without pretending it is an authenticated identity. Cross-device recovery is deferred as instructed. A later account-link operation must explicitly resolve conflicts; do not sum currencies or merge two active runs as a default.

Prepare an intent-based authority boundary and versioned command/event definitions now. Do not build a pretend anti-cheat system by signing client values with a secret embedded in the app.

Connected-phase specification must define:

- What the server computes, replays, validates or accepts within limits.
- Authenticated operation IDs, expected revisions, replay protection and durable reward/entitlement records.
- Server time and seed ownership for trusted results.
- Validation of build source data before an arena snapshot becomes eligible.
- The fate of legacy local and development-imported profiles when entering the validated environment.
- Retry, timeout, reconciliation and content-version compatibility behavior.

These are deferred technical/security integration tasks. They do not require deciding arena game balance now. No server-side combat mandate is adopted in the current client-only phase. Future competitive validation must be specified before competitive release; a client result alone is not proof of legitimate progression. Combat hosting/replay and seed ownership are deferred with the server design.

## 9. Development import

Compile import entry points only in the editor/development build configuration. Validate size, schema, references and supported versions, then import through a staged save transaction with backup. Mark provenance as development-imported. Do not include an active import command or hidden UI bypass in release builds.

A future server must establish trust independently of a removable local marker. The marker helps development diagnostics; it is not security evidence. Public export/import and account recovery tooling are outside the current phase.

## 10. Validation obligations

Test save/load continuation, interrupted writes, corrupt latest generation, concurrent save ordering, unsupported future schemas, missing mechanical IDs, migrations and offline retries. Verify application background/foreground behavior on Android/iOS during the mobile integration phase. Do not claim that editor-only persistence tests validate mobile termination behavior.

Distinguish same-version continuation tests from updated-balance rebase tests: only the former expect the old future combat result. Rebase tests verify the supplied transformation, current-version consistency, no rerolls/duplicate grants and recovery after interruption. Offline evaluator tests use formula fixtures and assert that no combat runner is invoked; those fixtures do not define production balance.
