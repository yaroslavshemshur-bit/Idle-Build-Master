# 00 — Game Vision

**Status:** Draft v0.1  
**Date:** 2026-09-27

## High concept

A portrait mobile idle RPG about **conscious progression and buildcraft**.

The player develops one unnamed hero in a familiar fantasy world. The hero has no fixed class: the current build determines who the character becomes.

A run may develop into a:

- Crit build
- Poison build
- Bleed build
- AoE build
- Tank / Thorns build
- Berserk build
- Summon build
- Hybrid build

**Class is the result of player decisions, not a choice made before playing.**

## Player fantasy

The primary fantasy is not simply watching numbers grow.

The player should feel:

1. **“I found a synergy.”**
2. **“I hit a wall.”**
3. **“I understand what is missing.”**
4. **“I know what I want to farm or change.”**
5. **“The new setup worked — I broke through.”**

The game should reward understanding of its systems.

## Platform and presentation

- Mobile
- Portrait orientation
- One persistent hero
- Compact static combat scene
- No character traversal during combat
- Fully automated basic combat
- Multiple enemies may attack simultaneously
- Global map is used to choose where to push or farm

## Setting

Classic readable fantasy.

Expected enemy families include:

- Goblins
- Orcs
- Spiders
- Skeletons
- Undead
- Bandits
- Slimes
- Elementals
- Demons

The setting should be immediately understandable without heavy lore exposition.

Enemy themes can naturally connect to mechanics and progression. Examples:

- Spiders → poison
- Skeletons → bones / crit / undead mechanics
- Goblins → gold / greed
- Orcs → armor / brute force

## Reference roles

### Idle Superpowers

Primary reference for:

- Buildcraft
- Synergy discovery
- Run variation
- Controlled randomness
- Challenges that test system understanding
- Progression that can materially change how a run is built

### Hypixel SkyBlock

Primary reference for:

- Multiple long-term goals
- Useful old content
- Collections
- Targeted farming
- Cross-system progression
- Turning grind into purposeful progression
- Gradual transition from linear onboarding to a network of player-chosen goals

### Gear Defenders

Production and visual reference:

- Readable simple characters
- Minimal animation complexity
- Scale / transform based animation where possible
- Limited VFX
- Strong readability over visual complexity
- Content depth should come primarily from game systems, not expensive animation production

### Idle Sword Master

Not a primary design reference.

It may be useful only as a general example of layering progression systems, but the project should avoid progression that consists mainly of stacking independent percentage bonuses.

## Design pillars

### 1. Build > Time

Good decisions should create a large difference in effectiveness.

Waiting alone should not be the best solution to every wall.

### 2. Idle execution, active strategy

The hero executes combat automatically.

The player decides:

- what to farm;
- where to push;
- how to build;
- what gear to use;
- when to change strategy;
- when a run should become a farming run;
- when to reset.

### 3. Every grind has a purpose

The player should usually be able to answer:

**“Why am I farming this location right now?”**

### 4. Progress unlocks possibilities

Progression should frequently unlock:

- new mechanics;
- new powers;
- new interactions;
- new build options;
- new farming targets.

Permanent +X% bonuses can exist, but should not be the main source of excitement.

### 5. Failure still progresses

A bad or unlucky build may be inefficient for pushing, but time spent should still generate useful account progress.

A failed push run can become a farming run.

### 6. Walls are problems

A wall should encourage the player to diagnose a weakness.

Examples:

- insufficient AoE;
- poor sustain;
- no armor solution;
- low single-target damage;
- inability to control summoned enemies.

The intended thought is:

**“How do I solve this?”**

not:

**“How many hours do I need to wait?”**

### 7. Controlled randomness

The player should be able to start a run with an intended direction, while random choices force adaptation.

The game should live between:

- fully deterministic perfect builds;
- completely random runs with no agency.

### 8. One hero, many identities

Content variety should come from combinations of mechanics rather than a large roster of expensive-to-produce characters.

## World structure

The beginning should be intentionally linear so the player is not overwhelmed.

Initial structure:

**Location 1 → Location 2 → Location 3 → Boss → Next Region**

Previously completed areas remain useful.

As systems unlock, the player's world changes into a network of objectives:

- Push further
- Farm a Collection
- Hunt a specific item
- Improve a mastery
- Farm gold
- Prepare for a boss
- Complete an achievement or challenge

The long-term target is **player-directed progression**, introduced gradually.

## Interaction philosophy

Basic combat should not require tapping.

Constant tap-to-damage would make optimal play depend on physical input rather than build quality, which conflicts with the core fantasy.

Possible active abilities such as heal, shield, cleanse or burst can be explored later, but they should be supplemental rather than the primary source of DPS or progression.

For the MVP, the preferred starting point is **no mandatory combat tapping**.

## Offline philosophy

Offline play performs work the player has already configured.

Expected offline gains:

- Basic currency
- Drops
- Collection progress
- Passive achievement progress

Offline play should not make major strategic choices for the player.

It should generally not:

- choose new Powers;
- rebuild equipment;
- select new progression branches;
- defeat important progression bosses automatically.

**Offline executes. The player decides.**

## Business model constraints

Current target:

- F2P mobile
- Ads + IAP

Monetization design is intentionally not defined yet.

Hard constraint:

- No energy system.

The game should not require a large number of currencies merely to support monetization.

## Early scope philosophy

The first version should be small enough to finish.

Avoid early feature creep such as simultaneously adding:

- pets;
- artifacts;
- runes;
- companions;
- awakening;
- multiple prestige currencies;
- PvP;
- gacha layers.

New systems should only be introduced when the existing loop demonstrates that they create meaningful decisions.

## Current core thesis

> The player does not control combat moment-to-moment. The player controls the development plan that determines whether combat succeeds.

The core emotional loop is:

**Build → Wall → Goal → Farm → Improvement → Breakthrough**
