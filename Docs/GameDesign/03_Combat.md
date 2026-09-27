# 03 — Combat

**Status:** Draft v0.1  
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

The exact formulas are intentionally postponed until balance work.

The important structural rule is that each primary attribute affects more than one meaningful combat outcome.

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

Attack Speed uses **soft cap / diminishing returns**, not a hard cap.

Players should be allowed to create extreme Attack Speed builds.

The system should support very high effective hit frequency while preventing runaway formula instability.

Visual animation does not need to literally play every hit at extreme speeds.

Implementation can later aggregate or compress visual feedback while preserving combat math.

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

Death does **not** reset the run.

When HP reaches zero:

1. the hero enters a downed / death state;
2. temporary combat-state buffs and stacks are cleared;
3. the hero recovers according to the recovery model;
4. the encounter can restart or the player can change strategy.

Death is intended to be a build-state reset, not a run reset.

## Combat stacks

A clear distinction exists between persistent run state and temporary combat state.

### Persistent for the run

Examples:

- Run Level;
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

On death, clear:

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
- persistent run modifiers.

Specific Powers may override these defaults.

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

## Encounter failure

### Progression encounter

If the hero dies:

- the encounter fails;
- push does not advance;
- the player may retry;
- change gear;
- farm elsewhere;
- or reset voluntarily.

No permanent penalty is applied.

### Farming encounter

The hero may repeatedly die and recover while farming, depending on final farming implementation.

Death can reduce efficiency because combat stacks are lost.

This creates a natural distinction between:

- stable farming build;
- fragile high-output build.

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

## Locked decisions from v0.1

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

## Open questions for later

1. Exact formulas for primary attributes.
2. Exact Block formula.
3. Exact Attack Speed curve.
4. Exact Accuracy / Evasion hit-chance formula.
5. Exact Regeneration / death recovery model.
6. Exact Crit baseline values.
7. Which DoTs exist in MVP.
8. Exact CC duration scaling and boss resistance rules.
9. Farming behavior after repeated deaths.
10. Exact Power trigger/effect schema.
