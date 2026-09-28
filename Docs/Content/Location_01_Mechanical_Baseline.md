# Location 01 — Goblin Outskirts / Goblin Camp

**Status:** Draft v0.4  
**Date:** 2026-09-28

## Purpose

Define the first playable Location as real game content rather than a disposable prototype.

This document defines both the approved mechanical baseline and the first visual/content dressing.

Working Location theme:

**Goblin Outskirts → Goblin Camp**

The player begins on the outskirts of a goblin-controlled area and gradually pushes toward the defended camp and its Chieftain.

The theme is intentionally simple and readable:

- classic fantasy;
- enemy roles are communicated by silhouette, weapon and shield;
- multiple mechanical roles reuse a small number of base bodies;
- visual production stays compatible with simple transform-based animation and limited VFX.

## Structural baseline

The first Location contains:

```
10 normal Stages
10 baseline required encounters per Stage
1 Location Boss
```

The no-compression first run therefore uses 100 physical encounters, but progression milestones are authored on normalized `LocationProgress = 0..100`. Stage Compression reduces physical encounters without moving milestone positions.

Total first-run progression before the Boss:

```
100 required encounters
```

The Location is normalized to the early Idle Superpowers progression arc from reference level 1 through the level-70 Boss.

Stage Compression can later reduce the number of required encounters, but it does not skip the Boss.

## Fresh-run hero baseline

Before EXP purchases:

```
STR = 1
VIT = 1
AGI = 1
DEX = 1
```

EXP is spent manually on these four primary stats.

## Progress milestones

| Location Progress | Reference equivalent | Event |
|---:|---:|---|
| 2 | complete level 1 / reach 2 | First Power choice: 1 of 3 starter Powers |
| 14 | reach 10 | Gear drops begin |
| 28 | reach 20 | Stormbrand unlocks |
| 43 | reach 30 | Ghost Step unlocks |
| 44 | complete 30 / reach 31 | Special first-run Power choice |
| 57 | reach 40 | Flame Ward unlocks |
| 72 | reach 50 | Force Grip unlocks |
| 73 | complete 50 / reach 51 | Normal Power choice |
| 86 | reach 60 | Battle Insight unlocks |
| 100 | reach 70 | Trickster Form unlocks + Location Boss |

## Enemy primary-stat baseline

Balanced ordinary-enemy primary stat:

```
P(L) = 4.306533075 × (10^((L - 1) / 50) - 1)
```

`L` is reference-equivalent progression level.

This gives:

```
P(70) = 99
```

### Archetypes

| Archetype | STR | VIT | AGI | DEX | Mechanical lesson |
|---|---:|---:|---:|---:|---|
| Basic | ×1.00 | ×1.00 | ×1.00 | ×1.00 | Baseline combat |
| Fast | ×0.55 | ×0.65 | ×1.80 | ×1.00 | Attack frequency / evasion pressure |
| Armored | ×1.25 | ×1.75 | ×0.35 | ×0.65 | Block / durability |
| Swarm | ×0.65 | ×0.65 | ×1.55 | ×1.15 | Multiple independent attackers |

For `N` simultaneous enemies:

```
PerEnemyStat =
P(L)
× ArchetypeMultiplier
/ sqrt(N)
```

## Enemy identities

The first Location maps the four mechanical archetypes to a small goblin enemy family.

| Mechanical role | Enemy | Visual language |
|---|---|---|
| Basic | **Goblin Grunt** | Standard goblin with sword or club |
| Fast | **Goblin Scout** | Smaller silhouette, dual daggers / light weapon |
| Armored | **Goblin Guard** | Shield, heavier helmet/armor |
| Swarm | **Goblin Runt** | Smaller body, weak-looking improvised weapon |
| Elite Armored | **Hobgoblin Guard** | Larger armored body, oversized shield |
| Boss | **Goblin Chieftain** | Large silhouette, heavy armor and prominent shield |

Production target:

- Grunt, Scout and Runt may share one base goblin body;
- Guard can reuse that body with shield/armor attachments;
- Hobgoblin Guard and Goblin Chieftain may share a larger base body;
- weapons, scale, headgear and pose provide most of the differentiation.

The player should be able to infer the mechanical role before reading a stat panel.

## Stage blueprint

| Stage | End equivalent level | End P(L) | Encounter | Design purpose |
|---:|---:|---:|---|---|
| 1 | 7.9 | 1.61 | Goblin Grunt ×1 | Learn auto-combat, EXP, first Power |
| 2 | 14.8 | 3.82 | Goblin Grunt ×1 | Establish stat spending; gear starts during this Stage |
| 3 | 21.7 | 6.87 | Goblin Grunt ×2 | First clear multi-enemy lesson |
| 4 | 28.6 | 11.04 | Goblin Scout ×2 | Feel Attack Speed / Accuracy / Evasion pressure |
| 5 | 35.5 | 16.79 | Goblin Guard ×1 | First explicit Block/durability lesson |
| 6 | 42.4 | 24.68 | Goblin Guard + Goblin Grunt + Goblin Scout | Mixed-pressure build check |
| 7 | 49.3 | 35.52 | Goblin Runt ×4 | Enemy count becomes a build variable |
| 8 | 56.2 | 50.41 | Hobgoblin Guard ×1 | First meaningful mini-wall |
| 9 | 63.1 | 70.88 | Goblin Runt ×6 | Sustain / reactive / multi-hit pressure |
| 10 | 70.0 | 99.00 | Goblin Guard + Goblin Scout ×2 | Pre-Boss mixed check |
| Boss | 70 | custom | Goblin Chieftain | Fortify → Break → Exposed; first full buildcraft hook |

### Stage 8 Elite

First calibration:

```
EliteMultiplier = 1.35
```

Applied to the Armored archetype after its role multipliers.

The Elite is intended to be the first noticeable slowdown, not a mandatory reset wall.

Its placement intentionally occurs near the reference-equivalent level-50 Power unlock / choice sequence so the player can feel a meaningful power jump around this part of the Location.

The Hobgoblin Guard should visually read as a mini-boss without introducing a new phase mechanic.

It remains mechanically an Elite Armored enemy:

- large shield;
- increased scale;
- heavier armor;
- stronger hit reaction / screen shake if needed.

It does **not** use the Boss Fortify → Break → Exposed cycle. This preserves that mechanic as the final hook of the Location.

## EXP reward rule

Every encounter owns one total EXP budget.

Visible enemy count does not multiply total EXP reward.

For encounter budget `B` and `N` initial enemies:

```
sum(enemy rewards) = B
```

Default equal split:

```
enemy EXP = B / N
```

This keeps single-target and swarm encounters economically comparable unless a Stage is deliberately authored as a better farming target.

The exact encounter EXP budgets are derived from the reference-normalized EXP curve in the economy document.

## Gear timing

All six equipment slots exist from the beginning.

Drops start at cumulative encounter 14, approximately the normalized reference level-10 milestone.

### Procedural drop chances

```
Normal encounter = 20% procedural gear chance
Stage 8 Hobgoblin Guard = 35% procedural gear chance
Goblin Chieftain = 100% procedural gear drop
```

Normal and Stage 8 procedural rarity:

```
Common 60%
Uncommon 25%
Rare 10%
Epic 4%
Legendary 0.9%
Godlike 0.1%
```

Goblin Chieftain guaranteed item rarity:

```
Rare 60%
Epic 30%
Legendary 9%
Godlike 1%
```

The Boss therefore always gives at least Rare gear and has strongly improved high-rarity odds.

### Item Level

```
ItemLevel = source reference-equivalent level
```

There is no separate Item Level RNG in the first Location.

### Affix count

```
Common = 1
Uncommon = 2
Rare = 3
Epic = 4
Legendary = 5
Godlike = 6
```

First affix is always one of:

```
STR / VIT / AGI / DEX
```

Later affixes can use the 12-stat pool defined in the Gear document.

### Named Stage 8 farm item

Hobgoblin Guard has an independent:

```
5% chance → Hobgoblin Bulwark
```

The named-item roll does not replace the normal 35% procedural gear roll.

The Stage therefore becomes the first explicit targeted gear farm in the game.

From gear unlock onward:

- any of the six slots can drop;
- ordinary equipment uses procedural affixes;
- encounter composition, not raw enemy count, determines EXP reward;
- rarity affects affix count, not affix magnitude.

## First Boss

Identity:

**Goblin Chieftain**

The Boss should visually communicate its defensive mechanic through an oversized shield and heavier armor.

Baseline primary stats:

```
STR = 109
VIT = 238
AGI = 64
DEX = 89
```

Derived approximately:

```
HP = 23,820
Min Damage = 49.5
Max Damage = 267.5
Block = 129.9
Damage Reduction = 56.50%
Accuracy = 189
Evasion = 164
Attacks/sec = 1.714
Regen/sec = 24.8
```

### Boss cycle

```
Normal: 6 sec
→ Fortify
→ Break after 16 successful hits
→ Exposed: 6 sec
→ repeat
```

Fortify:

```
Block ×5
Block ≈ 649.5
Damage Reduction ≈ 86.66%
```

Exposed:

```
Block ×0.25
Block ≈ 32.475
Damage Reduction ≈ 24.52%
```

No extra damage-taken multiplier is used during Exposed in the first implementation.

The Boss has no universal hard-fail timer and does not heal when the hero dies.

Its guaranteed clear reward is awarded once per run. The Boss is not a repeat-farm target in the first implementation. Deliberately navigating away from an unfinished Boss abandons that encounter; returning later starts it fresh.

### Boss presentation

The mechanic should be readable without requiring a tutorial popup.

**Normal**

- Chieftain fights normally with weapon + shield.
- Duration: 6 seconds.

**Fortify**

- Chieftain braces behind the large shield.
- Shield becomes the visual focus.
- A simple 16-segment shield/break indicator shows progress.
- Each successful hit removes one segment.
- Misses do not remove segments.

**Break**

On the 16th successful hit:

- short shield shake / recoil;
- optional small impact burst;
- shield drops or moves out of guard position.

**Exposed**

For 6 seconds:

- Chieftain uses a visibly vulnerable / staggered pose;
- Block is reduced to ×0.25;
- higher damage numbers provide the primary feedback that the build created an opening.

No extra positional mechanics or manual reaction input is required.

The intended learning is:

> Attack frequency and accuracy help create the opening; raw damage and Block bypass determine how much value the build extracts from it.

## Production constraints

The first Location should be producible with a very small art and animation set.

Suggested asset strategy:

### Base bodies

1. Standard Goblin body
2. Large Goblin / Hobgoblin body

### Reusable attachments

- club / sword;
- daggers;
- small shield;
- large shield;
- light helmet;
- heavy helmet / armor pieces.

### Minimal animation set

- idle bob;
- attack squash / recoil;
- hit reaction;
- death fall / scale-down;
- Fortify shield pose;
- Exposed stagger pose.

The goal is to make mechanical differences readable through:

- scale;
- weapon;
- shield;
- silhouette;
- pose;

rather than requiring unique full animation sets for every enemy.

This follows the project's Gear Defenders production reference: gameplay depth should come primarily from systems rather than expensive animation production.

## Balance intent

The first Location should be completable by almost every functional first-run build.

Expected qualitative curve:

- Stages 1–4: fast learning / low friction;
- Stage 5: first readable defensive check;
- Stages 6–7: combinations and multi-enemy pressure;
- Stage 8: first meaningful slowdown;
- Stage 9: strongest group-pressure lesson;
- Stage 10: pre-Boss mixed check;
- Boss: first memorable build-dependent interaction.

A player should already feel that Powers and stat allocation matter, but the first Location should not punish imperfect choices hard enough to cause early churn.

## Locked first implementation decisions

1. Fresh-run hero starts at 1/1/1/1 primary stats.
2. First Location contains 10 Stages × 10 required encounters + Boss.
3. EXP belongs to the encounter budget and is split across enemies.
4. First enemy baseline uses the approved P(L) formula.
5. Basic, Fast, Armored and Swarm coefficients use the table above.
6. Multi-enemy primary stats use 1/sqrt(N) normalization.
7. Stage 8 uses a 1.35× Elite Armored baseline.
8. First Boss uses 109/238/64/89 primary stats.
9. First Boss uses 6s Normal → Fortify ×5 Block / 16 successful hits → 6s Exposed ×0.25 Block.
10. First Location theme is Goblin Outskirts → Goblin Camp.
11. Basic/Fast/Armored/Swarm roles map to Goblin Grunt / Goblin Scout / Goblin Guard / Goblin Runt.
12. Stage 8 Elite is a Hobgoblin Guard and does not use the Boss phase mechanic.
13. First Boss is the Goblin Chieftain.
14. Goblin Chieftain visually communicates Fortify through a large shield and a 16-hit break indicator.
15. First Location art should primarily reuse two base bodies plus equipment/scale/pose variation.
16. Common through Godlike use 1 through 6 affixes respectively.
17. Item Level equals source reference-equivalent level in this Location.
18. AffixPower uses max(1, round(ItemLevel / 5)).
19. Normal encounters have a 20% procedural gear chance after unlock.
20. Stage 8 Hobgoblin Guard has a 35% procedural gear chance plus an independent 5% Hobgoblin Bulwark roll.
21. Normal/Elite rarity chances are 60/25/10/4/0.9/0.1% from Common through Godlike.
22. Goblin Chieftain guarantees one procedural item with Rare/Epic/Legendary/Godlike chances of 60/30/9/1%.
