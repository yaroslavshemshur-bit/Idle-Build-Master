using System;
using IBM.Domain;

namespace IBM.Application
{
    // Each application gets its own instance and expiry. Stale expiry revisions cannot remove a new stack.
    public sealed class EffectLifetimeController
    {
        private readonly ContentCatalog _catalog;
        private readonly int _expiryPriority;
        public EffectLifetimeController(ContentCatalog catalog, int expiryPriority)
        { _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); _expiryPriority = expiryPriority; }

        public CombatState Apply(CombatState source, ContentId effectId, ulong sourceActorId, ulong ownerActorId)
        {
            if (source == null || !source.Active || !_catalog.Effects.TryGetValue(effectId, out var definition) ||
                definition.Duration.Microseconds <= 0)
                throw new InvalidOperationException("Timed effect has no authored positive duration.");
            bool sourcePresent = false, ownerPresent = false;
            foreach (var actor in source.Actors)
            {
                if (actor.InstanceId == sourceActorId) sourcePresent = true;
                if (actor.InstanceId == ownerActorId) ownerPresent = true;
            }
            if (!sourcePresent || !ownerPresent || source.NextEffectSequence == ulong.MaxValue)
                throw new InvalidOperationException("Timed effect actors or sequence are invalid.");
            var draft = source.Copy();
            var scheduler = SimulationScheduler.Restore(draft.Time, draft.NextScheduleSequence, draft.PendingEvents);
            var effect = new EffectInstanceState { InstanceId = draft.NextEffectSequence++,
                DefinitionId = effectId, SourceActorId = sourceActorId, OwnerActorId = ownerActorId,
                CreatedAt = draft.Time, ExpiresAt = draft.Time.Add(definition.Duration), Stacks = 1,
                Revision = 1 };
            draft.Effects.Add(effect);
            scheduler.Schedule(effect.ExpiresAt, _expiryPriority, ScheduledEventKind.EffectExpire,
                new InstanceId(effect.InstanceId), effect.Revision, effect.DefinitionId);
            draft.NextScheduleSequence = scheduler.NextSequence;
            draft.PendingEvents.Clear();
            draft.PendingEvents.AddRange(scheduler.CapturePending());
            return draft;
        }

        public void HandleExpire(CombatState combat, ScheduledEvent entry)
        {
            if (entry.Kind != ScheduledEventKind.EffectExpire)
                throw new ArgumentException("Not an effect-expiry event.", nameof(entry));
            for (int i = 0; i < combat.Effects.Count; i++)
            {
                var effect = combat.Effects[i];
                if (effect.InstanceId != entry.Owner.Value) continue;
                if (effect.Revision != entry.OwnerRevision || effect.DefinitionId != entry.DefinitionId)
                    return;
                if (effect.ExpiresAt.CompareTo(entry.Due) != 0)
                    throw new InvalidOperationException("Effect expiry clock disagrees with scheduled event.");
                combat.Effects.RemoveAt(i);
                return;
            }
        }
    }
}
