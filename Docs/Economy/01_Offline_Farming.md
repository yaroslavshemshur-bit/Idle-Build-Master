# 01 — Offline Farming

**Status:** Locked baseline v0.1  
**Date:** 2026-09-28

## Purpose

Define the first implementation formula for rewards earned while the application is not running.

Offline farming is deliberately simple.

It does **not** simulate combat and does not estimate:

- enemy count;
- pack versus single-target performance;
- DPS;
- survivability;
- deaths;
- Boss phases;
- proc frequency;
- Attack Speed;
- AoE efficiency.

The primary progression variable is the **depth of the Stage the player has already completed and chosen to farm**.

The deeper Stage is valuable because its normal reward profile is better.

Offline farming only determines how many virtual encounter clears are awarded during the absence.

---

# 1. Core behavior

When the application goes offline:

1. the active combat encounter is frozen exactly as saved;
2. a separate eligible completed normal Stage is used as the Offline Farm Target;
3. offline time creates virtual clears of that Stage;
4. each virtual clear uses the Stage's normal reward rules;
5. on return, rewards are granted;
6. the frozen combat encounter resumes from its saved state.

Offline farming never:

- advances World Progress;
- completes a new Stage for the first time;
- defeats a required Boss;
- chooses a Power;
- spends EXP;
- equips gear;
- changes the farming target;
- changes the active saved combat encounter.

---

# 2. Baseline constants

First implementation:

```
MaxOfflineTime = 6 hours
OfflineEfficiency = 0.50
MinOfflineEncounterTime = 60 seconds
```

Interpretation:

- at baseline efficiency, one virtual encounter clear is earned every 120 seconds;
- offline farming can never resolve faster than one virtual clear per 60 seconds even if future permanent bonuses improve Offline Efficiency;
- only the first 6 hours of one continuous absence generate rewards in the baseline system.

Future permanent progression may increase:

- Max Offline Time;
- Offline Efficiency.

Those future upgrades must use explicit modifiers and must not change Stage reward definitions.

---

# 3. Formula

Let:

```
AbsenceSeconds = time away from the game
PreviousResidual = fractional clear progress carried from earlier offline periods
OfflineEfficiencyModifier = permanent offline-efficiency modifier
```

Baseline:

```
EligibleSeconds =
min(max(AbsenceSeconds, 0), 6 × 60 × 60)
```

Effective efficiency:

```
EffectiveOfflineEfficiency =
clamp(
    0.50 × OfflineEfficiencyModifier,
    0,
    1
)
```

Baseline modifier:

```
OfflineEfficiencyModifier = 1
```

Clear progress:

```
ClearProgress =
PreviousResidual
+ EligibleSeconds
× EffectiveOfflineEfficiency
/ 60
```

Completed virtual encounters:

```
OfflineClears = floor(ClearProgress)
```

Residual:

```
NewResidual =
ClearProgress - OfflineClears
```

Residual progress is preserved so many short offline sessions do not lose fractional entitlement.

## Baseline example

Player is away for 6 hours:

```
EligibleSeconds = 21,600
OfflineEfficiency = 0.50

ClearProgress =
21,600 × 0.50 / 60
= 180
```

Result:

```
180 virtual encounter clears
```

At 100% future Offline Efficiency:

```
21,600 × 1.00 / 60
= 360 clears
```

The 60-second minimum therefore remains the hard maximum offline clear rate.

---

# 4. Stage depth is the reward driver

Offline clear speed does not inspect encounter composition.

A Stage with:

- one Armored enemy;
- two Fast enemies;
- six Swarm enemies;

uses the same offline clear-rate formula.

The difference in value comes from the Stage's normal reward profile.

A deeper Stage may provide:

- more EXP;
- higher Item Level;
- better source-specific loot;
- later Collection progress;
- later resource tables.

Therefore the core offline decision is:

> Which already-completed Stage do I want to farm while away?

The offline system should not create a second hidden combat-balance model.

---

# 5. Reward resolution

For every virtual clear:

```
ResolveNormalStageRewards(OfflineFarmTarget)
```

Use the same applicable reward definitions as active farming.

This includes, where relevant:

- EXP;
- current frozen-run EXP multipliers;
- procedural gear rolls;
- rarity rolls;
- named-item drop rolls;
- future Gold/resources;
- future Collection progress;
- future passive achievement progress.

Use the dedicated offline RNG stream.

Do not create separate reduced offline drop tables unless a future design explicitly adds one.

## Example: Stage 8

If Stage 8 is the Offline Farm Target and the absence produces:

```
OfflineClears = 180
```

then the reward planner performs the equivalent of:

```
180 Stage-8 encounter reward resolutions
```

including the existing:

- Stage EXP reward profile;
- 35% procedural gear roll;
- 5% Hobgoblin Bulwark named-item roll.

The number of visible enemies in the Stage does not multiply or reduce those 180 clears.

---

# 6. Frozen reward modifiers

The combat fight remains frozen.

Reward modifiers that already existed when the offline period started may still affect rewards if their rules explicitly apply to offline farming.

Example:

- Battle Insight EXP Multi ×5;
- Domination EXP Multi ×10.

Offline-earned rewards do **not** modify the remaining part of the same offline period.

Examples:

- earned EXP is not spent automatically;
- dropped gear is not equipped automatically;
- a newly completed Collection reward does not increase the already-running offline calculation;
- a newly obtained item does not improve later virtual clears from the same absence.

This preserves the rule:

> Offline performs routine work; it does not make strategic decisions.

---

# 7. Eligible Offline Farm Target

Only an **accessible, completed normal Stage in the current run** is eligible.

Bosses are not eligible Offline Farm Targets.

Recommended selection order:

1. explicit Offline Farm Target selected by the player;
2. current completed Stage if Auto Push is OFF;
3. highest completed normal Stage currently accessible in the run;
4. if none exists, no offline farming rewards are generated.

If the saved active fight is a Boss:

- the Boss fight remains frozen;
- offline farming uses the valid normal Stage target separately;
- returning resumes the exact saved Boss state.

If a saved target becomes invalid after reset or content migration, re-evaluate the fallback list rather than farming inaccessible content.

---

# 8. Interaction with Stage Compression

Stage Compression changes how quickly a Stage is reached during active run progression.

It does **not** directly improve offline clear rate.

Once a Stage is completed and eligible for farming:

```
offline clear rate depends on offline constants/modifiers
reward value depends on that Stage's reward profile
```

This prevents Stage Compression from unintentionally becoming a second offline-income multiplier.

---

# 9. Interaction with build strength

The first implementation intentionally does **not** calculate offline speed from combat strength.

The system does not care whether the saved build would clear the Stage online in:

- 2 seconds;
- 20 seconds;
- 2 minutes.

Eligibility has already been proven by completing the Stage.

Active play remains mechanically richer and can clear content much faster than offline.

Offline provides predictable routine accumulation instead of reproducing build-specific combat performance.

---

# 10. Cap behavior

Only:

```
min(AbsenceTime, MaxOfflineTime)
```

earns progress.

Time beyond the cap is discarded for reward purposes.

The return UI should clearly show when the cap was reached.

Example:

```
Away: 11h 42m
Rewarded time: 6h
Offline cap reached
```

Future permanent progression may extend the cap.

---

# 11. Offline does not advance strategy

The following remain pending until the player returns:

- Power choices;
- stat spending;
- equipment decisions;
- selecting a new Stage;
- new push attempts;
- Boss progression;
- strategic Collection reward choices if such choices exist later.

If offline rewards unlock a passive permanent milestone, it may be recorded as earned, but it does not retroactively increase the same offline period.

---

# 12. First implementation acceptance cases

## Case A — short absence

```
Away = 30 sec
Efficiency = 50%
PreviousResidual = 0
```

Result:

```
ClearProgress = 0.25
OfflineClears = 0
Residual = 0.25
```

No progress is lost.

## Case B — two minutes

```
Away = 120 sec
Efficiency = 50%
```

Result:

```
1 virtual clear
```

## Case C — six hours

```
Away = 6h
Efficiency = 50%
```

Result:

```
180 virtual clears
```

## Case D — twelve hours

```
Away = 12h
MaxOfflineTime = 6h
```

Result:

```
only 6h counted
180 baseline clears
```

## Case E — Boss active

Player closes the game during Goblin Chieftain Fortify.

Offline target is Stage 8.

On return:

- Stage 8 offline rewards are granted;
- Goblin Chieftain remains at the exact saved HP / phase / Fortify state;
- no Boss timer advanced during absence.

## Case F — six-enemy Stage

A Stage with six enemies does not receive any offline speed penalty or bonus.

Its reward value is determined by its Stage reward profile, not visible enemy count.

---

# 13. Locked decisions

1. Active combat is frozen while offline.
2. Offline farming is a separate reward calculation.
3. Only completed normal Stages are eligible.
4. Bosses cannot be offline farm targets.
5. Baseline Max Offline Time is 6 hours.
6. Baseline Offline Efficiency is 50%.
7. Minimum offline encounter time is 60 seconds.
8. At baseline, one virtual clear is earned every 120 seconds.
9. Offline clear rate ignores enemy count and encounter composition.
10. Offline clear rate does not use player DPS/survivability/build combat power.
11. Stage depth matters through the Stage's normal reward profile.
12. Normal reward tables and offline RNG are reused.
13. EXP is granted offline but is not spent automatically.
14. Current reward multipliers can apply when explicitly eligible for offline rewards.
15. Rewards earned during the absence do not improve later clears in that same absence.
16. Offline farming never advances World Progress or chooses strategic actions.
17. Fractional clear progress is retained between offline periods.
18. Future permanent progression may improve cap and efficiency but cannot bypass the 60-second minimum without a new design decision.
