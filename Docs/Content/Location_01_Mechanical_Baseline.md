# Location 01 — Mechanical & Balance Baseline

**Status:** Draft v0.1  
**Date:** 2026-09-28

## Purpose

Define the first playable Location as real game content rather than a disposable prototype.

This document intentionally defines mechanics before visual theme.

Enemy names, environment, art direction and fantasy dressing will be assigned after the mechanical teaching sequence is stable.

## Structural baseline

The first Location contains:

```
10 normal Stages
10 required encounters per Stage
1 Location Boss
```

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

| Cumulative encounter | Reference equivalent | Event |
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

## Stage blueprint

| Stage | End equivalent level | End P(L) | Primary encounter pattern | Design purpose |
|---:|---:|---:|---|---|
| 1 | 7.9 | 1.61 | Basic ×1 | Learn auto-combat, EXP, first Power |
| 2 | 14.8 | 3.82 | Basic ×1 | Establish stat spending; gear starts during this Stage |
| 3 | 21.7 | 6.87 | Basic ×2 | First clear multi-enemy lesson |
| 4 | 28.6 | 11.04 | Fast ×2 | Feel Attack Speed / Accuracy / Evasion pressure |
| 5 | 35.5 | 16.79 | Armored ×1 | First explicit Block/durability lesson |
| 6 | 42.4 | 24.68 | Armored + Basic + Fast | Mixed-pressure build check |
| 7 | 49.3 | 35.52 | Swarm ×4 | Enemy count becomes a build variable |
| 8 | 56.2 | 50.41 | Elite Armored ×1 | First meaningful mini-wall |
| 9 | 63.1 | 70.88 | Swarm ×6 | Sustain / reactive / multi-hit pressure |
| 10 | 70.0 | 99.00 | Armored + Fast ×2 | Pre-Boss mixed check |
| Boss | 70 | custom | Fortify → Break → Exposed | First full buildcraft hook |

### Stage 8 Elite

First calibration:

```
EliteMultiplier = 1.35
```

Applied to the Armored archetype after its role multipliers.

The Elite is intended to be the first noticeable slowdown, not a mandatory reset wall.

Its placement intentionally occurs near the reference-equivalent level-50 Power unlock / choice sequence so the player can feel a meaningful power jump around this part of the Location.

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

From that point onward:

- any of the six slots can drop;
- ordinary equipment uses procedural affixes;
- named build-defining items are allowed from authored sources;
- encounter composition, not raw enemy count, determines the total reward budget.

## First Boss

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
10. Visual theme and concrete enemy identities remain deliberately unassigned until after the mechanical sequence is accepted.
