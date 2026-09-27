# 02 — Run & Progression Structure

**Status:** Draft v0.5  
**Date:** 2026-09-28

## Purpose of a run

A run is a temporary build and progression attempt through a world the account has already discovered.

Resetting should matter. The player is pushed back and must again clear location bosses to return to the frontier, but permanent progression increasingly shortens this repeated climb.

The intended feeling is:

> “I am rebuilding a new character through familiar content, but my account knowledge and permanent progression let me do it faster and more intelligently than before.”

A reset should therefore create three things:

1. A new build attempt.
2. A new routing / farming problem.
3. A faster return toward the previous frontier.

## Two progression layers

### Persistent account layer

The following survive reset:

- Collections
- Permanent system unlocks
- Highest world progress ever reached
- Discovered locations and world knowledge
- Achievements
- Mastery
- Challenge completion
- Reset / prestige progression
- Gear-retention upgrades
- Starting-build options unlocked by permanent progression

Collections are a foundational permanent progression system.

### Temporary run layer

The following reset:

- Current EXP
- STR / VIT / AGI / DEX upgrades purchased with EXP
- Current Powers
- Temporary run modifiers
- Temporary build synergies
- Current run boss-clear state
- Most gear, especially early in the game

Gear has its own retention system and is described separately below.

## World state after reset

World discovery is persistent, but **run progression through the world is not fully persistent**.

After a reset:

- the player is pushed back to a start point;
- locations before that start point are treated as already skipped / cleared for this run;
- from the current start point onward, the player must defeat location bosses again;
- defeating those bosses reopens progression toward the previous frontier.

This keeps repeated progression meaningful without forcing the player to restart from the absolute beginning forever.

## Location and Stage structure

World progression is organized hierarchically:

**World → Location → Stage → Encounter**

A normal Location uses the following first implementation baseline:

- 10 normal Stages;
- each Stage requires a number of completed encounters;
- after the final normal Stage comes the Location Boss;
- the system must support a different number of Stages or encounters through data rather than hard-coded constants.

The first implementation uses 10 Stages because it provides a readable progression rhythm, not because every future Location must use exactly 10.

### Encounter progress inside a Stage

Stage progression counts **completed encounters**, not individual enemies killed.

This keeps progression comparable between:

- a single durable enemy;
- a group of three enemies;
- a six-enemy swarm.

A six-enemy encounter therefore counts as one completed encounter unless a special rule says otherwise.

### Auto Push

The player can control whether completed Stage progress automatically advances forward.

**Auto Push ON**

- when the required encounter count is reached, move to the next Stage automatically;
- after the final Stage, attempt the Location Boss when available.

**Auto Push OFF**

- remain on the current Stage after its progression requirement is completed;
- continue farming that Stage indefinitely.

Any currently accessible Stage can be selected manually for farming.

This Push/Farm switch is part of the core loop rather than a later quality-of-life feature.

## Stage Compression

Repeated traversal of old Locations becomes faster through permanent progression.

The reference inspiration is Idle Superpowers' Sublevel progression.

Each Stage has:

```
RequiredEncounters = max(1, BaseRequiredEncounters - PermanentEncounterReduction)
```

First implementation baseline:

```
BaseRequiredEncounters = 10
PermanentEncounterReduction = 0..9
```

Result:

```
10 → 9 → 8 → ... → 1 required encounter per Stage
```

The exact permanent-upgrade source and unlock timing are deferred to Prestige / permanent progression design.

### What Stage Compression does not skip

Stage Compression never automatically skips the Location Boss.

If a Location must be re-cleared in the current run, its Boss must still be defeated.

This preserves the Boss as the benchmark for the current build while making familiar traversal dramatically faster over time.

### Farming after Stage completion

Completing the required encounter count only unlocks progression.

It does not disable the Stage.

The player can keep farming the same Stage indefinitely with Auto Push disabled.

This means Stage Compression accelerates **required traversal** without reducing farm access.

## Overkill as a separate future accelerator

Overkill-style progression is a compatible future system inspired by the reference, where excess damage can carry progression through additional enemies / encounters.

It should remain separate from Stage Compression.

The two systems solve different problems:

- **Stage Compression** reduces how many encounters are required.
- **Overkill-style progression** rewards extreme combat power by clearing routine content faster.

Overkill is not required for the first Location implementation but should not be blocked architecturally.

## World Progress vs EXP

World Progress and EXP are separate systems.

Example:

```
World Position: Location 2 / Stage 7
Current EXP: 12,450
Purchased stat upgrades: STR 18 / VIT 9 / AGI 14 / DEX 11
```

World Progress determines:

- what content is accessible;
- enemy difficulty;
- available drops;
- Power-choice milestones;
- permanent Progress unlock milestones;
- current push frontier.

EXP determines temporary run stat growth.

Enemies grant EXP.

The player manually spends EXP on:

- Strength;
- Vitality;
- Agility;
- Dexterity.

Spending EXP does **not** advance World Progress and does **not** directly grant Powers.

This follows the Idle Superpowers model rather than using a separate XP-driven EXP / primary-stat growth.

## Reset start point

The reset start point should improve as permanent reset progression increases.

Core rule:

**A stronger / later reset should allow the next run to begin farther into the world.**

The exact implementation is intentionally not locked yet.

Two compatible tools can be used:

### 1. Automatic start-range progression

Higher prestige / reset progression moves the minimum start point forward.

Example concept:

- Early resets begin near the start of the world.
- Later resets skip the earliest region.
- Much later resets begin several regions deeper.

Exact thresholds are balance questions.

### 2. Optional progression skip

Inspired by Idle Superpowers, the player may gain access to a quest, requirement or other mechanic that allows already-understood early progression to be skipped.

The important property is **agency**:

- the player can skip repetitive progression when qualified;
- the player may still choose to play older content when it is useful for farming, XP, Collections or gear.

The final system may use one or both approaches.

## Why bosses are re-cleared

Each location boss acts as a checkpoint test for the current build.

Re-clearing bosses after reset provides:

- a reason for the new build to exist;
- repeated moments of progression feedback;
- a benchmark against earlier runs;
- a natural way to feel permanent power growth;
- structure between farming and pushing.

The same boss should become easier across account progression, allowing the player to feel:

> “This used to be a wall. Now I erase it.”

Later start points prevent this from becoming endless repetitive busywork.

## Run phases

### Phase 1 — Rebuild

Immediately after reset:

- Current EXP has reset.
- STR / VIT / AGI / DEX upgrades purchased with EXP have reset.
- Current Powers are mostly gone.
- Most gear has been lost unless protected by the gear-retention system.
- The player begins from the current reset start point.
- A small deterministic starting-build foundation may already exist.

The player rebuilds temporary strength.

### Phase 2 — Re-clear

The player progresses through previously discovered locations again.

Each required location boss must be defeated for this run.

This phase should become dramatically faster over time due to:

- permanent progression;
- better Collections;
- retained gear;
- better starting-build control;
- later reset start points;
- player knowledge.

### Phase 3 — Frontier Push

The player reaches the previous frontier and attempts new progression.

This is where current build quality is tested most strongly.

### Phase 4 — Targeted Farm / Adapt

When a wall appears, the player can:

- farm Collections;
- hunt gear;
- farm more EXP and buy additional STR / VIT / AGI / DEX;
- seek additional Powers;
- change equipment;
- pursue another permanent objective;
- continue adapting the current build.

The player is not forced to reset after failing to push.

### Phase 5 — Voluntary Reset

Reset can be used whenever the player wants to:

- accelerate future progression;
- attempt a different build;
- escape poor build RNG;
- convert accumulated progress into reset value;
- try a better route.

Reset is voluntary.

There is no hard run timer.

## Infinite farming is allowed

The player may theoretically stay in one run indefinitely.

This is intentional.

The game should not force reset through:

- energy;
- run expiration;
- mandatory death reset;
- hard timers.

Instead, the current run should eventually encounter **extremely difficult progression walls** and diminishing push efficiency.

At that point, reset becomes attractive because it improves the next attempt.

The desired thought is:

> “I could keep farming here, but resetting now will make the next push much stronger / faster.”

## Death

Death should remain low-punishment.

Current direction:

- No lives.
- No energy.
- No forced reset.
- No loss of Collections or other permanent account progress.

When the player dies:

- the current encounter remains active;
- enemy HP is preserved;
- temporary combat buffs and stacks are cleared according to Combat rules;
- the hero recovers and revives;
- combat continues.

There is no universal defeat state.

The player decides when continued progression is too inefficient and whether to keep chipping the current enemy, farm elsewhere, change the build, or reset voluntarily.

Death is information about the build and a loss of combat momentum, not a session-ending punishment.

## EXP and stat upgrades

EXP is a temporary run resource.

Enemies grant EXP according to their reward values and current EXP multipliers.

The player spends EXP manually on the four primary attributes:

- Strength;
- Vitality;
- Agility;
- Dexterity.

These purchases are one of the main active progression decisions inside a run.

The player can deliberately farm a Stage with Auto Push disabled to gain more EXP before attempting harder World Progress.

On reset:

- unspent EXP resets;
- EXP-purchased STR / VIT / AGI / DEX upgrades reset;
- Powers reset according to the normal reset rules.

A Power such as `EXP Multi ×5` multiplies EXP income. It does not directly move World Progress or Power milestones.

## First-Location reference cadence

The first Location is normalized to the Idle Superpowers original-timeline progression from reference levels 1–70.

Baseline:

```
10 Stages × 10 required encounters = 100 required encounters
```

Reference milestone mapping:

```
FirstRunEncounterMilestone(L) = ceil(100 × (L - 1) / 69)
```

where `L` is the equivalent reference level.

| Reference equivalent | First-run cumulative encounter | Purpose |
|---:|---:|---|
| 2 | 2 | first Power choice |
| 10 | 14 | gear drops begin / gear loop introduced |
| 20 | 28 | Progress unlock: Stormbrand analogue |
| 30 | 43 | Progress unlock + special first-run Power choice |
| 40 | 57 | next Progress Power unlock |
| 50 | 72 | Progress unlock + normal Power choice |
| 60 | 86 | next Progress Power unlock |
| 70 | 100 | final first-Location Progress unlock + Location Boss |

These encounter numbers are a first-run calibration table, not a separate leveling system.

The denominator is 69 because the reference starts at level 1: reaching level 70 requires clearing levels 1–69. With the baseline 10 enemies per level, that is 690 ordinary enemy kills before the level-70 boss.

The authoritative trigger is normalized World Progress through the Location. When Stage Compression lowers required encounters, the player reaches the same World Progress milestones with fewer actual fights, matching the purpose of the reference Sublevel upgrade.

## Powers come from multiple sources

Powers are **not only automatic progression rewards**.

### Regular source: World Progress milestones

Power choices occur when designated progression milestones are reached.

They are not purchased with EXP and are not triggered by accumulated EXP.

### Event source: key progression events

Important events can also provide rewards, including sometimes Powers.

Examples of key events:

- defeating a location boss;
- defeating a major boss;
- reaching a world milestone;
- completing a challenge;
- completing an important Collection milestone;
- completing a special quest.

The reward does not always need to be a Power.

Possible rewards include:

- a Power choice;
- a reroll;
- a temporary modifier;
- gear;
- a permanent unlock;
- a Collection-related unlock;
- access to a new system.

This prevents the game from reducing all meaningful progression to XP alone.

## Starting-build control

The player should have a limited deterministic foundation after reset.

The goal is similar to Idle Superpowers:

> The player can intentionally lean toward a desired build, but cannot fully guarantee it.

Working model:

- permanent progression unlocks starting options;
- the player selects a small starting foundation;
- the remaining build emerges through random Power choices.

Example intention:

**“I want to build around Bleed.”**

Possible outcomes:

- Bleed + Crit
- Bleed + Attack Speed
- Bleed + AoE
- an unexpected hybrid
- a weak combination that the player decides not to push with

Exact starting-foundation mechanics will be defined in the Powers / Prestige documents.

## Controlled randomness

The target is:

**Player intention + random opportunities + adaptation**

RNG should be capable of ruining the *ideal* build.

It should not ruin the *value of the run*.

If the desired synergy does not appear, the player can:

- adapt;
- use the run for farming;
- complete Collections;
- hunt gear;
- gain account progress;
- reset when convenient.

## Bad run → farming run

This is a core rule.

A run that is poor for pushing should still have economic and progression value.

The player can decide:

> “This build will not beat the frontier boss, but it is good enough to farm this location for the next Collection milestone.”

This is one of the main bridges between run-based buildcraft and persistent SkyBlock-like progression.

## Gear and reset

Gear is **not fully persistent by default**.

Early in progression, gear is expected to reset with the run.

Permanent progression later unlocks the ability to preserve increasing amounts of gear across resets.

Working concept:

**Gear Retention / Protected Gear Slots**

- Early game: little or no gear survives reset.
- Later progression: the player can protect a limited number of items.
- Further progression: more items can be retained.

This creates another strategic reset decision:

**Which pieces are important enough to carry into the next run?**

The exact system name, number of retained items and unlock curve are TBD.

## Why gear retention matters

This model creates several useful effects:

- rare drops are exciting inside the current run;
- reset has meaningful cost;
- permanent progression visibly improves quality of life and build control;
- retained items can define the starting identity of the next build;
- later runs become faster without making gear permanently irrelevant.

Important risk to watch in testing:

A very rare item must not make the player feel permanently trapped in the current run because resetting would be too painful.

The retention system and reset rewards must keep this tension enjoyable rather than punitive.

## Collections

Collections are fully persistent and are one of the central account-progression systems.

They provide the stable value behind farming.

Even when a run resets:

- Collection progress remains;
- Collection unlocks remain;
- newly unlocked build possibilities remain available.

This means repeated farming never completely disappears into a reset.

Collections will be designed after the combat/build mechanics are defined, so their unlocks can support real gameplay interactions rather than arbitrary stat bonuses.

## World map and access

The global map serves two different functions.

### Known world

Shows discovered locations and long-term progression.

### Current-run route

Shows which locations have been re-cleared during the current run and which boss currently blocks forward progression.

After reset, discovered future locations may remain visible but become temporarily inaccessible until the required route is re-cleared or skipped.

## Push and Farm

The player should alternate naturally between:

### Push

- re-clear bosses;
- return to frontier;
- beat new progression;
- unlock new content.

### Farm

- gain EXP;
- advance Collections;
- hunt gear;
- complete passive goals;
- prepare for a wall.

Core relationship:

**Reset → Rebuild → Re-clear → Push → Wall → Farm / Adapt → Push → Reset**

## Key progression events

Not every meaningful event should be a Power choice.

Key events can be used to vary pacing.

Examples:

- Location boss → unlock route + reward
- Major boss → major permanent unlock
- Collection milestone → new mechanic / Power becomes available
- Challenge completion → build-rule unlock
- Reset milestone → later start point or additional retained gear

Exact rewards are deliberately postponed until the dependent systems are designed.

## First region design order

The first region should **not** be locked to specific enemies yet.

Enemy families are mostly thematic presentation until the underlying combat mechanics are defined.

Correct order:

1. Define combat mechanics.
2. Define build archetypes and meaningful checks.
3. Define the first region's teaching sequence.
4. Assign enemy families that visually communicate those mechanics.

For example, if the combat system needs an early AoE lesson, we first define the required encounter behavior and only then decide whether the enemies are goblins, skeletons, slimes or something else.

## First region goals

Even without specific enemies, the first region should eventually teach:

- basic automated combat;
- EXP and manual primary-stat upgrades;
- first Power choices;
- gear;
- Collections;
- multiple-enemy encounters;
- first meaningful build weakness;
- purposeful farming;
- boss re-clearing;
- voluntary reset.

The precise teaching sequence should be created after Combat and initial Power archetypes are defined.

## First reset

The first reset should become available only after the player has enough context to understand its purpose.

Before the first reset, the player should already understand:

- EXP and purchased primary-stat upgrades;
- Powers;
- basic gear;
- Collections;
- pushing;
- farming;
- at least one meaningful wall.

The first reset is voluntary once unlocked.

The player is free to continue the current run for as long as desired.

Exact unlock timing is a balance question.

## First post-reset experience

This is a critical identity moment.

The player should immediately understand:

### Lost

- Run Level
- Current Powers
- Temporary run progression
- Most unprotected gear
- Current-run boss clears

### Kept

- Collections
- Permanent unlocks
- Highest progress reached
- Discovered world
- Achievements / Mastery
- Reset progression
- Protected gear, if any
- Starting-build options

Then the player begins from the current reset start point and must again move through the required location bosses.

The important contrast is:

> The route is familiar, but the build and speed are different.

## Offline progression

Offline play should support farming but not solve build decisions.

Expected offline gains:

- routine resources;
- Collection progress;
- eligible drops;
- passive achievement progress.

Open question:

Whether offline play grants EXP directly.

Offline EXP can increase the spendable EXP pool, but it must not advance World Progress or automatically select Powers.

Offline should never automatically construct the player's build.

## Locked decisions from v0.2

The following are currently considered design decisions rather than hypotheses:

1. Reset is voluntary.
2. Current EXP and EXP-purchased primary-stat upgrades reset.
3. Powers reset.
4. Collections persist.
5. Most gear initially resets.
6. Permanent progression can increase how much gear survives reset.
7. Location bosses must be re-cleared after reset from the run's current start point onward.
8. Higher reset progression allows later start points and/or progression skipping.
9. Power choices are tied to World Progress milestones; EXP is spent on STR / VIT / AGI / DEX.
10. Key progression events also grant meaningful rewards and can sometimes grant Powers.
11. A player may remain in one run indefinitely.
12. Extremely difficult walls and diminishing returns, not timers or formal defeat states, encourage reset.
13. Starting-build control exists but does not guarantee the full build.
14. Enemy/location theming will be chosen after mechanics are defined.
15. A normal Location baseline is 10 Stages followed by a Location Boss.
16. Stage progress counts encounters rather than individual enemy kills.
17. Auto Push ON advances automatically; Auto Push OFF keeps farming the current Stage.
18. Stage Compression permanently reduces required encounters per Stage from a baseline of 10 toward a minimum of 1.
19. Stage Compression never skips a required Location Boss.
20. World Progress and EXP are separate systems; there is no XP-driven Run Level.

## Open questions for later sections

1. Exact reset reward structure.
2. Exact rule for advancing the reset start point.
3. Whether the progression skip is a quest, perk, milestone or combination.
4. Exact protected-gear progression after reset.
5. Exact EXP income and primary-stat upgrade cost curves.
6. Final Power-choice cadence beyond the reference-first early-game baseline.
7. Starting-foundation implementation.
8. Offline EXP behavior.
9. Exact first-Location encounter composition and teaching sequence.
10. Exact source / cost of Stage Compression progression.
11. Whether Overkill-style progression is a Power, permanent upgrade, or both.
