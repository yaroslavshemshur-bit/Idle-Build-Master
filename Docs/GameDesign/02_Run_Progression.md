# 02 — Run & Progression Structure

**Status:** Draft v0.2  
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

- Run Level
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

- Run Level is low.
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
- gain more Run Levels;
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

## Run Level

Each run has a temporary **Run Level**.

Enemies provide XP.

Run Level is one of the main predictable sources of Power acquisition.

Its purposes:

1. Pace the formation of the build.
2. Reward farming and progression.
3. Create a temporary power curve within each run.
4. Make XP efficiency and location choice meaningful.

Possible structure:

- Minor levels → small automatic stat growth.
- Power levels → Choose 1 of 3 Powers.
- Milestone levels → access to stronger Power tiers or special choices.

Exact cadence is a balance question.

## Powers come from multiple sources

Powers are **not only XP rewards**.

### Regular source: Run Level

XP produces recurring Power choices and is the backbone of build formation.

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

- gain XP;
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
- Run XP;
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

- Run Level;
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

Whether offline play grants Run XP directly.

If it does, Power choices must remain pending rather than being selected automatically.

Offline should never automatically construct the player's build.

## Locked decisions from v0.2

The following are currently considered design decisions rather than hypotheses:

1. Reset is voluntary.
2. Run Level resets.
3. Powers reset.
4. Collections persist.
5. Most gear initially resets.
6. Permanent progression can increase how much gear survives reset.
7. Location bosses must be re-cleared after reset from the run's current start point onward.
8. Higher reset progression allows later start points and/or progression skipping.
9. XP grants Powers.
10. Key progression events also grant meaningful rewards and can sometimes grant Powers.
11. A player may remain in one run indefinitely.
12. Extremely difficult walls and diminishing returns, not timers or formal defeat states, encourage reset.
13. Starting-build control exists but does not guarantee the full build.
14. Enemy/location theming will be chosen after mechanics are defined.

## Open questions for later sections

1. Exact reset reward structure.
2. Exact rule for advancing the reset start point.
3. Whether the progression skip is a quest, perk, milestone or combination.
4. Initial number of protected gear slots.
5. Gear slot and affix system.
6. Exact Run Level curve.
7. Power choice cadence.
8. Power rarity / tier structure.
9. Starting-foundation implementation.
10. Offline Run XP behavior.
11. First-region mechanical teaching sequence.
