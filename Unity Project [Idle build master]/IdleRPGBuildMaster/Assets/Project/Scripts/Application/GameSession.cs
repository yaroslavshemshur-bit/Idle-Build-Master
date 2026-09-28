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
        public HashSet<ContentId> OwnedPowers { get; }
        public List<ContentId> PendingPowerOffer { get; }
        public List<ItemInstanceState> Inventory { get; }
        public Dictionary<string, ulong> EquippedItems { get; }
        public RunState(string runId, ContentId firstStage)
        {
            if (string.IsNullOrWhiteSpace(runId)) throw new ArgumentException("Run ID is required.", nameof(runId));
            RunId = runId; SelectedStageId = firstStage; AutoPush = true;
            PurchasedStats = new Dictionary<ContentId, long>();
            CompletedStages = new HashSet<ContentId>();
            CompletedStageOrder = new List<ContentId>();
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
            OwnedPowers = new HashSet<ContentId>(other.OwnedPowers);
            PendingPowerOffer = new List<ContentId>(other.PendingPowerOffer);
            Inventory = new List<ItemInstanceState>(other.Inventory.Count);
            foreach (var item in other.Inventory) Inventory.Add(item.Copy());
            EquippedItems = new Dictionary<string, ulong>(other.EquippedItems, StringComparer.Ordinal);
        }
        internal RunState Copy() => new RunState(this);
    }

    public sealed class CombatState
    {
        public ContentId EncounterId { get; internal set; }
        public SimTime Time { get; internal set; }
        public bool Active { get; internal set; }
        public bool IsResolving { get; internal set; }
        public List<ActorState> Actors { get; } = new List<ActorState>();
        public List<EffectInstanceState> Effects { get; } = new List<EffectInstanceState>();
        public List<ScheduledEvent> PendingEvents { get; } = new List<ScheduledEvent>();
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
            foreach (var actor in Actors) copy.Actors.Add(actor.Copy());
            foreach (var effect in Effects) copy.Effects.Add(effect.Copy());
            copy.PendingEvents.AddRange(PendingEvents);
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
        public CombatState Combat { get; }
        public long Revision { get; internal set; }
        public VersionStamp Versions { get; }
        public SessionRandomState Random { get; }
        public OfflineAccountingState Offline { get; }
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
        internal GameState Copy() => new GameState(Account.Copy(), Run.Copy(), Combat.Copy(), Versions, Random.Copy(), Offline.Copy()) { Revision = Revision };
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

    // One writer. A rejected command never swaps in its draft or advances RNG.
    public sealed class GameSession
    {
        private GameState _state;
        private readonly ContentCatalog _catalog;
        private readonly HashSet<string> _recentOperations = new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _operationOrder = new Queue<string>();
        private const int RecentOperationLimit = 1024;

        public GameSession(GameState state, ContentCatalog catalog)
        {
            _state = state?.Copy() ?? throw new ArgumentNullException(nameof(state));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            if (!_catalog.Stages.ContainsKey(_state.Run.SelectedStageId)) throw new ArgumentException("Initial stage is absent from catalog.", nameof(state));
        }

        public SessionView ReadView() => new SessionView(_state);
        public GameState CaptureAtSafePoint()
        {
            if (_state.Combat.IsResolving) throw new InvalidOperationException("Cannot capture mid-action.");
            return _state.Copy();
        }

        public CommandResult Execute(GameCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_recentOperations.Contains(command.OperationId)) return new CommandResult(CommandStatus.AlreadyApplied, _state.Revision, "DuplicateOperation");
            if (command.ExpectedRevision != _state.Revision) return new CommandResult(CommandStatus.Rejected, _state.Revision, "StaleRevision");
            var draft = _state.Copy();
            string reason = Apply(draft, command);
            if (reason != null) return new CommandResult(CommandStatus.Rejected, _state.Revision, reason);
            draft.Revision = checked(draft.Revision + 1);
            _state = draft;
            _recentOperations.Add(command.OperationId);
            _operationOrder.Enqueue(command.OperationId);
            if (_operationOrder.Count > RecentOperationLimit) _recentOperations.Remove(_operationOrder.Dequeue());
            return new CommandResult(CommandStatus.Applied, _state.Revision, "");
        }

        private string Apply(GameState draft, GameCommand command)
        {
            if (command is SetAutoPushCommand autoPush)
            {
                draft.Run.AutoPush = autoPush.Enabled;
                return null;
            }
            if (command is SelectStageCommand select)
            {
                if (!_catalog.Stages.ContainsKey(select.StageId)) return "UnknownStage";
                if (draft.Combat.Active) return "EncounterTransitionPolicyRequired";
                if (select.StageId != draft.Run.SelectedStageId && !draft.Run.CompletedStages.Contains(select.StageId)) return "StageNotAccessible";
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
            return "UnsupportedCommand";
        }
    }
}
