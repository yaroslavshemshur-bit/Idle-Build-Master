# 00 — Reference Economy Baseline

**Status:** Draft v0.3  
**Date:** 2026-09-28

## Purpose

Define the first implementation baseline for EXP, primary-stat purchases and early enemy scaling.

The project is intentionally reference-first.

This document separates:

1. **Reference-verified rules** — directly supported by Idle Superpowers documentation.
2. **Derived math** — mathematically implied by verified rules.
3. **Calibration hypotheses** — provisional formulas used until exact reference internals are measured or replaced through playtesting.

Do not present calibration hypotheses as confirmed Idle Superpowers formulas.

## Reference-verified rules

### EXP is spendable

Idle Superpowers exposes **Current Experience** as a spendable resource.

The player purchases the four primary stats:

- Strength
- Vitality
- Agility
- Dexterity

World / level progression is separate from EXP spending.

### Base stat relationships

The first implementation already inherits the reference combat formulas:

```
MinDamage = 5 + Dexterity / 2
MaxDamage = MinDamage + 2 * Strength

MaxHealth = 20 + 100 * Vitality

Block = Strength / 10 + Vitality / 2

Accuracy = 100 + Dexterity
Evasion = 100 + Agility

AttackSpeedRating = 1 + Agility / 100
AttacksPerSecond = log2(2 * AttackSpeedRating)

RegenPerSecond = 1 + Vitality / 10
```

### Early Progress rewards

Reference Progress achievements:

| Reach level | EXP Multi reward | Additional early reward |
|---:|---:|---|
| 10 | ×1.01 | Shop |
| 20 | ×1.02 | Electrokinesis |
| 30 | ×1.03 | Phasing |
| 40 | ×1.04 | Pyrokinetic |
| 50 | ×1.05 | Telekinesis |
| 60 | ×1.06 | Psychometry |
| 70 | ×1.07 | Shapeshifting |

These EXP multipliers multiply cumulatively.

Therefore the cumulative Progress-only EXP multiplier becomes:

| Reached level | Cumulative multiplier |
|---:|---:|
| 10 | 1.010000 |
| 20 | 1.030200 |
| 30 | 1.061106 |
| 40 | 1.103550 |
| 50 | 1.158728 |
| 60 | 1.228251 |
| 70 | 1.314229 |

This means the seven early Progress achievements alone increase EXP gain by about **31.42%** by level 70.

### Early Power cadence

Reference early cadence:

- complete level 1 → first Power choice;
- Original Timeline only: complete level 30 → bonus Power choice;
- complete level 50 → normal Power choice;
- first story boss appears at level 70 in the Original Timeline.

### Sublevels

Baseline early progression uses multiple enemies per level.

The reference has a permanent Sublevel upgrade that reduces enemies required per level.

A community build description explicitly refers to killing **10 enemies at level 1** to unlock the first Power, which is consistent with the 10-sublevel baseline used for our first implementation.

## First Location normalization

Our first Location contains:

```
10 Stages × 10 encounters = 100 required encounters
```

Reference levels 1–69 contain:

```
69 levels × 10 enemies = 690 ordinary kills
```

Then the level-70 boss appears.

Mapping reference level `L` to first-Location encounter progress:

```
EncounterMilestone(L) = ceil(100 × (L - 1) / 69)
```

Important milestones:

| Reference level | Our cumulative encounter |
|---:|---:|
| 2 | 2 |
| 10 | 14 |
| 20 | 28 |
| 30 | 43 |
| 40 | 57 |
| 50 | 72 |
| 60 | 86 |
| 70 | 100 |

This keeps early reference pacing proportional while fitting our Location structure.

---

# Calibration Hypothesis A — EXP income

## Status

**Not reference-verified.**

Public documentation does not expose the exact early EXP-per-enemy formula.

For implementation calibration, start with:

```
BaseExpPerEnemy(L) = L²
ExpPerEnemy(L) = BaseExpPerEnemy(L) × ExpMulti
```

where:

- `L` = reference-equivalent progression level;
- `ExpMulti` includes unlocked Progress multipliers and Power effects.

Why use a square curve:

- it creates smooth accelerating income;
- it is mathematically compatible with a square stat-cost hypothesis;
- it produces plausible early stat growth under the reference's very large Power multipliers;
- it is trivial to replace if exact reference values are later measured.

## Cumulative EXP under this hypothesis

Assumptions:

- 10 enemies per reference level;
- levels 1–69 are cleared before the level-70 boss;
- Progress EXP multipliers apply from the milestone level onward;
- no Psychometry or other run-specific EXP multiplier;
- no extra farming.

Approximate cumulative EXP:

| Reached level | Cumulative EXP |
|---:|---:|
| 2 | 10 |
| 10 | 2,850 |
| 20 | 24,919 |
| 30 | 87,606 |
| 40 | 214,780 |
| 50 | 434,221 |
| 60 | 779,348 |
| 70 | 1,291,344 |

These are **calibration outputs**, not claimed reference values.

---

# Calibration Hypothesis B — primary-stat upgrade cost

## Status

**Not reference-verified.**

Public documentation confirms that EXP buys primary-stat upgrades, but does not expose the exact early cost function.

Initial calibration hypothesis:

```
CostOfNthUpgrade(n) = n²
```

Cumulative cost for `N` purchases of one stat:

```
TotalCost(N) = N(N + 1)(2N + 1) / 6
```

This gives a useful balance property:

- EXP income grows roughly quadratically with world level;
- repeated stat purchases also become quadratically more expensive;
- farming produces meaningful but diminishing incremental stat growth.

## Equal-spend sanity check

If the player distributes all EXP evenly across all four primary stats under Hypotheses A+B, approximate purchases per stat are:

| Reached level | Approx. purchases in each of 4 stats |
|---:|---:|
| 10 | 12 |
| 20 | 26 |
| 30 | 39 |
| 40 | 53 |
| 50 | 68 |
| 60 | 83 |
| 70 | 98 |

At level 70, four stats at 98 purchases cost:

```
4 × Σ(n², n=1..98) = 1,274,196 EXP
```

leaving roughly:

```
1,291,344 - 1,274,196 ≈ 17,148 EXP
```

This is a useful calibration target because it produces primary stats near 100 before Power multipliers, leaving the reference's ×5 / ×10 / larger Power modifiers as the dominant source of extreme builds.

Again: **this validates internal consistency, not historical accuracy.**

---

# Calibration Baseline C — enemy growth

## Status

**Approved first-implementation calibration formula. It is not claimed as the hidden Idle Superpowers enemy formula.**

Public reference data does not expose a reliable early enemy-stat generation formula.

Use a single early exponential threat curve:

```
EnemyBudget(L) = BaseBudget × 10^((L - 1) / 50)
```

Interpretation:

- approximately ×10 threat every 50 reference-equivalent levels;
- smooth growth rather than abrupt spikes;
- matches the cadence where major build-changing Power choices occur on roughly 50-level intervals.

Relative budget:

| Reference-equivalent level | Relative enemy budget |
|---:|---:|
| 1 | 1.00× |
| 10 | 1.51× |
| 20 | 2.40× |
| 30 | 3.80× |
| 40 | 6.03× |
| 50 | 9.55× |
| 60 | 15.14× |
| 70 | 23.99× |

The first Location therefore spans roughly a **24× baseline enemy-budget range** before the Boss mechanic.

## First-Location primary-stat baseline

The approved first-Location baseline anchors a balanced ordinary enemy near the player's balanced pre-Power primary-stat scale at the end of the Location.

Hero starts each fresh run at:

```
STR = VIT = AGI = DEX = 1
```

For a balanced ordinary enemy, define:

```
P(L) = 4.306533075 × (10^((L - 1) / 50) - 1)
```

where `L` is the reference-equivalent progression level.

This coefficient is chosen so that:

```
P(70) = 99
```

which approximately matches the balanced player's ~98 purchased upgrades per primary stat plus the starting value of 1 under the current EXP/stat-cost reconstruction.

Stage-end values:

| Stage | Equivalent level | P(L) |
|---:|---:|---:|
| 1 | 7.9 | 1.61 |
| 2 | 14.8 | 3.82 |
| 3 | 21.7 | 6.87 |
| 4 | 28.6 | 11.04 |
| 5 | 35.5 | 16.79 |
| 6 | 42.4 | 24.68 |
| 7 | 49.3 | 35.52 |
| 8 | 56.2 | 50.41 |
| 9 | 63.1 | 70.88 |
| 10 | 70.0 | 99.00 |

### Enemy archetype multipliers

Apply role multipliers to `P(L)` before multi-enemy normalization:

| Archetype | STR | VIT | AGI | DEX |
|---|---:|---:|---:|---:|
| Basic | ×1.00 | ×1.00 | ×1.00 | ×1.00 |
| Fast | ×0.55 | ×0.65 | ×1.80 | ×1.00 |
| Armored | ×1.25 | ×1.75 | ×0.35 | ×0.65 |
| Swarm | ×0.65 | ×0.65 | ×1.55 | ×1.15 |

For multi-enemy encounters, the approved first baseline is:

```
FinalEnemyPrimaryStat =
P(L)
× ArchetypeMultiplier
/ sqrt(EnemyCount)
```

An Elite multiplier is applied after the archetype multiplier where explicitly defined by content.

## Budget is not one stat

`EnemyBudget` must be distributed according to enemy role.

Examples:

### Basic

Balanced STR / VIT / AGI / DEX.

### Fast

More AGI / DEX, less VIT.

### Armored

More STR / VIT, less AGI.

### Pack

Lower per-unit budget, several independent attackers.

Do not multiply every stat by the full budget factor blindly.

## Multi-enemy normalization

For a group of `N` simultaneous enemies, first calibration baseline:

```
PerEnemyBudget(N) = SingleEnemyBudget / sqrt(N)
```

Total group budget becomes:

```
GroupBudget(N) = SingleEnemyBudget × sqrt(N)
```

Examples:

| Enemies | Per-enemy budget | Total group budget |
|---:|---:|---:|
| 1 | 1.000× | 1.000× |
| 2 | 0.707× | 1.414× |
| 3 | 0.577× | 1.732× |
| 4 | 0.500× | 2.000× |
| 6 | 0.408× | 2.449× |

This deliberately keeps groups more dangerous than a single enemy while avoiding a naive ×6 HP + ×6 DPS jump.

Because enemies attack independently, reactive Powers still gain extra trigger frequency in groups and must be measured separately.

## Encounter-owned EXP budget

EXP reward is owned by the **encounter**, not multiplied by the number of visible enemies.

For an encounter with total EXP budget `B`:

```
sum(all enemy EXP rewards) = B
```

If rewards are evenly split among `N` initial enemies:

```
EXP per enemy = B / N
```

Summoned/replacement enemies do not create EXP beyond the encounter budget unless explicitly authored.

## Stage Compression reward behavior

The first Location has an authored 100-slot baseline progression/reward curve.

Without compression, each required encounter consumes its corresponding slot.

With Stage Compression:

- progression still crosses the same normalized 0..100 Location Progress range;
- only physically completed encounters grant encounter rewards;
- skipped baseline slots do **not** grant or redistribute their EXP;
- a compressed Stage's remaining encounters sample the Stage's progress span in order, ending at that Stage's final reward profile.

This intentionally means Stage Compression reduces mandatory combat **and mandatory EXP**.

The player can recover more EXP by deliberately farming with Auto Push OFF.

After a Stage is complete, repeat farming uses the Stage's **final/end-of-Stage reward profile** for every repeat encounter.

---:|---:|---:|
| 1 | 1.000× | 1.000× |
| 2 | 0.707× | 1.414× |
| 3 | 0.577× | 1.732× |
| 4 | 0.500× | 2.000× |
| 6 | 0.408× | 2.449× |

This deliberately keeps groups more dangerous than a single enemy while avoiding a naive ×6 HP + ×6 DPS jump.

Because enemies attack independently, reactive Powers still gain extra trigger frequency in groups and must be measured separately.

---

# What is locked vs tunable

## Locked system design

- EXP is separate from World Progress.
- EXP is spent on STR / VIT / AGI / DEX.
- Progress milestones give Power / system unlocks independently of EXP spending.
- Progress EXP multipliers are multiplicative.
- First Location maps reference levels 1–70 to its 100 required encounters.
- Fresh-run starting primary stats are 1/1/1/1.
- First-Location enemy primary-stat baseline uses P(L) anchored to P(70)=99.
- Basic/Fast/Armored/Swarm archetype coefficients above are the first implementation baseline.
- Multi-enemy primary stats are normalized by 1/sqrt(N).
- EXP reward belongs to the encounter and is divided across enemies rather than multiplied by enemy count.
- Gear starts around normalized reference level 10.
- Power unlocks / choices follow the agreed early reference cadence.

## Calibration values — expected to move

- exact EXP per enemy;
- exact cost of the Nth stat purchase;
- exact long-term enemy-budget growth after the first Location;
- future archetype coefficient tuning;
- future multi-enemy normalization tuning;
- later Boss numeric multipliers.

These should be tuned only after the first Location has concrete enemies, gear drops and the Fortify → Break → Exposed Boss.

## Next calculation task

After the first Location enemy roles are selected, calculate:

1. exact Stage 1–10 enemy STR / VIT / AGI / DEX;
2. EXP rewards per enemy and encounter;
3. expected player stat ranges for common spending strategies;
4. expected TTK and deaths per Stage;
5. Boss HP / Block / Fortify requirement / Exposed duration;
6. gear item-level and affix values against the same progression curve.
