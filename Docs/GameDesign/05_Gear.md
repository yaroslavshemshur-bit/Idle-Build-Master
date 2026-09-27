# 05 — Gear

**Status:** Draft v0.1  
**Date:** 2026-09-28

## Purpose

Gear is a run-level build component and a major reason to farm specific content.

It should:

- create exciting drops;
- support Power synergies;
- create reasons to revisit Stages and Locations;
- offer both broad stat progression and specific build-defining chase items;
- remain useful without replacing Powers as the primary build system.

The game uses a **hybrid gear model**:

1. procedural ordinary gear;
2. rarer named build-defining items.

## Gear acquisition

Primary source:

**Enemy and Boss drops.**

A shop may exist later as a supporting system, but normal progression should not depend on repeatedly checking a shop for the main equipment upgrades.

Drop-based gear is important because it gives each Stage / Location direct farming value.

Future sources can include:

- Boss drops;
- elite enemies;
- quests;
- Collections;
- challenges;
- shops;
- crafting / upgrading systems if later needed.

## Six equipment slots

All six equipment slots are available from the beginning.

Working slot structure:

1. Weapon
2. Head
3. Chest
4. Gloves
5. Boots
6. Accessory

The exact fantasy labels can change, but the six-slot structure should remain data-driven.

The player should not need to unlock basic equipment slots one by one.

Progression should come from the items themselves and later from retention across resets.

## Procedural ordinary gear

Most normal drops are procedurally generated.

A normal item is described by:

- equipment slot;
- item level / source difficulty;
- rarity;
- affixes;
- numerical affix values.

This creates continuous loot variation without requiring every drop to be authored manually.

## Named build-defining gear

Some rare items are authored items with fixed or partially fixed mechanics.

Examples:

- Crits apply Bleed.
- Reflect damage can Crit.
- On Evade, gain temporary Attack Speed.
- Poison spreads when an enemy dies.
- Retain part of a combat stack after death.

These items are intended to become **farm targets**.

The player should be able to think:

> “I want that item, so I know what Boss / Stage / Location I should farm.”

Named items are one of the main bridges between world content and buildcraft.

## Rarity

First implementation uses a readable rarity ladder close to the reference direction:

1. Common
2. Uncommon
3. Rare
4. Epic
5. Legendary
6. Godlike

Rarity may influence:

- number of affixes;
- allowed affix tiers;
- numerical roll quality;
- probability of unusual combinations.

Rarity should **not** mean that a higher-rarity item is automatically correct for every build.

A lower-rarity item with the right affixes can be better for a specific build.

## No universal Gear Score

Do not reduce item evaluation to one authoritative Gear Score.

That would conflict with buildcraft.

For example:

- an item with more Strength may be better for a Max Damage build;
- an item with less raw damage but more Agility can be better for a proc build;
- defensive stats can be more valuable when preserving combat stacks matters.

The UI can show stat differences, but should not claim one item is universally better when meaningful trade-offs exist.

## Manual equipment

The player can always manually equip gear.

The item comparison UI should show:

- stats gained;
- stats lost;
- special mechanics gained/lost;
- relevant derived-stat changes where practical.

Manual choice is the authoritative behavior.

## Auto Equip

Auto Equip exists as optional convenience.

It must be conservative.

### Safe auto-equip

Automatically equip an item only when it is an obvious upgrade under the current comparison rules.

Example:

- same slot;
- no special mechanic is lost;
- the new item is equal or better across all compared baseline stats.

### Ambiguous trade-off

If an item gains some stats and loses others:

- do not automatically replace the equipped item;
- mark it for player review.

### Locked items

The player can lock an item.

A locked item:

- is not auto-sold;
- is not auto-destroyed;
- is not replaced by Auto Equip without explicit permission.

## Future Auto Equip profiles

Later QoL may allow profiles such as:

- Damage;
- Survival;
- Speed;
- custom stat weights.

These are not required for the first Location.

The first version only needs:

- manual equip;
- item comparison;
- Lock;
- conservative Auto Equip.

## Gear and buildcraft

Gear should primarily do three things.

### 1. Reinforce an existing build

Examples:

- more Strength;
- more Agility;
- more Accuracy;
- more Block.

### 2. Redirect a build

A strong drop can create a reason to adapt future Power choices.

Example:

A player planned Strength / Block but finds an item with a powerful Evasion interaction.

The run can pivot.

### 3. Unlock a mechanical interaction

Named items can create rule-breaking effects normally unavailable from basic stats.

This is the most valuable long-term gear design space.

## Gear and reset

Early progression:

**most gear resets with the run.**

Permanent progression later allows the player to retain more selected gear.

Working direction:

**Protected / Retained Gear**

- early reset: little or no gear retained;
- later reset progression: protect selected items;
- higher progression: retain more pieces.

The six equipped slots do not imply six retained slots.

Retention is a separate permanent progression system.

Important design constraint:

A rare drop should be exciting, but the cost of losing it on reset must not make the player feel permanently unable to reset.

The retention curve should solve that tension over time.

## Drop targeting

The system should support known drop sources.

A Location / Stage / Boss can have:

- general drop pool;
- slot biases;
- rarity modifiers;
- named-item table;
- unique Boss drops.

This allows purposeful farming.

The UI should eventually communicate where discovered items can drop.

## First Location requirements

The first Location should already demonstrate the gear loop.

It should include:

- drops across all six slots;
- simple procedural affixes;
- item comparison;
- manual equip;
- item locking;
- conservative Auto Equip;
- at least one memorable named / Boss-target item if content scope allows.

Exact item tables are defined together with first-Location content.

## Locked decisions from v0.1

1. Gear primarily drops from combat rather than being shop-first.
2. All six equipment slots are available immediately.
3. Gear uses a hybrid model: procedural ordinary gear + named build-defining items.
4. Rarity does not override build-specific item value.
5. There is no authoritative universal Gear Score.
6. Manual equipment is always available.
7. Auto Equip exists but is conservative.
8. Ambiguous trade-off items require player review rather than automatic replacement.
9. Items can be locked from automatic handling.
10. Most gear initially resets with the run.
11. Permanent progression later increases gear retention.
12. Specific content can have targetable item drop tables.

## Open questions

1. Exact affix library.
2. Exact item-level scaling.
3. Exact rarity probabilities.
4. Exact rarity-to-affix-count relationship.
5. Whether named items can also roll random affixes.
6. Initial inventory capacity / overflow behavior.
7. Sell / salvage behavior.
8. Exact retained-gear progression.
9. First Location item table.
