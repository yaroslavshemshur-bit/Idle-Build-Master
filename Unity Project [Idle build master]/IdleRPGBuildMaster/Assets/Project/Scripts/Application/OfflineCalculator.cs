using System;
using System.Globalization;
using IBM.Domain;

namespace IBM.Application
{
    public readonly struct OfflineCalculation
    {
        public decimal EligibleSeconds { get; }
        public long VirtualClears { get; }
        public decimal Residual { get; }
        public bool CapReached { get; }
        public OfflineCalculation(decimal eligibleSeconds, long virtualClears, decimal residual, bool capReached)
        { EligibleSeconds = eligibleSeconds; VirtualClears = virtualClears; Residual = residual; CapReached = capReached; }
    }

    // Pure analytical evaluator. Reward generation and idempotent application are separate transactions.
    public static class OfflineCalculator
    {
        public static readonly ContentId MaxSecondsKey = new ContentId("offline.max_seconds");
        public static readonly ContentId EfficiencyKey = new ContentId("offline.base_efficiency");
        public static readonly ContentId MinEncounterSecondsKey = new ContentId("offline.min_encounter_seconds");

        public static OfflineCalculation Calculate(TimeSpan absence, decimal previousResidual, decimal efficiencyModifier, BalanceCatalog balance)
        {
            if (balance == null) throw new ArgumentNullException(nameof(balance));
            if (previousResidual < 0 || previousResidual >= 1) throw new ArgumentOutOfRangeException(nameof(previousResidual));
            if (efficiencyModifier < 0) throw new ArgumentOutOfRangeException(nameof(efficiencyModifier));
            decimal cap = Read(balance, MaxSecondsKey, BalanceValueKind.DurationSeconds);
            decimal baselineEfficiency = Read(balance, EfficiencyKey, BalanceValueKind.Probability);
            decimal minimum = Read(balance, MinEncounterSecondsKey, BalanceValueKind.DurationSeconds);
            if (cap <= 0 || minimum <= 0) throw new InvalidOperationException("Offline cap and minimum encounter time must be positive.");
            decimal seconds = absence.Ticks <= 0 ? 0 : absence.Ticks / (decimal)TimeSpan.TicksPerSecond;
            decimal eligible = Math.Min(seconds, cap);
            decimal efficiency = Math.Min(1m, baselineEfficiency * efficiencyModifier);
            decimal progress = previousResidual + eligible * efficiency / minimum;
            long clears = checked((long)decimal.Floor(progress));
            return new OfflineCalculation(eligible, clears, progress - clears, seconds > cap);
        }

        public static ContentId ChooseTarget(RunState run, ContentCatalog catalog)
        {
            if (run == null || catalog == null) throw new ArgumentNullException(run == null ? nameof(run) : nameof(catalog));
            if (Eligible(run.OfflineTargetStageId, run, catalog)) return run.OfflineTargetStageId;
            if (!run.AutoPush && Eligible(run.SelectedStageId, run, catalog)) return run.SelectedStageId;
            for (int i = run.CompletedStageOrder.Count - 1; i >= 0; i--)
                if (Eligible(run.CompletedStageOrder[i], run, catalog)) return run.CompletedStageOrder[i];
            return default;
        }

        private static bool Eligible(ContentId id, RunState run, ContentCatalog catalog) =>
            !string.IsNullOrEmpty(id.Value) && run.CompletedStages.Contains(id) &&
            catalog.Stages.TryGetValue(id, out var stage) && !stage.IsBossStage;

        private static decimal Read(BalanceCatalog balance, ContentId id, BalanceValueKind kind) =>
            decimal.Parse(balance.Require(id, kind).Number.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
