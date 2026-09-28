using System;
using System.Collections.Generic;
using System.Text;

namespace IBM.Domain
{
    public enum CombatEventPriority
    {
        Replacement = 0,
        FatalPrevention = 1,
        DamageOrHealing = 2,
        Reaction = 3,
        DeathOrRevive = 4,
        EncounterChange = 5
    }
    public enum CombatEventOrigin { DirectAttack, Periodic, Reactive, Secondary, StateTransition }

    public readonly struct CombatEvent
    {
        public ulong RootActionId { get; }
        public ulong ActionId { get; }
        public ulong ParentActionId { get; }
        public ulong SourceActorId { get; }
        public ulong TargetActorId { get; }
        public ContentId EffectId { get; }
        public SimTime Time { get; }
        public CombatEventPriority Priority { get; }
        public CombatEventOrigin Origin { get; }
        public int Depth { get; }
        public CombatEvent(ulong rootActionId, ulong actionId, ulong parentActionId, ulong sourceActorId,
            ulong targetActorId, ContentId effectId, SimTime time, CombatEventPriority priority, CombatEventOrigin origin, int depth)
        {
            RootActionId = rootActionId; ActionId = actionId; ParentActionId = parentActionId;
            SourceActorId = sourceActorId; TargetActorId = targetActorId; EffectId = effectId;
            Time = time; Priority = priority; Origin = origin; Depth = depth;
        }
    }

    public sealed class CombatLoopException : InvalidOperationException
    {
        public CombatLoopException(string message) : base(message) { }
    }

    // One causal root at a time. Budget yields retain the queue; watchdog faults never discard events and continue.
    public sealed class CombatEventProcessor
    {
        private readonly Queue<CombatEvent>[] _queues = new Queue<CombatEvent>[6];
        private readonly Queue<CombatEvent> _trace = new Queue<CombatEvent>();
        private readonly int _maxDepth;
        private readonly int _maxPerRoot;
        private ulong _nextActionId = 1;
        private int _processedForRoot;
        private bool _active;
        public bool HasPending => _active;

        public CombatEventProcessor(int maxDepth, int maxPerRoot)
        {
            if (maxDepth <= 0 || maxPerRoot <= 0) throw new ArgumentOutOfRangeException();
            _maxDepth = maxDepth; _maxPerRoot = maxPerRoot;
            for (int i = 0; i < _queues.Length; i++) _queues[i] = new Queue<CombatEvent>();
        }

        public CombatEvent Begin(ulong sourceActorId, ulong targetActorId, ContentId effectId,
            SimTime time, CombatEventPriority priority, CombatEventOrigin origin)
        {
            if (_active) throw new InvalidOperationException("A causal root is already in progress.");
            _active = true; _processedForRoot = 0; _trace.Clear();
            ulong id = NextId();
            var root = new CombatEvent(id, id, 0, sourceActorId, targetActorId, effectId, time, priority, origin, 0);
            _queues[(int)priority].Enqueue(root);
            return root;
        }

        public CombatEvent Emit(CombatEvent parent, ulong sourceActorId, ulong targetActorId, ContentId effectId,
            CombatEventPriority priority, CombatEventOrigin origin)
        {
            if (!_active || parent.RootActionId == 0) throw new InvalidOperationException("No active parent action.");
            if (parent.Depth + 1 > _maxDepth) throw Fault("Reaction depth exceeded");
            var child = new CombatEvent(parent.RootActionId, NextId(), parent.ActionId, sourceActorId,
                targetActorId, effectId, parent.Time, priority, origin, parent.Depth + 1);
            _queues[(int)priority].Enqueue(child);
            return child;
        }

        public bool Resume(int maxEvents, Action<CombatEvent, CombatEventProcessor> handler)
        {
            if (!_active || handler == null) throw new InvalidOperationException("No active reaction or handler.");
            if (maxEvents <= 0) throw new ArgumentOutOfRangeException(nameof(maxEvents));
            int processed = 0;
            while (HasQueued())
            {
                if (processed >= maxEvents) return false;
                TryDequeue(out var current);
                if (++_processedForRoot > _maxPerRoot) throw Fault("Reaction count exceeded");
                _trace.Enqueue(current);
                if (_trace.Count > 32) _trace.Dequeue();
                handler(current, this);
                processed++;
            }
            _active = false;
            return true;
        }

        private bool TryDequeue(out CombatEvent value)
        {
            for (int i = 0; i < _queues.Length; i++)
                if (_queues[i].Count > 0) { value = _queues[i].Dequeue(); return true; }
            value = default; return false;
        }

        private bool HasQueued()
        {
            foreach (var queue in _queues) if (queue.Count > 0) return true;
            return false;
        }

        private ulong NextId()
        {
            if (_nextActionId == ulong.MaxValue) throw new OverflowException("Action ID sequence exhausted.");
            return _nextActionId++;
        }

        private CombatLoopException Fault(string reason)
        {
            var message = new StringBuilder(reason);
            foreach (var entry in _trace) message.Append(" -> ").Append(entry.ActionId).Append(':').Append(entry.EffectId);
            return new CombatLoopException(message.ToString());
        }
    }
}
