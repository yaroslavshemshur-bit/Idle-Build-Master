# Items

**Status:** Draft v0.1  
**Date:** 2026-09-28

## Purpose

Define authored named items that act as explicit buildcraft and farming targets.

Procedural ordinary equipment is governed by the Gear document.

# Location 01 — Goblin Outskirts / Goblin Camp

## I001 — Hobgoblin Bulwark

**Type:** Named equipment  
**Slot:** Accessory  
**Source:** Stage 8 — Hobgoblin Guard  
**Drop chance:** 5% per completed Hobgoblin Guard encounter  
**Drop roll:** Independent from the Stage 8 procedural gear roll  
**Item Level:** Source reference-equivalent level

### Unique effect

```
BonusMaxDamage = 50% of current Block
```

Equivalent:

```
MaxDamage += Block × 0.50
```

The effect updates dynamically when Block changes.

### Design role

Hobgoblin Bulwark is the first explicit targeted gear farm.

It teaches that an item can:

- connect two combat stats;
- turn defense into offense;
- create a reason to stop Auto Push and farm a known Stage;
- become more valuable depending on Powers and stat allocation.

Primary synergy directions:

- Strength;
- Vitality;
- Block;
- Iron Constitution;
- Stone Form;
- future Block-scaling effects.

It is intentionally **not required** to defeat the Goblin Chieftain.

### Farming expectation

At a 5% independent drop chance:

```
Expected encounters per drop = 1 / 0.05 = 20
```

Chance to see at least one copy during the mandatory 10 Stage-8 encounters:

```
1 - 0.95^10 ≈ 40.13%
```

This means many players will discover the concept naturally, while players who specifically want it can deliberately farm Stage 8.

### Procedural affixes on named items

Whether Hobgoblin Bulwark also rolls normal procedural affixes is intentionally unresolved.

The locked identity of the item is its unique Block → Max Damage conversion.
