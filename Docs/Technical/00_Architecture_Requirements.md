# 00 — Architecture Requirements & Future-Proofing

**Status:** Requirements v0.1  
**Date:** 2026-09-28

## Purpose

This document does **not** prescribe a concrete software architecture.

It defines what the game architecture must be able to support so implementation decisions do not accidentally block the intended design.

The implementation may use any reasonable structure as long as these capabilities remain possible.

Core principle:

> Build the first real game, not a throwaway prototype — but only generalize where the design already gives a real reason to generalize.

Avoid both extremes:

- hard-coding the first Location so tightly that future content requires rewrites;
- building a huge abstract framework for systems that may never exist.

---

# 1. Core simulation must be independent from presentation

The game must be able to resolve combat and progression without depending on:

- GameObjects;
- animation state;
- screen layout;
- VFX timing;
- frame rate;
- a specific scene being loaded.

Presentation should visualize results rather than define them.

This is important because the game will eventually need:

- offline progress;
- accelerated simulation;
- very high Attack Speed;
- repeated farming;
- batch resolution of trivial encounters;
- deterministic tests;
- possibly headless validation later.

A combat result should remain mathematically identical even if no animation is played.

---

# 2. Runtime state must have clear lifetimes

The architecture must distinguish at least three lifetimes.

## Persistent account state

Survives reset.

Examples:

- highest world progress;
- discovered Locations;
- unlocked Powers;
- Collections;
- achievements;
- mastery;
- permanent progression;
- future Prestige upgrades;
- gear-retention progression;
- starting-build options.

## Run state

Destroyed or rebuilt on reset.

Examples:

- current EXP;
- purchased STR / VIT / AGI / DEX;
- current Powers;
- current inventory;
- equipped gear;
- current run world progress;
- current-run Boss clears;
- temporary run modifiers.

## Encounter / combat state

Exists only while an encounter is active.

Examples:

- current HP;
- enemies currently alive;
- attack timers;
- temporary buffs/debuffs;
- timed stacks;
- cooldowns;
- Boss phase;
- Fortify break progress;
- temporary target state.

A value should have one authoritative lifetime.

Avoid state duplicated across several systems with unclear ownership.

---

# 3. Content must be data-driven

Adding ordinary content should not normally require new code.

The architecture should allow data definitions for at least:

- Locations;
- Stages;
- Encounters;
- enemy definitions;
- enemy archetypes;
- Boss definitions;
- Powers;
- items;
- affixes;
- loot tables;
- progression milestones;
- rewards;
- permanent unlocks.

The first Goblin Location must be content built on reusable systems, not a special-case level controller.

Future Location 2 should be possible primarily by authoring new data unless it introduces a genuinely new mechanic.

---

# 4. Stable IDs are mandatory

Every persistent or referencable content object should use a stable ID independent of display name.

Examples:

- Power ID;
- Item definition ID;
- Enemy ID;
- Location ID;
- Stage ID;
- Affix ID;
- Achievement ID;
- Collection ID;
- permanent upgrade ID.

Display names must be changeable without breaking saves.

Never use localized strings or object names as save keys.

---

# 5. Stat system must support composition

The current design already requires more than simple additive stats.

Architecture must support:

- flat additions;
- additive bonuses;
- multiplicative modifiers;
- derived stats;
- conversions from one stat into another;
- temporary modifiers;
- independent timed multiplicative stacks;
- conditional modifiers;
- target-dependent modifiers;
- enemy-stat modification.

Examples that must remain representable:

```
Strength ×10
Max Damage ×10
Block ×5
Min Damage += Accuracy ×0.20
Max Damage += Block ×0.50
Strength ×1.10 for 5 sec, independent stacks
Enemy Vitality ×0.75 for 3 sec
```

The agreed calculation order must remain enforceable:

1. primary flat/additive changes;
2. primary multipliers;
3. derived baseline formulas;
4. derived conversions/additions;
5. derived multipliers;
6. temporary combat modifiers.

Dependencies between derived stats must be explicit and acyclic by default.

Special mechanics may override the normal pipeline deliberately.

---

# 6. Do not encode builds or classes

The architecture must not assume formal classes such as Warrior, Rogue, Tank or Poisoner.

Build identity emerges from combinations of:

- stats;
- Powers;
- gear;
- affixes;
- temporary effects.

Future archetypes such as Crit/Bleed, Poison, Death/Revive, Block/Thorns or Summon should not require adding a new character-class system.

---

# 7. Combat must support variable enemy counts

Do not assume 1v1 combat.

The engine must support:

- one enemy;
- several enemies;
- 4–6 enemy swarms;
- Elite + adds;
- Boss + summons;
- future encounter shapes with more entities if needed.

Each enemy requires independent:

- HP;
- stats;
- attack timer;
- buffs/debuffs;
- target state;
- death state;
- source identity for combat events.

The number six is a presentation target, not an engine limit.

---

# 8. Targeting must be a rule, not hard-coded combat logic

Initial behavior:

> Attack the current primary / nearest valid enemy.

Future effects may require:

- lowest HP target;
- highest HP target;
- random target;
- enemy with a specific debuff;
- secondary targets;
- chained targets;
- all enemies;
- source enemy that hit the hero.

Target selection should be replaceable or parameterized by effects.

Manual player targeting is not currently required.

---

# 9. Combat event pipeline is a foundational extension point

The architecture must expose reusable combat events.

Current required families include:

## Offensive

- OnAttack;
- OnHit;
- OnMiss;
- OnCrit;
- OnKill;
- OnDamageDealt;
- OnDoTApplied;
- OnDoTTick;
- OnEnemyDeath.

## Defensive

- OnIncomingAttack;
- OnHitTaken;
- OnBlock;
- OnEvade;
- OnDamageTaken;
- OnLowHP;
- OnShieldBreak.

## State

- OnBuffGain;
- OnBuffExpire;
- OnDebuffApplied;
- OnStackGain;
- OnStackMaxed;
- OnDeath;
- OnRevive.

## Encounter

- OnEncounterStart;
- OnEnemySpawn;
- OnEnemyCountChanged;
- OnBossSpawn;
- OnBossPhaseChanged;
- OnEncounterWin.

Powers and gear should consume these hooks rather than force special-case checks into the main combat loop.

---

# 10. Effects should be composable

Most Powers and gear effects should be representable as combinations of reusable concepts:

- trigger;
- condition;
- target selector;
- effect;
- magnitude/scaling;
- duration;
- stack rule;
- proc chance;
- cooldown.

Examples of reusable effects:

- modify stat;
- deal damage;
- heal;
- apply buff;
- apply debuff;
- stun/delay;
- redirect attack;
- repeat/extra attack;
- ignore Block;
- revive;
- spawn enemy;
- add/remove stacks;
- modify reward.

Some Powers will always need custom logic.

The goal is not “zero custom code.”

The goal is:

> common mechanics should compose without every Power becoming a unique subsystem.

---

# 11. Effect resolution must be ordered and loop-safe

The combat system must preserve the agreed resolution priorities:

1. replacement / cancellation;
2. fatal prevention;
3. damage / healing;
4. reactions;
5. death / revive;
6. encounter-state changes.

Within equal priority, use stable deterministic ordering.

The architecture must protect against infinite reaction loops.

Examples:

- reflect triggering reflect;
- counterattack triggering counterattack forever;
- death effect causing death effect recursion;
- spawned event repeatedly reproducing itself.

Possible protections may include:

- effect/source ancestry;
- recursion depth limits;
- non-recursive flags;
- cooldowns;
- per-event execution guards.

The exact implementation is open, but loop safety is mandatory.

---

# 12. Simultaneous events need deterministic ordering

Several enemies can attack on the same simulation step.

The result must not depend unpredictably on frame timing.

A stable ordering must exist for:

- simultaneous attacks;
- simultaneous deaths;
- multiple OnHit effects;
- simultaneous expirations;
- Boss phase transitions.

This is important for:

- debugging;
- reproducible tests;
- save/load consistency;
- future replay or deterministic simulation.

---

# 13. RNG must be controllable

Randomness exists in:

- hit chance;
- damage rolls;
- Power offers;
- proc chances;
- loot;
- rarity;
- affixes;
- future encounters.

The architecture should allow RNG to be:

- seeded;
- replaced in tests;
- separated by domain where useful;
- logged/reproduced when debugging.

Do not scatter direct calls to global random APIs throughout gameplay logic.

Perfect multiplayer lockstep is not required, but reproducible simulation should be possible.

---

# 14. Time must be abstractable

Do not make combat rules depend directly on Unity frame delta everywhere.

The simulation must support:

- normal real-time play;
- pause;
- speed-up;
- large simulation steps where safe;
- offline calculation;
- aggregated high-frequency attacks.

Timed mechanics include:

- attack timers;
- buff durations;
- debuff durations;
- cooldowns;
- Boss phases;
- revive timing.

Changing visual frame rate must not change gameplay results.

---

# 15. Extreme Attack Speed must not break the game

The project deliberately has no gameplay hard cap on Attack Speed.

Eventually builds may produce attack rates that are unreasonable to animate or resolve one hit at a time.

The architecture should allow future aggregation such as:

- batched attacks;
- mathematically equivalent damage bundles;
- grouped proc evaluation where valid;
- reduced visual event frequency while preserving simulation output.

Do not require one animation or one visual event for every mathematical attack.

---

# 16. Death is not encounter failure

This must remain a structural assumption.

When the hero dies:

- the encounter stays active;
- enemy HP remains;
- temporary combat state is cleared according to rules;
- hero enters downed state;
- hero regenerates;
- hero revives;
- the same encounter continues.

Therefore the architecture must not equate:

```
HeroDead == EncounterEnded
```

Bosses and future mechanics may create special exceptions, but normal combat does not.

---

# 17. Encounter completion is separate from enemy kills

Stage progression counts completed encounters.

This must remain separate from:

- number of enemies killed;
- enemies spawned;
- summons killed;
- individual kill rewards.

A six-enemy swarm still produces one encounter completion.

This distinction is essential for:

- multi-enemy encounters;
- summons;
- Stage Compression;
- future Overkill;
- encounter-level EXP budgets.

---

# 18. Encounter reward budget is authoritative

For the first implementation, EXP belongs to the encounter rather than scaling automatically with visible enemy count.

The architecture must support an encounter-owned reward budget.

Enemies may receive portions of that budget, but:

```
sum(enemy EXP rewards) = encounter EXP budget
```

unless a mechanic explicitly adds additional reward.

This prevents summons and swarm size from accidentally duplicating economy rewards.

The same concept may later be useful for:

- gold;
- Collection progress;
- drop rolls;
- event rewards.

Do not assume every reward is always attached directly to an individual enemy.

---

# 19. World progression must remain data-driven

Current structure:

```
World → Location → Stage → Encounter
```

But future content may vary:

- different Stage counts;
- different encounter requirements;
- special Boss-only Locations;
- branching maps;
- optional farming Locations;
- challenge Locations;
- temporary events.

Do not hard-code:

```
every Location has exactly 10 Stages
every Stage has exactly 10 encounters
```

Those are current content values, not universal engine rules.

---

# 20. Auto Push and farming are first-class systems

The architecture must support:

## Auto Push ON

Automatically advance when progression requirements are satisfied.

## Auto Push OFF

Stay on the selected Stage indefinitely for farming.

The player must also be able to manually select accessible farming content.

This is part of the permanent core loop, not a temporary tutorial feature.

---

# 21. Stage Compression must not rewrite Location data

Permanent progression can change:

```
RequiredEncounters =
max(1, BaseRequiredEncounters - PermanentEncounterReduction)
```

The original Stage definition should remain unchanged.

Static content and account-specific effective rules must remain separable.

Future modifiers may also affect:

- start point;
- encounter count;
- skip rules;
- Overkill progression.

---

# 22. Bosses need extensible phase/state behavior

The first Boss uses:

```
Normal
→ Fortify
→ Exposed
→ Normal
```

Future Bosses may require:

- summons;
- phase thresholds;
- changing stat profiles;
- shields;
- conditional regeneration;
- target rules;
- enrage-like mechanics;
- interruptible states;
- death/revive interactions.

Do not make the Goblin Chieftain cycle a unique hard-coded combat-controller branch.

Boss-specific mechanics may still use authored custom behavior, but combat should expose reusable state-transition hooks.

---

# 23. DoT architecture must be possible even though it is not in Location 1

Future systems explicitly include:

- Bleed;
- Poison;
- other Damage over Time.

The architecture should be capable of representing:

- source;
- target;
- duration;
- tick frequency;
- stack rule;
- magnitude;
- snapshot vs dynamic scaling;
- dispel/expiration;
- OnDoTApplied;
- OnDoTTick.

Current default design:

- DoT does not Crit;
- specific Powers may override this later.

Do not bake “all damage is a direct attack” into the combat model.

---

# 24. AoE must be systemic, not positional

Future effects may:

- hit N additional enemies;
- hit all enemies;
- chain;
- splash;
- explode on death;
- select random secondary targets.

Combat should not require spatial hitboxes or positions to resolve these mechanics.

Visual positions are presentation.

Mechanical targeting is rule-based.

---

# 25. Summons must remain possible without redesign

Summon builds are a planned long-term direction.

Architecture does not need a complete summon system now, but it should avoid assumptions such as:

- only one friendly combatant can ever exist;
- all damage originates from the hero;
- all buffs target only the hero;
- enemy targeting always points directly to the hero.

Potential future entities include:

- permanent summon;
- temporary summon;
- turret;
- clone;
- pet-like combat entity.

Do not implement them until needed, but avoid closing the door.

---

# 26. Power ownership and Power availability are separate

The game distinguishes:

- Power unlocked in PowerDex permanently;
- Power owned in the current run;
- Power offered in the current choice;
- future starting/portable Powers.

These must not collapse into one boolean.

Future RNG-control systems may need:

- rerolls;
- banish;
- weighting;
- prerequisites;
- exclusions;
- starting Powers;
- portable Powers;
- additional offered choices.

The Power-selection system should be able to evolve without altering combat Power ownership.

---

# 27. Unlock conditions should be generic

Power/item/system unlocks may eventually come from:

- World Progress;
- Boss kills;
- Collections;
- achievements;
- stat milestones;
- challenges;
- Prestige;
- quests;
- Mastery.

Avoid embedding unlock logic directly inside individual content definitions as arbitrary code where a reusable condition system is sufficient.

Useful condition primitives may include:

- reached progress;
- killed enemy;
- completed Boss;
- stat threshold;
- count threshold;
- owns unlock;
- completed challenge.

Complex special cases can still exist.

---

# 28. Gear must allow both procedural and authored items

The architecture must support:

## Procedural items

Defined by:

- slot;
- Item Level;
- rarity;
- affixes;
- affix values.

## Named items

Defined by:

- stable item identity;
- source;
- unique effect;
- optional fixed stats;
- optional future procedural components.

Do not force named items into a procedural-only model.

Do not force procedural gear to require unique content definitions for every roll.

---

# 29. Affixes should be able to reuse gameplay effects

Current first-Location affixes are mostly stats.

Future gear may include:

- Crit applies Bleed;
- reflect can Crit;
- Poison spreads;
- On Evade gain Attack Speed;
- retain stacks after death;
- trigger an effect on Boss phase change.

Gear effects should eventually be able to reuse the same effect/trigger infrastructure as Powers where practical.

Avoid building one isolated modifier system for Powers and an incompatible second system for gear without a strong reason.

---

# 30. Inventory and equipment must not assume current UX limits

Open design questions remain:

- inventory capacity;
- overflow;
- auto-sell;
- salvage;
- filters;
- gear retention;
- multiple loadouts.

At minimum distinguish:

- item definition;
- rolled item instance;
- equipped state;
- lock/favorite state;
- ownership;
- retention/protection state.

---

# 31. Saves must be versioned and migratable

A player may keep one save through many updates.

Save architecture must support:

- explicit save version;
- migration from older versions;
- newly added fields with defaults;
- removed/deprecated content;
- renamed display text without ID changes;
- balance changes without corrupting state.

Avoid serializing arbitrary runtime object graphs as the only save format.

Save data should represent durable game state, not transient implementation details.

---

# 32. Missing or deprecated content must fail safely

During development and live updates:

- an item may be removed;
- a Power ID may change;
- a Location may change;
- an affix may become invalid.

The game should have defined behavior for unknown IDs rather than crashing the save.

Possible strategies:

- safe placeholder;
- migration;
- remove-and-compensate;
- development validation error.

Exact policy can be chosen later.

---

# 33. Offline progression should reuse gameplay rules where possible

Offline progress should not become a completely separate balance game.

The architecture should aim to reuse the same:

- EXP curves;
- loot tables;
- Stage definitions;
- item generation;
- Collection rules;
- permanent modifiers.

Offline simulation does **not** need to simulate every attack one by one.

Future offline logic may use aggregate models for content the player has already configured to farm.

Offline must not automatically make strategic choices such as:

- choosing Powers;
- changing gear;
- selecting a new Location;
- changing stat allocation;
- defeating important new progression Bosses by default.

---

# 34. Simulation and presentation speeds must be decoupled

Future progression may include:

- game speed multipliers;
- very fast farming;
- instant clear of obsolete content;
- Overkill;
- offline catch-up.

Architecture should allow:

```
simulation throughput != animation throughput
```

For example, the simulation may resolve many attacks while presentation shows only representative impacts and aggregate damage.

---

# 35. Content validation tools will become necessary

As content grows, manual inspection will not be enough.

The project should eventually be able to validate data automatically.

Useful validations:

- duplicate IDs;
- missing referenced IDs;
- invalid Power prerequisites;
- circular stat dependencies;
- impossible loot probabilities;
- rarity probabilities not summing correctly;
- invalid Stage references;
- missing enemy definitions;
- Boss state with no exit;
- affix unsupported on a stat type;
- negative durations/cooldowns;
- inaccessible progression milestone.

This does not need to exist on day one, but data structures should make validation possible.

---

# 36. Headless balance tests should be possible

The game will eventually need automated simulation of questions such as:

- How long does this build take to clear Stage 8?
- Which starter Power reaches the Boss fastest?
- How many times does the hero die against this Boss?
- What is the effective proc rate with six enemies?
- Does this Power create an infinite loop?
- How much EXP is earned over a full first Location?
- What percentage of runs receive an Epic item before the Boss?

Architecture should make it possible to create a run/build in code, simulate it and inspect results without opening the gameplay UI.

This will be one of the highest-value tools for balancing a combinatorial idle game.

---

# 37. Debug visibility is important

During development it should be possible to inspect:

- final stat values;
- contributing stat modifiers;
- current buffs/debuffs;
- trigger history;
- damage calculation;
- RNG roll;
- target choice;
- loot roll;
- Boss phase transitions;
- why a Power did or did not trigger.

A buildcraft game becomes extremely difficult to debug if the final value is visible but its derivation is not.

Consider making stat/effect calculations explainable in development builds.

---

# 38. Analytics hooks should remain optional observers

Future tuning will benefit from knowing:

- where players stop pushing;
- which starter Power they choose;
- stat-spending distributions;
- Stage clear times;
- death frequency;
- Boss clear time;
- which Powers coexist;
- item rarity obtained;
- which Stages players farm;
- reset timing.

Core gameplay should emit meaningful domain events that analytics can observe.

Gameplay logic should not depend on analytics being available.

---

# 39. Balance data must be adjustable without structural rewrites

Likely-to-change values include:

- EXP curves;
- stat-upgrade cost curves;
- enemy scaling;
- Power numbers;
- proc chance;
- duration;
- drop chance;
- rarity distribution;
- affix magnitude;
- Boss numbers;
- Stage encounter counts.

Keep balance coefficients separate from algorithmic code wherever practical.

The goal is to be able to tune:

```
16 Fortify hits → 14
20% gear drop → 18%
Strength ×10 → ×8
```

without changing implementation structure.

---

# 40. Do not assume numeric values stay small

The reference direction intentionally supports very large multiplicative builds.

Eventually values may grow by orders of magnitude.

Avoid architectural assumptions that:

- damage fits visually into a small integer;
- stats stay below 1000;
- multipliers stay near 1;
- only a few timed stacks exist.

Exact transition to a large-number representation is not required now, but serialization, formatting and calculation code should not make that future change unnecessarily painful.

---

# 41. Floating-point comparison rules need discipline

The game uses timers, multipliers, logarithmic Attack Speed, probabilities, durations and derived stats.

Avoid equality-based gameplay assumptions such as:

```
timer == 0
HP == threshold
progress == exactFloat
```

Use explicit threshold rules and stable rounding where gameplay requires discrete values.

Important progression counters should remain integer-based where possible.

---

# 42. Content should be taggable

Tags will help avoid hard dependencies later.

Potential tags:

- Goblin;
- Armored;
- Boss;
- Elite;
- Swarm;
- DoT;
- Poison;
- Bleed;
- Reflect;
- Summon;
- DirectAttack;
- Reactive;
- GearSource.

Future systems may query tags for:

- Collections;
- achievements;
- Powers;
- loot;
- challenges;
- UI filters;
- conditional bonuses.

Do not rely only on concrete class/type checks.

---

# 43. Collections should be able to observe gameplay, not own it

Collections are intentionally postponed, but architecture should allow them to consume events such as:

- enemy killed;
- encounter completed;
- item obtained;
- Boss defeated;
- damage type used;
- stat threshold reached.

Collections should generally reward existing gameplay rather than requiring combat systems to know about each Collection.

This keeps future Collections additive instead of invasive.

---

# 44. Prestige should modify rules through explicit modifiers

Prestige is postponed.

Likely future effects include:

- Stage Compression;
- later starting point;
- more offered Powers;
- retained gear;
- starting Powers;
- stat-upgrade value;
- EXP gain;
- offline efficiency.

Prefer:

```
base rule
+ account modifiers
→ effective rule
```

rather than rewriting content definitions when Prestige is introduced.

---

# 45. Monetization must remain outside core balance authority

Ads/IAP are not yet designed.

Architecture should allow future reward delivery, but core systems should not require monetization services to function.

A missing ad SDK, store SDK or network connection must not break core progression.

No Energy system is a hard design constraint.

---

# 46. UI should request actions, not own authoritative state

UI will eventually include:

- combat;
- stats;
- Power choice;
- inventory;
- gear comparison;
- world map;
- farming target;
- Collections;
- Prestige.

Closing a screen or rebuilding a view must not alter authoritative gameplay state accidentally.

UI may request actions such as:

```
BuyStatUpgrade
EquipItem
ChoosePower
SelectStage
ToggleAutoPush
```

but underlying game state should exist independently of the visual widget.

---

# 47. The first Location is the architecture acceptance test

Before expanding content significantly, the implementation should successfully support the complete first Location loop:

1. start at 1/1/1/1;
2. auto-fight;
3. gain EXP;
4. spend EXP on four stats;
5. receive first Power choice;
6. progress through Stages;
7. fight multiple enemies simultaneously;
8. unlock additional Powers in PowerDex;
9. receive later Power choices;
10. drop procedural gear;
11. equip and compare gear;
12. roll rarity/affixes;
13. farm Stage 8;
14. obtain Hobgoblin Bulwark;
15. switch Auto Push off/on;
16. fight the Goblin Chieftain;
17. resolve Fortify / Break / Exposed;
18. die and revive without resetting Boss HP;
19. complete the Location;
20. save/load the state correctly.

If these work through reusable systems rather than Location-specific exceptions, the architecture is ready for the next layer of content.

---

# 48. Future expansion directions the architecture should not block

These are **not MVP requirements**, but they are established design directions.

## Combat

- Crit;
- Bleed;
- Poison;
- other DoTs;
- AoE;
- Chain;
- explosions;
- shields;
- lifesteal;
- crowd control;
- enemy healing;
- summons;
- Boss adds;
- multiple Boss phases;
- enemy stat manipulation;
- Death/Revive builds;
- intentional Miss/Evasion builds;
- enemy-count scaling.

## Powers

- large Power libraries;
- prerequisites;
- soft weighting;
- reroll;
- banish;
- additional offered choices;
- Portable / starting Powers;
- Power Mastery;
- rare rule-breaking Powers.

## Gear

- named items;
- conditional affixes;
- triggered effects;
- build conversions;
- gear retention;
- item locking;
- auto-sell/salvage;
- source discovery;
- target farming;
- possible crafting/upgrading later.

## World

- many Locations;
- old-content farming;
- optional routes;
- location-specific loot;
- Collections;
- Challenges;
- alternate Bosses;
- special encounters;
- events.

## Progression

- Prestige / Time Travel analogue;
- later starting points;
- Stage Compression;
- Overkill;
- persistent Mastery;
- permanent stat-upgrade multipliers;
- more build control;
- offline progression.

## Operations

- save migrations;
- analytics;
- balance patches;
- content additions;
- localization;
- live events if ever needed.

---

# 49. Things that should explicitly NOT be generalized yet

Avoid building full frameworks for systems that remain speculative.

Do not prematurely implement:

- PvP;
- multiplayer/network synchronization;
- guilds;
- gacha;
- pets;
- crafting;
- procedural world generation;
- complex quest graphs;
- server-authoritative combat;
- positional/grid combat;
- skill trees;
- character roster systems.

Architecture should avoid making them impossible, but there is no requirement to prepare dedicated infrastructure for them.

---

# 50. Architecture review checklist

Before committing to a major implementation direction, ask:

1. Can this system run without Unity presentation?
2. Is its state lifetime clear: account, run or encounter?
3. Can new content be added mostly through data?
4. Are persistent objects referenced by stable IDs?
5. Can Stats handle conversions, multipliers and timed stacks?
6. Can Powers and gear react to reusable combat events?
7. Can multiple enemies resolve independently?
8. Can event ordering be deterministic?
9. Can RNG be seeded for tests?
10. Can time be accelerated/aggregated?
11. Can death happen without ending the encounter?
12. Can encounter rewards exist independently of enemy count?
13. Can Stage counts and encounter requirements vary by content?
14. Can permanent progression modify effective rules without rewriting base content?
15. Can save data be migrated later?
16. Can the simulation be tested headlessly?
17. Can balance constants change without structural rewrites?
18. Can future Crit/DoT/AoE/Summon systems fit the existing combat model?
19. Can Collections observe gameplay without combat knowing their details?
20. Can the full first Location be implemented without a growing list of hard-coded special cases?

If several answers are “no,” the architecture is probably becoming too tightly coupled to current content.

---

## Final principle

The project should optimize for:

> **Reusable gameplay primitives + data-authored content + explicit state lifetimes.**

Not every future feature needs to be implemented now.

The architecture only needs to ensure that the known future design space can be added by extending existing systems rather than replacing the foundation.
