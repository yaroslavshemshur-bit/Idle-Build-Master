# 04 — Simulation contracts

**Status:** technical specification v0.2, 2026-09-28. Not implemented.  
**Sources:** Architecture Requirements §1–29, §34–44; existing Combat and Power documents. Gameplay ambiguities are tracked in document 02.

## 1. Determinism contract

For identical initial state, catalog, rules version, RNG states and ordered commands, the simulation must produce identical results in its supported runtime environment independently of rendered frame count and `AdvanceTo` partitioning.

This is not a promise of bit-identical floating-point behavior across every CPU or client/server runtime. Golden vectors must verify supported targets; future server authority does not trust client hashes merely because deterministic execution was intended.

Every mechanical operation uses explicit simulation time and injected RNG. Wall-clock reads, frame delta, Unity object iteration, dictionary iteration order, thread completion order and visual animation callbacks must not choose outcomes.

## 2. Time and scheduler

`SimTime` is a signed 64-bit count of microseconds from session simulation origin, checked for overflow. Durations use the same unit. Absolute time is distinct from real UTC time and from time since last login. Content duration conversion uses one documented rounding operation, midpoint away from zero; negative durations are invalid unless a specific signed offset type allows them.

Use an event-driven scheduler for attacks, effect expiration, ticks and behavior transitions. `AdvanceTo` processes due events in increasing timestamp order. Advance continuous state (including regeneration) to an event timestamp before processing discrete events there. The real-time driver carries sub-microsecond conversion residue instead of rounding each rendered frame independently.

Scheduled work has a durable key:

```text
(dueTime, phasePriority, insertionSequence)
```

Insertion sequence is monotonic and saved. Equal-priority effects are registered in stable definition/instance order. Do not let asset loading order determine registration order. Mechanically meaningful precedence between simultaneous expiry, attacks and phase changes is supplied by a versioned `CombatRulesDefinition`; the scheduler itself must not guess a design priority. Within the agreed reaction tiers, preserve FIFO.

An attack clock records accumulated readiness, last evaluation time and revision. Changing rate updates the clock under an explicit `AttackClockPolicy`; stun and rate changes invalidate stale scheduled entries by revision. Do not accumulate unbounded obsolete entries: compact/rebuild the heap when invalid entries exceed an implementation threshold, preserving original ordering keys.

Continuous integration must be partition-invariant. For a constant segment use a stored segment origin/value and evaluate at the requested time; do not make the result depend on thousands of frame-sized additions. End/rebase segments when a rate or a boundary changes. If a rule changes at an HP threshold (including revive at full HP), schedule the threshold crossing rather than detecting it late on the next rendered frame. Unsupported continuously varying formulas require an explicit integrator with fixed, versioned stepping rules and tests.

## 3. Work budgets and high attack rates

`AdvanceTo(target, budget)` returns `ReachedTarget`, `Yielded` or `Faulted`, plus reached time, continuation state and diagnostics. The driver resumes yielded work before accepting later simulation time. Yield is a CPU scheduling action, never skipped damage or consumed gameplay time.

The event processor can yield inside a large reaction chain by retaining its continuation. No commands or save snapshots commit mid-chain; the last committed snapshot remains usable if the app is terminated during a yield.

Start with exact event processing. Optimize by indexes for active triggers, cached stat dependencies and coalesced presentation. Aggregate attacks only through a dedicated resolver that declares its supported conditions and passes equivalence tests against the exact resolver. Do not replace random procs by expected damage for ordinary online combat.

Owner decision A10 makes preservation of combat results mandatory. The view may omit impacts, animations and individual damage labels, but simulation may not omit the corresponding actions. An aggregation resolver needs a stated equivalence argument, including ordering, RNG consumption and state changes; tests alone are not proof for every build. Do not substitute the future offline analytical model for active combat.

No arbitrary attack-rate cap or hidden truncation of trigger count is allowed. If a valid build exceeds the execution budget, report simulation backlog and keep unprocessed work; use measured evidence to add a valid aggregation method. Timing benchmarks in document 06 decide whether an implementation is adequate.

Microsecond scheduling also has a representational limit: intervals smaller than one unit must not be rounded to zero or silently clamped to one unit. Detect that condition explicitly. Before such content is enabled, provide a tested finer/sub-unit timing representation or an equivalent aggregation resolver. Numeric/time format changes require versioning and migration, not a hidden gameplay cap.

## 4. RNG contract

Define `IRandomStream.NextUInt32()`, unbiased bounded integer selection and a `[0,1)` uniform conversion. Pin a reproducible algorithm with explicit state; technical default is PCG32, implemented with specified integer overflow behavior and golden-vector tests before gameplay use. Never rely on runtime-default `Random` algorithm stability.

Separate streams for combat, loot, Power offers and offline farming. Seed derivation uses a specified stable hash over root seed and stream identifier, not `GetHashCode`. Save the root/stream algorithm version and every stream's state. Future arena matches need isolated seeds and no access to the live run streams; ownership of trusted seeds belongs to the deferred server design. All current streams are local.

For each handler document whether a roll is consumed when chance is zero/one, a target is absent or an earlier effect cancels the action. These consumption rules belong to versioned implementation contracts. Unit tests pin them; changing them is a rules-version change.

Rejected commands must not consume RNG. Item rolls and Power offers are generated once into state and loaded as stored instances. Opening a UI or retrying a committed request must not reroll them.

## 5. Numeric representation

Use integer types for counts, revisions and discrete purchases; `double` for bounded rates, ratios and probabilities. Do not use `float` as authoritative combat storage. Range-check inputs and reject NaN/infinity.

Implement `GameNumber` with large exponents from T01. No finite-double-only interim implementation is allowed. Technical default: decimal scientific arithmetic with a signed integer coefficient of up to 34 significant decimal digits and a checked signed 64-bit base-10 exponent. The value is `coefficient × 10^exponent`; coefficient arithmetic uses `System.Numerics.BigInteger`, bounded after each public operation. Zero has the unique representation `(0, 0)`; normalize nonzero values by removing trailing decimal zeros and adjusting the exponent.

Round arithmetic results to 34 significant digits, nearest with ties to even. This is numeric precision, not a change to gameplay rules for rounding EXP or item stats. Basic arithmetic works on the coefficient/exponent representation without converting the full value to `double`. Addition/subtraction must use bounded alignment with guard/sticky information and cancellation tests; never allocate `10^exponentDifference` for unbounded gaps. Comparison uses sign, decimal order and significant digits. Division by zero, invalid input and exponent overflow return explicit faults rather than infinity, wraparound or silent saturation.

Persist coefficient and exponent as invariant decimal strings in a versioned object; round-trip them without JSON floating-point conversion. Formatting for labels is a separate service. `10^1000`, multiplication beyond `10^1000` and ratios of similarly large values must work in the initial implementation.

This gives a very large range with bounded precision; it is not infinitely exact arithmetic. Values below the retained significance can round away. Tests must document cancellation, tiny deltas and purchase comparisons, and never claim unit accuracy at arbitrarily large magnitudes. Combat equivalence means the same results under this specified numeric model, not replacement of random combat by its expectation.

Bounded probabilities/ratios may convert explicitly to `double`. For logarithms, decompose the value into a bounded mantissa and decimal order instead of converting the whole number; e.g. `log2(x) = log2(mantissa) + decimalOrder × log2(10)`. Pin operation ordering and tolerances with golden tests. Powers with fractional exponents likewise require a versioned numeric routine with tested error bounds; avoid an unchecked full-value conversion. Cross-runtime transcendental bit identity is not promised. Profile coefficient allocations on mobile before changing the representation or precision.

The API needs addition/subtraction, multiplication/division, ordering, min/max, explicit conversion to bounded ratios and required exponent/log operations. Formatting belongs to a separate presentation service. All rounding of EXP, item level and affix rolls must refer to an approved rule; an unspecified rounding rule is a content-validation error for that feature, not an invitation to use an arbitrary language default.

## 6. Stat evaluator

Data shapes:

```text
StatDefinition: id, numericKind, baseFormula, dependencyIds, boundsPolicy
Modifier: instanceId, sourceId, targetStatId, operation, valueExpression,
          layer, conditionId, stackGroup, activation/expiry
StatContext: actorId, targetId?, stateRevision, evaluationTime
StatResult: value, orderedContributions, dependencyTrace
```

A modifier operation is an explicit enum/handler, not free-form expression code. Expressions are a bounded typed tree of supported constants, stat reads, contextual reads and mathematical operations. Compile dependencies to a directed acyclic graph; reject cycles with a path explaining the offending content.

Distinguish a baseline stat read from a resolved stat read and a contextual snapshot read. Every conversion declares which it consumes. This supports Accuracy-to-Min-Damage and Block-to-Max-Damage; the approved D02 semantics require these conversions to read fully resolved source stats before the target stat's own multiplier.

Implement the agreed additive/multiplicative layers as a compiled evaluation plan. Temporary modifiers invalidate downstream stats; do not simply adjust Strength after derived damage has already been finalized. A content rule that cannot be expressed consistently with the agreed ordering must fail validation until its semantics are clarified.

Target-dependent values are evaluated in an actor/target context and must not leak into an actor-global cache. HP-dependent values invalidate when relevant HP changes. A MaxHP change emits a transition intent resolved by the approved percentage-preserving HealthRebasePolicy; the evaluator itself never emits damage/heal/death side effects.

Debug queries return contribution traces without rerunning RNG or mutating state. Use the same evaluation for UI comparisons and combat. Offline farming rate does not depend on combat stats under the current design.

## 7. Effects and actions

```text
EffectDefinition: id, triggerKind, condition, targetSelector,
                  operations[], magnitude, proc, cooldown, duration,
                  stackPolicy, tags, eligibility, schemaVersion
EffectInstance: instanceId, definitionId, sourceActorId, sourceItemOrPowerId,
                ownerActorId, createdAt, expiresAt?, stacks/state, revision
ActionContext: rootActionId, actionId, parentActionId?, sourceId, targetIds,
               originKind, tags, ancestry, timestamp, rulesVersion
```

Stack policy supports independent instances, refresh, bounded stack count and explicit additive/multiplicative composition as specified by content. Store expiry per independently timed stack. Optimized compression must preserve expiry and provenance needed by mechanics.

Target selection returns an ordered actor-ID list. Geometry is not an input to ordinary targeting. Factions, source ownership and actor identity permit future summons without changing every hero-only function.

Separate effect operations from trigger eligibility: a damage operation is not automatically a basic attack. It carries direct/periodic/reactive/secondary origin and explicit capability flags. New Crit/DoT/AoE behavior must supply its approved eligibility policy rather than inherit every OnHit trigger accidentally.

## 8. Resolution pipeline

Use explicit stages: prepare intent, validate/replace/cancel, compute proposed result, apply fatal prevention, commit damage/healing, resolve reactions, resolve death/revive, resolve encounter transition. This is a technical transaction model; exact trigger placement follows the approved Combat rules and uses the resolved D03 transaction semantics from document 02.

Retain separate values for rolled damage, outgoing-modified damage, mitigated damage and actual HP lost. Reflect/healing handlers select an explicit basis in content; they cannot all consume an ambiguous field named `damage`.

Reaction processing uses the priority families already documented: replacement, fatal prevention, damage/healing, reactions, death/revive, encounter changes. Complete the causal action under its defined ordering before later unrelated simulation time. On observer notification, the action is already committed.

Loop safety:

- Tag every action with root, parent and origin.
- Each trigger declares allowed origins, re-entry policy and any per-root execution bound.
- Validate obvious direct trigger cycles in content.
- Keep an execution watchdog for unexpected loops. It reports the causal trace and halts the faulty transaction/session safely; it does not quietly discard legitimate damage and continue.
- A CPU work-budget yield is distinct from a loop-safety fault.

## 9. Encounter and reward invariants

Enemy death and encounter completion are separate facts. Completion is committed once per encounter ID under its completion policy. The encounter budget owns reward allocation; initial enemies have stable assigned shares, and summoned actors do not create new budget implicitly.

Record reward share claims atomically with the death/completion that grants them. Floating/discrete splitting and modifier timing follow approved economy rules; retain remainder explicitly if the selected economy uses integer rewards. Repeated farming creates a new encounter ID and a new ledger.

Death is not generic encounter failure. Use explicit alive/downed/reviving actor state and allow the existing encounter to continue. The resolved D06 policy pauses enemy attack clocks, Boss phase clocks, enemy timed effects and enemy regeneration while the hero is downed, unless content explicitly overrides it.

Boss behavior is a serializable state machine with state ID, entry time, local variables and scheduled transitions. Custom behavior implements the same snapshot/restore interface as authored state machines. No coroutine is the sole authority for a boss phase.

## 10. Commands, reset and future arena snapshots

Reset is an application transaction: validate, build a reset plan, capture the final build inputs needed by future arena support, resolve approved retention/unlocks, replace run/combat state, commit. A retry with the same operation ID cannot reset again or duplicate permanent rewards.

The snapshot is build data, not a Unity prefab and not the complete active PvE fight. It stores sufficient source inputs, content/rules versions and provenance for a future arena ruleset to instantiate a match. Its treatment of temporary effects and permanent modifiers is deferred design; do not activate arena snapshot interpretation with a guessed default.

The combat kernel accepts a ruleset/context so arena can later reuse stat/effect primitives without adopting PvE's endless revival, progression or reward policies by accident. No arena outcome rules are specified here.

## 11. Required contract tests

- Identical commands and seeds produce matching state across different advance partitions.
- Saving/restoring at every permitted safe boundary produces the same future result as uninterrupted execution.
- Rejected/stale commands do not mutate state or advance RNG.
- Equal-time actions preserve stable order after save/load.
- Individual stack expiry removes only that stack; dependency caches invalidate correctly.
- Multi-enemy kills never multiply an encounter's base reward budget.
- Loop watchdog identifies the source trace; yielding/resuming does not lose actions.
- RNG algorithm/state serialization matches pinned vectors.
- Large-number arithmetic and save round-trips cover at least `10^1000`, close-value subtraction, tiny deltas, rounding ties, equivalent canonical representations and exponent-overflow rejection.
- Unsupported or unresolved rule fields fail validation clearly.
- Every implemented optimized resolver is checked against the exact resolver on representative supported builds.
