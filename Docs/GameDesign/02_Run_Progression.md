# 02 — Run & Progression Structure

**Status:** Draft v0.1  
**Date:** 2026-09-27

## Purpose of a run

A run is **not** a replay of the entire world from the beginning.

A run is the temporary build state of the hero layered on top of persistent world progress.

The world remembers what the player has discovered. The run determines **how the player is currently capable of fighting through that world**.

This distinction is important:

- World progression should feel permanent.
- Build progression should be resettable.
- Old locations should remain useful.
- Resetting should create a new planning problem, not force the player through repeated onboarding.

## Core model

The game has two major progression layers.

### Persistent account / world layer

Expected to survive a reset:

- Unlocked regions and locations
- Highest world progression reached
- Collections
- Gear inventory
- Equipped gear
- Permanent system unlocks
- Achievements
- Mastery
- Challenge completion
- Future prestige / memory progression

### Temporary run layer

Expected to reset:

- Hero run level
- Current Powers
- Temporary run-specific modifiers
- Temporary build synergies created by Powers
- Any future run-only bonuses

Exact currencies are intentionally not defined yet.

## The run fantasy

At the start of a new run, the player should not feel:

> “I lost everything and need to replay the tutorial.”

The intended feeling is:

> “I have my account progression, gear and unlocked world. Now I need to rebuild a temporary combat engine and decide how I want this run to develop.”

The first strategic question after a reset is therefore:

**Where should I rebuild my power?**

Possible answers:

- A safe old location for fast XP
- A location with a Collection milestone the player wants
- A location that drops a desired item
- A harder location with better rewards but higher risk
- The current frontier if the account is already strong enough

## Run phases

A healthy run is expected to move through four phases.

### Phase 1 — Rebuild

Immediately after reset:

- Run Level is low.
- The hero has only the chosen starting foundation.
- Previously unlocked locations remain available.
- The player chooses where to rebuild.

The purpose of this phase is to regain enough temporary power to form a recognizable build.

This phase should become faster as permanent progression improves.

### Phase 2 — Push

The player returns to the current frontier and attempts to progress.

The build is tested against:

- tougher enemies;
- new enemy mechanics;
- encounter composition;
- elite enemies;
- bosses.

If the build succeeds, permanent world progress advances.

### Phase 3 — Targeted Farm / Rebuild

When the player hits a wall, the game should expose several possible responses.

The player may:

- farm a Collection;
- search for a specific gear drop;
- gain additional run levels;
- change equipped gear;
- change farming location;
- pursue a mastery milestone;
- continue the run and adapt future Power choices.

The player is not forced to immediately reset.

A weak push run can become a productive farming run.

### Phase 4 — Reset Decision

Eventually the player decides the current run has reached diminishing returns.

Typical reasons:

- the Power combination is poor for the current wall;
- important synergy pieces did not appear;
- the player wants to try a different build direction;
- persistent farming goals have been completed;
- the next reset can start from a stronger strategic foundation.

Reset is voluntary.

Death is **not** the reset trigger.

## Death

Death should have low punishment.

Current proposal:

- No lives.
- No energy.
- No loss of gear.
- No loss of persistent progress.
- No forced reset.

When the hero dies during push, the current encounter fails.

The player can then:

- retry;
- change gear;
- return to a cleared farming location;
- continue gaining persistent resources;
- reset voluntarily.

Death communicates:

**“Your current setup cannot solve this encounter yet.”**

It should not communicate:

**“You lost your session.”**

## World progression vs run progression

### World progression

World progression is the permanent frontier.

Defeating important milestones permanently unlocks:

- new locations;
- new enemy families;
- new Collections;
- new drops;
- new systems;
- new challenges.

World progression should generally not be reset.

### Run progression

Run progression is temporary combat development.

Its main purpose is to produce Power choices and build evolution.

The player repeatedly rebuilds run progression in different ways while pushing the permanent world frontier.

## Run Level

The current proposal is to use a simple **Run Level**.

Enemies grant run XP during active play.

Run Level has two jobs:

1. Provide predictable temporary power growth.
2. Trigger Power choices.

Not every level needs a major decision.

A possible structure:

- Minor levels: small automatic base-stat growth.
- Power levels: Choose 1 of 3 Powers.
- Milestone levels: access to stronger / rarer Power tiers.

Exact cadence and numbers are balance questions and remain TBD.

## Why Run Level exists

Run Level solves several problems at once.

It gives the player:

- a reason to farm old locations after reset;
- a visible sense of run development;
- a pacing mechanism for Power choices;
- a way to compare safe farming vs dangerous high-XP farming;
- a natural build curve from weak → functional → specialized.

It also creates route optimization.

A player may ask:

> “Do I farm safe enemies quickly, or move to a dangerous location that gives much better XP?”

That decision can later become part of mastery and optimization.

## Power acquisition

At Power milestones, the player receives a random selection.

Baseline proposal:

**Choose 1 of 3 Powers.**

The offered pool depends on:

- Powers permanently unlocked on the account;
- current Power tier;
- future build-affinity systems;
- current run state;
- exclusions / prerequisites where necessary.

The system should avoid offering only meaningless options, but should not guarantee the perfect build.

## Starting foundation

A reset should include a small amount of deterministic control.

Working concept:

Before starting a run, the player chooses a limited **starting foundation** from permanently unlocked options.

Possible forms:

- one starting Power;
- one Trait;
- one remembered Power;
- one build Affinity.

The exact presentation is not locked yet.

The purpose is:

> “I want to attempt a Crit / Bleed run.”

not:

> “The game randomly decided what I am allowed to play.”

However, the starting foundation should not guarantee the complete build.

Random Power choices must still be capable of redirecting the run.

## Controlled randomness

The target relationship is:

**Player intention + random opportunity + adaptation**

Example:

The player starts with a Crit-oriented foundation.

Later choices can lead toward:

- Crit + Bleed
- Crit + Attack Speed
- Crit + Execute
- Crit + AoE
- a hybrid the player did not originally plan

Sometimes the ideal support Power does not appear.

This is intentional.

The player then decides whether to:

- adapt;
- keep pushing;
- turn the run into farming;
- reset after completing useful goals.

## Bad RNG protection

Bad RNG should reduce push efficiency, not erase progress.

A run with weak synergy can still produce:

- permanent gear;
- Collection progress;
- achievements;
- mastery progress;
- persistent resources;
- useful farming time.

This creates an important state:

**Failed push run → successful farm run**

No run needs to become completely worthless.

## Gear and runs

Current direction:

**Gear is persistent.**

Gear does not disappear on reset.

Reasons:

- rare drops retain long-term excitement;
- farming has permanent value;
- gear gives the player a stable baseline between runs;
- Powers can interact with persistent equipment to create different run identities.

The exact itemization system is deferred to the Gear section.

Important constraint:

Gear should support build decisions rather than replace Power buildcraft.

A single high-stat item should not automatically be better for every build.

## Location selection

The player chooses a location from the global map.

Each unlocked location has at least two conceptual purposes:

### Push value

Can this location advance the permanent frontier?

### Farm value

What useful persistent progress does this location provide?

Examples:

- Collection progress
- Specific gear drops
- Enemy-family mastery
- Basic resources
- Achievement progress

A previously completed location should remain relevant because its farm value can remain useful after its push value is exhausted.

## Farming after reset

Because the map is persistent, the player can deliberately select the best rebuilding route.

Example:

1. Reset.
2. Start at Run Level 1 with one chosen build foundation.
3. Farm a safe old location.
4. Reach the first Power choice.
5. Move to a harder location with better XP.
6. Receive another Power.
7. Decide whether the emerging build is ready for the frontier.
8. Push, or continue targeted farming.

This loop should become increasingly efficient as the player learns the game.

## First region — proposed structure

The first region should be mostly linear.

Its purpose is to teach:

- basic combat;
- gear drops;
- Power choices;
- enemy groups;
- Collections;
- the difference between Push and Farm;
- the first meaningful wall;
- the value of revisiting old locations.

Working region:

### 1. Overgrown Road

Enemy themes:

- Slimes
- Small beasts

Primary lessons:

- Basic combat
- Run XP
- First gear drops
- First Collection progress
- First Power choice

Combat profile:

Mostly simple single-target enemies.

### 2. Goblin Camp

Enemy themes:

- Goblins
- Goblin groups

Primary lessons:

- Multiple simultaneous enemies
- AoE value
- Faster enemy waves
- Targeted farming

Combat profile:

Several weak enemies can appear together.

This is the first soft AoE check.

### 3. Spider Den

Enemy themes:

- Spiders
- Venomous enemies

Primary lessons:

- Damage-over-time pressure
- Sustain
- Defensive build considerations
- Revisiting earlier content for a solution

Combat profile:

The hero can win the damage race but still fail from accumulated poison pressure.

This is a good location for the first explicit:

**“Maybe I should farm something elsewhere first.”**

moment.

### 4. Old Graveyard

Enemy themes:

- Skeletons
- Undead
- Necromancer-style enemies

Primary lessons:

- Mixed encounter composition
- Adds / summons
- Need for both single-target and multi-target performance
- First major region boss

Combat profile:

The region boss should test more than raw DPS.

A Necromancer-like boss can combine:

- a priority single target;
- recurring summoned enemies;
- sustained pressure.

This creates the first meaningful build check.

## First region progression shape

Intended onboarding arc:

**Overgrown Road**  
Learn the basic loop.

↓

**Goblin Camp**  
Discover that enemy composition matters.

↓

**Spider Den**  
Discover that raw DPS is not the only solution.

↓

**Return to earlier farming if needed**  
Discover purposeful backtracking.

↓

**Old Graveyard**  
Combine previous lessons.

↓

**Region Boss**  
First major build test.

↓

**First major reset / prestige system becomes relevant**

Exact timing is not yet defined.

## The first deliberate backtrack

The first region should intentionally create a reason to revisit older content.

Example only:

- Spider Den creates a sustain problem.
- A useful sustain-related unlock exists in an earlier Collection.
- The player can also solve the problem through gear or a strong burst build.

The important principle is not the specific reward.

The principle is:

**The game teaches that returning to old content can be an intelligent progression decision.**

There should be multiple viable solutions so the game does not become a disguised quest checklist.

## First region and reset timing

The first reset should not happen before the player understands:

- what Powers are;
- what a build is;
- what a wall feels like;
- what farming an old location can accomplish.

Current preferred direction:

**The first major reset becomes available around the completion of the first region or its boss.**

Why:

- Reset has context.
- The player has already formed a build once.
- The player has experienced enough systems to understand why rebuilding differently could be valuable.
- The first post-reset run can immediately demonstrate the persistent-world model.

Exact session duration remains a balance decision.

## First post-reset experience

This moment is critical.

The player resets and observes:

### Lost

- Run Level
- Current Powers
- Temporary run progression

### Kept

- World unlocks
- Gear
- Collections
- Permanent unlocks
- Achievements / Mastery
- Starting-foundation options

Then the player sees the entire first region still available.

The game asks:

**“Where do you want to rebuild?”**

This should be one of the first moments where the project's identity becomes clear.

## Offline progression in relation to runs

Current recommendation:

Offline play should primarily progress persistent farming outputs.

It can provide:

- basic resources;
- gear drops;
- Collection progress;
- passive achievement progress.

For the first implementation, offline play should **not automatically select Powers**.

Preferred MVP option:

- Run Level progression is primarily active.
- Offline mode uses the last confirmed build.
- Offline does not make strategic build choices.

If later testing shows that offline run XP is important, XP can be banked while Power selections remain pending.

This can be revisited after the active loop is playable.

## Session loop example

1. Player opens the game.
2. Claims offline farming from Goblin Camp.
3. Goblin Collection reaches a milestone.
4. A new build option becomes available.
5. Player resets because the current run is already stalled.
6. Chooses a Crit-oriented starting foundation.
7. Farms an older location for early Run Levels.
8. Receives random Power choices.
9. The run unexpectedly develops toward Crit + AoE.
10. Player returns to the frontier.
11. The new setup breaks the previous wall.
12. A new location opens.
13. A new enemy mechanic exposes another weakness.
14. The player creates a new farming objective.

The session continues through player-created goals.

## Design constraints

The run system should avoid:

- forcing replay of tutorial content;
- mandatory tap gameplay;
- harsh death punishment;
- fully deterministic perfect builds;
- runs that become worthless after bad RNG;
- resets that exist only to multiply all numbers;
- too many temporary currencies;
- automatic offline decision-making.

## Open questions for the next sections

The following remain intentionally unresolved:

1. Exact base combat stats.
2. Exact Run Level curve.
3. Power choice cadence.
4. Power rarity / tier structure.
5. Exact first prestige reward.
6. Starting-foundation implementation.
7. Gear slot count and affix system.
8. Whether farming has selectable difficulty tiers inside each location.
9. Exact offline XP behavior.
10. Exact first-region enemies and Collection rewards.

These should be resolved through the Combat, Powers, Collections and Prestige documents rather than guessed in isolation.
