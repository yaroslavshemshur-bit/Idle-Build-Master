using System;
using IBM.Domain;

namespace IBM.Application
{
    // One authored Normal/Fortify/Exposed cycle. The scheduler owns its durable transition clock.
    public sealed class BossCycleController
    {
        private readonly BossCycleDefinition _cycle;
        private readonly ulong _bossActorId;
        private readonly int _transitionPriority;

        public BossCycleController(BossCycleDefinition cycle, ulong bossActorId, int transitionPriority)
        {
            _cycle = cycle ?? throw new ArgumentNullException(nameof(cycle));
            if (bossActorId == 0) throw new ArgumentOutOfRangeException(nameof(bossActorId));
            _bossActorId = bossActorId;
            _transitionPriority = transitionPriority;
        }

        public CombatState Initialize(CombatState source)
        {
            if (source == null || !source.Active || !string.IsNullOrEmpty(source.Boss.PhaseId.Value))
                throw new InvalidOperationException("Boss cycle requires an active uninitialized encounter.");
            bool found = false;
            foreach (var actor in source.Actors)
                if (actor.InstanceId == _bossActorId && !actor.IsHero && actor.Life == ActorLifeState.Alive &&
                    actor.DefinitionId.Value != null)
                    found = true;
            if (!found) throw new InvalidOperationException("Boss actor is absent or not alive.");
            var draft = source.Copy();
            draft.Boss.PhaseId = _cycle.NormalPhaseId;
            draft.Boss.EnteredAt = draft.Time;
            draft.Boss.Counter = 0;
            draft.Boss.TransitionRevision = checked(draft.Boss.TransitionRevision + 1);
            var scheduler = SimulationScheduler.Restore(draft.Time, draft.NextScheduleSequence, draft.PendingEvents);
            ScheduleNext(scheduler, draft.Boss, _cycle.NormalDuration);
            draft.NextScheduleSequence = scheduler.NextSequence;
            draft.PendingEvents.Clear();
            draft.PendingEvents.AddRange(scheduler.CapturePending());
            return draft;
        }

        public GameNumber EffectiveBlock(CombatState combat, ActorState target, GameNumber baseBlock)
        {
            if (target.InstanceId != _bossActorId) return baseBlock;
            var phase = combat.Boss.PhaseId;
            if (phase == _cycle.NormalPhaseId) return baseBlock.Multiply(_cycle.NormalBlockMultiplier);
            if (phase == _cycle.FortifyPhaseId) return baseBlock.Multiply(_cycle.FortifyBlockMultiplier);
            if (phase == _cycle.ExposedPhaseId) return baseBlock.Multiply(_cycle.ExposedBlockMultiplier);
            throw new InvalidOperationException("Unknown active Boss phase: " + phase);
        }

        public void OnSuccessfulDirectHeroHit(CombatState combat, SimulationScheduler scheduler,
            ulong targetActorId)
        {
            if (targetActorId != _bossActorId || combat.Boss.PhaseId != _cycle.FortifyPhaseId) return;
            combat.Boss.Counter = checked(combat.Boss.Counter + 1);
            if (combat.Boss.Counter >= _cycle.FortifyBreakHits)
            {
                Enter(combat.Boss, _cycle.ExposedPhaseId, scheduler.Now);
                ScheduleNext(scheduler, combat.Boss, _cycle.ExposedDuration);
            }
        }

        public CombatState ApplySuccessfulDirectHeroHit(CombatState source, ulong targetActorId)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var draft = source.Copy();
            var scheduler = SimulationScheduler.Restore(draft.Time, draft.NextScheduleSequence, draft.PendingEvents);
            OnSuccessfulDirectHeroHit(draft, scheduler, targetActorId);
            draft.NextScheduleSequence = scheduler.NextSequence;
            draft.PendingEvents.Clear();
            draft.PendingEvents.AddRange(scheduler.CapturePending());
            return draft;
        }

        public void HandleTransition(CombatState combat, SimulationScheduler scheduler, ScheduledEvent entry)
        {
            if (entry.Kind != ScheduledEventKind.BossTransition || entry.Owner.Value != _bossActorId ||
                entry.DefinitionId != _cycle.Id)
                throw new InvalidOperationException("Boss transition belongs to another cycle.");
            if (entry.OwnerRevision != combat.Boss.TransitionRevision) return;
            if (combat.Boss.PhaseId == _cycle.NormalPhaseId)
                Enter(combat.Boss, _cycle.FortifyPhaseId, scheduler.Now);
            else if (combat.Boss.PhaseId == _cycle.ExposedPhaseId)
            {
                Enter(combat.Boss, _cycle.NormalPhaseId, scheduler.Now);
                ScheduleNext(scheduler, combat.Boss, _cycle.NormalDuration);
            }
            else throw new InvalidOperationException("Unexpected timed transition from Boss phase " + combat.Boss.PhaseId);
        }

        private static void Enter(BossState boss, ContentId phase, SimTime now)
        {
            boss.PhaseId = phase;
            boss.EnteredAt = now;
            boss.Counter = 0;
            boss.TransitionRevision = checked(boss.TransitionRevision + 1);
        }

        private void ScheduleNext(SimulationScheduler scheduler, BossState boss, SimDuration duration) =>
            scheduler.Schedule(scheduler.Now.Add(duration), _transitionPriority, ScheduledEventKind.BossTransition,
                new InstanceId(_bossActorId), boss.TransitionRevision, _cycle.Id);
    }
}
