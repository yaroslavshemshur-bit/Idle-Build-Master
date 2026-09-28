using System;
using System.Collections.Generic;
using System.Numerics;

namespace IBM.Domain
{
    public enum ProgressMilestoneKind { UnlockPower, PowerChoice, GearDropsBegin, BossAccess }

    public readonly struct ProgressMilestone
    {
        public ContentId Id { get; }
        public int Point { get; }
        public ProgressMilestoneKind Kind { get; }
        public ContentId RewardId { get; }
        public bool FirstRunOnly { get; }
        public IReadOnlyList<ContentId> FixedOfferOptions { get; }
        public ProgressMilestone(ContentId id, int point, ProgressMilestoneKind kind, ContentId rewardId, bool firstRunOnly,
            IReadOnlyList<ContentId> fixedOfferOptions = null)
        {
            if (string.IsNullOrEmpty(id.Value) || point <= 0 || !Enum.IsDefined(typeof(ProgressMilestoneKind), kind))
                throw new ArgumentException("Invalid progress milestone.");
            Id = id; Point = point; Kind = kind; RewardId = rewardId; FirstRunOnly = firstRunOnly;
            FixedOfferOptions = LocationDefinition.Copy(fixedOfferOptions ?? Array.Empty<ContentId>());
            if (FixedOfferOptions.Count > 0 && kind != ProgressMilestoneKind.PowerChoice)
                throw new ArgumentException("Only a Power-choice milestone can author fixed offer options.");
            var ids = new HashSet<ContentId>();
            foreach (var option in FixedOfferOptions)
                if (string.IsNullOrEmpty(option.Value) || !ids.Add(option))
                    throw new ArgumentException("Fixed Power offer contains an invalid or duplicate ID.");
        }
    }

    public static class LocationProgression
    {
        // Exact rational comparisons avoid moving milestones when Stage Compression changes R.
        // stageIndex is zero-based, completedEncounter is 1..requiredEncounters.
        public static ProgressMilestone[] Crossed(LocationDefinition location, int stageIndex,
            int requiredEncounters, int completedEncounter, bool firstRun)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            if (stageIndex < 0 || stageIndex >= location.ProgressStageCount || requiredEncounters <= 0 ||
                completedEncounter <= 0 || completedEncounter > requiredEncounters)
                throw new ArgumentOutOfRangeException(nameof(completedEncounter));
            var crossed = new List<ProgressMilestone>();
            BigInteger denominator = new BigInteger(location.ProgressStageCount) * requiredEncounters;
            BigInteger previous = new BigInteger(location.ProgressUnits) *
                (new BigInteger(stageIndex) * requiredEncounters + completedEncounter - 1);
            BigInteger current = new BigInteger(location.ProgressUnits) *
                (new BigInteger(stageIndex) * requiredEncounters + completedEncounter);
            foreach (var milestone in location.Milestones)
            {
                if (milestone.FirstRunOnly && !firstRun) continue;
                BigInteger threshold = new BigInteger(milestone.Point) * denominator;
                if (previous < threshold && current >= threshold) crossed.Add(milestone);
            }
            crossed.Sort((left, right) =>
            {
                int order = left.Point.CompareTo(right.Point);
                if (order != 0) return order;
                order = Priority(left.Kind).CompareTo(Priority(right.Kind));
                return order != 0 ? order : left.Id.CompareTo(right.Id);
            });
            return crossed.ToArray();
        }

        private static int Priority(ProgressMilestoneKind kind) => kind == ProgressMilestoneKind.UnlockPower ? 0 : 1;
    }
}
