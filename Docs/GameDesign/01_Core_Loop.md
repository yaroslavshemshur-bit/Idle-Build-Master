# 01 — Core Loop

**Status:** Draft v0.1  
**Date:** 2026-09-27

## Core loop

The current top-level loop is:

1. Choose a goal.
2. Choose a location.
3. Fight automatically.
4. Receive resources, gear and progression.
5. Gain or choose new build components.
6. Adjust the build.
7. Attempt to push further.
8. Hit a wall.
9. Diagnose why the build fails.
10. Change build, equipment or farming target.
11. Break through the wall.
12. Unlock new content.
13. Eventually reset.
14. Prepare the foundation of the next run.
15. Repeat with more options.

Condensed:

**Push → Wall → Farm / Rebuild → Improvement → Push**

## Primary player question

The central question should usually be:

**“What is the best thing for me to do next?”**

rather than:

**“Which +10% upgrade do I buy next?”**

Examples:

- Continue pushing the Graveyard.
- Return to Spider Forest to unlock Poison Collection III.
- Farm goblins for a specific item.
- Improve a mastery.
- Change gear for the current boss.
- Abandon an unlucky push attempt and convert the run into farming.
- Reset and start a new attempt with a better foundation.

## Combat execution

Combat is automatic.

The hero can face different encounter shapes:

- One strong enemy
- Several normal enemies
- A swarm of weak enemies
- Elite + adds
- Boss + summons

Enemy count is a real combat variable.

This allows AoE to become a meaningful build dimension rather than a cosmetic damage type.

A build may be excellent at single-target damage and fail against swarms, or vice versa.

## Push and Farm

The player should naturally alternate between two modes.

### Push

Purpose:

- Reach a new stage
- Defeat bosses
- Unlock locations
- Unlock systems
- Reach progression milestones

### Farm

Purpose:

- Advance a Collection
- Find a specific item
- Gain currency
- Improve mastery
- Complete passive achievements
- Prepare a solution to a wall

The relationship is:

**Push → Wall → Farm → Improvement → Push**

Neither mode should permanently replace the other.

## Reward channels

At this stage, combat rewards are grouped into four broad channels.

### 1. Basic currency

A simple universal progression resource.

It can support baseline power growth, but should not solve all progression by itself.

### 2. Gear

Primary source of item excitement.

Gear is expected to support:

- base stats;
- rarity;
- affixes;
- build-specific effects.

Exact gear structure is TBD.

### 3. Collection progress

Fighting specific enemy families or obtaining their resources advances related Collections.

Collections should frequently unlock gameplay possibilities rather than only permanent percentages.

### 4. Run progress

The current run produces opportunities to gain Powers and shape the current build.

The exact mechanism — hero level, milestones, XP or another structure — will be defined in the progression document.

## Powers and build decisions

Powers are the primary run-level buildcraft layer.

A typical choice can present several mechanically different options.

Example:

**Heavy Strikes**  
+Damage, -Attack Speed

**Bloodlust**  
Kills temporarily increase Attack Speed and can stack.

**Hemorrhage**  
Critical hits apply Bleed.

The choice should become contextual.

The strongest option depends on:

- existing Powers;
- current equipment;
- current enemy type;
- intended farming target;
- current wall.

## Controlled RNG

The player should begin a major run with some control over the intended direction.

Example concept:

- Choose a small starting set of Traits / Memories / Powers.
- The rest of the run presents random selections.

The player may enter intending:

**Crit + Bleed**

but the run can evolve into:

- Crit + Bleed
- Crit + Attack Speed
- Crit + Execute
- another unexpected hybrid

Randomness should create adaptation rather than remove agency.

The exact starting system is TBD.

## Bad RNG must remain productive

An unlucky run should not mean wasted playtime.

If the current build is poor for pushing, the player should still be able to gain:

- currency;
- gear;
- Collection progress;
- achievement progress;
- future account progression.

A useful player decision becomes:

**“This is no longer a push run. I will use it as a farming run, then reset.”**

This is a key bridge between run-based buildcraft and persistent SkyBlock-like progression.

## Walls

A wall should be a readable gameplay problem.

### Example: Swarm

Many weak enemies appear simultaneously.

Tests:

- AoE
- Chain effects
- Explosions
- Attack speed
- Crowd handling

### Example: Armored Orc

Very high defense.

Tests:

- Armor penetration
- Damage-over-time
- Alternative scaling
- Specialized effects

### Example: Necromancer

Continuously creates adds.

Tests:

- AoE
- Single-target pressure
- Target throughput

### Example: Spider Queen

Applies high sustained poison pressure.

Tests:

- Sustain
- Resistance
- Burst
- Cleanse-like solutions if such mechanics exist

Exact enemies are examples, not locked content.

The important rule is that walls should test build properties rather than only raw account power.

## Three time scales

### Seconds: combat feedback

The player observes:

- kill speed;
- incoming damage;
- survival;
- behavior against packs;
- Power triggers;
- the impact of a new item or Power.

The player mostly reads the build rather than manually executing combat.

### Minutes: tactical decisions

The player periodically makes meaningful changes:

- choose a Power;
- equip or compare an item;
- switch location;
- continue pushing;
- start targeted farming;
- react to a Collection unlock.

This is the main active-session decision layer.

### Tens of minutes and multiple sessions: strategic goals

Example chain:

1. The player wants to defeat a Graveyard boss.
2. The current build lacks AoE.
3. A desired AoE mechanic is unlocked through Goblin Collection.
4. The player returns to Goblin Camp.
5. A rare item drops that strongly scales Attack Speed.
6. A new Power supports Attack Speed.
7. The player considers changing the original plan and creating a new hybrid build.
8. That new build may solve the original boss in an unexpected way.

One goal should be able to generate new goals.

This is a major driver of long active sessions.

## Offline loop

Before leaving, the player effectively configures:

**“Farm this known location.”**

Offline progress can then generate routine gains from already understood content.

When the player returns:

1. Claim offline gains.
2. Review drops / Collection progress.
3. Make new decisions.
4. Rebuild if useful.
5. Push or assign a new farming target.

Offline time should feed the decision loop rather than replace it.

## Desired session behavior

A successful session can begin as:

**“I will log in for five minutes.”**

Then:

1. Offline loot finishes a Collection milestone.
2. The milestone unlocks a new Poison interaction.
3. It synergizes with an owned item.
4. The player rebuilds.
5. The new setup defeats yesterday's wall.
6. A new location opens.
7. The new enemies expose a weakness in the current build.
8. The player chooses a new farming target.

The session extends because new decisions create new objectives, not because the game forces repetitive tapping.

## Decisions intentionally postponed

Do not lock these yet:

- first prestige timing;
- stage counts;
- exact DPS / HP formulas;
- exact Power frequency;
- exact gear rarity system;
- prestige currencies;
- ad placements;
- IAP structure;
- gacha;
- PvP;
- exact offline duration cap.

The next design task is to define **the structure of one run and the progression of the first region** so these later systems have a concrete foundation.
