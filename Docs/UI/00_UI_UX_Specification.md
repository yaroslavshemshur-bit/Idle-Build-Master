# 00 — UI / UX Specification

**Status:** Draft v0.3  
**Date:** 2026-09-28  
**Platform:** Mobile  
**Orientation:** Portrait

## Purpose

Define the complete first-implementation UI structure for Idle Build Master:

- primary screens;
- navigation;
- first-time user journey;
- returning-player journey;
- all major screen elements;
- modal dialogs;
- popups;
- tooltips;
- notifications;
- empty / locked / error states;
- future screens the UI architecture should not block.

This document is a **product/UI specification**, not a visual style guide.

Exact colors, fonts, iconography, spacing and animation timings will be defined later in the visual UI system.

---

# 1. UI principles

## 1.1 Combat is always the context

The game is about watching the consequences of build decisions.

The UI should therefore keep the player oriented around:

- current Location;
- current Stage;
- current encounter;
- current build;
- current progression goal.

The player should not feel like they leave the game when opening Stats, Powers or Gear.

## 1.2 Idle execution, active decisions

The player does not need to tap to attack.

The UI should emphasize decisions:

- spend EXP;
- choose a Power;
- equip gear;
- choose a Stage;
- toggle Push/Farm;
- inspect why a build succeeds or fails.

Avoid adding fake interaction purely to keep fingers busy.

## 1.3 Information should be layered

Default view should be readable.

Detailed formulas and explanations should be available through:

- info icons;
- tap/hold tooltips;
- detail sheets;
- expandable sections.

Do not place every derived stat and formula on the Combat screen.

## 1.4 Do not hide meaningful trade-offs behind a single score

There is no authoritative Gear Score.

The UI may mark an item as an **obvious upgrade** only when it is strictly non-worse under the conservative Auto Equip rules.

Items with trade-offs must show:

- what is gained;
- what is lost;
- what unique mechanics change.

## 1.5 Interrupt only for decisions that cannot be automated safely

Blocking modal examples:

- Power Choice;
- first-time system tutorial requiring acknowledgement;
- reset confirmation;
- critical save/error recovery.

Non-blocking examples:

- normal gear drops;
- EXP gain;
- Stage completion;
- ordinary unlock notifications;
- common item acquisition.

---

# 2. Navigation model

## 2.1 Persistent bottom navigation

First implementation recommendation:

1. **Combat**
2. **Stats**
3. **Powers**
4. **Gear**
5. **World**

This navigation remains visible on all primary screens unless a blocking modal is open.

### Combat

Current fight and progression status.

### Stats

EXP spending and primary / derived stat inspection.

### Powers

Current-run Powers and permanent PowerDex.

### Gear

Equipped items, inventory and comparison.

### World

Locations, Stages, farming target and progression route.

## 2.2 Secondary systems

Systems not required in the first playable version should not consume permanent bottom-nav slots yet.

Future examples:

- Collections;
- Achievements;
- Mastery;
- Prestige;
- Challenges;
- Shop;
- Settings / account services.

These can initially be reached from:

- contextual unlock buttons;
- World / account header;
- a future More / Menu entry.

The bottom navigation may be revisited once these systems are actually implemented.

## 2.3 Global settings access

A small Settings / Menu icon is available from the top-right of primary screens.

---

# 3. Simulation behavior while navigating UI

## 3.1 Combat continues while browsing non-blocking screens

Combat continues while the player views:

- Stats;
- Powers;
- PowerDex;
- Gear;
- item details;
- World;
- Location details;
- Settings.

The player should be able to inspect and modify the build without losing farming time.

## 3.2 Blocking decisions pause progression

Combat / progression pauses while a blocking modal is active.

First required example:

- Power Choice.

Potential future examples:

- reset confirmation after final confirmation;
- mandatory tutorial choice;
- save conflict resolution.

## 3.3 Opening World does not move the player

The current encounter continues until the player explicitly selects a different Stage / Location.

Merely browsing the map does not change the farming target.

---

# 4. High-level screen map

```
Launch
  ↓
Combat
 ├─ Stats
 ├─ Powers
 │   ├─ Current Run
 │   └─ PowerDex
 ├─ Gear
 │   └─ Item Detail / Comparison
 └─ World
     └─ Location Detail
         └─ Stage Select

Blocking overlays from anywhere:
- Power Choice
- System Unlock
- Boss Intro
- Location Complete
- Offline Rewards
- Reset Confirmation (future)

Non-blocking overlays:
- Item Drop
- Rare Item Drop
- Power Unlocked
- Stage Complete
- New Location
- Tooltip / Info Sheet
```

---

# 5. First-time user journey

The onboarding should mostly happen through the real first Location.

Avoid a separate fake tutorial level.

## Step 1 — First launch

Player enters:

**Combat → Goblin Outskirts → Stage 1**

Visible immediately:

- hero;
- one Goblin Grunt;
- HP bars;
- Stage progress;
- Auto Push state;
- bottom navigation.

One-time coach mark:

> Combat is automatic. Improve your build to progress.

No tap-to-attack tutorial.

## Step 2 — First EXP

After first EXP gain:

- EXP display animates;
- Stats tab receives a one-time attention pulse / badge.

Coach mark:

> Spend EXP to improve Strength, Vitality, Agility or Dexterity.

Opening Stats shows the four upgrade cards.

## Step 3 — First Power

After cumulative encounter 2:

- combat pauses;
- Power Choice modal opens;
- all three starter Powers are shown;
- player chooses exactly one.

After selection:

- short acquisition feedback;
- combat resumes.

## Step 4 — Normal progression

Player sees:

- Stage progress fill;
- Auto Push advance between Stages;
- stats become affordable;
- current Power affects combat.

No additional mandatory tutorial popup.

## Step 5 — Gear unlock

At cumulative encounter 14:

- System Unlock notification: **Gear Drops Unlocked**;
- Gear tab becomes visually active if it was previously subdued;
- next item drop creates a non-blocking item card.

On first item:

- one-time coach mark points to Gear tab.

## Step 6 — First item comparison

Player opens Gear:

- item is highlighted as New;
- tapping it opens Item Detail;
- equipped comparison is visible;
- Equip action available.

One-time help explains:

> Higher rarity means more affixes, not automatically a better item for every build.

## Step 7 — First multiple-enemy encounter

Stage 3 introduces two Goblin Grunts.

One-time non-blocking tooltip:

> Enemies attack independently. Some builds perform differently against groups.

## Step 8 — First Armored enemy

Stage 5 Goblin Guard.

One-time contextual tooltip anchored to Block / shield indicator:

> Block reduces incoming damage. Some Powers can ignore part of it.

## Step 9 — Push vs Farm

After the player completes a Stage for the first time:

Auto Push is already ON by default.

A one-time tooltip on the toggle:

> Turn Auto Push OFF to stay on this Stage and farm it.

Do not require the player to disable it during onboarding.

## Step 10 — Stage 8 named-item farm

When Stage 8 becomes accessible:

- Location detail can show discovered / discoverable special drop area;
- after Hobgoblin Bulwark is first seen, its source becomes permanently visible in known drops.

Do not force the player to farm it.

## Step 11 — Boss

Before first Goblin Chieftain attempt:

- short Boss Intro overlay;
- Boss name;
- one-sentence mechanic hint.

During first Fortify:

- one-time tooltip explains successful hits break Fortify;
- 16-segment break indicator is visible.

## Step 12 — First Location completion

On Boss death:

- Location Complete modal;
- rewards / new unlocks;
- next route unlocked;
- CTA to continue to World.

No forced reset at this point unless later design explicitly changes first-reset timing.

---

# 6. Returning-player journey

Recommended flow:

```
Launch
↓
Offline Rewards if applicable
↓
Combat screen at configured farming target
↓
Review:
- EXP
- gear
- progression
↓
Choose:
- keep farming
- spend EXP
- change gear
- push
- change Stage / Location
```

If a strategic choice became pending while the player was away:

- do not auto-select it;
- present it when the player returns.

Example:

Power Choice remains pending and becomes the first blocking modal after offline rewards.

---

# 7. Global UI shell

Elements shared across primary screens.

## 7.1 Top context bar

Recommended elements:

- current Location name;
- current Stage / Boss label;
- compact progression indicator;
- Settings / Menu button.

Optional contextual icon:

- farming / push state.

## 7.2 Bottom navigation

Five tabs:

- Combat;
- Stats;
- Powers;
- Gear;
- World.

Each supports notification badges.

## 7.3 Badge rules

### Stats badge

May pulse when:

- player receives EXP for the first time;
- tutorial requires opening Stats.

Do **not** permanently show a red badge every time one upgrade is affordable.

That would create constant noise.

### Powers badge

Show for:

- newly unlocked PowerDex entry not yet viewed;
- new Power-related system unlock.

A pending Power Choice uses a blocking modal instead.

### Gear badge

Show count of newly acquired items not yet viewed.

### World badge

Show when:

- new Stage / Location becomes accessible;
- new route unlocks;
- first-time location information becomes available.

---

# 8. Screen — Combat

## Purpose

Main live gameplay screen.

The player should be able to understand within seconds:

- what they are fighting;
- whether they are winning efficiently;
- where they are in progression;
- whether they are pushing or farming.

## Layout zones

Portrait recommendation:

### A. Top context zone

Contains:

- Location name;
- Stage number / Boss label;
- encounter progress;
- Auto Push toggle;
- Settings icon.

Example:

```
Goblin Outskirts
Stage 7
Encounter 6 / 10
Auto Push [ON]
```

### B. Enemy combat zone

Largest visual area.

Contains:

- enemy sprites;
- per-enemy HP;
- status icons;
- target highlight;
- Boss phase indicator where applicable;
- floating damage / healing feedback.

For multiple enemies:

- each enemy must remain individually identifiable;
- current primary target is visually distinct.

### C. Hero combat zone

Contains:

- hero sprite;
- hero HP bar;
- downed / revive state;
- important current buffs/debuffs;
- temporary stack indicators when relevant.

### D. Compact economy / state strip

Contains:

- current EXP;
- optional tap shortcut to Stats;
- newly gained EXP feedback.

Do not place every currency here.

### E. Bottom navigation

Persistent navigation.

## Combat screen interactions

### Auto Push toggle

ON:

- automatically advances when Stage requirement is met.

OFF:

- remains on current Stage.

Tap tooltip / info:

> Auto Push ON advances when the Stage is complete. OFF keeps farming this Stage.

### Stage label tap

Opens Location / Stage selector.

### EXP tap

Opens Stats.

### Enemy tap

Opens Enemy Info sheet.

It does **not** manually retarget combat.

### Hero HP / status area tap

Opens Hero Combat Details sheet.

## Combat states

### Normal encounter

Standard combat.

### Multi-enemy encounter

Several enemy panels / HP bars.

### Elite

Elite label / frame.

### Boss

Additional:

- Boss name;
- phase;
- mechanic indicator;
- phase-specific UI.

### Hero downed

Replace normal HP feedback with:

- Downed label;
- revive progress / HP refill;
- enemy HP remains visible.

Do not show:

- Defeat;
- Retry;
- Game Over.

### Encounter complete

Short non-blocking feedback:

- EXP gained;
- drops;
- Stage progress increment.

Next encounter begins automatically unless blocked by:

- Power Choice;
- Boss transition;
- player moving to another Stage.

---

# 9. Combat — Boss UI

## Boss intro overlay

Shown on first encounter with a Boss in the current account progression.

Elements:

- Boss portrait / silhouette;
- Boss name;
- short mechanic line;
- Continue button.

Goblin Chieftain hint:

> Break his Fortify with successful hits, then punish the Exposed window.

The full mathematical values do not need to be in the intro.

## Boss header

Contains:

- Boss name;
- HP bar;
- phase name;
- phase icon.

## Fortify state

Show:

- shield icon;
- **16-segment break indicator**;
- current segments remaining or broken;
- Fortify label.

Tap info tooltip:

> Successful direct hits damage Fortify. Misses do not. Breaking Fortify makes the Chieftain Exposed.

## Exposed state

Show:

- Exposed label;
- remaining Exposed time;
- obvious visual state change.

Tooltip:

> Block is greatly reduced while Exposed.

## Boss HP after hero death

Must remain visibly unchanged when hero enters Downed state.

This reinforces the persistent-chip rule.

---

# 10. Enemy Info sheet

Opened by tapping an enemy.

Non-blocking bottom sheet.

## Elements

- enemy name;
- type tags, e.g. Armored / Elite / Boss;
- current / max HP;
- Damage range;
- Attack Speed;
- Accuracy;
- Evasion;
- Block;
- relevant active buffs/debuffs;
- special mechanics;
- known drops if discovered.

## Derived stat tooltips

Tap any stat for short explanation.

Example Block:

> Block reduces incoming damage with diminishing returns. Effects that ignore Block reduce its effective value before mitigation is calculated.

Do not expose unnecessary internal IDs.

---

# 11. Hero Combat Details sheet

Non-blocking.

## Elements

- current HP / max HP;
- Regen;
- current Damage range;
- Attack Speed;
- Accuracy;
- Evasion;
- Block;
- active buffs;
- active debuffs;
- temporary stacks;
- cooldowns relevant to visible Powers.

Each modifier should eventually support source explanation.

Example:

```
Strength
Base: 41
Giant's Might: ×10
Pain-Forged Strength: ×1.1 × 4 stacks
Final: ...
```

Full source breakdown can initially be development-only if production scope requires.

---

# 12. Screen — Stats

## Purpose

Spend EXP and understand character stat relationships.

## Header

- current EXP;
- optional EXP multiplier indicator;
- close/back not required because bottom nav remains visible.

## Primary-stat cards

Four cards:

1. Strength
2. Vitality
3. Agility
4. Dexterity

Each card contains:

- icon;
- stat name;
- current value;
- next upgrade cost;
- +1 upgrade button;
- short summary of affected derived stats.

Example Strength:

```
Strength 41
+1 Cost: 1,764 EXP
Affects:
Max Damage
Block
[ +1 ]
```

## Upgrade interaction

First implementation:

- tap buys one upgrade;
- hold repeats purchases at controlled rate.

Do not require hundreds of individual taps at high progression.

Future QoL can add:

- Buy ×10;
- Buy Max;
- saved allocation presets.

These are not required for first playable.

## Insufficient EXP

Button becomes disabled.

Tap / press tooltip:

> Need X more EXP.

No blocking popup.

## Derived stats section

Expandable section below primary stats.

Contains:

- Min Damage;
- Max Damage;
- Max HP;
- Regen;
- Block;
- Accuracy;
- Evasion;
- Attack Speed.

Each shows:

- final value;
- info tooltip.

Optional later:

- source breakdown.

## Primary stat tooltips

### Strength

> Increases Max Damage and Block.

### Vitality

> Increases Max Health, Regeneration and Block.

### Agility

> Increases Attack Speed and Evasion.

### Dexterity

> Increases Min Damage and Accuracy.

## EXP tooltip

> EXP is earned from combat and spent on primary stats. Spending EXP does not advance World Progress.

---

# 13. Screen — Powers

Two internal tabs:

1. **Current Run**
2. **PowerDex**

## Current Run tab

### Purpose

Show the build currently active in this run.

### Elements

- owned Power count;
- Power cards;
- search/filter later if library becomes large.

Power card:

- icon;
- name;
- concise effect;
- mechanic tags;
- tap for detail.

No rarity color hierarchy.

### Empty state

Before first Power:

> Your first Power choice arrives early in the run.

## PowerDex tab

### Purpose

Permanent library / unlock progression.

### Power states

- unlocked;
- locked;
- newly unlocked;
- future Mastery state.

### Unlocked card

Shows:

- icon;
- name;
- effect summary;
- source / unlock category;
- viewed/new marker.

### Locked card

Depending on design intent:

- silhouette;
- exact unlock requirement if known;
- or category hint if intentionally undiscovered.

Do not falsely show rarity.

## Power Detail sheet

Elements:

- name;
- full description;
- current-run ownership state;
- permanent unlock state;
- triggers;
- proc chance;
- cooldown;
- duration;
- stacking rule;
- relevant tags;
- unlock source;
- future Mastery section when implemented.

### Rules terminology tooltips

Examples:

**Independent stacks**

> Each stack has its own duration and expires separately.

**Block Ignore**

> Reduces the target's effective Block for this effect.

**Source enemy**

> The enemy responsible for the triggering attack.

---

# 14. Modal — Power Choice

## Blocking behavior

- pauses combat;
- cannot be dismissed without choosing unless a future system explicitly allows postponing;
- Android/system Back does not discard the choice.

## Elements

Header:

- Choose a Power.

Cards:

- 3 offered Powers in first implementation;
- name;
- icon;
- full readable effect;
- relevant tags;
- current synergy hints only if they can be explained objectively.

Action:

- tap card selects it;
- optional confirmation only for first-ever choice.

Avoid a second confirmation on every Power choice.

## First starter choice

Always shows:

- Giant's Might;
- Windstep;
- Precision Training.

## Future controls

Reserved space / architecture support for:

- reroll;
- banish;
- more offered choices;
- portable Powers.

Do not show disabled fake buttons before systems exist.

---

# 15. Popup — Power Unlocked

Non-blocking unless combined with a Power Choice milestone.

Elements:

- Power icon;
- name;
- “Added to PowerDex”;
- short effect line;
- View button.

If several unlock simultaneously:

- show a compact sequence or grouped modal;
- do not stack multiple independent overlays on top of each other.

---

# 16. Screen — Gear

## Purpose

Manage equipped gear and inventory.

## Header

- Gear title;
- Auto Equip action;
- optional filter/sort.

No universal Gear Score.

## Equipped area

Six slots:

1. Weapon
2. Head
3. Chest
4. Gloves
5. Boots
6. Accessory

Each slot shows:

- equipped item icon;
- rarity frame;
- Item Level;
- empty state if none.

Tapping equipped item opens Item Detail.

## Inventory area

Scrollable list/grid.

Item card displays:

- item name;
- slot icon;
- rarity;
- Item Level;
- 1–2 most relevant affixes;
- New marker;
- lock icon if locked;
- named-item marker if authored unique.

Do not show “+15% better” universal recommendation.

## Sort options

First implementation recommendation:

- Newest;
- Rarity;
- Item Level;
- Slot.

Future:

- stat;
- source;
- custom filters.

## Filters

- All;
- by slot.

Advanced filters can wait.

## Auto Equip

Button opens Auto Equip explanation / performs conservative pass.

Rule:

Auto Equip only replaces when the new item is an obvious non-tradeoff upgrade and no unique mechanic is lost.

If no safe replacement exists:

> No obvious upgrades found.

Do not force equipment changes.

---

# 17. Item Detail / Comparison sheet

Opened from inventory or equipped slot.

## Elements

- item name;
- rarity text + frame;
- slot;
- Item Level;
- all affixes;
- unique effect if named item;
- source if discovered;
- lock toggle.

## Comparison section

When comparing to equipped item:

Two-column or gained/lost presentation.

Example:

```
+14 STR
+21 Accuracy
-10 VIT
Lost: On Evade → Attack Speed
```

Derived-stat changes may be shown where practical.

Avoid green/red coloring as the only signal; use +/- text and icons.

## Actions

For inventory item:

- Equip;
- Lock / Unlock.

For equipped item:

- Unequip only if empty slots are allowed by final UX;
- Lock / Unlock.

Sell/Salvage is not shown until that system is designed.

## Named item example

Hobgoblin Bulwark:

> Max Damage increases by 50% of current Block.

Tooltip on “current Block” clarifies it updates dynamically.

---

# 18. Item drop presentation

## Common / Uncommon

Non-blocking compact loot toast.

Contains:

- item icon;
- item name;
- rarity;
- slot.

Auto-dismiss.

## Rare

More visible toast / card.

Still non-blocking.

## Epic / Legendary / Godlike

Use a stronger acquisition popup animation, but do not stop combat by default.

Elements:

- item;
- rarity;
- key affixes;
- View button.

Godlike should feel exceptional.

## Named item

Special acquisition presentation.

Example:

**Hobgoblin Bulwark Found**

- unique icon/frame;
- unique effect line;
- source discovery recorded;
- View Item button.

Combat continues unless the player opens the item detail.

---

# 19. Screen — World

## Purpose

Choose where to push or farm and understand long-term route.

## Layout

Vertical / scrollable portrait map.

Each Location node shows:

- name;
- visual icon;
- discovered / locked state;
- current-run cleared state;
- account highest progress state;
- Boss status;
- current target marker.

The UI must distinguish:

- discovered permanently;
- accessible in current run;
- already re-cleared this run;
- known but temporarily blocked after reset.

## Current frontier

Visually marked.

## Current farming target

Separate marker.

The player may farm older content while frontier remains elsewhere.

## Location states

### Undiscovered

Hidden or silhouette.

### Discovered but inaccessible this run

Visible with route-lock indicator.

Tooltip:

> Re-clear the required route in this run to access this Location.

### Accessible

Selectable.

### Current

Highlighted.

### Completed this run

Boss-clear indicator.

---

# 20. Location Detail screen / sheet

Opened by selecting a Location node.

## Elements

- Location name;
- visual thumbnail;
- current-run status;
- highest progress;
- Stage list;
- Boss node;
- known drops;
- future Collection information when implemented.

## Stage list

For each Stage:

- Stage number;
- enemy composition icon(s);
- accessibility;
- completion / encounter requirement;
- current farming marker;
- special drop indicator if discovered.

Example Stage 8:

```
Stage 8
Hobgoblin Guard
Required: 10 encounters
Special Drop: Hobgoblin Bulwark [discovered]
```

## Stage actions

Accessible Stage:

- **Go / Farm Here**

If already current:

- Current.

Locked:

- disabled with requirement tooltip.

## Boss node

Shows:

- Boss name if discovered;
- defeated-this-run state;
- first-clear state;
- known mechanics after discovery;
- known drops.

---

# 21. Push / Farm UX

## Auto Push ON

Combat screen clearly shows ON state.

When Stage completes:

- brief Stage Complete feedback;
- automatically starts next Stage.

## Auto Push OFF

Current Stage remains active indefinitely.

Combat screen should show:

**Farming Stage X**

rather than implying progression is stuck.

## Selecting an older Stage

Player chooses it from World / Location Detail.

Result:

- current encounter transitions to selected Stage;
- Auto Push state remains whatever the player selected unless design later decides otherwise.

Recommended:

Keep Auto Push OFF automatically when intentionally selecting an older completed Stage for farming.

Show a small confirmation toast:

> Farming Stage 8. Auto Push turned OFF.

This prevents the game from immediately leaving the farming target.

---

# 22. Popup — System Unlock

Used for first-time system unlocks.

Examples:

- Gear Drops Unlocked;
- New Location Unlocked;
- future Collections;
- future Prestige.

Elements:

- system icon;
- name;
- one-sentence purpose;
- Go There / Continue.

Avoid long tutorial text.

---

# 23. Popup — Stage Complete

Non-blocking.

Elements:

- Stage complete;
- next Stage;
- optional newly unlocked milestone.

Usually toast/banner rather than modal.

If Auto Push OFF:

> Stage complete — continuing to farm.

---

# 24. Popup — Location Complete

Blocking celebratory modal after Boss death.

Elements:

- Location name;
- Boss defeated;
- meaningful rewards;
- newly unlocked route/content;
- Continue button.

Primary CTA:

**View World**

Secondary CTA if appropriate:

**Keep Farming**

Exact availability depends on world progression implementation.

---

# 25. Offline Rewards popup

Shown when returning after an eligible offline period that produced progress or rewards.

## Elements

- total time away;
- rewarded time;
- offline cap status if reached;
- Offline Farm Target;
- virtual encounter clears;
- EXP earned;
- items obtained;
- future Collection progress;
- notable rare drops.

Baseline informational line:

```
Offline Efficiency: 50%
Minimum encounter time: 60 sec
```

If the cap is reached:

```
Away: 11h 42m
Rewarded: 6h
Offline cap reached
```

## Item summary

Do not list large numbers of common items individually.

Show grouped summary plus notable items.

Example:

```
63 items found
1 Godlike
5 Epic
57 other
```

Named items should always appear individually.

CTA:

- Continue;
- View Gear if notable items exist.

## Strategic limits

Offline farming never:

- chooses a Power;
- spends EXP;
- equips gear;
- changes the farming target;
- advances World Progress;
- defeats a required Boss.

The active encounter resumes from its saved state after the offline reward transaction.

# 26. Screen — Settings

Minimal first implementation.

## Audio

- Music volume;
- SFX volume.

## Feedback

- Haptics on/off;
- screen shake on/off;
- damage-number density: Full / Reduced.

## Accessibility / readability

- reduce flashes;
- large-number notation preference if implemented;
- language when localization exists.

## Other

- version/build number;
- credits/legal/support links later.

Do not place gameplay progression settings here.

---

# 27. Generic tooltip behavior

## Desktop-style hover is unavailable

Mobile tooltip triggers:

- tap info icon;
- tap stat label where affordance exists;
- press-and-hold for compact definitions where useful.

## Tooltip form

Small anchored card or bottom sheet depending on content size.

Should contain:

- term name;
- concise explanation;
- optional formula / example;
- close by tapping outside.

## Tooltip stacking

Only one tooltip open at a time.

Opening another replaces the current tooltip.

---

# 28. Required gameplay tooltips

## EXP

> Earned from combat. Spend it on Strength, Vitality, Agility and Dexterity. Spending EXP does not advance World Progress.

## World Progress

> Determines where you can progress and when major unlocks occur. It is separate from EXP.

## Strength

> Increases Max Damage and Block.

## Vitality

> Increases Max Health, Regeneration and Block.

## Agility

> Increases Attack Speed and Evasion.

## Dexterity

> Increases Min Damage and Accuracy.

## Min Damage

> The lowest normal damage roll of a direct attack.

## Max Damage

> The highest normal damage roll of a direct attack.

## Block

> Reduces incoming damage with diminishing returns.

## Accuracy

> Improves the chance to hit evasive enemies.

## Evasion

> Makes enemy direct attacks more likely to miss.

## Attack Speed

> Increases attacks per second with diminishing returns.

## Regen

> Restores Health over time. Regeneration is faster while Downed.

## Auto Push

> ON advances after Stage completion. OFF keeps farming the current Stage.

## Stage Progress

> Counts completed encounters, not enemies killed.

## Rarity

> Higher rarity adds more affixes. It does not automatically make an item correct for every build.

## Item Level

> Determines the strength of an item's affixes.

## PowerDex

> Permanently unlocked Powers that can appear in eligible Power choices.

## Fortify

> Successful direct hits break Fortify. Misses do not.

## Exposed

> The Boss has greatly reduced Block while Exposed.

## Downed

> You are temporarily unable to fight. Enemies keep their current Health while you recover.

---

# 29. Contextual first-time coach marks

Coach marks are one-time onboarding hints, not permanent tooltips.

Required first implementation set:

1. Combat is automatic.
2. Stats tab after first EXP.
3. First Power Choice.
4. Gear unlocked.
5. First item comparison.
6. Multiple enemies attack independently.
7. Armored enemy / Block.
8. Auto Push can be turned off to farm.
9. Boss Fortify break indicator.

Rules:

- never show more than one coach mark simultaneously;
- never cover critical combat information;
- dismiss permanently once understood;
- player can revisit explanations through normal tooltips.

---

# 30. Notification / toast queue

Several events may happen in one combat second.

Examples:

- Stage complete;
- item drop;
- PowerDex unlock;
- achievement later;
- new Location.

The UI needs a queue / priority system.

Recommended priority:

1. blocking strategic choice;
2. system unlock;
3. Legendary/Godlike/named item;
4. Location/Stage unlock;
5. Rare/Epic item;
6. ordinary item;
7. routine progression feedback.

Do not allow five overlapping acquisition popups.

---

# 31. Empty states

## Gear — no items

> Gear begins dropping as you progress through the first Location.

## Current Powers — none

> Reach the first Power milestone to begin your build.

## PowerDex — locked entry

Show unlock hint when appropriate.

## World — only first Location

Do not show a giant empty map.

Focus camera/scroll region tightly on available world.

## Known drops — none discovered

> Special drops have not been discovered here yet.

---

# 32. Locked states

Locked UI should explain the route to unlock whenever that information is intended to be known.

Examples:

**Gear before encounter 14**

> Gear drops unlock as you progress further through Goblin Outskirts.

**Future Location**

> Defeat the current Location Boss.

**Power**

> Reach World Progress level equivalent 40.

Avoid:

> Locked.

with no useful explanation.

---

# 33. Error / confirmation patterns

## No EXP

Non-blocking inline feedback.

## Locked Stage

Tooltip / inline explanation.

## Equip locked item

Allowed.

Locking means protection from automatic handling, not inability to equip.

## Auto Equip

No destructive confirmation needed because it only performs conservative safe replacements.

## Leaving blocking Power Choice

Not allowed.

## Reset

Future reset action requires a clear confirmation modal explaining:

- what is lost;
- what is kept;
- expected reset gain.

Exact reset UI is deferred until Prestige is designed.

---

# 34. Future screen — Collections

Not required for first playable implementation.

UI architecture should reserve a future screen capable of:

- Collection categories;
- enemy/resource progress;
- milestone track;
- claimed/unlocked rewards;
- source navigation;
- “Go Farm” action.

Collections should be reachable from relevant enemies/Locations later.

No detailed layout is locked yet.

---

# 35. Future screen — Prestige / Reset

Not required until Prestige design is finalized.

Expected needs:

- current reset value;
- benefits gained;
- what resets;
- what persists;
- start-point improvement;
- Stage Compression;
- gear retention;
- starting-build control.

Reset must never be represented as a vague “Restart” button.

The player should understand the trade.

---

# 36. Future screen — Achievements / Mastery / Challenges

Architecture should support:

- category list;
- progress values;
- permanent rewards;
- Power unlock rewards;
- Power Mastery detail;
- challenge rules.

Not needed in first UI pass.

---

# 37. Future screen — Shop / Monetization

Monetization is deliberately undefined.

Do not reserve critical main-navigation space yet.

Future Shop must not become required for basic progression.

No Energy UI should exist.

---

# 38. Visual hierarchy guidelines

Without defining final art style, use the following hierarchy.

## Highest attention

- Power Choice;
- Boss mechanics;
- death/downed state;
- Legendary/Godlike/named item;
- Location completion.

## Medium attention

- Stage completion;
- Rare/Epic item;
- Power unlock;
- system unlock.

## Low attention

- normal EXP gain;
- common item;
- routine encounter completion.

If every reward uses a full-screen animation, nothing will feel important.

---

# 39. Rarity accessibility

Rarity should use more than color.

Every item should also include:

- written rarity name or recognizable icon/frame treatment;
- consistent border pattern;
- optional VFX intensity for high rarity.

Do not rely on color alone.

---

# 40. Numeric formatting

The game will eventually use large values.

UI must support compact notation.

Recommended progression:

```
999
1.2K
15.4K
2.1M
...
```

Where precision matters, tap/tooltip may show exact value.

Avoid layout that assumes values remain 2–4 digits.

---

# 41. Safe-area / portrait requirements

All primary screens must support:

- phone safe areas;
- tall portrait aspect ratios;
- bottom gesture areas;
- notches / camera cutouts.

Critical controls should not be placed directly against unsafe edges.

Minimum tap targets should follow mobile readability standards.

---

# 42. Back-button behavior

Recommended behavior:

## On primary tab

Back does not quit immediately if a sheet/modal is open.

Close order:

1. tooltip;
2. bottom sheet;
3. non-blocking popup detail;
4. return to previous primary screen if navigation history is used;
5. system exit behavior.

## Blocking Power Choice

Back does not close the choice.

## Item Detail

Back closes detail and returns to Gear.

## Location Detail

Back returns to World map.

---

# 43. UI state must survive screen rebuilds

The game should preserve relevant UX state such as:

- selected Gear filter;
- selected Powers tab;
- World scroll position where practical;
- current Stage selection;
- Auto Push setting;
- viewed/new flags.

Do not make opening another tab reset all UI context unnecessarily.

---

# 44. First playable UI scope

The first implementation should have complete functional UI for:

1. Launch / loading;
2. Combat;
3. Stats;
4. Powers — Current Run;
5. Powers — PowerDex;
6. Power Choice modal;
7. Gear;
8. Item Detail / Comparison;
9. World;
10. Location / Stage selection;
11. Enemy Info;
12. Hero Combat Details;
13. Boss Intro;
14. Boss phase UI;
15. item-drop notifications;
16. Power unlock notification;
17. system unlock popup;
18. Stage Complete feedback;
19. Location Complete popup;
20. Settings.

Offline Rewards can be implemented once offline progression is actually enabled.

Collections / Prestige / Mastery / Achievements / Shop remain future screens.

---

# 45. UI acceptance path for Location 01

A complete first-Location UI build must allow the player to perform this journey without debug controls:

```
Launch
→ watch Stage 1 combat
→ gain EXP
→ open Stats
→ purchase stat
→ receive Power Choice
→ choose starter Power
→ continue pushing
→ unlock gear
→ receive item
→ open Gear
→ compare/equip item
→ inspect current Power
→ inspect PowerDex
→ fight multiple enemies
→ understand Block
→ switch Auto Push OFF
→ manually farm a Stage
→ return Auto Push ON
→ select Stage 8 from World
→ farm Hobgoblin Guard
→ inspect named item if obtained
→ reach Boss
→ understand Fortify
→ experience Downed/revive
→ kill Boss
→ complete Location
→ view World route
```

If any step requires a hidden debug action or unexplained state change, the UI flow is incomplete.

---

# 46. Open / Later UI decisions

The following are intentionally not required for the first playable implementation:

1. Final visual style, colors and fonts.
2. Exact bottom-navigation iconography.
3. Whether Stats eventually gets Buy ×10 / Buy Max.
4. Sell / salvage / auto-disposal UI.
5. Exact reset / Prestige screen.
6. Collections screen layout.
7. Achievement / Mastery screen layout.
8. Monetization / Shop screens.
9. Cloud-save/account UI.
10. Advanced PowerDex search/filter UI.
11. Future loadouts / gear profiles.
12. Future inventory-cap / overflow UI if a hard capacity is introduced later.
13. Future repeat-Boss farming UI if later content explicitly adds repeatable Boss rewards.

Current baseline:

- inventory has no hard capacity;
- no overflow UI is required;
- defeated required Location Bosses are not repeat-farmed in the same run.

Later items should be decided when their dependent game systems are designed.

---

## Final UX principle

The main UI loop should feel like:

> **Watch → Notice a problem → Inspect build → Change something → See the result immediately.**

Combat provides feedback.

Stats, Powers, Gear and World provide decisions.

The UI should make moving between those two states fast, readable and low-friction.
