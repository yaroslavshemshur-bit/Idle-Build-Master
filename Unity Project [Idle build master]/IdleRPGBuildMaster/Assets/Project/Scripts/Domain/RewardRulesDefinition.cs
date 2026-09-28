using System;
using System.Collections.Generic;

namespace IBM.Domain
{
    public sealed class RewardRollDefinition
    {
        public ContentId LootTableId { get; }
        public double Chance { get; }
        public int RollCount { get; }
        public int ItemLevel { get; }
        public bool Procedural { get; }
        public IReadOnlyList<double> RarityWeights { get; }

        public RewardRollDefinition(ContentId lootTableId, double chance, int rollCount,
            int itemLevel, bool procedural, IReadOnlyList<double> rarityWeights = null)
        {
            if (string.IsNullOrEmpty(lootTableId.Value) || double.IsNaN(chance) ||
                double.IsInfinity(chance) || chance < 0 || chance > 1 || rollCount <= 0 || itemLevel <= 0)
                throw new ArgumentException("Invalid reward roll definition.");
            LootTableId = lootTableId; Chance = chance; RollCount = rollCount;
            ItemLevel = itemLevel; Procedural = procedural;
            var weights = new List<double>(rarityWeights ?? Array.Empty<double>());
            if (procedural && weights.Count != 6)
                throw new ArgumentException("Procedural roll needs six rarity weights.");
            if (!procedural && weights.Count != 0)
                throw new ArgumentException("Named roll cannot carry rarity weights.");
            double total = 0;
            foreach (var weight in weights)
            {
                if (double.IsNaN(weight) || double.IsInfinity(weight) || weight < 0)
                    throw new ArgumentException("Invalid rarity weight.");
                total += weight;
            }
            if (procedural && (total <= 0 || double.IsInfinity(total)))
                throw new ArgumentException("Rarity weights have no finite total.");
            RarityWeights = weights.AsReadOnly();
        }
    }

    public sealed class EncounterRewardProfileDefinition
    {
        public ContentId EncounterId { get; }
        public IReadOnlyList<RewardRollDefinition> Rolls { get; }
        public EncounterRewardProfileDefinition(ContentId encounterId, IReadOnlyList<RewardRollDefinition> rolls)
        {
            if (string.IsNullOrEmpty(encounterId.Value)) throw new ArgumentException("Reward profile needs an encounter ID.");
            EncounterId = encounterId;
            Rolls = LocationDefinition.Copy(rolls ?? Array.Empty<RewardRollDefinition>());
        }
    }

    public sealed class PowerExpModifierDefinition
    {
        public ContentId PowerId { get; }
        public GameNumber Multiplier { get; }
        public PowerExpModifierDefinition(ContentId powerId, GameNumber multiplier)
        {
            if (string.IsNullOrEmpty(powerId.Value) || multiplier.CompareTo(GameNumber.Zero) <= 0)
                throw new ArgumentException("Power EXP multiplier must be positive.");
            PowerId = powerId; Multiplier = multiplier;
        }
    }
}
