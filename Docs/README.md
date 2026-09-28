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
- [04 — Powers & Build Archetypes](GameDesign/04_Powers_Builds.md)
- [05 — Gear](GameDesign/05_Gear.md)

Planned next:

- 06 — World & Locations
- 07 — Collections
- 08 — Prestige

### Content

- [Powers](Content/Powers.md)
- [Items](Content/Items.md)
- [Location 01 — Goblin Outskirts / Goblin Camp](Content/Location_01_Mechanical_Baseline.md)

Planned next:

- Enemies

### Economy

- [00 — Reference Economy Baseline](Economy/00_Reference_Economy_Baseline.md)

Planned next:

- Balance Principles

### UI / UX

- [00 — UI / UX Specification](UI/00_UI_UX_Specification.md)

### Technical

- [00 — Architecture Requirements & Future-Proofing](Technical/00_Architecture_Requirements.md)
- [01 — Engineering Baseline & Chat Handoff](Technical/01_Engineering_Handoff.md)
- [02 — Architecture Scope & Decision Register](Technical/02_Architecture_Decisions.md)
- [03 — System Architecture](Technical/03_System_Architecture.md)
- [04 — Simulation Contracts](Technical/04_Simulation_Contracts.md)
- [05 — Persistence & Services](Technical/05_Persistence_and_Services.md)
- [06 — Implementation & Verification](Technical/06_Implementation_and_Verification.md)

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

## Balance reuse policy

When a mechanic is functionally equivalent to Idle Superpowers, use the reference game's values and curves as the first prototype baseline.

Adjust only where our systems materially differ, especially:

- multiple simultaneous enemies;
- AoE;
- Crit;
- Bleed / Poison;
- gear reset / retention;
- different progression pacing.

## Current design thesis

The hero fights automatically. The player wins by understanding progression, choosing goals, assembling synergies and adapting a build.

The central loop is:

**Build → Wall → Goal → Farm → Improvement → Breakthrough**
