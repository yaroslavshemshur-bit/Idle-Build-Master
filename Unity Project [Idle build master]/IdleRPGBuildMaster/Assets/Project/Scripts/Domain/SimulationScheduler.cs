using System;
using System.Collections.Generic;

namespace IBM.Domain
{
    public enum ScheduledEventKind { Attack, EffectExpire, PeriodicTick, BossTransition, Revive }
    public enum AdvanceStatus { ReachedTarget, Yielded, Faulted }

    public readonly struct ScheduledEvent : IComparable<ScheduledEvent>
    {
        public SimTime Due { get; }
        public int PhasePriority { get; }
        public ulong Sequence { get; }
        public ScheduledEventKind Kind { get; }
        public InstanceId Owner { get; }
        public long OwnerRevision { get; }
        public ContentId DefinitionId { get; }
        public ScheduledEvent(SimTime due, int phasePriority, ulong sequence, ScheduledEventKind kind,
            InstanceId owner, long ownerRevision, ContentId definitionId)
        {
            Due = due; PhasePriority = phasePriority; Sequence = sequence;
            Kind = kind; Owner = owner; OwnerRevision = ownerRevision; DefinitionId = definitionId;
        }
        public int CompareTo(ScheduledEvent other)
        {
            int result = Due.CompareTo(other.Due);
            if (result != 0) return result;
            result = PhasePriority.CompareTo(other.PhasePriority);
            return result != 0 ? result : Sequence.CompareTo(other.Sequence);
        }
    }

    public readonly struct AdvanceResult
    {
        public AdvanceStatus Status { get; }
        public SimTime ReachedTime { get; }
        public int ProcessedEvents { get; }
        public string Diagnostic { get; }
        public AdvanceResult(AdvanceStatus status, SimTime reachedTime, int processedEvents, string diagnostic)
        { Status = status; ReachedTime = reachedTime; ProcessedEvents = processedEvents; Diagnostic = diagnostic; }
    }

    public interface IScheduledEventHandler
    {
        void AdvanceContinuous(SimTime from, SimTime to);
        void Handle(ScheduledEvent scheduled, SimulationScheduler scheduler);
    }

    // Mechanics choose phase priorities; this scheduler only guarantees stable key ordering.
    public sealed class SimulationScheduler
    {
        private readonly List<ScheduledEvent> _heap = new List<ScheduledEvent>();
        private ulong _nextSequence;
        public SimTime Now { get; private set; }
        public ulong NextSequence => _nextSequence;
        public int PendingCount => _heap.Count;

        public SimulationScheduler(SimTime now, ulong nextSequence = 1)
        {
            if (nextSequence == 0) throw new ArgumentOutOfRangeException(nameof(nextSequence));
            Now = now; _nextSequence = nextSequence;
        }

        public ScheduledEvent Schedule(SimTime due, int phasePriority, ScheduledEventKind kind,
            InstanceId owner, long ownerRevision, ContentId definitionId)
        {
            if (due.CompareTo(Now) < 0) throw new ArgumentOutOfRangeException(nameof(due), "Cannot schedule into the past.");
            if (_nextSequence == ulong.MaxValue) throw new OverflowException("Scheduler sequence exhausted.");
            var entry = new ScheduledEvent(due, phasePriority, _nextSequence++, kind, owner, ownerRevision, definitionId);
            Push(entry);
            return entry;
        }

        public AdvanceResult AdvanceTo(SimTime target, int maxEvents, IScheduledEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (target.CompareTo(Now) < 0) throw new ArgumentOutOfRangeException(nameof(target));
            if (maxEvents <= 0) throw new ArgumentOutOfRangeException(nameof(maxEvents));
            int processed = 0;
            while (_heap.Count > 0 && _heap[0].Due.CompareTo(target) <= 0)
            {
                if (processed >= maxEvents) return new AdvanceResult(AdvanceStatus.Yielded, Now, processed, "SimulationBacklog");
                var entry = Pop();
                handler.AdvanceContinuous(Now, entry.Due);
                Now = entry.Due;
                handler.Handle(entry, this);
                processed++;
            }
            handler.AdvanceContinuous(Now, target);
            Now = target;
            return new AdvanceResult(AdvanceStatus.ReachedTarget, Now, processed, "");
        }

        public ScheduledEvent[] CapturePending()
        {
            var copy = _heap.ToArray();
            Array.Sort(copy);
            return copy;
        }

        // Used by explicit clock policies such as freezing enemy clocks during hero downed time.
        public void ShiftPending(Func<ScheduledEvent, bool> predicate, SimDuration delay)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (delay.Microseconds == 0) return;
            var pending = CapturePending();
            _heap.Clear();
            foreach (var entry in pending)
            {
                var shifted = predicate(entry)
                    ? new ScheduledEvent(entry.Due.Add(delay), entry.PhasePriority, entry.Sequence,
                        entry.Kind, entry.Owner, entry.OwnerRevision, entry.DefinitionId)
                    : entry;
                Push(shifted);
            }
        }

        public static SimulationScheduler Restore(SimTime now, ulong nextSequence, IReadOnlyList<ScheduledEvent> pending)
        {
            var scheduler = new SimulationScheduler(now, nextSequence);
            var keys = new HashSet<ulong>();
            foreach (var entry in pending)
            {
                if (entry.Due.CompareTo(now) < 0 || entry.Sequence == 0 || entry.Sequence >= nextSequence || !keys.Add(entry.Sequence))
                    throw new ArgumentException("Invalid persisted scheduler entry.", nameof(pending));
                scheduler.Push(entry);
            }
            return scheduler;
        }

        private void Push(ScheduledEvent entry)
        {
            int child = _heap.Count;
            _heap.Add(entry);
            while (child > 0)
            {
                int parent = (child - 1) / 2;
                if (_heap[parent].CompareTo(entry) <= 0) break;
                _heap[child] = _heap[parent]; child = parent;
            }
            _heap[child] = entry;
        }

        private ScheduledEvent Pop()
        {
            var result = _heap[0];
            int last = _heap.Count - 1;
            var replacement = _heap[last];
            _heap.RemoveAt(last);
            if (last == 0) return result;
            int parent = 0;
            while (true)
            {
                int left = parent * 2 + 1;
                if (left >= _heap.Count) break;
                int right = left + 1;
                int best = right < _heap.Count && _heap[right].CompareTo(_heap[left]) < 0 ? right : left;
                if (_heap[best].CompareTo(replacement) >= 0) break;
                _heap[parent] = _heap[best]; parent = best;
            }
            _heap[parent] = replacement;
            return result;
        }
    }
}
