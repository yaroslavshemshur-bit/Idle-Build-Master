using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    public sealed class AccountState
    {
        public string ProfileId { get; }
        public HashSet<ContentId> DiscoveredStages { get; }
        public HashSet<ContentId> UnlockedPowers { get; }
        public Dictionary<ContentId, long> CollectionCounters { get; }
        public long CompletedRunCount { get; internal set; }
        public AccountState(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId)) throw new ArgumentException("Profile ID is required.", nameof(profileId));
            ProfileId = profileId;
            DiscoveredStages = new HashSet<ContentId>();
            UnlockedPowers = new HashSet<ContentId>();
            CollectionCounters = new Dictionary<ContentId, long>();
        }
        private AccountState(AccountState other)
        {
            ProfileId = other.ProfileId;
            CompletedRunCount = other.CompletedRunCount;
            DiscoveredStages = new HashSet<ContentId>(other.DiscoveredStages);
            UnlockedPowers = new HashSet<ContentId>(other.UnlockedPowers);
            CollectionCounters = new Dictionary<ContentId, long>(other.CollectionCounters);
        }
        internal AccountState Copy() => new AccountState(this);
    }

    public sealed class RunState
    {
        public string RunId { get; }
        public ContentId SelectedStageId { get; internal set; }
        public ContentId OfflineTargetStageId { get; internal set; }
        public bool AutoPush { get; internal set; }
        public GameNumber Exp { get; internal set; }
        public Dictionary<ContentId, long> PurchasedStats { get; }
        public HashSet<ContentId> CompletedStages { get; }
        public List<ContentId> CompletedStageOrder { get; }
        public Dictionary<ContentId, long> StageRequiredClears { get; }
        public HashSet<ContentId> TriggeredMilestones { get; }
        public List<ContentId> PendingPowerChoiceMilestones { get; }
        public HashSet<ContentId> OwnedPowers { get; }
        public List<ContentId> PendingPowerOffer { get; }
        public ContentId ActivePowerOfferMilestoneId { get; internal set; }
        public List<ItemInstanceState> Inventory { get; }
        public ulong NextItemSequence { get; set; } = 1;
        public Dictionary<string, ulong> EquippedItems { get; }
        public RunState(string runId, ContentId firstStage)
        {
            if (string.IsNullOrWhiteSpace(runId)) throw new ArgumentException("Run ID is required.", nameof(runId));
            RunId = runId; SelectedStageId = firstStage; AutoPush = true;
            PurchasedStats = new Dictionary<ContentId, long>();
            CompletedStages = new HashSet<ContentId>();
            CompletedStageOrder = new List<ContentId>();
            StageRequiredClears = new Dictionary<ContentId, long>();
            TriggeredMilestones = new HashSet<ContentId>();
            PendingPowerChoiceMilestones = new List<ContentId>();
            OwnedPowers = new HashSet<ContentId>();
            PendingPowerOffer = new List<ContentId>();
            Inventory = new List<ItemInstanceState>();
            EquippedItems = new Dictionary<string, ulong>(StringComparer.Ordinal);
        }
        private RunState(RunState other)
        {
            RunId = other.RunId; SelectedStageId = other.SelectedStageId; OfflineTargetStageId = other.OfflineTargetStageId; AutoPush = other.AutoPush; Exp = other.Exp;
            PurchasedStats = new Dictionary<ContentId, long>(other.PurchasedStats);
            CompletedStages = new HashSet<ContentId>(other.CompletedStages);
            CompletedStageOrder = new List<ContentId>(other.CompletedStageOrder);
            StageRequiredClears = new Dictionary<ContentId, long>(other.StageRequiredClears);
            TriggeredMilestones = new HashSet<ContentId>(other.TriggeredMilestones);
            PendingPowerChoiceMilestones = new List<ContentId>(other.PendingPowerChoiceMilestones);
            OwnedPowers = new HashSet<ContentId>(other.OwnedPowers);
            PendingPowerOffer = new List<ContentId>(other.PendingPowerOffer);
            ActivePowerOfferMilestoneId = other.ActivePowerOfferMilestoneId;
            Inventory = new List<ItemInstanceState>(other.Inventory.Count);
            NextItemSequence = other.NextItemSequence;
            foreach (var item in other.Inventory) Inventory.Add(item.Copy());
            EquippedItems = new Dictionary<string, ulong>(other.EquippedItems, StringComparer.Ordinal);
        }
        internal RunState Copy() => new RunState(this);
    }

    public sealed class CombatState
    {
        public CombatState() { }
        public CombatState(ContentId encounterId, SimTime startTime, IEnumerable<ActorState> actors, bool active = true)
        {
            if (string.IsNullOrEmpty(encounterId.Value) || actors == null) throw new ArgumentException("Encounter and actors are required.");
            EncounterId = encounterId;
            Time = startTime;
            Active = active;
            var ids = new HashSet<ulong>();
            ulong highest = 0;
            foreach (var actor in actors)
            {
                if (actor == null || actor.InstanceId == 0 || !ids.Add(actor.InstanceId))
                    throw new ArgumentException("Encounter actors need unique nonzero instance IDs.", nameof(actors));
                Actors.Add(actor.Copy());
                highest = Math.Max(highest, actor.InstanceId);
            }
            if (Actors.Count == 0 || highest == ulong.MaxValue) throw new ArgumentException("Encounter actor set is invalid.", nameof(actors));
            NextActorSequence = highest + 1;
        }
        public ContentId EncounterId { get; internal set; }
        public SimTime Time { get; internal set; }
        public bool Active { get; internal set; }
        public bool IsResolving { get; internal set; }
        public List<ActorState> Actors { get; } = new List<ActorState>();
        public List<EffectInstanceState> Effects { get; } = new List<EffectInstanceState>();
        public List<ScheduledEvent> PendingEvents { get; } = new List<ScheduledEvent>();
        public List<ScheduledEvent> PausedEvents { get; } = new List<ScheduledEvent>();
        public SimTime DownedStartedAt { get; internal set; }
        public HeroDownedAttackClockPolicy DownedAttackPolicy { get; internal set; }
        public ulong NextScheduleSequence { get; internal set; } = 1;
        public ulong NextActorSequence { get; internal set; } = 1;
        public ulong NextEffectSequence { get; internal set; } = 1;
        public BossState Boss { get; internal set; } = new BossState();
        public bool EncounterRewardClaimed { get; internal set; }
        public CombatState Copy()
        {
            var copy = new CombatState { EncounterId = EncounterId, Time = Time, Active = Active, IsResolving = IsResolving,
                NextScheduleSequence = NextScheduleSequence, NextActorSequence = NextActorSequence,
                NextEffectSequence = NextEffectSequence, Boss = Boss.Copy(), EncounterRewardClaimed = EncounterRewardClaimed };
            copy.DownedStartedAt = DownedStartedAt;
            copy.DownedAttackPolicy = DownedAttackPolicy;
            foreach (var actor in Actors) copy.Actors.Add(actor.Copy());
            foreach (var effect in Effects) copy.Effects.Add(effect.Copy());
            copy.PendingEvents.AddRange(PendingEvents);
            copy.PausedEvents.AddRange(PausedEvents);
            return copy;
        }
    }

    public enum ActorLifeState { Alive, Downed, Reviving, Dead }
    public sealed class ActorState
    {
        public ulong InstanceId { get; set; }
        public ContentId DefinitionId { get; set; }
        public bool IsHero { get; set; }
        public GameNumber Hp { get; set; }
        public GameNumber MaxHp { get; set; }
        public ActorLifeState Life { get; set; }
        public long AttackClockRevision { get; set; }
        public SimTime NextAttackAt { get; set; }
        public SimTime RegenAnchorAt { get; set; }
        public GameNumber RegenAnchorHp { get; set; }
        public ActorState Copy() => (ActorState)MemberwiseClone();
    }
    public sealed class EffectInstanceState
    {
        public ulong InstanceId { get; set; }
        public ContentId DefinitionId { get; set; }
        public ulong SourceActorId { get; set; }
        public ulong OwnerActorId { get; set; }
        public SimTime CreatedAt { get; set; }
        public SimTime ExpiresAt { get; set; }
        public int Stacks { get; set; }
        public long Revision { get; set; }
        public EffectInstanceState Copy() => (EffectInstanceState)MemberwiseClone();
    }
    public sealed class BossState
    {
        public ContentId PhaseId { get; set; }
        public SimTime EnteredAt { get; set; }
        public long Counter { get; set; }
        public BossState Copy() => (BossState)MemberwiseClone();
    }
    public sealed class ItemInstanceState
    {
        public ulong InstanceId { get; set; }
        public ContentId DefinitionId { get; set; }
        public int ItemLevel { get; set; }
        public ContentId[] AffixIds { get; set; } = Array.Empty<ContentId>();
        public bool Locked { get; set; }
        public ItemInstanceState Copy() => new ItemInstanceState { InstanceId = InstanceId, DefinitionId = DefinitionId,
            ItemLevel = ItemLevel, AffixIds = (ContentId[])AffixIds.Clone(), Locked = Locked };
    }

    public sealed class RngStreamState
    {
        public ulong State { get; set; }
        public ulong Increment { get; set; }
        public RngStreamState Copy() => (RngStreamState)MemberwiseClone();
    }
    public sealed class SessionRandomState
    {
        public ulong RootSeed { get; set; }
        public RngStreamState Combat { get; set; } = new RngStreamState();
        public RngStreamState Loot { get; set; } = new RngStreamState();
        public RngStreamState Offers { get; set; } = new RngStreamState();
        public RngStreamState Offline { get; set; } = new RngStreamState();
        public SessionRandomState Copy() => new SessionRandomState { RootSeed = RootSeed, Combat = Combat.Copy(),
            Loot = Loot.Copy(), Offers = Offers.Copy(), Offline = Offline.Copy() };
        public static SessionRandomState Create(ulong rootSeed) => new SessionRandomState
        {
            RootSeed = rootSeed,
            Combat = Seed(rootSeed, "combat"), Loot = Seed(rootSeed, "loot"),
            Offers = Seed(rootSeed, "offers"), Offline = Seed(rootSeed, "offline")
        };
        private static RngStreamState Seed(ulong root, string id)
        {
            var random = new Pcg32(RandomSeeds.Derive(root, id), RandomSeeds.Derive(root, id + ".sequence"));
            return new RngStreamState { State = random.State, Increment = random.Increment };
        }
    }
    public sealed class OfflineAccountingState
    {
        public string AccountedThroughUtc { get; set; }
        public string Residual { get; set; } = "0";
        public string LastOperationId { get; set; }
        public OfflineAccountingState Copy() => (OfflineAccountingState)MemberwiseClone();
    }

    public sealed class GameState
    {
        public AccountState Account { get; }
        public RunState Run { get; }
        public CombatState Combat { get; internal set; }
        public long Revision { get; internal set; }
        public VersionStamp Versions { get; }
        public SessionRandomState Random { get; internal set; }
        public OfflineAccountingState Offline { get; }
        public List<string> RecentOperationIds { get; } = new List<string>();
        public GameState(AccountState account, RunState run, CombatState combat, VersionStamp versions,
            SessionRandomState random, OfflineAccountingState offline)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            Run = run ?? throw new ArgumentNullException(nameof(run));
            Combat = combat ?? throw new ArgumentNullException(nameof(combat));
            Versions = versions;
            Random = random ?? throw new ArgumentNullException(nameof(random));
            Offline = offline ?? throw new ArgumentNullException(nameof(offline));
        }
        internal GameState Copy()
        {
            var copy = new GameState(Account.Copy(), Run.Copy(), Combat.Copy(), Versions, Random.Copy(), Offline.Copy()) { Revision = Revision };
            copy.RecentOperationIds.AddRange(RecentOperationIds);
            return copy;
        }
    }

    public abstract class GameCommand
    {
        public string OperationId { get; }
        public long ExpectedRevision { get; }
        protected GameCommand(string operationId, long expectedRevision)
        {
            if (string.IsNullOrWhiteSpace(operationId)) throw new ArgumentException("Operation ID is required.", nameof(operationId));
            OperationId = operationId;
            ExpectedRevision = expectedRevision;
        }
    }
    public sealed class SetAutoPushCommand : GameCommand
    {
        public bool Enabled { get; }
        public SetAutoPushCommand(string operationId, long expectedRevision, bool enabled) : base(operationId, expectedRevision) => Enabled = enabled;
    }
    public sealed class SelectStageCommand : GameCommand
    {
        public ContentId StageId { get; }
        public SelectStageCommand(string operationId, long expectedRevision, ContentId stageId) : base(operationId, expectedRevision) => StageId = stageId;
    }
    public sealed class SelectOfflineTargetCommand : GameCommand
    {
        public ContentId StageId { get; }
        public SelectOfflineTargetCommand(string operationId, long expectedRevision, ContentId stageId) : base(operationId, expectedRevision) => StageId = stageId;
    }
    public sealed class ChoosePowerCommand : GameCommand
    {
        public ContentId PowerId { get; }
        public ChoosePowerCommand(string operationId, long expectedRevision, ContentId powerId)
            : base(operationId, expectedRevision) => PowerId = powerId;
    }
    public sealed class SetItemLockCommand : GameCommand
    {
        public ulong ItemInstanceId { get; }
        public bool Locked { get; }
        public SetItemLockCommand(string operationId, long expectedRevision, ulong itemInstanceId, bool locked)
            : base(operationId, expectedRevision) { ItemInstanceId = itemInstanceId; Locked = locked; }
    }
    public sealed class EquipItemCommand : GameCommand
    {
        public ulong ItemInstanceId { get; }
        public EquipItemCommand(string operationId, long expectedRevision, ulong itemInstanceId)
            : base(operationId, expectedRevision) => ItemInstanceId = itemInstanceId;
    }
    public sealed class UnequipItemCommand : GameCommand
    {
        public string Slot { get; }
        public UnequipItemCommand(string operationId, long expectedRevision, string slot)
            : base(operationId, expectedRevision) => Slot = slot;
    }
    public sealed class BuyStatUpgradeCommand : GameCommand
    {
        public PrimaryStatKind Stat { get; }
        public BuyStatUpgradeCommand(string operationId, long expectedRevision, PrimaryStatKind stat)
            : base(operationId, expectedRevision) => Stat = stat;
    }

    // Live gear changes can alter HP and attack speed. Their combat transition is supplied explicitly.
    public interface IEquipmentTransitionPolicy
    {
        void Apply(GameState before, GameState after, string changedSlot);
    }

    // The selection rule is injected until normal offer size and weighting are authored.
    public interface IPowerOfferPolicy
    {
        IReadOnlyList<ContentId> CreateOffer(ProgressMilestone milestone,
            IReadOnlyList<ContentId> eligiblePowers, IRandomStream random);
    }

    public readonly struct PowerOfferResult
    {
        public bool Generated { get; }
        public long Revision { get; }
        public ContentId MilestoneId { get; }
        public IReadOnlyList<ContentId> Options { get; }
        public PowerOfferResult(bool generated, long revision, ContentId milestoneId, IReadOnlyList<ContentId> options)
        {
            Generated = generated; Revision = revision; MilestoneId = milestoneId; Options = options;
        }
    }

    public enum CommandStatus { Applied, AlreadyApplied, Rejected }
    public readonly struct CommandResult
    {
        public CommandStatus Status { get; }
        public long Revision { get; }
        public string Reason { get; }
        public CommandResult(CommandStatus status, long revision, string reason) { Status = status; Revision = revision; Reason = reason; }
    }

    public readonly struct SessionView
    {
        public long Revision { get; }
        public ContentId SelectedStageId { get; }
        public bool AutoPush { get; }
        public GameNumber Exp { get; }
        public bool EncounterActive { get; }
        public SessionView(GameState state)
        {
            Revision = state.Revision; SelectedStageId = state.Run.SelectedStageId;
            AutoPush = state.Run.AutoPush; Exp = state.Run.Exp; EncounterActive = state.Combat.Active;
        }
    }

    public readonly struct EncounterClearResult
    {
        public bool Applied { get; }
        public long Revision { get; }
        public GameNumber ExpGranted { get; }
        public IReadOnlyList<ItemInstanceState> ItemsGranted { get; }
        public IReadOnlyList<ProgressMilestone> Milestones { get; }
        public bool StageCompleted { get; }
        public ContentId SelectedStageId { get; }
        public EncounterClearResult(bool applied, long revision, GameNumber expGranted,
            IReadOnlyList<ItemInstanceState> itemsGranted, IReadOnlyList<ProgressMilestone> milestones,
            bool stageCompleted, ContentId selectedStageId)
        {
            Applied = applied; Revision = revision; ExpGranted = expGranted;
            ItemsGranted = itemsGranted; Milestones = milestones;
            StageCompleted = stageCompleted; SelectedStageId = selectedStageId;
        }
    }

    public sealed class EncounterRewardPlan
    {
        public GameNumber ExpGranted { get; }
        public IReadOnlyList<ItemInstanceState> Items { get; }
        public EncounterRewardPlan(GameNumber expGranted, IReadOnlyList<ItemInstanceState> items)
        {
            if (expGranted.CompareTo(GameNumber.Zero) < 0) throw new ArgumentOutOfRangeException(nameof(expGranted));
            ExpGranted = expGranted;
            var copied = new List<ItemInstanceState>();
            if (items != null)
                foreach (var item in items)
                    copied.Add(item?.Copy() ?? throw new ArgumentException("Reward item is null.", nameof(items)));
            Items = copied.AsReadOnly();
        }
    }

    // Owns approved loot rolls and reward modifiers. The session owns the atomic grant.
    public interface IEncounterRewardPolicy
    {
        EncounterRewardPlan Resolve(EncounterDefinition encounter, GameState snapshot, IRandomStream lootRandom);
    }

    // One writer. A rejected command never swaps in its draft or advances RNG.
    public sealed class GameSession
    {
        private GameState _state;
        private readonly ContentCatalog _catalog;
        private const int RecentOperationLimit = 1024;

        public GameSession(GameState state, ContentCatalog catalog)
        {
            _state = state?.Copy() ?? throw new ArgumentNullException(nameof(state));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            if (!_catalog.Stages.ContainsKey(_state.Run.SelectedStageId)) throw new ArgumentException("Initial stage is absent from catalog.", nameof(state));
        }

        public SessionView ReadView() => new SessionView(_state);

        public PowerOfferResult GenerateNextPowerOffer(IPowerOfferPolicy policy = null)
        {
            var run = _state.Run;
            if (run.PendingPowerOffer.Count != 0)
                return new PowerOfferResult(false, _state.Revision, run.ActivePowerOfferMilestoneId,
                    run.PendingPowerOffer.ToArray());
            if (run.PendingPowerChoiceMilestones.Count == 0)
                return new PowerOfferResult(false, _state.Revision, default, Array.Empty<ContentId>());
            var milestone = FindMilestone(run.PendingPowerChoiceMilestones[0]);
            var eligible = EligiblePowers(_state, milestone);
            if (eligible.Count == 0)
                return new PowerOfferResult(false, _state.Revision, milestone.Id, Array.Empty<ContentId>());
            var draft = _state.Copy();
            var random = Pcg32.Restore(draft.Random.Offers.State, draft.Random.Offers.Increment);
            IReadOnlyList<ContentId> proposed;
            if (milestone.FixedOfferOptions.Count != 0) proposed = eligible;
            else
            {
                if (policy == null) throw new InvalidOperationException("Normal Power choice needs an authored offer policy.");
                proposed = policy.CreateOffer(milestone, eligible.AsReadOnly(), random);
            }
            if (proposed == null || proposed.Count == 0)
                throw new InvalidOperationException("Offer policy returned no options despite an eligible pool.");
            var allowed = new HashSet<ContentId>(eligible);
            var unique = new HashSet<ContentId>();
            foreach (var option in proposed)
                if (!allowed.Contains(option) || !unique.Add(option))
                    throw new InvalidOperationException("Offer policy returned an ineligible or duplicate Power.");
            draft.Run.PendingPowerOffer.AddRange(proposed);
            draft.Run.ActivePowerOfferMilestoneId = milestone.Id;
            draft.Random.Offers.State = random.State;
            draft.Revision = checked(draft.Revision + 1);
            _state = draft;
            return new PowerOfferResult(true, draft.Revision, milestone.Id, draft.Run.PendingPowerOffer.ToArray());
        }

        public CombatTimelineResult AdvanceCombat(SimTime target, int maxEvents, ICombatStatsProvider stats,
            ICombatTargetPolicy targets, BalanceCatalog balance, int attackPriority, int revivePriority,
            HeroDownedAttackClockPolicy heroAttackPolicy)
        {
            if (_state.Combat.IsResolving) throw new InvalidOperationException("A combat action is still resolving.");
            var draft = _state.Copy();
            var result = CombatTimeline.Advance(draft.Combat, draft.Random, target, maxEvents,
                stats, targets, balance, attackPriority, revivePriority, heroAttackPolicy);
            draft.Combat = result.Combat;
            draft.Random = result.Random;
            _state = draft;
            return result;
        }

        // Commits reward and traversal together only after the combat result is final.
        public EncounterClearResult ResolveEncounterClear(IEncounterRewardPolicy rewardPolicy = null)
        {
            if (_state.Combat.Active || _state.Combat.IsResolving)
                throw new InvalidOperationException("Encounter is not at a clear safe point.");
            if (_state.Combat.EncounterRewardClaimed)
                return new EncounterClearResult(false, _state.Revision, GameNumber.Zero,
                    Array.Empty<ItemInstanceState>(), Array.Empty<ProgressMilestone>(), false, _state.Run.SelectedStageId);
            bool hasHero = false, hasEnemy = false;
            foreach (var actor in _state.Combat.Actors)
            {
                if (actor.IsHero) hasHero = true;
                else
                {
                    hasEnemy = true;
                    if (actor.Life != ActorLifeState.Dead)
                        throw new InvalidOperationException("Encounter cannot clear while an enemy is alive.");
                }
            }
            if (!hasHero || !hasEnemy) throw new InvalidOperationException("Cleared encounter has no complete actor set.");
            var draft = _state.Copy();
            if (!_catalog.Stages.TryGetValue(draft.Run.SelectedStageId, out var stage) ||
                !Contains(stage.EncounterIds, draft.Combat.EncounterId))
                throw new InvalidOperationException("Cleared encounter is not part of the selected Stage.");
            if (stage.IsBossStage && draft.Run.CompletedStages.Contains(stage.Id))
                throw new InvalidOperationException("Required Boss reward is once per run.");
            bool wasCompleted = draft.Run.CompletedStages.Contains(stage.Id);
            if (wasCompleted && !stage.IsBossStage &&
                draft.Combat.EncounterId != stage.EncounterIds[stage.EncounterIds.Count - 1])
                throw new InvalidOperationException("Repeat farming must use the Stage end reward profile.");
            var encounter = _catalog.Encounters[draft.Combat.EncounterId];
            bool needsPolicy = !string.IsNullOrEmpty(encounter.LootTableId.Value) || draft.Run.OwnedPowers.Count != 0;
            if (needsPolicy && rewardPolicy == null)
                throw new NotSupportedException("Loot and Power reward modifiers need an installed reward policy.");
            var lootRandom = Pcg32.Restore(draft.Random.Loot.State, draft.Random.Loot.Increment);
            var reward = needsPolicy
                ? rewardPolicy.Resolve(encounter, draft.Copy(), lootRandom)
                : new EncounterRewardPlan(encounter.ExpBudget, Array.Empty<ItemInstanceState>());
            if (reward == null) throw new InvalidOperationException("Reward policy returned no plan.");
            var grantedItems = new List<ItemInstanceState>();
            foreach (var item in reward.Items)
            {
                if (item.InstanceId != 0 || item.ItemLevel < 1 || !(_catalog.Items.ContainsKey(item.DefinitionId)) ||
                    item.AffixIds == null || item.AffixIds.Length != 0 || draft.Run.NextItemSequence == ulong.MaxValue)
                    throw new InvalidOperationException("Reward policy returned an invalid item instance.");
                var granted = item.Copy();
                granted.InstanceId = draft.Run.NextItemSequence++;
                draft.Run.Inventory.Add(granted);
                grantedItems.Add(granted.Copy());
            }
            draft.Random.Loot.State = lootRandom.State;
            draft.Run.Exp = draft.Run.Exp.Add(reward.ExpGranted);
            var milestones = new List<ProgressMilestone>();
            bool stageCompleted = false;
            if (stage.IsBossStage)
            {
                draft.Run.CompletedStages.Add(stage.Id);
                draft.Run.CompletedStageOrder.Add(stage.Id);
                stageCompleted = true;
            }
            else if (!wasCompleted)
            {
                draft.Run.StageRequiredClears.TryGetValue(stage.Id, out long previous);
                long next = checked(previous + 1);
                if (next > stage.BaseRequiredEncounters)
                    throw new InvalidOperationException("Stage progress exceeds the authored requirement.");
                draft.Run.StageRequiredClears[stage.Id] = next;
                var location = _catalog.Locations[stage.LocationId];
                int index = IndexOf(location.StageIds, stage.Id);
                if (index < 0) throw new InvalidOperationException("Stage is absent from its Location order.");
                foreach (var milestone in LocationProgression.Crossed(location, index,
                    stage.BaseRequiredEncounters, checked((int)next), draft.Account.CompletedRunCount == 0))
                {
                    if (!draft.Run.TriggeredMilestones.Add(milestone.Id)) continue;
                    milestones.Add(milestone);
                    if (milestone.Kind == ProgressMilestoneKind.UnlockPower)
                        draft.Account.UnlockedPowers.Add(milestone.RewardId);
                    else if (milestone.Kind == ProgressMilestoneKind.PowerChoice)
                        draft.Run.PendingPowerChoiceMilestones.Add(milestone.Id);
                }
                if (next == stage.BaseRequiredEncounters)
                {
                    draft.Run.CompletedStages.Add(stage.Id);
                    draft.Run.CompletedStageOrder.Add(stage.Id);
                    stageCompleted = true;
                }
            }
            if (stageCompleted && draft.Run.AutoPush)
            {
                var location = _catalog.Locations[stage.LocationId];
                int index = IndexOf(location.StageIds, stage.Id);
                if (index + 1 < location.StageIds.Count)
                {
                    draft.Run.SelectedStageId = location.StageIds[index + 1];
                    draft.Account.DiscoveredStages.Add(draft.Run.SelectedStageId);
                }
            }
            draft.Combat.EncounterRewardClaimed = true;
            draft.Revision = checked(draft.Revision + 1);
            _state = draft;
            return new EncounterClearResult(true, draft.Revision, reward.ExpGranted,
                grantedItems.AsReadOnly(), milestones.AsReadOnly(), stageCompleted, draft.Run.SelectedStageId);
        }

        public void BeginEncounter(CombatState prepared)
        {
            if (prepared == null || !prepared.Active || prepared.EncounterRewardClaimed)
                throw new ArgumentException("A fresh active encounter is required.", nameof(prepared));
            if (_state.Combat.Active || (!string.IsNullOrEmpty(_state.Combat.EncounterId.Value) &&
                !_state.Combat.EncounterRewardClaimed))
                throw new InvalidOperationException("Previous encounter is still active or has an unclaimed reward.");
            if (prepared.Time.CompareTo(_state.Combat.Time) < 0)
                throw new InvalidOperationException("Encounter cannot rewind simulation time.");
            var stage = _catalog.Stages[_state.Run.SelectedStageId];
            if (stage.IsBossStage && _state.Run.CompletedStages.Contains(stage.Id))
                throw new InvalidOperationException("Required Boss cannot be farmed again in this run.");
            if (_state.Run.PendingPowerOffer.Count != 0 ||
                (_state.Run.PendingPowerChoiceMilestones.Count != 0 &&
                 EligiblePowers(_state, FindMilestone(_state.Run.PendingPowerChoiceMilestones[0])).Count != 0))
                throw new InvalidOperationException("An eligible Power choice must be resolved before combat continues.");
            ContentId expected;
            if (stage.IsBossStage) expected = stage.EncounterIds[0];
            else if (_state.Run.CompletedStages.Contains(stage.Id)) expected = stage.EncounterIds[stage.EncounterIds.Count - 1];
            else
            {
                _state.Run.StageRequiredClears.TryGetValue(stage.Id, out long completed);
                if (completed < 0 || completed >= stage.BaseRequiredEncounters)
                    throw new InvalidOperationException("Invalid current Stage progress.");
                if (stage.EncounterIds.Count == 1) expected = stage.EncounterIds[0];
                else if (stage.EncounterIds.Count == stage.BaseRequiredEncounters)
                    expected = stage.EncounterIds[checked((int)completed)];
                else throw new InvalidOperationException("Stage encounter profiles must be one shared profile or one per required clear.");
            }
            if (prepared.EncounterId != expected)
                throw new InvalidOperationException("Prepared encounter is not the current Stage reward/combat profile.");
            var expectedEnemies = new List<ContentId>(_catalog.Encounters[expected].EnemyIds);
            var actualEnemies = new List<ContentId>();
            int heroCount = 0;
            foreach (var actor in prepared.Actors)
            {
                if (actor.Life != ActorLifeState.Alive)
                    throw new InvalidOperationException("New encounter contains a nonliving actor.");
                if (actor.IsHero) heroCount++;
                else actualEnemies.Add(actor.DefinitionId);
            }
            expectedEnemies.Sort(); actualEnemies.Sort();
            if (heroCount != 1 || expectedEnemies.Count != actualEnemies.Count)
                throw new InvalidOperationException("Prepared encounter actor count differs from content.");
            for (int i = 0; i < expectedEnemies.Count; i++)
                if (expectedEnemies[i] != actualEnemies[i])
                    throw new InvalidOperationException("Prepared encounter enemy differs from content.");
            var draft = _state.Copy();
            draft.Combat = prepared.Copy();
            draft.Revision = checked(draft.Revision + 1);
            _state = draft;
        }
        public GameState CaptureAtSafePoint()
        {
            if (_state.Combat.IsResolving) throw new InvalidOperationException("Cannot capture mid-action.");
            return _state.Copy();
        }

        public CommandResult Execute(GameCommand command, IEquipmentTransitionPolicy equipmentTransition = null,
            BalanceCatalog purchaseBalance = null, IPrimaryStatTransitionPolicy primaryStatTransition = null)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_state.RecentOperationIds.Contains(command.OperationId)) return new CommandResult(CommandStatus.AlreadyApplied, _state.Revision, "DuplicateOperation");
            if (command.ExpectedRevision != _state.Revision) return new CommandResult(CommandStatus.Rejected, _state.Revision, "StaleRevision");
            var draft = _state.Copy();
            string reason = Apply(draft, command, equipmentTransition, purchaseBalance, primaryStatTransition);
            if (reason != null) return new CommandResult(CommandStatus.Rejected, _state.Revision, reason);
            draft.Revision = checked(draft.Revision + 1);
            draft.RecentOperationIds.Add(command.OperationId);
            if (draft.RecentOperationIds.Count > RecentOperationLimit) draft.RecentOperationIds.RemoveAt(0);
            _state = draft;
            return new CommandResult(CommandStatus.Applied, _state.Revision, "");
        }

        private string Apply(GameState draft, GameCommand command, IEquipmentTransitionPolicy equipmentTransition,
            BalanceCatalog purchaseBalance, IPrimaryStatTransitionPolicy primaryStatTransition)
        {
            if (command is SetAutoPushCommand autoPush)
            {
                draft.Run.AutoPush = autoPush.Enabled;
                return null;
            }
            if (command is SelectStageCommand select)
            {
                if (!_catalog.Stages.ContainsKey(select.StageId)) return "UnknownStage";
                if (!IsAccessible(draft.Run, select.StageId)) return "StageNotAccessible";
                if (draft.Combat.Active && select.StageId != draft.Run.SelectedStageId)
                    draft.Combat = new CombatState { Time = draft.Combat.Time };
                draft.Run.SelectedStageId = select.StageId;
                return null;
            }
            if (command is SelectOfflineTargetCommand farm)
            {
                if (!_catalog.Stages.TryGetValue(farm.StageId, out var stage)) return "UnknownStage";
                if (stage.IsBossStage || !draft.Run.CompletedStages.Contains(farm.StageId)) return "OfflineTargetNotEligible";
                draft.Run.OfflineTargetStageId = farm.StageId;
                return null;
            }
            if (command is ChoosePowerCommand choice)
            {
                if (draft.Run.PendingPowerChoiceMilestones.Count == 0 || draft.Run.PendingPowerOffer.Count == 0 ||
                    draft.Run.ActivePowerOfferMilestoneId != draft.Run.PendingPowerChoiceMilestones[0])
                    return "NoActivePowerOffer";
                if (!draft.Run.PendingPowerOffer.Contains(choice.PowerId)) return "PowerNotOffered";
                if (!draft.Run.OwnedPowers.Add(choice.PowerId)) return "PowerAlreadyOwned";
                draft.Run.PendingPowerChoiceMilestones.RemoveAt(0);
                draft.Run.PendingPowerOffer.Clear();
                draft.Run.ActivePowerOfferMilestoneId = default;
                return null;
            }
            if (command is SetItemLockCommand itemLock)
            {
                var item = FindItem(draft.Run, itemLock.ItemInstanceId);
                if (item == null) return "ItemNotOwned";
                item.Locked = itemLock.Locked;
                return null;
            }
            if (command is BuyStatUpgradeCommand buy)
            {
                if (!Enum.IsDefined(typeof(PrimaryStatKind), buy.Stat)) return "UnknownPrimaryStat";
                if (purchaseBalance == null) return "StatBalanceRequired";
                if (draft.Combat.Active && primaryStatTransition == null) return "LiveStatPolicyRequired";
                var id = PrimaryStatProgression.Id(buy.Stat);
                draft.Run.PurchasedStats.TryGetValue(id, out long previous);
                if (previous < 0 || previous == long.MaxValue) return "StatPurchaseLimit";
                long next = previous + 1;
                var cost = PrimaryStatProgression.Cost(buy.Stat, next, purchaseBalance);
                if (draft.Run.Exp.CompareTo(cost) < 0) return "InsufficientExp";
                draft.Run.Exp = draft.Run.Exp.Subtract(cost);
                draft.Run.PurchasedStats[id] = next;
                if (draft.Combat.Active) primaryStatTransition.Apply(_state.Copy(), draft, buy.Stat);
                return null;
            }
            if (command is EquipItemCommand equip)
            {
                var item = FindItem(draft.Run, equip.ItemInstanceId);
                if (item == null) return "ItemNotOwned";
                if (!_catalog.Items.TryGetValue(item.DefinitionId, out var definition)) return "UnknownItemDefinition";
                string slot = definition.Slot;
                if (draft.Combat.Active && equipmentTransition == null) return "LiveEquipmentPolicyRequired";
                draft.Run.EquippedItems[slot] = item.InstanceId;
                if (draft.Combat.Active) equipmentTransition.Apply(_state.Copy(), draft, slot);
                return null;
            }
            if (command is UnequipItemCommand unequip)
            {
                if (string.IsNullOrWhiteSpace(unequip.Slot) || !draft.Run.EquippedItems.ContainsKey(unequip.Slot))
                    return "SlotNotEquipped";
                if (draft.Combat.Active && equipmentTransition == null) return "LiveEquipmentPolicyRequired";
                draft.Run.EquippedItems.Remove(unequip.Slot);
                if (draft.Combat.Active) equipmentTransition.Apply(_state.Copy(), draft, unequip.Slot);
                return null;
            }
            return "UnsupportedCommand";
        }

        private static ItemInstanceState FindItem(RunState run, ulong instanceId)
        {
            if (instanceId == 0) return null;
            foreach (var item in run.Inventory) if (item.InstanceId == instanceId) return item;
            return null;
        }

        private ProgressMilestone FindMilestone(ContentId id)
        {
            foreach (var location in _catalog.Locations.Values)
                foreach (var milestone in location.Milestones)
                    if (milestone.Id == id) return milestone;
            throw new InvalidOperationException("Unknown Power-choice milestone: " + id);
        }

        private static List<ContentId> EligiblePowers(GameState state, ProgressMilestone milestone)
        {
            var eligible = new List<ContentId>();
            if (milestone.FixedOfferOptions.Count != 0)
            {
                foreach (var id in milestone.FixedOfferOptions)
                    if (state.Account.UnlockedPowers.Contains(id) && !state.Run.OwnedPowers.Contains(id)) eligible.Add(id);
            }
            else
                foreach (var id in state.Account.UnlockedPowers)
                    if (!state.Run.OwnedPowers.Contains(id)) eligible.Add(id);
            eligible.Sort();
            return eligible;
        }

        private bool IsAccessible(RunState run, ContentId stageId)
        {
            if (stageId == _catalog.StartingStageId || run.CompletedStages.Contains(stageId) ||
                stageId == run.SelectedStageId) return true;
            var stage = _catalog.Stages[stageId];
            var location = _catalog.Locations[stage.LocationId];
            int index = IndexOf(location.StageIds, stageId);
            return index > 0 && run.CompletedStages.Contains(location.StageIds[index - 1]);
        }

        private static bool Contains(IReadOnlyList<ContentId> values, ContentId target) => IndexOf(values, target) >= 0;
        private static int IndexOf(IReadOnlyList<ContentId> values, ContentId target)
        {
            for (int i = 0; i < values.Count; i++) if (values[i] == target) return i;
            return -1;
        }
    }
}
