# Idle Build Master — Design Documentation

Source of truth for game design and production decisions.

## Project snapshot

- **Platform:** Mobile
- **Orientation:** Portrait
- **Genre:** Idle RPG / buildcraft / progression
- **Business model:** F2P, ads + IAP (exact monetization TBD)
- **Core references:** Idle Superpowers, Hypixel SkyBlock
- **Visual/production reference:** Gear Defenders
- **Current phase:** Game skeleton / pre-production

## Documentation map

### Game Design

- [00 — Game Vision](GameDesign/00_Game_Vision.md)
- [01 — Core Loop](GameDesign/01_Core_Loop.md)
- [02 — Run & Progression Structure](GameDesign/02_Run_Progression.md)
- [03 — Combat](GameDesign/03_Combat.md)

Planned next:

- 04 — Powers & Build Archetypes
- 05 — Collections
- 06 — World & Locations
- 07 — Prestige

### Content

Will be added when the systems require concrete content tables:

- Enemies
- Locations
- Items
- Powers

### Economy

Will be added after the main progression systems are defined:

- Economy
- Balance Principles

### Production

Will be added after the MVP gameplay skeleton is stable:

- Art Guidelines
- MVP Scope
- Roadmap

## Documentation rules

1. **Git is the source of truth.** Chat discussions become decisions only after they are reflected in Docs.
2. **Do not split documents too early.** Create a separate file only when a topic becomes large enough to justify it.
3. **Separate decisions from open questions.** Avoid presenting assumptions as locked design.
4. **Prefer systems over feature lists.** Every mechanic should have a clear role in the core loop.
5. **Production cost matters.** Prefer mechanics that create combinatorial gameplay depth without requiring large amounts of unique animation or art.
6. **Balance numbers come later.** First define relationships, choices and progression structure; then tune timings and values.
7. **Discuss first, commit second.** New design branches are discussed in chat before they are promoted to locked decisions in Docs.

## Current design thesis

The hero fights automatically. The player wins by understanding progression, choosing goals, assembling synergies and adapting a build.

The central loop is:

**Build → Wall → Goal → Farm → Improvement → Breakthrough**
