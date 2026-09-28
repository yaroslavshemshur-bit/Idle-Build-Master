using System;

namespace IBM.Domain
{
    public sealed class BossCycleDefinition
    {
        public ContentId Id { get; }
        public ContentId NormalPhaseId { get; }
        public ContentId FortifyPhaseId { get; }
        public ContentId ExposedPhaseId { get; }
        public SimDuration NormalDuration { get; }
        public SimDuration ExposedDuration { get; }
        public long FortifyBreakHits { get; }
        public GameNumber NormalBlockMultiplier { get; }
        public GameNumber FortifyBlockMultiplier { get; }
        public GameNumber ExposedBlockMultiplier { get; }

        public BossCycleDefinition(ContentId id, ContentId normalPhaseId, ContentId fortifyPhaseId,
            ContentId exposedPhaseId, SimDuration normalDuration, SimDuration exposedDuration,
            long fortifyBreakHits, GameNumber normalBlockMultiplier,
            GameNumber fortifyBlockMultiplier, GameNumber exposedBlockMultiplier)
        {
            if (string.IsNullOrEmpty(id.Value) || string.IsNullOrEmpty(normalPhaseId.Value) ||
                string.IsNullOrEmpty(fortifyPhaseId.Value) || string.IsNullOrEmpty(exposedPhaseId.Value) ||
                normalPhaseId == fortifyPhaseId || normalPhaseId == exposedPhaseId ||
                fortifyPhaseId == exposedPhaseId || normalDuration.Microseconds <= 0 ||
                exposedDuration.Microseconds <= 0 || fortifyBreakHits <= 0 ||
                normalBlockMultiplier.CompareTo(GameNumber.Zero) <= 0 ||
                fortifyBlockMultiplier.CompareTo(GameNumber.Zero) <= 0 ||
                exposedBlockMultiplier.CompareTo(GameNumber.Zero) <= 0)
                throw new ArgumentException("Boss cycle has invalid phases or balance values.");
            Id = id; NormalPhaseId = normalPhaseId; FortifyPhaseId = fortifyPhaseId;
            ExposedPhaseId = exposedPhaseId; NormalDuration = normalDuration;
            ExposedDuration = exposedDuration; FortifyBreakHits = fortifyBreakHits;
            NormalBlockMultiplier = normalBlockMultiplier;
            FortifyBlockMultiplier = fortifyBlockMultiplier;
            ExposedBlockMultiplier = exposedBlockMultiplier;
        }
    }
}
