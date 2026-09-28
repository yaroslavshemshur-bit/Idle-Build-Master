using System;
using System.Collections.Generic;
using System.Globalization;
using IBM.Domain;

namespace IBM.Application
{
    [Serializable] public sealed class NumberDto { public string coefficient; public string exponent; }
    [Serializable] public sealed class CountDto { public string id; public string value; }
    [Serializable] public sealed class SlotDto { public string slot; public string instanceId; }
    [Serializable] public sealed class ItemDto
    { public string instanceId; public string definitionId; public int itemLevel; public string[] affixIds;
      public bool procedural; public int rarity; public bool locked; }
    [Serializable] public sealed class ActorDto
    {
        public string instanceId, definitionId, hpCoefficient, hpExponent, maxHpCoefficient, maxHpExponent;
        public bool isHero;
        public int life;
        public string attackClockRevision, nextAttackAt;
        public string regenAnchorAt, regenAnchorHpCoefficient, regenAnchorHpExponent;
    }
    [Serializable] public sealed class EffectInstanceDto
    {
        public string instanceId, definitionId, sourceActorId, ownerActorId, createdAt, expiresAt, revision;
        public int stacks;
    }
    [Serializable] public sealed class ScheduledEventDto
    {
        public string due, sequence, ownerId, ownerRevision, definitionId;
        public int priority, kind;
    }
    [Serializable] public sealed class RngDto { public string state, increment; }
    [Serializable] public sealed class GameSnapshotDto
    {
        public const int CurrentSchemaVersion = 1;
        public string profileId, runId, stateRevision, contentVersion, mechanicalHash;
        public string completedRunCount;
        public string nextItemSequence;
        public int schemaVersion, rulesVersion, numericVersion, rngVersion;
        public string[] discoveredStages, unlockedPowers, completedStages, completedStageOrder, ownedPowers, pendingPowerOffer;
        public string[] triggeredMilestones, pendingPowerChoiceMilestones;
        public string activePowerOfferMilestoneId;
        public string[] recentOperationIds;
        public CountDto[] collectionCounters, purchasedStats, stageRequiredClears;
        public string selectedStageId, offlineTargetStageId;
        public bool autoPush;
        public NumberDto exp;
        public ItemDto[] inventory;
        public SlotDto[] equippedItems;
        public string encounterId, combatTime, nextScheduleSequence, nextActorSequence, nextEffectSequence;
        public bool combatActive, encounterRewardClaimed;
        public ActorDto[] actors;
        public EffectInstanceDto[] effects;
        public ScheduledEventDto[] pendingEvents;
        public ScheduledEventDto[] pausedEvents;
        public string downedStartedAt;
        public int downedAttackPolicy;
        public string bossPhaseId, bossEnteredAt, bossCounter;
        public string bossTransitionRevision;
        public string rootSeed;
        public RngDto combatRng, lootRng, offersRng, offlineRng;
        public string offlineAccountedThroughUtc, offlineResidual, offlineLastOperationId;

        public static GameSnapshotDto FromState(GameState state, string mechanicalHash)
        {
            if (state.Combat.IsResolving) throw new InvalidOperationException("Cannot serialize an in-flight action.");
            var dto = new GameSnapshotDto
            {
                profileId = state.Account.ProfileId, runId = state.Run.RunId,
                completedRunCount = S(state.Account.CompletedRunCount),
                recentOperationIds = state.RecentOperationIds.ToArray(),
                stateRevision = S(state.Revision), contentVersion = state.Versions.Content, mechanicalHash = mechanicalHash,
                schemaVersion = state.Versions.Schema, rulesVersion = state.Versions.Rules,
                numericVersion = state.Versions.Numeric, rngVersion = state.Versions.Rng,
                discoveredStages = Ids(state.Account.DiscoveredStages), unlockedPowers = Ids(state.Account.UnlockedPowers),
                collectionCounters = Counts(state.Account.CollectionCounters),
                completedStages = Ids(state.Run.CompletedStages), completedStageOrder = OrderedIds(state.Run.CompletedStageOrder),
                stageRequiredClears = Counts(state.Run.StageRequiredClears),
                triggeredMilestones = Ids(state.Run.TriggeredMilestones),
                pendingPowerChoiceMilestones = OrderedIds(state.Run.PendingPowerChoiceMilestones),
                ownedPowers = Ids(state.Run.OwnedPowers), pendingPowerOffer = OrderedIds(state.Run.PendingPowerOffer),
                activePowerOfferMilestoneId = state.Run.ActivePowerOfferMilestoneId.Value,
                purchasedStats = Counts(state.Run.PurchasedStats),
                selectedStageId = state.Run.SelectedStageId.Value, offlineTargetStageId = state.Run.OfflineTargetStageId.Value,
                autoPush = state.Run.AutoPush, exp = N(state.Run.Exp),
                nextItemSequence = S(state.Run.NextItemSequence),
                inventory = new ItemDto[state.Run.Inventory.Count], equippedItems = new SlotDto[state.Run.EquippedItems.Count],
                encounterId = state.Combat.EncounterId.Value, combatTime = S(state.Combat.Time.Microseconds),
                combatActive = state.Combat.Active, encounterRewardClaimed = state.Combat.EncounterRewardClaimed,
                nextScheduleSequence = S(state.Combat.NextScheduleSequence), nextActorSequence = S(state.Combat.NextActorSequence),
                nextEffectSequence = S(state.Combat.NextEffectSequence),
                actors = new ActorDto[state.Combat.Actors.Count], effects = new EffectInstanceDto[state.Combat.Effects.Count],
                pendingEvents = new ScheduledEventDto[state.Combat.PendingEvents.Count],
                pausedEvents = new ScheduledEventDto[state.Combat.PausedEvents.Count],
                downedStartedAt = S(state.Combat.DownedStartedAt.Microseconds),
                downedAttackPolicy = (int)state.Combat.DownedAttackPolicy,
                bossPhaseId = state.Combat.Boss.PhaseId.Value, bossEnteredAt = S(state.Combat.Boss.EnteredAt.Microseconds),
                bossCounter = S(state.Combat.Boss.Counter),
                bossTransitionRevision = S(state.Combat.Boss.TransitionRevision), rootSeed = S(state.Random.RootSeed),
                combatRng = R(state.Random.Combat), lootRng = R(state.Random.Loot),
                offersRng = R(state.Random.Offers), offlineRng = R(state.Random.Offline),
                offlineAccountedThroughUtc = state.Offline.AccountedThroughUtc,
                offlineResidual = state.Offline.Residual, offlineLastOperationId = state.Offline.LastOperationId
            };
            int i = 0;
            foreach (var item in state.Run.Inventory) dto.inventory[i++] = new ItemDto { instanceId = S(item.InstanceId),
                definitionId = item.DefinitionId.Value, itemLevel = item.ItemLevel, affixIds = OrderedIds(item.AffixIds),
                procedural = item.Procedural, rarity = (int)item.Rarity, locked = item.Locked };
            i = 0;
            foreach (var slot in state.Run.EquippedItems) dto.equippedItems[i++] = new SlotDto { slot = slot.Key, instanceId = S(slot.Value) };
            for (i = 0; i < dto.actors.Length; i++)
            {
                var actor = state.Combat.Actors[i];
                dto.actors[i] = new ActorDto { instanceId = S(actor.InstanceId), definitionId = actor.DefinitionId.Value,
                    hpCoefficient = S(actor.Hp.Coefficient), hpExponent = S(actor.Hp.Exponent),
                    maxHpCoefficient = S(actor.MaxHp.Coefficient), maxHpExponent = S(actor.MaxHp.Exponent),
                    isHero = actor.IsHero, life = (int)actor.Life,
                    attackClockRevision = S(actor.AttackClockRevision), nextAttackAt = S(actor.NextAttackAt.Microseconds),
                    regenAnchorAt = S(actor.RegenAnchorAt.Microseconds),
                    regenAnchorHpCoefficient = S(actor.RegenAnchorHp.Coefficient),
                    regenAnchorHpExponent = S(actor.RegenAnchorHp.Exponent) };
            }
            for (i = 0; i < dto.effects.Length; i++)
            {
                var effect = state.Combat.Effects[i];
                dto.effects[i] = new EffectInstanceDto { instanceId = S(effect.InstanceId), definitionId = effect.DefinitionId.Value,
                    sourceActorId = S(effect.SourceActorId), ownerActorId = S(effect.OwnerActorId),
                    createdAt = S(effect.CreatedAt.Microseconds), expiresAt = S(effect.ExpiresAt.Microseconds),
                    stacks = effect.Stacks, revision = S(effect.Revision) };
            }
            for (i = 0; i < dto.pendingEvents.Length; i++)
            {
                var entry = state.Combat.PendingEvents[i];
                dto.pendingEvents[i] = Event(entry);
            }
            for (i = 0; i < dto.pausedEvents.Length; i++) dto.pausedEvents[i] = Event(state.Combat.PausedEvents[i]);
            return dto;
        }

        public GameState ToState(ContentCatalog catalog, VersionStamp expected)
        {
            if (schemaVersion != expected.Schema || rulesVersion != expected.Rules || numericVersion != expected.Numeric ||
                rngVersion != expected.Rng || contentVersion != expected.Content || mechanicalHash != catalog.MechanicalHash)
                throw new InvalidOperationException("Save requires explicit schema/content/rules compatibility handling.");
            var account = new AccountState(profileId);
            account.CompletedRunCount = string.IsNullOrEmpty(completedRunCount) ? 0 : L(completedRunCount);
            Add(account.DiscoveredStages, discoveredStages); Add(account.UnlockedPowers, unlockedPowers);
            Add(account.CollectionCounters, collectionCounters);
            var run = new RunState(runId, new ContentId(selectedStageId)) { OfflineTargetStageId = Optional(offlineTargetStageId),
                AutoPush = autoPush, Exp = V(exp) };
            Add(run.CompletedStages, completedStages); Add(run.CompletedStageOrder, completedStageOrder);
            if (stageRequiredClears != null) Add(run.StageRequiredClears, stageRequiredClears);
            if (triggeredMilestones != null) Add(run.TriggeredMilestones, triggeredMilestones);
            if (pendingPowerChoiceMilestones != null) Add(run.PendingPowerChoiceMilestones, pendingPowerChoiceMilestones);
            Add(run.OwnedPowers, ownedPowers); Add(run.PendingPowerOffer, pendingPowerOffer);
            run.ActivePowerOfferMilestoneId = Optional(activePowerOfferMilestoneId);
            Add(run.PurchasedStats, purchasedStats);
            foreach (var item in inventory)
                run.Inventory.Add(new ItemInstanceState { InstanceId = U(item.instanceId), DefinitionId = new ContentId(item.definitionId),
                    ItemLevel = item.itemLevel, AffixIds = ParseIds(item.affixIds),
                    Procedural = item.procedural, Rarity = (ItemRarity)item.rarity, Locked = item.locked });
            if (string.IsNullOrEmpty(nextItemSequence))
            {
                ulong highest = 0;
                foreach (var item in run.Inventory) highest = Math.Max(highest, item.InstanceId);
                if (highest == ulong.MaxValue) throw new InvalidOperationException("Saved item sequence is exhausted.");
                run.NextItemSequence = highest + 1;
            }
            else run.NextItemSequence = U(nextItemSequence);
            foreach (var slot in equippedItems) run.EquippedItems.Add(slot.slot, U(slot.instanceId));
            var combat = new CombatState { EncounterId = Optional(encounterId), Time = new SimTime(L(combatTime)), Active = combatActive,
                EncounterRewardClaimed = encounterRewardClaimed, NextScheduleSequence = U(nextScheduleSequence),
                NextActorSequence = U(nextActorSequence), NextEffectSequence = U(nextEffectSequence),
                Boss = new BossState { PhaseId = Optional(bossPhaseId), EnteredAt = new SimTime(L(bossEnteredAt)),
                    Counter = L(bossCounter), TransitionRevision = string.IsNullOrEmpty(bossTransitionRevision) ? 0 : L(bossTransitionRevision) },
                DownedStartedAt = string.IsNullOrEmpty(downedStartedAt) ? new SimTime(0) : new SimTime(L(downedStartedAt)),
                DownedAttackPolicy = (HeroDownedAttackClockPolicy)downedAttackPolicy };
            foreach (var actor in actors)
                combat.Actors.Add(new ActorState { InstanceId = U(actor.instanceId), DefinitionId = new ContentId(actor.definitionId),
                    Hp = GameNumber.Parse(actor.hpCoefficient, actor.hpExponent), MaxHp = GameNumber.Parse(actor.maxHpCoefficient, actor.maxHpExponent),
                    IsHero = actor.isHero, Life = (ActorLifeState)actor.life,
                    AttackClockRevision = L(actor.attackClockRevision), NextAttackAt = new SimTime(L(actor.nextAttackAt)),
                    RegenAnchorAt = string.IsNullOrEmpty(actor.regenAnchorAt) ? new SimTime(L(combatTime)) : new SimTime(L(actor.regenAnchorAt)),
                    RegenAnchorHp = string.IsNullOrEmpty(actor.regenAnchorHpCoefficient) ?
                        GameNumber.Parse(actor.hpCoefficient, actor.hpExponent) :
                        GameNumber.Parse(actor.regenAnchorHpCoefficient, actor.regenAnchorHpExponent) });
            foreach (var effect in effects)
                combat.Effects.Add(new EffectInstanceState { InstanceId = U(effect.instanceId), DefinitionId = new ContentId(effect.definitionId),
                    SourceActorId = U(effect.sourceActorId), OwnerActorId = U(effect.ownerActorId),
                    CreatedAt = new SimTime(L(effect.createdAt)), ExpiresAt = new SimTime(L(effect.expiresAt)),
                    Stacks = effect.stacks, Revision = L(effect.revision) });
            foreach (var entry in pendingEvents)
                combat.PendingEvents.Add(ParseEvent(entry));
            if (pausedEvents != null)
                foreach (var entry in pausedEvents) combat.PausedEvents.Add(ParseEvent(entry));
            var random = new SessionRandomState { RootSeed = U(rootSeed), Combat = Parse(combatRng),
                Loot = Parse(lootRng), Offers = Parse(offersRng), Offline = Parse(offlineRng) };
            var offline = new OfflineAccountingState { AccountedThroughUtc = offlineAccountedThroughUtc,
                Residual = offlineResidual, LastOperationId = offlineLastOperationId };
            var state = new GameState(account, run, combat, expected, random, offline) { Revision = L(stateRevision) };
            if (recentOperationIds != null) state.RecentOperationIds.AddRange(recentOperationIds);
            Validate(state, catalog);
            return state;
        }

        private static void Validate(GameState state, ContentCatalog catalog)
        {
            if (state.Revision < 0 || !catalog.Stages.ContainsKey(state.Run.SelectedStageId)) throw new InvalidOperationException("Invalid saved run state.");
            if (state.Account.CompletedRunCount < 0) throw new InvalidOperationException("Invalid completed-run count.");
            foreach (var purchase in state.Run.PurchasedStats)
                if (!PrimaryStatProgression.IsKnownId(purchase.Key) || purchase.Value < 0)
                    throw new InvalidOperationException("Invalid purchased primary stat: " + purchase.Key);
            foreach (var pair in state.Run.StageRequiredClears)
                if (!catalog.Stages.TryGetValue(pair.Key, out var stage) || stage.IsBossStage ||
                    pair.Value < 0 || pair.Value > stage.BaseRequiredEncounters)
                    throw new InvalidOperationException("Invalid saved Stage clear count: " + pair.Key);
            foreach (var completed in state.Run.CompletedStages)
            {
                if (!catalog.Stages.TryGetValue(completed, out var stage))
                    throw new InvalidOperationException("Unknown completed Stage: " + completed);
                if (!stage.IsBossStage && (!state.Run.StageRequiredClears.TryGetValue(completed, out long count) ||
                    count != stage.BaseRequiredEncounters))
                    throw new InvalidOperationException("Completed Stage has incomplete required progress: " + completed);
            }
            var orderedCompleted = new HashSet<ContentId>();
            foreach (var completed in state.Run.CompletedStageOrder)
                if (!orderedCompleted.Add(completed) || !state.Run.CompletedStages.Contains(completed))
                    throw new InvalidOperationException("Invalid completed Stage order.");
            if (orderedCompleted.Count != state.Run.CompletedStages.Count)
                throw new InvalidOperationException("Completed Stage order is incomplete.");
            var milestoneKinds = new Dictionary<ContentId, ProgressMilestoneKind>();
            foreach (var location in catalog.Locations.Values)
                foreach (var milestone in location.Milestones)
                    if (!milestoneKinds.TryAdd(milestone.Id, milestone.Kind))
                        throw new InvalidOperationException("Duplicate milestone identity across Locations.");
            foreach (var triggered in state.Run.TriggeredMilestones)
                if (!milestoneKinds.ContainsKey(triggered)) throw new InvalidOperationException("Unknown triggered milestone: " + triggered);
            foreach (var pending in state.Run.PendingPowerChoiceMilestones)
                if (!state.Run.TriggeredMilestones.Contains(pending) || !milestoneKinds.TryGetValue(pending, out var kind) ||
                    kind != ProgressMilestoneKind.PowerChoice)
                    throw new InvalidOperationException("Invalid pending Power-choice milestone: " + pending);
            if (new HashSet<ContentId>(state.Run.PendingPowerChoiceMilestones).Count != state.Run.PendingPowerChoiceMilestones.Count)
                throw new InvalidOperationException("Duplicate pending Power-choice milestone.");
            foreach (var power in state.Account.UnlockedPowers)
                if (!catalog.Powers.ContainsKey(power)) throw new InvalidOperationException("Unknown unlocked Power: " + power);
            foreach (var power in state.Run.OwnedPowers)
                if (!catalog.Powers.ContainsKey(power) || !state.Account.UnlockedPowers.Contains(power))
                    throw new InvalidOperationException("Unknown or locked owned Power: " + power);
            var itemIds = new HashSet<ulong>();
            var itemsById = new Dictionary<ulong, ItemInstanceState>();
            foreach (var item in state.Run.Inventory)
            {
                if (item.InstanceId == 0 || item.InstanceId >= state.Run.NextItemSequence ||
                    !itemIds.Add(item.InstanceId) || !catalog.Items.ContainsKey(item.DefinitionId) ||
                    item.ItemLevel < 1 || item.AffixIds == null)
                    throw new InvalidOperationException("Invalid saved item instance.");
                GameSession.ValidateItemAffixes(item, catalog);
                itemsById.Add(item.InstanceId, item);
            }
            if (state.Run.NextItemSequence == 0)
                throw new InvalidOperationException("Saved item sequence is zero.");
            foreach (var slot in state.Run.EquippedItems)
                if (string.IsNullOrWhiteSpace(slot.Key) || !itemsById.TryGetValue(slot.Value, out var item) ||
                    !StringComparer.Ordinal.Equals(catalog.Items[item.DefinitionId].Slot, slot.Key))
                    throw new InvalidOperationException("Invalid equipped item reference.");
            if (state.Run.PendingPowerOffer.Count == 0)
            {
                if (!string.IsNullOrEmpty(state.Run.ActivePowerOfferMilestoneId.Value))
                    throw new InvalidOperationException("Offer source without offer options.");
            }
            else
            {
                if (state.Run.PendingPowerChoiceMilestones.Count == 0 ||
                    state.Run.ActivePowerOfferMilestoneId != state.Run.PendingPowerChoiceMilestones[0])
                    throw new InvalidOperationException("Power offer does not match first pending choice.");
                ProgressMilestone? source = null;
                foreach (var location in catalog.Locations.Values)
                    foreach (var milestone in location.Milestones)
                        if (milestone.Id == state.Run.ActivePowerOfferMilestoneId) source = milestone;
                if (!source.HasValue) throw new InvalidOperationException("Power offer source is unknown.");
                var uniqueOptions = new HashSet<ContentId>();
                foreach (var option in state.Run.PendingPowerOffer)
                    if (!uniqueOptions.Add(option) || !catalog.Powers.ContainsKey(option) ||
                        !state.Account.UnlockedPowers.Contains(option) || state.Run.OwnedPowers.Contains(option) ||
                        (source.Value.FixedOfferOptions.Count != 0 && !Contains(source.Value.FixedOfferOptions, option)))
                        throw new InvalidOperationException("Invalid saved Power offer option: " + option);
            }
            if (state.RecentOperationIds.Count > 1024 || new HashSet<string>(state.RecentOperationIds, StringComparer.Ordinal).Count != state.RecentOperationIds.Count)
                throw new InvalidOperationException("Invalid saved operation history.");
            foreach (var id in state.RecentOperationIds)
                if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Invalid saved operation ID.");
            if (state.Combat.NextScheduleSequence == 0 || state.Combat.NextActorSequence == 0 || state.Combat.NextEffectSequence == 0)
                throw new InvalidOperationException("Invalid saved instance sequence.");
            var actorIds = new HashSet<ulong>();
            foreach (var actor in state.Combat.Actors)
            {
                if (!actorIds.Add(actor.InstanceId) || actor.Hp.CompareTo(GameNumber.Zero) < 0 || actor.Hp.CompareTo(actor.MaxHp) > 0)
                    throw new InvalidOperationException("Invalid actor identity or HP in save.");
                if (actor.RegenAnchorAt.CompareTo(state.Combat.Time) > 0 || actor.RegenAnchorHp.CompareTo(GameNumber.Zero) < 0 ||
                    actor.RegenAnchorHp.CompareTo(actor.MaxHp) > 0)
                    throw new InvalidOperationException("Invalid regeneration anchor in save.");
                if (!actor.IsHero && !catalog.Enemies.ContainsKey(actor.DefinitionId)) throw new InvalidOperationException("Unknown enemy in save: " + actor.DefinitionId);
            }
            foreach (var effect in state.Combat.Effects)
                if (!catalog.Effects.ContainsKey(effect.DefinitionId) || !actorIds.Contains(effect.OwnerActorId) || effect.Stacks <= 0)
                    throw new InvalidOperationException("Unknown or invalid effect in save.");
            if (!string.IsNullOrEmpty(state.Combat.Boss.PhaseId.Value))
            {
                bool knownPhase = false;
                foreach (var cycle in catalog.BossCycles.Values)
                    if (state.Combat.Boss.PhaseId == cycle.NormalPhaseId ||
                        state.Combat.Boss.PhaseId == cycle.FortifyPhaseId ||
                        state.Combat.Boss.PhaseId == cycle.ExposedPhaseId)
                        knownPhase = true;
                if (!knownPhase || state.Combat.Boss.TransitionRevision <= 0 ||
                    state.Combat.Boss.Counter < 0 || state.Combat.Boss.EnteredAt.CompareTo(state.Combat.Time) > 0)
                    throw new InvalidOperationException("Invalid saved Boss phase state.");
            }
            SimulationScheduler.Restore(state.Combat.Time, state.Combat.NextScheduleSequence, state.Combat.PendingEvents);
            var sequenceIds = new HashSet<ulong>();
            foreach (var entry in state.Combat.PendingEvents) sequenceIds.Add(entry.Sequence);
            foreach (var entry in state.Combat.PausedEvents)
                if (entry.Sequence == 0 || entry.Sequence >= state.Combat.NextScheduleSequence ||
                    !sequenceIds.Add(entry.Sequence) || entry.Due.CompareTo(state.Combat.DownedStartedAt) < 0)
                    throw new InvalidOperationException("Invalid paused scheduler event in save.");
            bool heroDowned = false;
            foreach (var actor in state.Combat.Actors) if (actor.IsHero && actor.Life == ActorLifeState.Downed) heroDowned = true;
            if (heroDowned && (state.Combat.DownedStartedAt.CompareTo(state.Combat.Time) > 0 ||
                !state.Combat.PendingEvents.Exists(entry => entry.Kind == ScheduledEventKind.Revive)))
                throw new InvalidOperationException("Downed hero has no valid revive continuation.");
            if (!Enum.IsDefined(typeof(HeroDownedAttackClockPolicy), state.Combat.DownedAttackPolicy))
                throw new InvalidOperationException("Unknown saved downed attack-clock policy.");
            if (!heroDowned && state.Combat.PausedEvents.Count > 0)
                throw new InvalidOperationException("Paused clocks require a downed hero.");
            decimal residual = decimal.Parse(state.Offline.Residual, NumberStyles.Float, CultureInfo.InvariantCulture);
            if (residual < 0 || residual >= 1) throw new InvalidOperationException("Invalid offline residual.");
        }

        private static NumberDto N(GameNumber value) => new NumberDto { coefficient = S(value.Coefficient), exponent = S(value.Exponent) };
        private static ScheduledEventDto Event(ScheduledEvent entry) => new ScheduledEventDto
        {
            due = S(entry.Due.Microseconds), priority = entry.PhasePriority, sequence = S(entry.Sequence),
            kind = (int)entry.Kind, ownerId = S(entry.Owner.Value), ownerRevision = S(entry.OwnerRevision),
            definitionId = entry.DefinitionId.Value
        };
        private static ScheduledEvent ParseEvent(ScheduledEventDto entry) => new ScheduledEvent(new SimTime(L(entry.due)),
            entry.priority, U(entry.sequence), (ScheduledEventKind)entry.kind, new InstanceId(U(entry.ownerId)),
            L(entry.ownerRevision), Optional(entry.definitionId));
        private static GameNumber V(NumberDto value) => GameNumber.Parse(value.coefficient, value.exponent);
        private static RngDto R(RngStreamState value) => new RngDto { state = S(value.State), increment = S(value.Increment) };
        private static RngStreamState Parse(RngDto value) => new RngStreamState { State = U(value.state), Increment = U(value.increment) };
        private static string S(object value) => value is System.Numerics.BigInteger big
            ? big.ToString(CultureInfo.InvariantCulture) : Convert.ToString(value, CultureInfo.InvariantCulture);
        private static long L(string value) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        private static ulong U(string value) => ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        private static ContentId Optional(string value) => string.IsNullOrEmpty(value) ? default : new ContentId(value);
        private static string[] Ids(IEnumerable<ContentId> values) { var result = new List<string>(OrderedIds(values)); result.Sort(StringComparer.Ordinal); return result.ToArray(); }
        private static string[] OrderedIds(IEnumerable<ContentId> values) { var result = new List<string>(); foreach (var value in values) result.Add(value.Value); return result.ToArray(); }
        private static ContentId[] ParseIds(string[] values) { var result = new ContentId[values.Length]; for (int i = 0; i < result.Length; i++) result[i] = new ContentId(values[i]); return result; }
        private static CountDto[] Counts(Dictionary<ContentId, long> values) { var result = new List<CountDto>(); foreach (var value in values) result.Add(new CountDto { id = value.Key.Value, value = S(value.Value) }); result.Sort((a, b) => StringComparer.Ordinal.Compare(a.id, b.id)); return result.ToArray(); }
        private static void Add(HashSet<ContentId> target, string[] values) { foreach (var value in values) if (!target.Add(new ContentId(value))) throw new InvalidOperationException("Duplicate saved ID: " + value); }
        private static void Add(List<ContentId> target, string[] values) { foreach (var value in values) target.Add(new ContentId(value)); }
        private static void Add(Dictionary<ContentId, long> target, CountDto[] values) { foreach (var value in values) target.Add(new ContentId(value.id), L(value.value)); }
        private static bool Contains(IReadOnlyList<ContentId> values, ContentId target)
        { foreach (var value in values) if (value == target) return true; return false; }
    }
}
