# 05 — Gear

**Status:** Draft v0.4  
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

All six equipment slots exist from the beginning.

Gear drops begin only after an early progression milestone, normalized to approximately the reference game's level-10 gear introduction in the first Location.

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

## Item Level

For the first Location:

```
ItemLevel = reference-equivalent level of the source
```

There is no additional Item Level RNG in the first implementation.

Examples:

- gear unlock area → around Item Level 10;
- mid-Location drops → around Item Level 30–40;
- Stage 8 → around Item Level 50–56;
- Stage 10 / Boss → up to Item Level 70.

Later permanent progression may increase effective item level or source quality, but the first Location uses the source level directly.

## Affix magnitude

First calibration baseline:

```
AffixPower = max(1, round(ItemLevel / 5))
```

Primary affixes:

```
STR = +AffixPower
VIT = +AffixPower
AGI = +AffixPower
DEX = +AffixPower
```

Derived affixes use:

| Affix | Formula |
|---|---:|
| Min Damage | +0.75 × AffixPower |
| Max Damage | +3.0 × AffixPower |
| Max Health | +150 × AffixPower |
| Regen/sec | +0.15 × AffixPower |
| Accuracy | +1.5 × AffixPower |
| Evasion | +1.5 × AffixPower |
| Block | +0.75 × AffixPower |
| Attack Speed Rating | +0.015 × AffixPower |

These derived values are intentionally stronger in their narrow domain than the equivalent primary-stat contribution because primary stats improve multiple combat outcomes simultaneously.

Example at Item Level 70:

```
AffixPower = 14
STR / VIT / AGI / DEX = +14
Min Damage = +10.5
Max Damage = +42
Max Health = +2100
Regen/sec = +2.1
Accuracy = +21
Evasion = +21
Block = +10.5
Attack Speed Rating = +0.21
```

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

### Named-item affix rule

First implementation:

- named items use their authored fixed identity/effect;
- named items do **not** roll the ordinary procedural affix set by default;
- a future named item may explicitly opt into procedural affixes through its own definition.

This keeps a targeted named drop predictable and avoids turning its identity into another rarity-roll problem.

## Rarity

First implementation uses a readable rarity ladder close to the reference direction:

1. Common
2. Uncommon
3. Rare
4. Epic
5. Legendary
6. Godlike

For the first implementation, rarity determines **affix count**:

| Rarity | Affix count |
|---|---:|
| Common | 1 |
| Uncommon | 2 |
| Rare | 3 |
| Epic | 4 |
| Legendary | 5 |
| Godlike | 6 |

Rarity does **not** multiply the numerical strength of an affix.

Affix magnitude is determined primarily by Item Level.

The first affix on every ordinary procedural item is always one of the four primary attributes:

- Strength;
- Vitality;
- Agility;
- Dexterity.

Additional affixes can roll from the full first-implementation pool:

- Strength;
- Vitality;
- Agility;
- Dexterity;
- Min Damage;
- Max Damage;
- Max Health;
- Regeneration;
- Accuracy;
- Evasion;
- Block;
- Attack Speed Rating.

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

Automatically equip an item only when it is a strict, context-free dominance upgrade.

First implementation comparison:

1. resolve the hero's neutral baseline stat snapshot with the currently equipped item;
2. resolve the same snapshot with the candidate item;
3. compare:
   - Min Damage;
   - Max Damage;
   - Max Health;
   - Regeneration;
   - Accuracy;
   - Evasion;
   - Block;
   - attacks per second;
4. candidate must be **not lower in every compared output** and **strictly higher in at least one**.

Auto Equip does not replace an item if either side contains:

- a named/unique effect;
- an event trigger;
- a conditional effect;
- another mechanic whose value depends on encounter/build context.

Those items require manual review.

This is intentionally conservative rather than attempting to score build value.

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

**Lock is not reset retention.**

Reset protection/retention is a separate future permanent-progression flag/system.

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

## Inventory capacity

First implementation has **no hard inventory capacity**.

Therefore:

- normal drops are never rejected because a bag is full;
- offline reward batches do not lose items to overflow;
- no overflow mailbox is required for MVP;
- inventory performance must still be tested with large item counts.

A future inventory cap may be introduced only together with explicit overflow/disposal rules.

Sell / salvage / auto-disposal are **Later** systems and are not required for the first playable Location.

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

Baseline timing:

- the first ~14% of the Location teaches combat and the first Power without loot noise;
- gear drops begin around the normalized reference level-10 milestone;
- from that point onward, all six slots can drop.

### Procedural drop chances

After gear is unlocked:

```
Normal encounter procedural gear chance = 20%
Stage 8 Elite procedural gear chance = 35%
Boss procedural gear drop = guaranteed
```

Normal / Elite procedural rarity table:

| Rarity | Chance |
|---|---:|
| Common | 60% |
| Uncommon | 25% |
| Rare | 10% |
| Epic | 4% |
| Legendary | 0.9% |
| Godlike | 0.1% |

Boss guaranteed procedural rarity table:

| Rarity | Chance |
|---|---:|
| Rare | 60% |
| Epic | 30% |
| Legendary | 9% |
| Godlike | 1% |

The Boss therefore never drops Common or Uncommon gear from its guaranteed procedural reward.

This gives the first Boss a visibly better loot profile without making Legendary/Godlike routine.

### First named farm item

Stage 8 Hobgoblin Guard has an independent named-item roll:

```
Hobgoblin Bulwark drop chance = 5%
```

This roll is separate from the Stage 8 procedural 35% gear roll.

The item is defined in `Docs/Content/Items.md`.

The first Location includes:

- drops across all six slots;
- procedural affixes;
- item comparison;
- manual equip;
- item locking;
- conservative Auto Equip;
- one explicit targeted named-item farm source.

Exact slot weighting and later-location loot tables remain open.

## Locked decisions from v0.3

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
13. Common through Godlike use 1 through 6 affixes respectively.
14. The first affix on an ordinary item is always a primary stat; later affixes use the 12-stat first-implementation pool.
15. Rarity does not increase affix magnitude; Item Level controls affix magnitude.
16. First-Location Item Level equals source reference-equivalent level.
17. First-Location AffixPower is max(1, round(ItemLevel / 5)).
18. Normal encounter procedural gear chance is 20% after gear unlock.
19. Stage 8 Elite procedural gear chance is 35%.
20. Normal/Elite rarity chances are 60/25/10/4/0.9/0.1% from Common through Godlike.
21. First Boss guarantees one procedural item with Rare/Epic/Legendary/Godlike chances of 60/30/9/1%.
22. Stage 8 Hobgoblin Guard has a separate 5% named-item roll for Hobgoblin Bulwark.
23. First implementation has no hard inventory capacity and therefore no item-overflow loss.
24. Named items do not roll ordinary procedural affixes unless the item definition explicitly opts in.
25. Lock protects against automatic handling but is separate from reset retention.
26. Auto Equip only performs strict context-free dominance upgrades and never evaluates named/triggered/conditional effects automatically.
27. Location Boss guaranteed gear is a first-clear-per-run reward under Run Progression rules.

## Later / not required for the first playable Location

The following do not block current implementation:

1. Sell / salvage / auto-disposal behavior.
2. Exact retained-gear progression.
3. Exact slot weighting / slot biases.
4. Later-location rarity and Item Level progression.
5. Possible future inventory capacity and overflow rules.
6. Named items that explicitly opt into hybrid fixed + procedural affixes.
