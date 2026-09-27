# Initial Implementation Powers

**Status:** Draft v0.1  
**Date:** 2026-09-28  
**Target:** First 25-Power reference-first implementation pool

## Purpose

This file contains the first concrete Power content planned for game implementation.

The first pool intentionally maximizes reuse of **Idle Superpowers** mechanics and values.

For v0.1:

- all 25 Powers have direct Idle Superpowers analogues;
- reference coefficients are reused as the prototype baseline;
- unlock order is adapted to our progression structure but stays close to the reference;
- custom Crit / Bleed / Poison Powers are intentionally postponed to the next content wave;
- multi-enemy changes are introduced only where 1v1 reference semantics are ambiguous or clearly scale with incoming enemy count.

## Reference snapshot

Primary reference pages checked on 2026-09-28:

- https://idle-superpowers.fandom.com/wiki/Powers
- https://idle-superpowers.fandom.com/wiki/Progress
- https://idle-superpowers.fandom.com/wiki/Achievements
- https://idle-superpowers.fandom.com/wiki/Builds

Reference values marked with `*` on the wiki are affected by Power Mastery in Idle Superpowers.

For the initial implementation, listed values below are the **base pre-Mastery values**.

## Multi-enemy event-scope rules

The reference game is primarily 1v1, so the following rules define how equivalent effects behave in our multi-enemy combat.

### Source-enemy rule

If an effect triggers **when the hero is hit** and affects "the enemy", it affects the enemy that caused that hit.

Examples:

- reflect damage;
- enemy Attack Speed reduction;
- enemy attacks itself.

### Primary-target rule

If an effect has no source enemy and the reference simply says "damage the enemy", it affects the hero's current primary target.

### Incoming-hit frequency

Effects triggered on getting hit initially trigger from every valid incoming hit.

This preserves reference semantics but can become much stronger against 4–6 enemies.

These Powers are explicitly marked **Normalize/Test** and should be benchmarked before balance is considered stable.

### Death-damage rule

A reference death effect that damages "the enemy" targets the current primary enemy in initial implementation v0.1.

It does **not** automatically hit all enemies.

AoE death variants can be separate future Powers.

### AoE proc rule

The first 25-Power pool does not depend on AoE attacks.

When AoE Powers are introduced later, proc rules must explicitly define whether secondary targets generate independent on-hit rolls.

Do not assume every secondary hit triggers every Power.

---

# Starting PowerDex

These three Powers are available on a fresh account.

The first Power choice presents all three.

## P001 — Giant's Might

**Reference:** Super Strength  
**Initial effect:** Strength ×10  
**Reference unlock:** Automatic  
**Our unlock:** Starting PowerDex  
**Family:** Strength / Max Damage  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Simple stat multiplier

**Primary synergies:**
- Force Grip: Strength raises Max Damage, then Force Grip multiplies Max Damage.
- Executioner's Instinct: multiplicative baseline plus missing-enemy-HP scaling.
- Radiation Curse: increases the Max Damage used by death burst.

---

## P002 — Windstep

**Reference:** Super Speed  
**Initial effect:** Agility ×10  
**Reference unlock:** Automatic  
**Our unlock:** Starting PowerDex  
**Family:** Agility / Attack Speed / Evasion  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Simple stat multiplier

**Primary synergies:**
- Spider Instinct: more Agility plus enemy Attack Speed/Evasion debuffs.
- Mirror Veil: Agility increases Evasion, making defensive avoidance stronger.
- Later Evasion→Damage conversions.

---

## P003 — Precision Training

**Reference:** Super Dexterity  
**Initial effect:** Dexterity ×10  
**Reference unlock:** Automatic  
**Our unlock:** Starting PowerDex  
**Family:** Dexterity / Min Damage / Accuracy  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Simple stat multiplier

**Primary synergies:**
- Eagle Eye: Accuracy and Min Damage reinforce each other.
- Blood Drinker: higher reliable damage increases sustain value.
- Future Crit/Bleed extensions.

---

# First-reset unlock

## P004 — Iron Constitution

**Reference:** Super Vitality  
**Initial effect:** Vitality ×10  
**Reference unlock:** Time Travel Level 1  
**Our unlock:** First voluntary reset completed  
**Family:** Vitality / HP / Regeneration / Block  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Simple stat multiplier

**Primary synergies:**
- Healing Focus.
- Calm Guard.
- Stone Form.
- Blood Drinker.

---

# World Progress unlock track

This track intentionally mirrors the early Idle Superpowers `Progress` Power sequence.

It teaches one new combat relationship at a time.

## P005 — Stormbrand

**Reference:** Electrokinesis  
**Initial effects:**
- 50% chance to ignore 50% of enemy Block.
- 20% chance to Stun the enemy for 1 second on hit.
- Stun cooldown: 1 second.

**Reference unlock:** Progress Level 2 / reach level 20  
**Our unlock:** First Location reference-equivalent level 20 milestone  
**Family:** Block Bypass / CC  
**Trade-off:** None  
**Multi-enemy adjustment:** No immediate change; proc applies to current hit target. Re-evaluate once AoE attacks exist.  
**Implementation:** OnHit + proc chance + stun cooldown + block bypass

**Primary synergies:**
- Windstep / Spider Instinct: more attacks create more proc opportunities.
- Dragon Breath: strong anti-Block package.
- Pain-Forged Strength: offensive build that can also suppress enemies.

---

## P006 — Ghost Step

**Reference:** Phasing  
**Initial effects:**
- 20% chance to directly attack after getting hit.
- Attack first at encounter start.

**Reference unlock:** Progress Level 3 / reach level 30  
**Our unlock:** First Location reference-equivalent level 30 milestone  
**Family:** Counterattack / Initiative  
**Trade-off:** None  
**Multi-enemy adjustment:** **Normalize/Test.** More enemies create more incoming-hit proc opportunities.  
**Implementation:** OnHitTaken → extra attack; encounter-start attack-charge advantage

**Primary synergies:**
- Stone Form: slower normal attacks can be supplemented by counterattacks.
- Pain-Forged Strength: incoming hits both buff Strength and can trigger an attack.
- Spike Skin / Flame Ward: reactive-build core.

---

## P007 — Flame Ward

**Reference:** Pyrokinetic  
**Initial effects:**
- 100% chance to damage the source enemy for 20% of damage when getting hit.
- 100% chance to ignore 20% of enemy Block.

**Reference unlock:** Progress Level 4 / reach level 40  
**Our unlock:** First Location reference-equivalent level 40 milestone  
**Family:** Reactive Damage / Block Bypass  
**Trade-off:** None  
**Multi-enemy adjustment:** **Normalize/Test.** Reflect effect can trigger much more often in swarm encounters.  
**Implementation:** OnHitTaken → counter damage to source enemy; passive block bypass

**Primary synergies:**
- Stone Form.
- Spike Skin.
- Ghost Step.
- Iron Constitution.

---

## P008 — Force Grip

**Reference:** Telekinesis  
**Initial effect:** Max Damage ×10  
**Reference unlock:** Progress Level 5 / reach level 50  
**Our unlock:** First Location reference-equivalent level 50 milestone  
**Family:** Max Damage  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Simple derived-stat multiplier

**Primary synergies:**
- Giant's Might.
- Radiation Curse.
- Executioner's Instinct.

**Reference-known combo:** Super Strength + Telekinesis + Radiation Body is an established early reference build.

---

## P009 — Battle Insight

**Reference:** Psychometry  
**Initial effects:**
- Accuracy ×5.
- EXP Multi ×5.

**Reference unlock:** Progress Level 6 / reach level 60  
**Our unlock:** First Location reference-equivalent level 60 milestone  
**Family:** Accuracy / Progression  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Stat multiplier + EXP-income multiplier

**Balance note:** EXP ×5 is intentionally copied from the reference. It accelerates spendable stat growth but does not directly advance World Progress or Power milestones.

**Primary synergies:**
- Eagle Eye.
- Precision Training.
- Any build that benefits from faster STR / VIT / AGI / DEX growth.

---

## P010 — Trickster Form

**Reference:** Shapeshifting  
**Initial effects:**
- 10% chance that an attacking enemy attacks itself instead.
- Attack first at encounter start.

**Reference unlock:** Progress Level 7 / reach level 70  
**Our unlock:** First Location reference-equivalent level 70 milestone  
**Family:** Control / Initiative  
**Trade-off:** None  
**Multi-enemy adjustment:** Source-enemy rule. Each attacking enemy resolves its own self-hit chance. **Normalize/Test** in large groups.  
**Implementation:** Incoming-attack replacement + encounter-start initiative

**Primary synergies:**
- Mirror Veil.
- Calm Guard.
- Ghost Step.

---

## P011 — Dragon Breath

**Reference:** Fire Breathing  
**Initial effect:** 50% chance to ignore 70% of enemy Block.  
**Reference unlock:** Progress Level 8 / reach level 80  
**Our unlock:** Early World Progress milestone 8  
**Family:** Block Bypass  
**Trade-off:** None  
**Multi-enemy adjustment:** None before AoE attacks are introduced.  
**Implementation:** OnHit block-bypass roll

**Primary synergies:**
- Stormbrand.
- High Attack Speed builds.
- Executioner's Instinct against durable armored enemies.

---

## P012 — Domination

**Reference:** Mind Control  
**Initial effects:**
- 30% chance that an attacking enemy attacks itself instead.
- EXP Multi ×10.

**Reference unlock:** Progress Level 9 / reach level 90  
**Our unlock:** Early World Progress milestone 9  
**Family:** Control / Progression  
**Trade-off:** None  
**Multi-enemy adjustment:** **Normalize/Test.** Independent enemies create more self-hit opportunities.  
**Implementation:** Incoming-attack replacement + XP multiplier

**Balance note:** The reference's EXP ×10 is intentionally retained. It strongly accelerates spendable stat growth but does not directly change Power-choice cadence.

**Primary synergies:**
- Trickster Form.
- Calm Guard.
- Battle Insight for extreme EXP and primary-stat acceleration.

---

## P013 — Pain-Forged Strength

**Reference:** Pain Muscles  
**Initial effect:** On getting hit, Strength ×1.1 for 5 seconds. Trigger chance: 100%.  
**Reference unlock:** Progress Level 10 / reach level 100  
**Our unlock:** Early World Progress milestone 10  
**Family:** Reactive Strength / Buff Stacking  
**Trade-off:** Requires taking hits to realize value.  
**Multi-enemy adjustment:** **High-priority Normalize/Test.** Six independent enemies can produce far more buff events than the 1v1 reference.  
**Implementation:** OnHitTaken → timed Strength multiplier

**Initial rule:** Start with the reference value and normal buff-stacking behavior. Do not pre-nerf until measured.

**Primary synergies:**
- Ghost Step.
- Stone Form.
- Giant's Might.
- Spike Skin.

---

# Stat-achievement unlocks

These Powers reward the player for using specific parts of the combat system.

## P014 — Executioner's Instinct

**Reference:** Fear Strength  
**Initial effect:** Increase Strength by 20% for each missing 1% of the current primary enemy's Health.  
**Reference unlock:** Strong Level 1  
**Our unlock:** Strength Achievement I  
**Family:** Strength / Execute Scaling  
**Trade-off:** Weakest at full enemy HP; strongest near the kill.  
**Multi-enemy adjustment:** Uses current primary target only.  
**Implementation:** Dynamic Strength modifier based on target missing-HP percentage

**Primary synergies:**
- Giant's Might.
- Force Grip.
- Dragon Breath against armored bosses.

---

## P015 — Feline Grace

**Reference:** Cat Power  
**Initial effects:**
- Agility ×5.
- Max Damage ×5.

**Reference unlock:** Fast Level 2  
**Our unlock:** Agility Achievement II  
**Family:** Agility / Max Damage  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Two stat multipliers

**Primary synergies:**
- Windstep.
- Spider Instinct.
- Future Agility/Evasion conversion Powers.

---

## P016 — Spider Instinct

**Reference:** Spider Power  
**Initial effects:**
- Agility ×3.
- 10% chance on hit to multiply enemy Attack Speed by 0.5 for 1 second.
- 10% chance on hit to multiply enemy Evasion by 0.5 for 1 second.

**Reference unlock:** Fast Level 1  
**Our unlock:** Agility Achievement I  
**Family:** Agility / Debuff  
**Trade-off:** None  
**Multi-enemy adjustment:** Debuffs apply to the hit target. No change until AoE proc scope is introduced.  
**Implementation:** Agility multiplier + two OnHit debuff rolls

**Primary synergies:**
- Windstep.
- Stormbrand.
- Feline Grace.

---

## P017 — Eagle Eye

**Reference:** Eagle Eyes  
**Initial effects:**
- Accuracy ×5.
- Increase Min Damage by 20% of Accuracy.

**Reference unlock:** Mr Level 1 / Accuracy achievement  
**Our unlock:** Accuracy Achievement I  
**Family:** Accuracy / Min Damage Conversion  
**Trade-off:** None  
**Multi-enemy adjustment:** None  
**Implementation:** Accuracy multiplier + conversion

**Primary synergies:**
- Precision Training.
- Battle Insight.
- Blood Drinker.

---

## P018 — Stone Form

**Reference:** Stone Body  
**Initial effects:**
- Strength ×2.
- Block ×5.
- Attack Speed ×0.5.
- Evasion ×0.5.
- Regeneration ×0.25.

**Reference unlock:** Blocky Level 1  
**Our unlock:** Block Achievement I  
**Family:** Block / Strength / Trade-off  
**Trade-off:** Major reduction to Attack Speed, Evasion and Regeneration.  
**Multi-enemy adjustment:** None directly; becomes more strategically valuable against many attackers.  
**Implementation:** Five stat multipliers

**Primary synergies:**
- Spike Skin.
- Flame Ward.
- Ghost Step.
- Pain-Forged Strength.

---

## P019 — Spiked Armor

**Reference:** Spike Skin  
**Initial effect:** 50% chance to damage the source enemy for 50% of damage when getting hit.  
**Reference unlock:** Blocky Level 2  
**Our unlock:** Block Achievement II  
**Family:** Reactive Damage  
**Trade-off:** Requires being hit.  
**Multi-enemy adjustment:** **Normalize/Test.** Incoming-hit frequency scales with enemy count.  
**Implementation:** OnHitTaken → proc → counter damage

**Primary synergies:**
- Stone Form.
- Flame Ward.
- Iron Constitution.
- Pain-Forged Strength.

---

## P020 — Crippling Presence

**Reference:** Cockroach Power  
**Initial effects:**
- Accuracy ×3.
- Vitality ×0.5.
- 50% chance on hit to multiply enemy Vitality by 0.75 for 3 seconds.

**Reference unlock:** Healthy Level 1  
**Our unlock:** Vitality Achievement I  
**Family:** Accuracy / Enemy Vitality Debuff / Trade-off  
**Trade-off:** Vitality ×0.5.  
**Multi-enemy adjustment:** Debuff applies to the hit target.  
**Implementation:** Two stat multipliers + timed enemy Vitality debuff

**Primary synergies:**
- Eagle Eye.
- Precision Training.
- High Attack Speed builds that keep the debuff active.

---

## P021 — Calm Guard

**Reference:** Calming  
**Initial effects:**
- Vitality ×2.
- 50% chance when hit to multiply source enemy Attack Speed by 0.5 for 1 second.

**Reference unlock:** Healthy Level 2  
**Our unlock:** Vitality Achievement II  
**Family:** Vitality / Defensive Debuff  
**Trade-off:** None  
**Multi-enemy adjustment:** **Normalize/Test.** Each attacker can independently trigger the slow against itself.  
**Implementation:** Vitality multiplier + OnHitTaken source-enemy debuff

**Primary synergies:**
- Iron Constitution.
- Stone Form.
- Trickster Form / Domination.

---

## P022 — Healing Focus

**Reference:** Healing  
**Initial effects:**
- Vitality ×5.
- Regeneration ×10.
- Max Damage ×0.5.
- Min Damage ×0.5.

**Reference unlock:** Healer Level 3  
**Our unlock:** Regeneration Achievement III  
**Family:** Sustain / Major Trade-off  
**Trade-off:** Both Min and Max Damage are halved.  
**Multi-enemy adjustment:** None  
**Implementation:** Four stat multipliers

**Primary synergies:**
- Iron Constitution.
- Calm Guard.
- Stone Form for extreme defense, with severe offensive cost.

---

## P023 — Blood Drinker

**Reference:** Matter Ingestion  
**Initial effects:**
- Vitality ×5.
- Regeneration ×2.
- 25% chance to heal for 10% of damage on hit.

**Reference unlock:** Accurate Level 4  
**Our unlock:** Dexterity Achievement IV  
**Family:** Sustain / OnHit  
**Trade-off:** None  
**Multi-enemy adjustment:** No change in single-target prototype. AoE healing proc scope must be defined later.  
**Implementation:** Two stat multipliers + OnHit heal proc

**Primary synergies:**
- Precision Training.
- Eagle Eye.
- Fast attack builds.

---

## P024 — Mirror Veil

**Reference:** Illusion Creation  
**Initial effects:**
- 10% chance that an attacking enemy attacks itself instead.
- Evasion ×5.

**Reference unlock:** Miss Level 3  
**Our unlock:** Evasion Achievement III  
**Family:** Evasion / Control  
**Trade-off:** None  
**Multi-enemy adjustment:** Source-enemy rule. **Normalize/Test** in group encounters.  
**Implementation:** Evasion multiplier + incoming-attack replacement

**Primary synergies:**
- Windstep.
- Trickster Form.
- Domination.

---

## P025 — Radiation Curse

**Reference:** Radiation Body  
**Initial effects:**
- Regeneration ×0.2.
- 100% chance to damage the source enemy for 50% of damage when getting hit.
- 100% chance to ignore 50% of enemy Block.
- On death, damage the current primary enemy for 200% Max Damage.

**Reference unlock:** Ouch Level 2 / cumulative damage taken  
**Our unlock:** Damage Taken Achievement II  
**Family:** Death / Reactive Damage / Block Bypass  
**Trade-off:** Regeneration ×0.2.  
**Multi-enemy adjustment:** **High-priority Normalize/Test.** Reactive damage triggers per incoming hit; death damage remains single-target in v0.1.  
**Implementation:** Regen modifier + OnHitTaken damage + block bypass + OnDeath damage

**Primary synergies:**
- Giant's Might.
- Force Grip.
- Ghost Step.
- Stone Form can be either synergy or anti-synergy depending on whether the goal is survival or rapid death cycling.

**Reference-known combo:** Super Strength + Telekinesis + Radiation Body is a documented early progression build in the reference community.

---

# Initial unlock sequence

The first Location is normalized to the Idle Superpowers original-timeline progression from levels 1–70.

| Phase | Newly available Powers | Approx. total PowerDex |
|---|---|---:|
| Fresh account | P001–P003 eligible for first choice | 3 |
| Reference-equivalent level 20 | P005 Stormbrand unlocks | 4 |
| Reference-equivalent level 30 | P006 Ghost Step unlocks + special first-run Power choice | 5 |
| Reference-equivalent level 40 | P007 Flame Ward unlocks | 6 |
| Reference-equivalent level 50 | P008 Force Grip unlocks + normal Power choice | 7 |
| Reference-equivalent level 60 | P009 Battle Insight unlocks | 8 |
| Reference-equivalent level 70 / first Location Boss | P010 Trickster Form unlocks | 9 |
| First reset | P004 Iron Constitution unlocks | 10 |
| Following progression | P011–P013 and achievement Powers enter over time | 11+ |

The exact display names of the progression milestones can differ from the reference, but the early cadence is intentionally inherited.

# First implementation build clusters

These are not formal classes. They are useful test clusters.

## Max Damage / Burst

Core:
- Giant's Might
- Force Grip
- Executioner's Instinct

Support:
- Dragon Breath
- Stormbrand
- Feline Grace

Question to test:

Can this build delete bosses but struggle with repeated group pressure?

## Reactive Tank

Core:
- Stone Form
- Spiked Armor
- Flame Ward

Support:
- Ghost Step
- Pain-Forged Strength
- Calm Guard

Question to test:

Does having more enemies create an interesting strength for this build without making swarm encounters trivial?

## Accuracy / Min Damage Sustain

Core:
- Precision Training
- Eagle Eye
- Blood Drinker

Support:
- Battle Insight
- Crippling Presence

Question to test:

Can high reliability + sustain compete with Max Damage burst?

## Control / Avoidance

Core:
- Windstep
- Mirror Veil
- Trickster Form

Support:
- Domination
- Spider Instinct

Question to test:

Can control reduce incoming pressure without creating permanent enemy lockout?

## Death / Revive

Core:
- Radiation Curse
- Force Grip
- Giant's Might

Support:
- Ghost Step

Question to test:

Does the reference death build survive adaptation to a multi-enemy encounter model?

# Known high-risk balance ports

These should be implemented at reference values first, then measured.

## Incoming-hit triggers

Highest risk:

- Ghost Step
- Flame Ward
- Pain-Forged Strength
- Spiked Armor
- Calm Guard
- Radiation Curse

Reason:

Several enemies can independently attack the hero, increasing trigger frequency beyond the 1v1 reference.

## EXP multipliers

Reference values:

- Battle Insight: EXP ×5
- Domination: EXP ×10

These behave close to the reference because EXP is spent on primary stats rather than used as a Power-progress bar.

They can still dramatically increase combat growth and therefore indirectly accelerate world pushing.

Keep the reference values first and calibrate only after the first Location economy is playable.

## Control

Highest risk:

- Stormbrand
- Trickster Form
- Domination
- Mirror Veil

Reason:

Independent enemies change the value of stun and self-attack effects.

The design goal is useful crowd control, not permanent shutdown.

# Explicitly postponed from v0.1

The following are intentionally excluded from this first reference-first pool:

- Crit-specific Powers
- Bleed
- Poison
- AoE / Chain / Explosion
- Gold/resource scaling
- Portable Powers
- Power-manipulation Powers
- advanced Death/Revive chains
- Rotation-style weird Powers

These are the next expansion layer after the 25 reference-based Powers are implemented.

# Implementation data requirements

Each Power should ultimately be represented using reusable data where possible.

Minimum fields:

- id
- name
- referenceId
- unlockCondition
- triggers
- conditions
- effects
- targets
- procChance
- cooldown
- duration
- stackRule
- masteryEligibleFields
- tags
- multiEnemyPolicy

The goal is for most of the 25 Powers above to require no unique custom combat code.
