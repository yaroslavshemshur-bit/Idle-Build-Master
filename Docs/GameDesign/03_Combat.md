# 03 — Combat

**Status:** Draft v0.5  
**Date:** 2026-09-28

## Combat philosophy

Combat is fully automatic.

The player does not win through manual execution or targeting. The player wins by:

- choosing build direction;
- selecting Powers;
- choosing gear;
- deciding where to farm;
- solving combat checks through synergies.

Combat should be mechanically simple at the surface while exposing many hooks for buildcraft.

The main reference for the starting combat model is **Idle Superpowers**, adapted for:

- multiple simultaneous enemies;
- Crit;
- DoT archetypes;
- more explicit AoE mechanics.

## Base attributes

The starting combat model uses four primary attributes.

### Strength

Primary roles:

- increases Max Damage;
- increases Block.

### Vitality

Primary roles:

- increases Max HP;
- increases Regeneration;
- may contribute partially to Block.

### Agility

Primary roles:

- increases Attack Speed;
- increases Evasion.

### Dexterity

Primary roles:

- increases Min Damage;
- increases Accuracy.

The first implementation uses the Idle Superpowers formulas as the baseline.

At the beginning of a fresh run, before EXP purchases, the hero starts at:

```
Strength = 1
Vitality = 1
Agility = 1
Dexterity = 1
```

Starting at 1 rather than 0 keeps the first encounters functional and ensures multiplicative Powers such as Strength ×10 have immediate value.

The important structural rule is that each primary attribute affects more than one meaningful combat outcome.

## Baseline combat formulas

These formulas are implementation defaults, not merely examples.

They are inherited from Idle Superpowers unless explicitly marked as a project deviation.

### Min Damage

```
MinDamage = 5 + Dexterity / 2
```

### Max Damage

```
MaxDamage = MinDamage + 2 * Strength
```

A normal direct hit rolls damage between Min Damage and Max Damage.

### Max Health

```
MaxHealth = 20 + 100 * Vitality
```

### Block

Raw Block rating:

```
Block = Strength / 10 + Vitality / 2
```

Damage multiplier after Block:

```
DamageTakenMultiplier = 100 / (Block + 100)
```

Equivalent damage reduction:

```
DamageReduction = Block / (Block + 100)
```

If an effect ignores part of Block, reduce the target's effective Block before applying the formula.

Multiple independent Block-ignore effects multiply the remaining Block rather than adding ignored percentages.

Example:

- ignore 50% Block;
- then ignore 70% of remaining Block;
- effective Block = original Block × 0.5 × 0.3.

This prevents ordinary Block-bypass stacking from exceeding 100%.

### Accuracy

```
Accuracy = 100 + Dexterity
```

### Evasion

```
Evasion = 100 + Agility
```

### Hit Chance

```
HitChance = clamp(Accuracy / TargetEvasion, 0.05, 0.95)
```

A single Hit Chance roll resolves both hit and evade.

There is no second independent dodge roll.

### Attack Speed

Reference rating:

```
AttackSpeedRating = 1 + Agility / 100
```

Effective attacks per second:

```
AttacksPerSecond = log2(2 * AttackSpeedRating)
```

Idle Superpowers hard-caps actual attacks at 30 attacks/sec.

**Project deviation:** Idle Build Master keeps the logarithmic diminishing-return curve but does not use a gameplay hard cap by design.

If extremely high values become technically expensive, attacks may be simulated in aggregate while preserving the mathematical result.

### Regeneration

While alive:

```
RegenPerSecond = 1 + Vitality / 10
```

While downed:

```
DeathRegenPerSecond = RegenPerSecond * 10
```

This ×10 death-regeneration multiplier is the initial reference baseline and can later be modified by permanent progression or Powers.

### Crit

Crit is a project extension and has no inherited baseline formula yet.

The combat architecture supports:

- Crit Chance;
- Crit Damage;
- OnCrit events.

Exact starting Crit values are deferred until Crit enters actual content.

Crit is therefore **not required for the first-location reference-based Power set**.

## Damage range

Basic attacks use a **Min Damage / Max Damage** range rather than a single fixed damage value.

This creates additional design space for Powers and gear.

Examples of future interactions:

- effects based on Min Damage;
- effects based on Max Damage;
- reduced variance;
- increased variance;
- bonuses when rolling near the minimum;
- bonuses when rolling near the maximum.

The normal combat UI can present this simply as:

**Damage: X–Y**

## Basic attack

The hero attacks automatically.

The baseline attack model uses:

- Min Damage
- Max Damage
- Attack Speed
- Accuracy
- Crit Chance
- Crit Damage

No manual attack input is required.

## Targeting

Targeting is automatic.

Default rule:

**Attack the current primary / nearest valid enemy.**

The player does not manually switch targets.

Powers may later override targeting behavior.

Examples:

- target lowest HP;
- target highest HP;
- target random enemy;
- target enemy with most Poison;
- target backline enemy;
- attack several enemies.

Targeting is treated as part of build behavior, not a separate control layer.

## Multiple enemies

The combat system must support multiple simultaneous enemies.

Expected common encounters:

- 1 boss;
- 2–3 normal enemies;
- 4–6 enemy groups;
- elite + adds;
- boss + summons.

Six visible enemies is the current presentation target, but the code should not hard-code a maximum of six.

The system should remain data-driven and flexible.

## Independent enemy attacks

Each enemy has its own:

- Attack Speed;
- attack timer;
- hit resolution.

Multiple enemies therefore generate multiple independent incoming-hit events.

This matters mechanically.

A group of enemies is not merely a larger HP pool.

More enemies can create more:

- damage events;
- Block checks;
- Evasion checks;
- Thorns triggers;
- on-hit reactions;
- rage / retaliation effects.

This enables defensive and reactive builds that may actually benefit from fighting larger groups.

## AoE

AoE is systemic rather than positional.

The game does not require geometric placement or tactical movement.

Examples:

- hit 2 additional enemies;
- damage all enemies;
- chain to N targets;
- splash X% damage;
- explosion on kill;
- damage random secondary targets.

Visual positioning communicates the fight, but does not determine hitboxes or tactical range.

## Accuracy and Evasion

Accuracy and Evasion remain part of the starting combat model.

Attacks may miss.

This is only valuable if misses and evades can become build hooks.

Expected design space:

- on miss;
- on evade;
- consecutive hits;
- guaranteed hit conditions;
- Accuracy sacrifice for damage;
- counterattack after evade.

If testing later shows that Accuracy/Evasion adds frustration without enough build value, the system can be simplified.

For now it remains.

## Crit

Crit is an explicit extension beyond the base reference model.

Core stats:

- Crit Chance
- Crit Damage

Direct attacks can critically strike.

Damage-over-time effects do **not** critically strike by default.

Special Powers can override this rule.

Examples:

- Poison can crit at reduced Crit Chance.
- Bleed inherits a portion of Crit Damage.
- Crits increase DoT stack generation.

This is intentionally used as a source of high-value synergy unlocks.

## Block

Block is a rating with diminishing returns.

It should not scale linearly toward trivial 100% mitigation through ordinary stats.

Design goals:

- no simple hard cap that makes additional Block worthless;
- no easy path to complete immunity through base stats;
- increasing Block remains useful but progressively less efficient.

Powers may break or reinterpret the rule.

Examples:

- convert Block into damage;
- gain bonuses above high Block thresholds;
- partially ignore enemy Block;
- increase Block effectiveness conditionally.

Exact formula is deferred to balance work.

## Attack Speed

Attack Speed uses the logarithmic reference formula defined above.

This gives strong diminishing returns while still allowing extreme stat values.

The project intentionally avoids a gameplay hard cap.

Visual animation does not need to literally play every mathematical hit at extreme speeds.

Rendering and simulation may aggregate attacks while preserving combat outcomes.

## Sustain

Sustain starts from the reference-style model and should remain mechanically diverse.

Potential forms include:

- Regeneration;
- Heal on Kill;
- Lifesteal;
- Shield;
- low-HP recovery;
- revive effects;
- conditional healing.

Not all of these need to be base stats.

Preferred direction:

- Regeneration is part of the baseline stat model.
- Other sustain types are primarily introduced through Powers and gear.

This keeps the base stat surface readable while preserving build depth.

## Death and downed state

Death does **not** reset the run and does **not** reset the encounter.

When HP reaches zero:

1. resolve fatal-hit prevention effects, if any;
2. if death still occurs, fire OnDeath;
3. clear the hero's temporary combat-state buffs and stacks;
4. the hero enters the downed state;
5. the hero regenerates at Death Regeneration speed;
6. when restored to full HP, the hero revives;
7. the same encounter continues.

Enemy HP is **not restored** when the hero dies.

This deliberately allows the player to slowly chip away at an enemy or boss across repeated deaths.

By default, enemies do not regenerate while the hero is downed, matching the reference behavior.

A specific enemy or boss mechanic may explicitly override this rule.

Death is a build-state reset, not an encounter failure.

## Combat stacks

A clear distinction exists between persistent run state and temporary combat state.

### Persistent for the run

Examples:

- current unspent EXP;
- EXP-purchased primary-stat upgrades;
- Powers;
- gear;
- permanent effects granted by current Powers.

### Temporary combat state

Examples:

- kill stacks;
- combo;
- temporary Attack Speed stacks;
- rage;
- temporary damage buffs;
- temporary defensive buffs;
- temporary debuffs;
- other timed combat-state effects.

By default, death clears temporary combat state.

Example:

**Momentum**  
Gain +0.01% Attack Speed per kill.  
Stacks up to 500 times.

On death:

**500 stacks → 0**

This creates meaningful value for survivability.

A high-DPS build may depend on maintaining momentum for several minutes.

## Death as a build hook

Because death clears combat state, death itself can later become a build archetype.

Potential Power hooks:

- retain X% of combat stacks on death;
- revive faster;
- gain a temporary burst after revive;
- damage enemies on death;
- trigger effects when revived;
- preserve selected buffs through death.

This is allowed to become a real build direction rather than only a failure state.

## Combat buffs and death

Default rule:

On death, clear from the hero:

- timed buffs;
- timed debuffs;
- kill stacks;
- combo stacks;
- rage-like temporary resources;
- short-term proc state.

Do not clear:

- Powers;
- Run Level;
- gear;
- persistent run modifiers;
- enemy current HP;
- the current encounter.

Specific Powers may override these defaults.

## Buff and debuff stacking rules

Unless an effect explicitly says otherwise:

- repeated timed buffs from the same Power create independent stacks;
- repeated timed debuffs create independent stacks;
- each stack has its own duration;
- multiplicative stacks multiply with each other;
- additive effects add with each other;
- expiration removes only the expiring stack.

Example:

A Power grants:

```
Strength ×1.1 for 5 seconds on getting hit
```

If it triggers 10 times before any stack expires:

```
Strength multiplier = 1.1^10
```

This exponential interaction is intentional and is one of the main sources of extreme builds.

A Power may explicitly define:

- non-stackable;
- refresh duration instead of adding a stack;
- maximum stack count;
- additive stacking;
- shared cooldown.

Those rules override the default.

## Stat calculation order

To avoid ambiguous Power interactions, stats resolve in layers.

### Layer 1 — Primary attributes

Resolve flat/additive changes to:

- Strength;
- Vitality;
- Agility;
- Dexterity.

Then apply multiplicative modifiers to those attributes.

### Layer 2 — Baseline derived stats

Calculate:

- Min Damage;
- Max Damage;
- Max Health;
- Block;
- Accuracy;
- Evasion;
- Attack Speed;
- Regeneration.

using the baseline formulas.

### Layer 3 — Derived-stat conversions and additions

Apply effects such as:

- add Min Damage based on Accuracy;
- add Strength based on missing HP;
- add Max Damage based on another resolved stat.

Dependencies must be acyclic.

If a Power would create a circular dependency, it requires an explicit custom resolution rule and cannot rely on generic stat calculation.

### Layer 4 — Derived-stat multipliers

Apply multiplicative modifiers such as:

- Max Damage ×10;
- Block ×5;
- Regeneration ×0.25.

### Layer 5 — temporary state

Apply currently active timed buffs/debuffs using the same additive-then-multiplicative rule for the stat they modify.

This order should be data-driven so individual Powers do not implement their own private stat math.

## Crowd Control

Crowd Control is not a separate major base-stat system.

It primarily comes from Powers and gear.

Potential effects:

- Stun;
- Attack Speed reduction;
- Accuracy reduction;
- delay next attack;
- fear / skipped attack;
- negative Regeneration;
- forced targeting / taunt-like effects.

Strong hard-CC effects should have limitations.

Bosses should preferably use reduced CC effectiveness rather than complete immunity.

Example:

**Boss Stun Duration ×0.2**

This preserves CC build value without allowing permanent boss lockdown.

## Boss design

Bosses are build checks, not manual-skill checks.

Bosses may introduce simple rules such as:

- summon adds periodically;
- gain a shield;
- enrage over time;
- heal while adds are alive;
- punish many weak rapid hits;
- periodically gain Armor;
- apply escalating damage-over-time;
- suppress regeneration;
- create phases based on HP thresholds.

The player should solve these by changing:

- build;
- Powers;
- gear;
- farming priorities.

The player should not need to dodge telegraphs manually.

## First Location Boss baseline

The first Location Boss must already demonstrate the game's buildcraft hook.

It should not be a placeholder stat wall.

At the same time, it should be passable for almost every functional first-run build because the first Boss should create curiosity rather than rejection.

### Baseline primary stats

Initial calibration:

```
Strength = 109
Vitality = 238
Agility = 64
Dexterity = 89
```

Using the universal combat formulas, this gives approximately:

```
Max HP = 23,820
Min Damage = 49.5
Max Damage = 267.5
Block = 129.9
Block Damage Reduction = 56.50%
Accuracy = 189
Evasion = 164
Attacks/sec = 1.714
Regeneration/sec = 24.8
```

These are first-Location calibration values and may move after live combat measurement without changing the Boss mechanic.

### Core mechanic: Fortify → Break → Exposed

Cycle baseline:

1. Boss begins in **Normal** state for 6 seconds.
2. Boss enters **Fortify**.
3. During Fortify:
   - Block ×5;
   - each successful direct hero hit contributes 1 Break point;
   - 16 successful hits break Fortify.
4. Boss enters **Exposed** for 6 seconds.
5. During Exposed:
   - Block ×0.25;
   - no extra universal damage-taken multiplier is added in the first implementation.
6. Boss returns to Normal and the cycle repeats.

With the baseline Boss stats:

```
Normal Block = 129.9
Fortify Block = 649.5
Fortify Damage Reduction ≈ 86.66%

Exposed Block = 32.475
Exposed Damage Reduction ≈ 24.52%
```

Only successful hits count toward Break.

Therefore:

- Attack Speed improves Break speed;
- Accuracy affects Break reliability;
- high raw damage benefits strongly from Exposed windows;
- Block-bypass Powers can partially ignore Fortify;
- sustain can survive repeated cycles;
- death/revive builds can keep making permanent HP progress across deaths.

### Accessibility constraint

The first Boss has no universal hard fail.

For the first implementation it does not use:

- healing to full;
- unavoidable regeneration that invalidates most builds;
- hard enrage wipe;
- mandatory manual timing;
- a global DPS timer.

Boss HP persists through hero deaths.

Target experience:

- strong synergy → fast, satisfying kill;
- average build → normal kill;
- weak but functional build → several cycles and possibly several deaths;
- only pathological / nonfunctional setups should feel practically impossible.

The mechanic is intentionally real from version one: the player should already feel that different builds interact with the same Boss in meaningfully different ways.

## No formal defeat state

Normal combat has no global "Defeat" state.

If the hero dies, the encounter continues after revival with enemy HP preserved.

The player decides when the current situation should be treated as a wall.

A run may therefore be:

- progressing efficiently;
- progressing slowly;
- effectively stalled;
- being used for farming.

The player may continue chipping an enemy indefinitely if that is worthwhile.

This is intentional.

A boss can create a harder wall through its own mechanics, for example:

- regeneration;
- healing;
- shields;
- summons;
- escalation;
- enrage;
- another rule that outpaces the player's progress.

Those are encounter mechanics, not a universal defeat timer.

Death still reduces efficiency because temporary combat stacks and buffs are lost.

This creates a natural distinction between:

- stable farming builds;
- fragile high-output builds;
- death/revive builds that exploit the downed cycle.

## No manual active skill

The MVP does not include a mandatory manual combat skill button.

No manual:

- heal button;
- burst button;
- shield timing;
- tap-to-damage;
- manual target switching.

If active abilities are explored later, they must not become the primary way optimal farming or progression is achieved.

## Build power can be extreme

The game intentionally allows strong synergies to become dramatically stronger than ordinary builds.

A successful combination may produce:

- 10×;
- 50×;
- 100×;
- or greater effective performance than a mediocre setup.

This is a feature, not automatically a balance failure.

The desired player reaction is:

> “I found something broken.”

Balance should focus on:

- preventing one universal dominant solution;
- creating different strong answers to different problems;
- ensuring discovery and experimentation remain valuable.

The goal is **not** to make every viable build equally strong at all times.

## Combat resolution order

Combat uses deterministic event ordering so Power interactions do not depend on implementation accident.

### Continuous update

Between discrete events:

1. advance attack timers;
2. advance buff/debuff durations;
3. apply Regeneration;
4. apply periodic effects such as future DoT;
5. process expired effects.

### Direct attack pipeline

When an actor's attack becomes ready:

1. **Select Target**
2. **OnAttack**
3. resolve attack-redirection effects such as "enemy attacks itself"
4. calculate Hit Chance
5. roll Hit / Miss
6. if Miss → **OnMiss** / **OnEvade**, then stop this attack
7. roll base damage between Min Damage and Max Damage
8. resolve Crit if the attack is Crit-eligible
9. apply outgoing damage modifiers
10. resolve Block-bypass effects
11. apply target Block reduction
12. apply HP damage
13. if the hit would be fatal, resolve fatal-hit prevention effects
14. emit **OnDamageDealt / OnDamageTaken**
15. emit **OnHit / OnHitTaken**
16. resolve triggered effects through the event queue
17. resolve deaths caused by the attack or its triggered effects
18. emit **OnKill / OnEnemyDeath / OnDeath** as applicable
19. if all enemies are dead → encounter clears

A successful hit can therefore trigger OnHit effects even when that hit is fatal.

### Trigger queue

Triggered combat effects resolve in FIFO order within their priority tier.

Priority tiers:

1. attack replacement / cancellation;
2. fatal-hit prevention;
3. damage/healing application;
4. hit/damage reactions;
5. death/revive effects;
6. encounter-state effects.

A triggered effect may enqueue another event.

The event system must prevent infinite trigger loops through explicit source rules, cooldowns, or a safety recursion limit.

### Simultaneous enemy attacks

Enemies have independent attack timers.

If several attacks become ready on the same simulation step, resolve them in a stable deterministic order, then advance to newly generated events.

Do not merge multiple enemies into one synthetic group attack.

### Encounter clear while hero is downed

If the final enemy dies while the hero is downed, the encounter counts as cleared.

The next encounter waits until the hero revives unless a future rule explicitly says otherwise.

## Combat event hooks

The combat system should be architected around reusable events.

Minimum expected hooks:

### Offensive

- On Attack
- On Hit
- On Miss
- On Crit
- On Kill
- On Damage Dealt
- On DoT Applied
- On DoT Tick
- On Enemy Death

### Defensive

- On Incoming Attack
- On Hit Taken
- On Block
- On Evade
- On Damage Taken
- On Low HP
- On Shield Break

### State

- On Buff Gain
- On Buff Expire
- On Debuff Applied
- On Stack Gain
- On Stack Maxed
- On Death
- On Revive

### Encounter

- On Encounter Start
- On Enemy Spawn
- On Enemy Count Changed
- On Boss Spawn
- On Boss Phase Changed
- On Encounter Win

These hooks are critical because Powers should primarily be combinations and transformations of shared combat events rather than unique one-off code.

## Design constraint: data-driven combat

Combat mechanics should be implemented so new Powers can often be created by configuring:

- trigger;
- condition;
- target;
- effect;
- scaling;
- duration;
- stack rule;
- cooldown.

The production goal is to create a large amount of build content without requiring unique code for every Power.

## Locked decisions from v0.2

1. Fully automatic combat.
2. No manual target selection.
3. Four base attributes: Strength, Vitality, Agility, Dexterity.
4. Min–Max Damage model.
5. Accuracy and Evasion remain for now.
6. Crit Chance and Crit Damage exist.
7. DoT does not crit by default.
8. Block uses diminishing returns.
9. Attack Speed uses a soft cap / diminishing returns.
10. Multiple enemies attack independently.
11. Six enemies is a presentation target, not a hard-coded system cap.
12. AoE is systemic, not positional.
13. Death does not reset the run.
14. Death clears temporary combat stacks and buffs by default.
15. Sustain is mostly built through Powers / gear beyond baseline Regeneration.
16. CC is primarily Power-driven.
17. Bosses test builds, not manual execution.
18. No mandatory active combat skill in MVP.
19. Extremely strong and partially broken synergies are intentionally allowed.
20. Combat should be built around reusable event hooks.
21. Baseline STR/VIT/AGI/DEX-derived formulas inherit Idle Superpowers values.
22. Death does not restore enemy HP and does not restart the encounter.
23. There is no universal defeat state; the player decides when progression is stalled.
24. Standard enemies do not regenerate while the hero is downed unless explicitly overridden.
25. Downed regeneration starts at 10× normal Regeneration.
26. Repeated timed buffs/debuffs independently stack by default.
27. Multiplicative timed stacks multiply exponentially.
28. Combat uses an explicit deterministic event-resolution order.
29. Attack Speed keeps the reference logarithmic curve but removes the gameplay hard cap as a deliberate project deviation.
30. The first Location Boss uses a real build-check mechanic rather than a placeholder stat wall.
31. First Boss baseline is Fortify → Break → Exposed.
32. The first Boss avoids a universal hard fail and should be eventually passable by almost every functional build.
33. Fresh-run starting primary stats are STR/VIT/AGI/DEX = 1/1/1/1.
34. First Boss baseline stats are 109 STR / 238 VIT / 64 AGI / 89 DEX.
35. First Boss cycle is 6s Normal → Fortify (Block ×5, break after 16 successful hits) → 6s Exposed (Block ×0.25).

## Open questions for later

The following do **not** block implementation of the first location's reference-based combat:

1. Exact Crit baseline values.
2. Which DoTs enter the first content expansion.
3. Exact CC resistance model for later bosses.
4. Final AoE secondary-hit proc policy.
5. Final technical strategy for aggregating extremely high attack rates.
6. Advanced Power trigger/effect schema beyond the current 25-Power set.

Enemy curves, XP curves and encounter composition are content/pacing decisions and should be defined together with the first location rather than inside the universal combat rules.
