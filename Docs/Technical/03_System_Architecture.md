# 03 — System architecture

**Status:** technical specification v0.2, 2026-09-28. Not implemented.  
**Scope:** the whole documented game, implemented incrementally. No new gameplay decisions.

## 1. Authority and reading order

`00_Architecture_Requirements.md` defines required capabilities. `02_Architecture_Decisions.md` records owner constraints and unresolved design dependencies. This document and 04–06 specify a concrete technical foundation.

Technical choices in 03–06 are implementation defaults, not claims that the owner selected individual libraries or algorithms. Existing design documents remain authoritative for gameplay. A conflicting or missing rule must be reported against the dependency register; architecture must not silently supply a gameplay answer. Future features need contracts now, not complete implementations.

An implementer reads this document, the relevant contract document, then only the design sources needed by the implementation increment. Use `06_Implementation_and_Verification.md` for order and acceptance.

## 2. Technical baseline

- Unity version: **6000.6.0f1**, verified in `ProjectSettings/ProjectVersion.txt`.
- Targets: Android and iOS, portrait. Distant Web support is a portability constraint, not a current build target.
- Existing project includes URP, uGUI, Input System and Unity Test Framework. Keep current package versions unless a specific implementation need justifies a change.
- Gameplay: plain C#, without references to UnityEngine, UnityEditor, scenes, global Unity RNG or frame time.
- Presentation: uGUI screens and ordinary 2D views using the existing project stack. Presentation technology must not affect simulation contracts.
- Composition: explicit constructors and one Unity composition root. No mandatory DI framework, global service locator, ECS conversion or third-party reactive framework.
- Execution: one authoritative session writer. Start on the Unity main thread with bounded work per frame; worker/server execution is possible behind the same plain-C# contracts.
- All current computation is client-side. Implement no server, account service or remote simulation as a prerequisite; retain the interfaces for later integrations.
- Large-number support starts in the foundation; use the numeric contract in document 04 rather than a temporary double-only model.
- Serialization: explicit versioned DTOs with a codec boundary. Use a Unity-compatible JSON codec for the initial local implementation, selected and pinned during the persistence increment. Do not serialize domain instances or enable arbitrary CLR type metadata.

## 3. Assembly and folder layout

Under `Unity Project [Idle build master]/IdleRPGBuildMaster/Assets/Project/`:

| Folder / assembly | Responsibility | Allowed project dependencies |
|---|---|---|
| `Scripts/Domain` / `IBM.Domain` | State, identifiers, value types, combat, progression, item generation and effects | None |
| `Scripts/Application` / `IBM.Application` | Session coordinator, commands, transactions, snapshots, use cases and external-service interfaces | Domain |
| `Scripts/Infrastructure` / `IBM.Infrastructure` | Storage, codecs, platform clock, bundled content loading, future network adapters | Domain, Application |
| `Scripts/Presentation` / `IBM.Presentation` | UI presenters, view models, combat views, input routing | Domain read models, Application |
| `Scripts/Bootstrap` / `IBM.Bootstrap` | Construct and connect concrete adapters; application lifecycle bridge | All runtime assemblies |
| `Editor` / `IBM.Editor` | Content authoring, compiler, validation, development tools | Runtime assemblies as needed; editor-only |
| `Tests/EditMode` / `IBM.Tests.EditMode` | Domain/application/serialization tests | Assemblies under test, test framework |
| `Tests/PlayMode` / `IBM.Tests.PlayMode` | Scene/lifecycle/view integration | Runtime assemblies, test framework |
| `Content/Authoring` | ScriptableObject authoring assets | Editor compilation input |
| `Content/Generated` | Immutable bundled catalog and manifest | Build artifacts committed only under the agreed content workflow |
| `Scenes`, `Prefabs`, `Art`, `Audio` | Presentation assets | No authoritative gameplay state |

Set Domain and Application assembly definitions to disallow engine references. Application DTOs and ports must remain plain C#. Use narrow types, not a general-purpose `Common` assembly. Do not create empty assemblies for every future system.

Dependency direction is inward: Unity and network adapters call Application; Application invokes Domain. Domain never calls a UI, disk, HTTP client or SDK. Domain events used by combat are separate from outward notifications used by UI, analytics and persistence.

## 4. Authoritative state and ownership

`GameSession` is the application-level owner of a `GameState` aggregate. Only its command executor and simulation advance operation mutate that state. Views receive read-only projections; content definitions are immutable.

| State | Owner and lifetime | Minimum contents |
|---|---|---|
| `AccountState` | Account; survives reset | Stable local profile ID, discoveries, unlocks, highest progress, collections, achievements, mastery, permanent modifiers, reset progression |
| `RunState` | Current run; rebuilt by reset | Run ID, EXP/wallets, stat purchases, Powers, inventory/equipment, run route, boss clears, selected stage, Auto Push, pending offers and claimed milestones |
| `CombatState` | Active combat context | Entities, HP, action clocks, effects, cooldowns, targets, boss behavior, event/instance counters, reward allocation ledger |
| `HeroCombatContinuityState` | Combat state with explicit transition policy | Hero effects/resources that may span encounters; not implicitly destroyed with enemy entities |
| `EncounterState` | One encounter inside combat context | Instance ID, initial/spawned enemies, completion status, encounter reward budget, claimed shares |
| `ArenaBuildSnapshot` | Captured before run reset; future feature | Immutable build inputs and provenance; independent of current run and of full PvE combat state |
| `SessionMetadata` | Durable technical state | State revision, content/rules versions, RNG states, monotonic ID sequences, last committed operation, offline accounting cursor |
| `ViewState` | Screen lifetime; disposable | Selection, scroll, expanded panels, presentation interpolation and VFX |

Each value has one authority. Final stats are derived caches, not competing saved truth. Cache keys include relevant content/rules version, modifier revision, actor and contextual target. On load, rebuild and verify caches.

The distinction between combat context and encounter prevents accidental destruction of possible cross-encounter effects. A `CombatTransitionPolicy` declares what is retained at each transition; its gameplay values depend on the design source and are not chosen here.

## 5. Session and command boundary

Application entry points:

```csharp
CommandResult Execute(GameCommand command);
AdvanceResult AdvanceTo(SimTime target, WorkBudget budget);
SessionView ReadView();
SaveSnapshot CaptureAtSafePoint();
```

Commands express intent: `BuyStatUpgrade`, `EquipItem`, `SetItemLock`, `ChoosePower`, `SelectStage`, `SetAutoPush`, `RequestReset`, `ClaimOfflineReward`. Do not use client-supplied results such as `SetExp` or `SetBossCleared` in the public command surface.

The envelope contains a command ID and expected state revision. Validation checks references, current state, affordability/eligibility and authority mode before mutation. Outcomes are `Applied`, `AlreadyApplied`, `Rejected` or `PendingExternalCommit`, with a stable reason code and resulting revision. Invalid commands leave state and RNG unchanged.

Commands run at safe simulation boundaries. Commit the state change, generated rewards and persistent markers together; publish observer notifications only afterward. Observers cannot synchronously re-enter the writer. A future network authority can accept the same intent without accepting the local result as truth.

Do not imply that every command requires a fresh HTTP request in connected mode: batching and acknowledgements belong to the later validated protocol.

## 6. Content model and authoring

Use ScriptableObjects for Unity authoring convenience. Compile them to plain immutable definitions plus a manifest. Domain consumes `IContentCatalog`, never ScriptableObject references. The same catalog schema can later be loaded from a validated server bundle.

Identifiers are immutable strings with explicit types at API boundaries (`PowerId`, `EnemyId`, etc.). Existing P001–P025 and I001 can remain stable identifiers; do not rename them merely to adopt a style. New IDs must be unique within a documented namespace. Use neither display names nor Unity asset GUIDs as save keys.

Runtime instance IDs are different from definition IDs. Allocate actor/effect/item instances from durable scoped counters or deterministic identifiers; do not call wall-clock/random GUID generation during combat. External operation IDs may use independent UUIDs outside gameplay RNG.

| Definition | Required structural fields |
|---|---|
| Location / Stage | ID, ordered route references, encounter references, base encounter requirement, unlock/reward/milestone references |
| Encounter | ID, spawn specification, composition, total reward budget, completion policy ID, tags |
| Enemy / Boss | ID, stat profile, tags, effect references, behavior definition, presentation ID |
| Power | ID, localization keys, unlock reference, effect definitions, tags, mastery-eligible fields |
| Item / Affix | ID, slots, procedural or named structure, stat/effect references, generation constraints |
| Loot table | ID, roll groups, weights/probabilities with explicit semantics, source/eligibility predicates |
| Milestone / Unlock | ID, condition definition, reward specification, claim scope |
| Permanent upgrade | ID, eligibility/cost references, effective-rule modifiers |

Definition schema and content balance version are distinct. Catalog compilation rejects duplicate/missing IDs, unsupported effect kinds, invalid numbers, cycles and incompatible rules versions. Every build produces a deterministic content hash from canonical ordered data. Asset hashes for art may be separate from mechanical content hashes.

Ordinary balance edits and compositions use data. A genuinely new mechanic adds an explicit code handler and an effect schema version; remote data cannot invoke arbitrary classes or script code. Handler registration uses a bounded explicit registry compatible with ahead-of-time compilation.

## 7. System boundaries for the whole game

| Module | Owns behavior | Integration / extension contract |
|---|---|---|
| Stats | Derived values and explanations | Modifier graph, contextual evaluation, supported numeric operations |
| Combat | Action schedule and resolution | Target selector, effect handlers, damage intents, combat rule set |
| World progression | Route eligibility and encounter completion | Stage policy and permanent rule modifiers; do not mutate static stage data |
| Rewards | Authoritative grants and duplicate prevention | Typed reward specification, claim scope, durable operation identity |
| Powers | Availability, ownership and offers | Separate unlock catalog, run ownership and stored offer instances |
| Gear | Item generation, ownership and equipment | Affix/effect reuse; item instance distinct from definition and equipped slot |
| Collections / achievements | Persistent counters and claims | Consume committed gameplay facts; issue reward intents through Rewards |
| Prestige / reset | Validated reset transaction | Capture arena build, resolve approved retention, rebuild run, apply permanent changes |
| Mastery / challenges | Persistent progression and eligibility | Condition evaluators and explicit rule overrides; no global combat branches |
| Offline farming | Analytical reward plan | Pure game-design-supplied efficiency formula plus existing reward/content rules; no combat simulation or strategic actions |
| Arena | Future isolated matches | Immutable build snapshots, match rule set, server authority boundary |
| Monetization | Future verified reward entitlement | Service adapter submits validated entitlement; never directly edits balances |
| Analytics | Observation | Committed event projection; unavailable analytics cannot affect gameplay |

A reward transaction can affect several lifetimes. One application coordinator commits it; modules do not each serialize partial account state independently.

## 8. Expansion compatibility

| Documented expansion | Foundation to provide | Deliberately deferred |
|---|---|---|
| Crit, DoT, Bleed, Poison | Damage classification, origin, tick scheduling, effect stacks, eligibility flags | Specific coefficients, tick/proc/stack rules |
| AoE, chain, explosions | Ordered target selection, parent action identity, explicit proc eligibility | Secondary-hit policy and authored content |
| Summons | Actor identity, faction, owner/source identity; collections of friendly actors | Lifetimes, control and reward rules |
| Shields, lifesteal, revive | Damage/prevention/heal intents; effect hooks | Gameplay values and exact feature policies |
| Boss phases and adds | Serializable behavior state, transitions, spawn intents | New boss content |
| Portable Powers, reroll, banish | Offer state independent from ownership; reward and selection policies | Costs, timing and allowed pool rules |
| Collections, mastery, challenges | Persistent counters, generic conditions and rewards | Acquisition conditions and progression curves |
| Retained gear, loadouts | Stable item instances, retention separate from lock/equipped state | Capacity, retention and inventory policies |
| Stage Compression, Overkill | Effective stage rules; completion distinct from kills | New advancement algorithms and proof of equivalent rewards |
| Remote content / events | Catalog manifest, validation, versioned loader and activation boundary | Backend, admin UI, rollout operations |
| Async PvP | Build snapshot and separate match context | Arena rules, matchmaking, ranking and economy |
| Web | Engine-free core; storage/network/time interfaces | Web adapters, browser lifecycle, build and testing |

## 9. Unity composition and presentation

Use a bootstrap scene with one persistent `GameRoot`. It loads validated content, restores or creates a session, constructs adapters, then opens gameplay. Scene/view reconstruction must not create a second session or reset simulation. Dispose subscriptions when views close.

Presenters translate user input into commands and read committed projections. Combat views map actor IDs to pooled visual objects. Presentation notifications may be sampled/coalesced for high attack rates; authoritative events, rewards and proc evaluation may not be dropped.

Use explicit presentation references keyed by content ID, and resolve missing art to development placeholders without changing mechanical content. Load/unload art by location and pool actors, hit indicators and VFX. Avoid per-hit disk writes, UI hierarchy rebuilds and allocations proportional to total lifetime event count.

UI receives stat explanations and item comparison deltas from domain queries. It must not implement a second stat formula or universal Gear Score.

## 10. Completion of this architecture layer

The architecture is complete enough to implement the foundation when assembly boundaries, state ownership, content compilation, command processing and snapshot contracts are testable independently from gameplay presentation. A future feature with missing gameplay rules remains disabled and explicitly marked; it does not justify inventing those rules or blocking unrelated foundation work.
