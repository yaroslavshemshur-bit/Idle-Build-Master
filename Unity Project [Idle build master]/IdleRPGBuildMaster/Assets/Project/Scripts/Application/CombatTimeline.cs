using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    public enum HeroDownedAttackClockPolicy { PauseRemaining, RestartOnRevive }

    public interface ICombatStatsProvider
    {
        DerivedStats Resolve(ActorState actor);
    }

    public interface ICombatTargetPolicy
    {
        ulong ChooseTarget(ActorState attacker, IReadOnlyList<ActorState> actors);
    }

    public readonly struct CombatAttackRecord
    {
        public SimTime Time { get; }
        public ulong AttackerId { get; }
        public ulong TargetId { get; }
        public BasicAttackResult Result { get; }
        public CombatAttackRecord(SimTime time, ulong attackerId, ulong targetId, BasicAttackResult result)
        { Time = time; AttackerId = attackerId; TargetId = targetId; Result = result; }
    }

    public sealed class CombatTimelineResult
    {
        public CombatState Combat { get; }
        public SessionRandomState Random { get; }
        public AdvanceResult Advance { get; }
        public IReadOnlyList<CombatAttackRecord> Attacks { get; }
        public CombatTimelineResult(CombatState combat, SessionRandomState random, AdvanceResult advance,
            IReadOnlyList<CombatAttackRecord> attacks)
        { Combat = combat; Random = random; Advance = advance; Attacks = attacks; }
    }

    // Exact ordinary-attack slice. It commits only complete attacks to a copied state.
    // Effects, phase changes and the downed lifecycle are separate scheduled policies.
    public static class CombatTimeline
    {
        public static CombatState StartOrdinaryAttacks(CombatState source, ICombatStatsProvider stats, int attackPriority)
        {
            if (source == null || stats == null) throw new ArgumentNullException(source == null ? nameof(source) : nameof(stats));
            if (!source.Active || source.PendingEvents.Count != 0) throw new InvalidOperationException("Encounter is not ready to initialize attacks.");
            if (source.Effects.Count != 0)
                throw new NotSupportedException("Ordinary attacks require an installed effect handler for active effects.");
            var draft = source.Copy();
            var scheduler = new SimulationScheduler(draft.Time, draft.NextScheduleSequence);
            var ordered = new List<ActorState>(draft.Actors);
            ordered.Sort((left, right) => left.InstanceId.CompareTo(right.InstanceId));
            foreach (var actor in ordered)
            {
                actor.RegenAnchorAt = draft.Time;
                actor.RegenAnchorHp = actor.Hp;
                if (actor.Life != ActorLifeState.Alive) continue;
                var derived = stats.Resolve(actor);
                if (derived.AttacksPerSecond <= 0d) continue;
                actor.AttackClockRevision = checked(actor.AttackClockRevision + 1);
                actor.NextAttackAt = draft.Time.Add(CombatMath.AttackInterval(derived));
                scheduler.Schedule(actor.NextAttackAt, attackPriority, ScheduledEventKind.Attack,
                    new InstanceId(actor.InstanceId), actor.AttackClockRevision, actor.DefinitionId);
            }
            draft.NextScheduleSequence = scheduler.NextSequence;
            draft.PendingEvents.AddRange(scheduler.CapturePending());
            return draft;
        }

        public static CombatTimelineResult Advance(CombatState source, SessionRandomState randomState,
            SimTime targetTime, int maxEvents, ICombatStatsProvider stats, ICombatTargetPolicy targets,
            BalanceCatalog balance, int attackPriority, int revivePriority,
            HeroDownedAttackClockPolicy heroAttackPolicy, BossCycleController bossCycle = null,
            EffectLifetimeController effects = null, ICombatAttackModifierPolicy attackModifiers = null)
        {
            if (source == null || randomState == null || stats == null || targets == null || balance == null)
                throw new ArgumentNullException("A combat state, RNG, stats, target policy and balance are required.");
            if (!source.Active) throw new InvalidOperationException("No active encounter to advance.");
            if ((source.Effects.Count != 0 && effects == null) ||
                (!string.IsNullOrEmpty(source.Boss.PhaseId.Value) && bossCycle == null))
                throw new NotSupportedException("Effect and Boss handlers must be installed before this encounter can advance.");
            if (!Enum.IsDefined(typeof(HeroDownedAttackClockPolicy), heroAttackPolicy))
                throw new ArgumentOutOfRangeException(nameof(heroAttackPolicy));
            foreach (var actor in source.Actors)
                if (actor.IsHero && actor.Life == ActorLifeState.Downed && source.DownedAttackPolicy != heroAttackPolicy)
                    throw new InvalidOperationException("Downed attack-clock policy changed during a saved recovery.");
            var draft = source.Copy();
            var randomDraft = randomState.Copy();
            var random = Pcg32.Restore(randomDraft.Combat.State, randomDraft.Combat.Increment);
            var scheduler = SimulationScheduler.Restore(draft.Time, draft.NextScheduleSequence, draft.PendingEvents);
            var handler = new OrdinaryAttackHandler(draft, scheduler, random, stats, targets, balance,
                attackPriority, revivePriority, heroAttackPolicy, bossCycle, effects, attackModifiers);
            var advance = scheduler.AdvanceTo(targetTime, maxEvents, handler);
            draft.Time = scheduler.Now;
            draft.NextScheduleSequence = scheduler.NextSequence;
            draft.PendingEvents.Clear();
            if (draft.Active) draft.PendingEvents.AddRange(scheduler.CapturePending());
            randomDraft.Combat.State = random.State;
            return new CombatTimelineResult(draft, randomDraft, advance, handler.Attacks.ToArray());
        }

        private sealed class OrdinaryAttackHandler : IScheduledEventHandler
        {
            private readonly CombatState _combat;
            private readonly SimulationScheduler _scheduler;
            private readonly Pcg32 _random;
            private readonly ICombatStatsProvider _stats;
            private readonly ICombatTargetPolicy _targets;
            private readonly BalanceCatalog _balance;
            private readonly int _attackPriority;
            private readonly int _revivePriority;
            private readonly HeroDownedAttackClockPolicy _heroAttackPolicy;
            private readonly BossCycleController _bossCycle;
            private readonly EffectLifetimeController _effects;
            private readonly ICombatAttackModifierPolicy _attackModifiers;
            public readonly List<CombatAttackRecord> Attacks = new List<CombatAttackRecord>();

            public OrdinaryAttackHandler(CombatState combat, SimulationScheduler scheduler, Pcg32 random,
                ICombatStatsProvider stats, ICombatTargetPolicy targets, BalanceCatalog balance,
                int attackPriority, int revivePriority, HeroDownedAttackClockPolicy heroAttackPolicy,
                BossCycleController bossCycle, EffectLifetimeController effects,
                ICombatAttackModifierPolicy attackModifiers)
            {
                _combat = combat; _scheduler = scheduler; _random = random; _stats = stats;
                _targets = targets; _balance = balance; _attackPriority = attackPriority;
                _revivePriority = revivePriority; _heroAttackPolicy = heroAttackPolicy;
                _bossCycle = bossCycle;
                _effects = effects;
                _attackModifiers = attackModifiers;
            }

            public void AdvanceContinuous(SimTime from, SimTime to)
            {
                if (!_combat.Active) return;
                bool heroDowned = false;
                foreach (var actor in _combat.Actors)
                    if (actor.IsHero && actor.Life == ActorLifeState.Downed) heroDowned = true;
                foreach (var actor in _combat.Actors)
                {
                    if (actor.Life != ActorLifeState.Alive && actor.Life != ActorLifeState.Downed) continue;
                    if (heroDowned && !actor.IsHero) continue;
                    if (actor.RegenAnchorAt.CompareTo(to) > 0) throw new InvalidOperationException("Regeneration anchor is in the future.");
                    long elapsed = checked(to.Microseconds - actor.RegenAnchorAt.Microseconds);
                    var duration = GameNumber.FromInt64(elapsed).Divide(GameNumber.FromInt64(1000000));
                    var rate = actor.Life == ActorLifeState.Downed
                        ? CombatMath.DeathRegenerationPerSecond(_stats.Resolve(actor), _balance)
                        : _stats.Resolve(actor).RegenPerSecond;
                    var gain = rate.Multiply(duration);
                    actor.Hp = GameNumber.Min(actor.MaxHp, actor.RegenAnchorHp.Add(gain));
                }
            }

            public void Handle(ScheduledEvent scheduled, SimulationScheduler scheduler)
            {
                if (!_combat.Active) return;
                if (scheduled.Kind == ScheduledEventKind.Revive)
                {
                    Revive(scheduled, scheduler);
                    return;
                }
                if (scheduled.Kind == ScheduledEventKind.BossTransition)
                {
                    if (_bossCycle == null) throw new NotSupportedException("Boss transition has no handler.");
                    _bossCycle.HandleTransition(_combat, scheduler, scheduled);
                    return;
                }
                if (scheduled.Kind == ScheduledEventKind.EffectExpire)
                {
                    if (_effects == null) throw new NotSupportedException("Effect expiry has no handler.");
                    _effects.HandleExpire(_combat, scheduled);
                    return;
                }
                if (scheduled.Kind != ScheduledEventKind.Attack)
                    throw new NotSupportedException("The ordinary-attack slice does not handle " + scheduled.Kind + ".");
                var attacker = Find(scheduled.Owner.Value);
                if (attacker.Life != ActorLifeState.Alive || attacker.AttackClockRevision != scheduled.OwnerRevision) return;
                ulong targetId = _targets.ChooseTarget(attacker, _combat.Actors);
                if (_attackModifiers != null)
                    targetId = _attackModifiers.ReplaceTarget(attacker, targetId, _random);
                var target = Find(targetId);
                bool selfRedirect = !attacker.IsHero && target.InstanceId == attacker.InstanceId;
                if (target.Life != ActorLifeState.Alive ||
                    (target.IsHero == attacker.IsHero && !selfRedirect))
                    throw new InvalidOperationException("Target policy returned a dead or friendly actor.");
                var attackerStats = _stats.Resolve(attacker);
                var targetStats = _stats.Resolve(target);
                var block = _bossCycle == null ? targetStats.Block :
                    _bossCycle.EffectiveBlock(_combat, target, targetStats.Block);
                if (_attackModifiers != null)
                    block = _attackModifiers.ModifyBlock(attacker, target, block, _random);
                var attack = BasicAttackResolver.Resolve(attackerStats, targetStats, target.Hp,
                    block, _random, _balance);
                Attacks.Add(new CombatAttackRecord(scheduler.Now, attacker.InstanceId, target.InstanceId, attack));
                bool heroBecameDowned = false;
                if (attack.Hit)
                {
                    target.Hp = attack.Damage.FinalHp;
                    target.RegenAnchorAt = scheduler.Now;
                    target.RegenAnchorHp = target.Hp;
                    if (target.Hp.IsZero)
                    {
                        if (target.IsHero)
                        {
                            target.Life = ActorLifeState.Downed;
                            heroBecameDowned = true;
                        }
                        else
                        {
                            target.Life = ActorLifeState.Dead;
                            bool enemyAlive = false;
                            foreach (var actor in _combat.Actors)
                                if (!actor.IsHero && actor.Life == ActorLifeState.Alive) enemyAlive = true;
                            if (!enemyAlive) _combat.Active = false;
                        }
                    }
                    if (attacker.IsHero && _bossCycle != null)
                        _bossCycle.OnSuccessfulDirectHeroHit(_combat, scheduler, target.InstanceId);
                }
                if (_combat.Active && attacker.Life == ActorLifeState.Alive && attackerStats.AttacksPerSecond > 0d)
                {
                    attacker.NextAttackAt = scheduler.Now.Add(CombatMath.AttackInterval(attackerStats));
                    _scheduler.Schedule(attacker.NextAttackAt, _attackPriority, ScheduledEventKind.Attack,
                        new InstanceId(attacker.InstanceId), attacker.AttackClockRevision, attacker.DefinitionId);
                }
                if (heroBecameDowned) EnterDowned(target, scheduler);
            }

            private void EnterDowned(ActorState hero, SimulationScheduler scheduler)
            {
                _combat.DownedStartedAt = scheduler.Now;
                _combat.DownedAttackPolicy = _heroAttackPolicy;
                var paused = scheduler.SuspendPending(entry =>
                {
                    if (entry.Kind == ScheduledEventKind.BossTransition) return true;
                    if (entry.Kind != ScheduledEventKind.Attack && entry.Kind != ScheduledEventKind.EffectExpire &&
                        entry.Kind != ScheduledEventKind.PeriodicTick) return false;
                    if (entry.Kind == ScheduledEventKind.Attack) return true;
                    foreach (var effect in _combat.Effects)
                        if (effect.InstanceId == entry.Owner.Value)
                            return !Find(effect.OwnerActorId).IsHero;
                    throw new InvalidOperationException("Timed event has no effect owner.");
                });
                _combat.PausedEvents.AddRange(paused);
                var rate = CombatMath.DeathRegenerationPerSecond(_stats.Resolve(hero), _balance);
                long recoveryMicroseconds = RecoveryTimeMicroseconds(hero.MaxHp, rate, scheduler.Now);
                scheduler.Schedule(new SimTime(checked(scheduler.Now.Microseconds + recoveryMicroseconds)),
                    _revivePriority, ScheduledEventKind.Revive, new InstanceId(hero.InstanceId),
                    hero.AttackClockRevision, hero.DefinitionId);
            }

            private void Revive(ScheduledEvent scheduled, SimulationScheduler scheduler)
            {
                var hero = Find(scheduled.Owner.Value);
                if (!hero.IsHero || hero.Life != ActorLifeState.Downed || hero.Hp.CompareTo(hero.MaxHp) < 0)
                    throw new InvalidOperationException("Revive event is not at the full-HP threshold.");
                var delay = new SimDuration(checked(scheduler.Now.Microseconds - _combat.DownedStartedAt.Microseconds));
                var resume = new List<ScheduledEvent>();
                foreach (var entry in _combat.PausedEvents)
                    if (_heroAttackPolicy != HeroDownedAttackClockPolicy.RestartOnRevive ||
                        entry.Kind != ScheduledEventKind.Attack || entry.Owner.Value != hero.InstanceId)
                        resume.Add(entry);
                scheduler.ResumePending(resume, delay);
                foreach (var actor in _combat.Actors)
                {
                    if (actor.IsHero)
                    {
                        if (_heroAttackPolicy == HeroDownedAttackClockPolicy.PauseRemaining &&
                            actor.NextAttackAt.CompareTo(_combat.DownedStartedAt) >= 0)
                            actor.NextAttackAt = actor.NextAttackAt.Add(delay);
                    }
                    else
                    {
                        actor.RegenAnchorAt = actor.RegenAnchorAt.Add(delay);
                        if (actor.Life == ActorLifeState.Alive && actor.NextAttackAt.CompareTo(_combat.DownedStartedAt) >= 0)
                            actor.NextAttackAt = actor.NextAttackAt.Add(delay);
                    }
                }
                foreach (var effect in _combat.Effects)
                    if (!Find(effect.OwnerActorId).IsHero) effect.ExpiresAt = effect.ExpiresAt.Add(delay);
                _combat.Boss.EnteredAt = _combat.Boss.EnteredAt.Add(delay);
                _combat.PausedEvents.Clear();
                _combat.DownedStartedAt = new SimTime(0);
                hero.Life = ActorLifeState.Alive;
                hero.RegenAnchorAt = scheduler.Now;
                hero.RegenAnchorHp = hero.Hp;
                if (_heroAttackPolicy == HeroDownedAttackClockPolicy.RestartOnRevive)
                {
                    var stats = _stats.Resolve(hero);
                    if (stats.AttacksPerSecond > 0d)
                    {
                        hero.AttackClockRevision = checked(hero.AttackClockRevision + 1);
                        hero.NextAttackAt = scheduler.Now.Add(CombatMath.AttackInterval(stats));
                        scheduler.Schedule(hero.NextAttackAt, _attackPriority, ScheduledEventKind.Attack,
                            new InstanceId(hero.InstanceId), hero.AttackClockRevision, hero.DefinitionId);
                    }
                }
            }

            private static long RecoveryTimeMicroseconds(GameNumber maxHp, GameNumber rate, SimTime now)
            {
                if (rate.CompareTo(GameNumber.Zero) <= 0 || maxHp.CompareTo(GameNumber.Zero) <= 0 || now.Microseconds < 0)
                    throw new InvalidOperationException("Downed recovery requires positive HP, regeneration and simulation time.");
                long limit = long.MaxValue - now.Microseconds;
                long upper = 1;
                while (!Recovered(upper, maxHp, rate))
                {
                    if (upper == limit) throw new OverflowException("Revive time exceeds the simulation time range.");
                    upper = upper > limit / 2 ? limit : upper * 2;
                }
                long lower = 0;
                while (lower + 1 < upper)
                {
                    long midpoint = lower + (upper - lower) / 2;
                    if (Recovered(midpoint, maxHp, rate)) upper = midpoint;
                    else lower = midpoint;
                }
                return upper;
            }

            private static bool Recovered(long microseconds, GameNumber maxHp, GameNumber rate) =>
                rate.Multiply(GameNumber.FromInt64(microseconds).Divide(GameNumber.FromInt64(1000000)))
                    .CompareTo(maxHp) >= 0;

            private ActorState Find(ulong id)
            {
                foreach (var actor in _combat.Actors) if (actor.InstanceId == id) return actor;
                throw new InvalidOperationException("Scheduled actor or selected target is absent: " + id);
            }
        }
    }
}
