# 04 — Powers & Build Archetypes

**Status:** Draft v0.3  
**Date:** 2026-09-28

## Power-system philosophy

Powers are the main source of temporary build identity inside a run.

The target feeling is close to **Idle Superpowers**:

- each Power is a distinct mechanic, not merely another item tier;
- Powers combine into emergent builds;
- the player does not choose a formal class;
- global progression expands the possible Power pool;
- permanent progression gradually gives the player more control over RNG;
- some combinations should become dramatically stronger than ordinary setups.

The game should produce stories like:

> “I started with a Bleed idea, found an Attack Speed interaction, then got a Crit conversion and the entire run changed.”

## Unique Powers inside a run

Baseline rule:

**A normal Power is obtained once per run.**

Receiving the same Power repeatedly is not the default way to scale it.

If the player already owns a Power, it should normally be removed from future random choices unless a specific mechanic explicitly allows duplication.

This keeps every Power choice focused on adding a new interaction rather than repeatedly upgrading the same card.

## No in-run Power levels

The baseline system does not use:

- Hemorrhage Lv1
- Hemorrhage Lv2
- Hemorrhage Lv3

Instead:

- the player obtains Hemorrhage once;
- later choices add other Powers that interact with it;
- permanent systems can improve Hemorrhage outside the run.

This follows the reference philosophy more closely and maximizes combinatorial build growth.

## No standard Power rarity

Powers do not use a default:

- Common
- Rare
- Epic
- Legendary

rarity ladder.

A Power is judged by its mechanics, conditions and synergy value.

Some Powers may be:

- simple;
- build-defining;
- highly conditional;
- rule-breaking;
- late-game unlocks.

But they do not need a rarity color that tells the player what to pick.

This prevents random choices from degenerating into:

> “Always click the highest rarity.”

## Trade-offs

Not every Power needs a downside.

Most Powers may provide positive effects.

However, strong build-defining Powers can use meaningful trade-offs.

Examples:

- much higher Block but lower Attack Speed and Regeneration;
- very high Evasion but lower Vitality;
- high sustain but lower damage;
- much higher Max Damage but reduced Accuracy;
- strong low-HP bonuses that make survival less stable.

Trade-offs should exist because they create interesting build direction, not because every Power requires an artificial penalty.

## Power-choice model

The player chooses from a random subset of unlocked Powers when designated **World Progress milestones** are reached.

EXP does not grant Power choices.

Working baseline:

**Choose 1 from several offered Powers.**

The exact starting offer count is a balance question.

Permanent progression can increase the number of Powers shown per choice.

This is an important RNG-control lever.

As the account unlocks more Powers, the total pool becomes broader. To compensate, later progression can provide tools such as:

- more offered choices;
- rerolls;
- banish / exclude;
- starting Powers;
- protected build directions;
- future filtering or weighting systems.

The intended long-term tension is:

**More possible builds → larger RNG pool → more permanent tools to control that RNG.**

## No hard simultaneous-Power cap

The game does not impose a fixed universal cap such as:

> “You may only own 6 Powers.”

The practical number of Powers available in a run is determined by progression:

- how many Power rewards the player reaches;
- how far the run progresses;
- starting-build systems;
- future carry-over / portable-Power systems;
- special events.

Progression limits the build, not an arbitrary inventory-slot rule.

## Power unlocks are permanent progression

The account does not begin with the full Power library.

New Powers are added to the available pool through permanent goals.

Possible sources:

- Collections;
- Achievements;
- Challenges;
- Mastery;
- boss milestones;
- reset progression;
- special quests;
- later systems.

This is a major pillar of global progression.

A reward can therefore be:

**“This new Power can now appear in future runs.”**

instead of only:

**“+10% Damage forever.”**

## Power-pool progression

The Power pool itself is part of the game's difficulty and strategy.

Early game:

- smaller pool;
- easier to understand;
- easier to assemble simple combinations.

Later game:

- wider pool;
- more unusual mechanics;
- more possible synergies;
- more possible misses;
- stronger need for RNG-control tools.

Permanent progression should expand both:

1. **possibility space**, and
2. **control over possibility space**.

## Prerequisite philosophy

Power relationships should form a **web**, not a strict skill tree.

Hard prerequisites are used only when a Power would otherwise be mechanically meaningless.

Example:

**Poison Can Crit**

If the player has no way to apply Poison, this Power may need a prerequisite or weighting rule.

However, synergy components should usually be allowed to appear in any order.

Example:

- Crit applies Bleed.
- Bleed increases Attack Speed.
- Attack Speed increases Crit Damage.

All three may appear independently.

The player should be able to collect the pieces in different orders.

This increases the chance of assembling 3+ Power combinations and makes runs less deterministic.

## Soft prerequisites and weighting

Before using hard locks, prefer softer tools:

- increase offer weight if related mechanics are already owned;
- reduce offer weight if the effect currently has no use;
- guarantee that at least one choice is broadly functional;
- mark future-synergy Powers clearly;
- allow the player to take a speculative Power before its combo piece appears.

The goal is to avoid two bad extremes:

- choices full of currently useless Powers;
- rigid chains that make multi-Power combos nearly impossible to assemble.

## Hidden build archetypes

There are no formal character classes.

The game should not declare:

- Warrior
- Assassin
- Poisoner
- Tank

as locked identities.

Build archetypes emerge from the Powers the player collects.

The player may describe the run as:

> “Crit/Bleed”

or:

> “Block/Thorns”

but the game system treats these as combinations of mechanics.

## Initial Power families

The Power library should eventually support a broad range similar in spirit to the reference game.

Current design families:

1. Strength / Max Damage
2. Dexterity / Min Damage / Accuracy
3. Agility / Attack Speed
4. Vitality / HP / Regeneration
5. Block / mitigation
6. Evasion / on-Evade
7. Crit
8. Bleed
9. Poison / DoT
10. Reactive / Thorns / on-Hit-Taken
11. Buff stacking / Momentum
12. Low HP / missing HP
13. Death / Revive
14. Crowd Control / enemy debuffs
15. Enemy-stat manipulation
16. Gold / resource scaling
17. AoE / Chain / Explosion
18. Power manipulation / build manipulation

Not every family must appear in the first playable iteration.

The goal is to keep the underlying combat architecture capable of supporting them.

## AoE is a cross-build axis

AoE should not necessarily be treated as a standalone class.

Different archetypes can gain their own multi-target solutions.

Examples:

- Crit → explosion;
- Poison → spread;
- Bleed → splash / rupture;
- Block → retaliatory AoE;
- Kill → chain explosion;
- high Attack Speed → multi-hit secondary targets.

This allows encounter composition to change which version of a build is effective.

## Cross-synergies

The most valuable Powers often convert one mechanic into another.

Examples:

- Evasion → Damage
- Accuracy → Min Damage
- HP → Damage
- Block → AoE
- Crit → Bleed
- Bleed → Attack Speed
- Poison → Crit
- Missing HP → Strength
- Enemy Count → Regeneration
- Kill Count → Attack Speed
- Death → Damage
- Gold → Max Damage
- Buff Duration → stacking potential

The system should intentionally create loops where multiple Powers amplify one another.

## Weird builds are part of the long-term identity

The project should support unconventional builds such as:

- Death / Revive;
- intentional Miss;
- Evasion retaliation;
- low-HP glass cannon;
- Min Damage manipulation;
- high damage variance;
- enemy-count scaling;
- gold/resource-based damage;
- builds that want to be hit;
- builds that intentionally lose temporary stacks and rebuild them.

Not all of these need to ship in the first MVP Power pool.

The combat architecture should not prevent them.

## Power Mastery

Power Mastery is planned as a persistent per-Power progression layer.

Reference direction:

- each Power can gain its own permanent Mastery;
- Mastery survives resets;
- Mastery improves eligible effects of that Power;
- using unusual Powers becomes valuable even when they are not currently meta.

Idle Superpowers uses a model where a Power's Mastery is tied to the highest enemy defeated while using only that Power, and each 100 Mastery improves eligible base effects by roughly 10% of their base value.

For this project, that reference model should be the **starting balance hypothesis**, not an immutable rule.

The exact acquisition condition will be revisited after our world progression and encounter structure are clearer.

## Starting-build control

A reset should provide some deterministic control over the next build.

The player may eventually be able to bring or select a limited number of starting Powers or equivalent build seeds.

This system should:

- express player intent;
- accelerate repeated progression;
- reward reset progress;
- never guarantee the entire desired combo.

Reference-style permanent progression that increases portable / starting Power control is a strong candidate.

Exact implementation belongs in the Prestige document.

## Reference-balance reuse policy

The project should reuse **Idle Superpowers balance as the first baseline whenever the mechanic is functionally equivalent**.

Examples of reusable reference data:

- base attribute relationships;
- Power multipliers;
- Power proc chances;
- buff / debuff durations;
- cooldown ratios;
- Mastery scaling;
- progression multipliers;
- number-of-choice progression;
- reset-quality-of-life upgrade shapes.

### Default process

For an equivalent mechanic:

1. Start with the reference value or curve.
2. Implement it in the prototype.
3. Measure how it behaves in our combat model.
4. Change it only when our differences create a clear problem.

This avoids inventing dozens of arbitrary coefficients before we have playable data.

## Where reference numbers cannot be copied blindly

Our game differs from Idle Superpowers in important ways.

Reference values require adaptation when affected by:

- multiple simultaneous enemies;
- AoE;
- Crit;
- Bleed / Poison;
- independent enemy attack events;
- gear retention across resets;
- different world / boss structure;
- different Power-choice cadence;
- mobile session and monetization pacing.

Example:

A 50% on-hit proc that is reasonable in a 1v1 system may become excessive when one Power can hit six enemies or trigger from many simultaneous events.

Likewise, effects triggered **when hit** become much stronger when six enemies can attack independently.

Therefore:

**Reuse the reference baseline first, but normalize around our event frequency.**

## Balance inheritance hierarchy

When defining a new mechanic, use this order:

### Tier 1 — Direct reference equivalent

Use the reference number / formula as the starting value.

### Tier 2 — Reference mechanic with different event frequency

Preserve the intended power level, then adjust proc chance, cooldown or scaling for our multi-enemy system.

### Tier 3 — New mechanic built from reference components

Estimate its value from similar existing reference effects.

Examples:

- combine a reference damage proc with a reference stun duration;
- use reference stat multipliers as anchors for Crit or DoT conversion.

### Tier 4 — Entirely new mechanic

Only here do we create a new baseline from scratch.

Crit, Bleed and Poison extensions will often fall into Tier 3 or Tier 4.

## Known deliberate deviations from the reference

Even while reusing balance aggressively, some decisions are already intentionally different.

### Attack Speed

Our design uses a soft-cap / diminishing-return philosophy rather than relying only on the exact reference cap behavior.

### Multiple enemies

Our combat supports several simultaneous independent enemies.

### Crit

Crit Chance and Crit Damage are explicit mechanics in our game.

### DoT

Bleed and Poison are planned as deeper build axes.

### Gear reset

Early gear is mostly lost on reset, with retention increasing through permanent progression.

These differences must be considered whenever importing reference values.

## Initial PowerDex rollout

The first prototype should follow the onboarding structure of Idle Superpowers closely.

The player should **not** begin with the full prototype Power library.

### Starting PowerDex

At account start, only three reference-equivalent Powers are eligible for the first Power choice:

- **Strength starter** — reference equivalent of Super Strength: Strength ×10.
- **Speed starter** — reference equivalent of Super Speed: Agility ×10.
- **Dexterity starter** — reference equivalent of Super Dexterity: Dexterity ×10.

These are intentionally simple.

Their purpose is to teach the relationship between the four primary combat attributes before more complex conversion Powers appear.

### First Power choice

Deliberate deviation from the reference:

**After the first progression milestone, the first choice shows all three starting Powers and the player chooses 1 of 3.**

Idle Superpowers begins with the same three Powers in the eligible starter pool but initially shows only one offered option.

Our version gives immediate agency because build choice is the main product fantasy.

The values remain reference-based for the prototype.

### Vitality enters after the first reset

The reference-equivalent **Super Vitality** Power is unlocked after the first reset / prestige event.

Prototype baseline:

**Vitality ×10**

This mirrors Idle Superpowers, where Super Vitality unlocks after the first Time Travel.

This has two benefits:

1. the first run has a simpler offensive stat space;
2. the first reset immediately expands the build pool in a visible way.

## First-run PowerDex expansion

The first run should rapidly expand the PowerDex through progression.

The player begins with 3 available Powers, but should not remain at 3 for long.

The first prototype should use several unlock channels.

### World Progress

World progression unlocks broadly useful mechanics.

Examples:

- Block interaction;
- simple crowd control;
- counter / reactive effects;
- enemy-stat manipulation;
- basic utility Powers.

These are the closest equivalent to the reference game's Progress unlock track.

### Stat Achievements

Using and growing combat stats unlocks Powers related to those stats.

Examples:

- Strength milestones → Strength / Max Damage interactions;
- Agility milestones → Attack Speed / Evasion interactions;
- Dexterity milestones → Accuracy / Min Damage interactions;
- Vitality milestones → HP / Regeneration interactions;
- Block milestones → Block / reactive Powers;
- Evasion milestones → on-Evade Powers;
- Accuracy milestones → hit / Min Damage Powers.

This follows the reference game's achievement-driven Power unlock structure.

### Collections

Collections are our major extension to the reference structure.

Collections should unlock world-themed mechanics and new build axes.

Early candidates:

- Crit interactions;
- Bleed;
- Poison;
- AoE variants;
- enemy-family-specific conversions;
- thematic utility Powers.

Collections should preferably unlock **new possibilities**, not simply give permanent +X% bonuses.

### Challenges

Challenges are reserved for stranger or more rule-breaking Powers.

Examples:

- Death / Revive mechanics;
- intentional Miss builds;
- unusual stat conversions;
- self-damaging builds;
- extreme low-HP mechanics;
- Powers that alter normal combat rules.

### Reset / Prestige progression

Reset progression primarily improves build control and repetition speed.

Candidate rewards:

- more offered Powers per choice;
- Portable Powers;
- starting Powers;
- rerolls;
- banish / exclusion tools;
- later reset start points;
- additional gear retention.

## Prototype Power-library size

Target for the first meaningful prototype:

**Approximately 20–25 implemented Powers.**

This is the implementation library, not the player's starting pool.

Expected exposure curve:

### New account

**3 available Powers**

Simple primary-stat multipliers.

### First run

PowerDex grows toward roughly:

**8–10 available Powers**

The player starts seeing the first conversions and trade-offs.

### Early post-reset progression

PowerDex grows toward roughly:

**12–15 available Powers**

The player can begin forming multi-part synergies intentionally.

### First broader meta loop

PowerDex reaches roughly:

**20–25 available Powers**

At this point RNG-control tools become increasingly important.

Exact counts are prototype targets, not final balance commitments.

## RNG-control progression

A widening PowerDex naturally reduces the probability of finding a specific combo.

Permanent progression should compensate by increasing agency.

### Offered Power count

Idle Superpowers treats the number of offered Powers as a permanent progression axis.

We should reuse this concept.

Our starting value is deliberately higher than the reference because the first choice is 1 of 3.

Later progression can increase the number of visible options.

Exact maximum is deferred to balance testing.

### Portable Powers

Portable Powers are a strong reference mechanic for our reset system.

They allow the player to carry selected build pieces into a new run.

This creates a natural progression:

- Early game: small PowerDex, little RNG control required.
- Mid game: larger PowerDex, first Portable Power.
- Later game: several Portable Powers and more offered choices.

Portable Powers should make a desired direction more likely without guaranteeing the complete finished build.

### Rerolls and future controls

Additional tools may include:

- free or paid reroll;
- banish;
- temporary weighting;
- build-direction bias;
- exclusion of already unwanted families.

These should unlock only as the Power pool becomes broad enough to justify them.

## Reference-first early balance

For the early prototype, reference-equivalent Powers should use reference values whenever possible.

Locked initial examples:

- Strength starter: Strength ×10.
- Speed starter: Agility ×10.
- Dexterity starter: Dexterity ×10.
- Vitality starter after first reset: Vitality ×10.

For subsequent reference-equivalent Powers:

1. copy the reference effect structure;
2. copy the reference baseline coefficient;
3. test it in our combat model;
4. adjust only if multi-enemy combat, Crit, DoT or another deliberate deviation materially changes effective power.

The first prototype is intended to test **our progression structure and multi-enemy adaptation**, not to rebalance every proven reference coefficient from zero.

## Unlock-order principle

The player should learn systems in this order:

**Simple stats → conversions → synergies → rule-breaking mechanics**

Do not introduce Poison, Bleed, Death loops and complex resource conversions before the player understands the baseline stat relationships.

The first Power unlocks should explain the combat system.

Later Power unlocks should challenge the player's understanding of it.

## Content-production goal

A Power should preferably be expressible through reusable data:

- trigger;
- condition;
- target;
- effect;
- magnitude;
- duration;
- stack rule;
- proc chance;
- cooldown.

Only genuinely rule-breaking Powers should require custom logic.

This lets a relatively small development team create a large Power library.

## Locked decisions from v0.1

1. Normal Powers are unique within a run.
2. Duplicate Power selection is not the default scaling model.
3. Powers do not have in-run Lv1/Lv2/Lv3 progression.
4. Powers do not use a standard rarity ladder.
5. Strong build-defining Powers may use significant trade-offs.
6. There is no fixed universal simultaneous-Power cap.
7. Global progression expands the Power pool.
8. Global progression also increases RNG-control tools.
9. Hard prerequisites are used sparingly.
10. Prefer a synergy web over rigid prerequisite chains.
11. Build archetypes are hidden / emergent rather than formal classes.
12. The eventual Power library should be broad, similar in systemic variety to Idle Superpowers.
13. Weird / broken builds are intentionally supported.
14. Power Mastery is planned as persistent progression.
15. Reference balance should be reused as the first baseline whenever mechanics are equivalent.
16. Reference values are adjusted when our multi-enemy or new-mechanic event frequency materially changes their power.
17. Starting PowerDex contains reference-equivalent Super Strength, Super Speed and Super Dexterity.
18. The first Power choice is deliberately 1 of 3, giving more agency than the reference's initial offer count.
19. Super Vitality-equivalent unlocks after the first reset and begins at Vitality ×10.
20. First prototype implementation target is approximately 20–25 total Powers, exposed gradually.
21. World Progress, stat Achievements, Collections, Challenges and Prestige are distinct Power-unlock/control channels.
22. Portable Powers and increased offered-choice count are planned as core RNG-control progression.

## Next design task

Create the first concrete **prototype Power content set** in `Docs/Content/Powers.md`.

For each Power record:

- game-facing working name;
- Idle Superpowers reference analogue, if any;
- exact prototype effect;
- reference baseline value;
- unlock source;
- Power family;
- trade-off, if any;
- important 2-Power synergies;
- important 3+ Power synergies;
- multi-enemy adjustment required or not;
- implementation complexity.

The first pass should target approximately 20–25 Powers and maximize reuse of reference mechanics and balance.
