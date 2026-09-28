using System;
using IBM.Domain;

namespace IBM.Application
{
    public enum PrimaryStatKind { Strength, Vitality, Agility, Dexterity }

    public static class PrimaryStatProgression
    {
        public static ContentId Id(PrimaryStatKind stat)
        {
            switch (stat)
            {
                case PrimaryStatKind.Strength: return new ContentId("primary.strength");
                case PrimaryStatKind.Vitality: return new ContentId("primary.vitality");
                case PrimaryStatKind.Agility: return new ContentId("primary.agility");
                case PrimaryStatKind.Dexterity: return new ContentId("primary.dexterity");
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
        }

        public static bool IsKnownId(ContentId id) =>
            id == Id(PrimaryStatKind.Strength) || id == Id(PrimaryStatKind.Vitality) ||
            id == Id(PrimaryStatKind.Agility) || id == Id(PrimaryStatKind.Dexterity);

        public static PrimaryAttributes Resolve(RunState run, BalanceCatalog balance)
        {
            if (run == null || balance == null) throw new ArgumentNullException(run == null ? nameof(run) : nameof(balance));
            return new PrimaryAttributes(Value(run, balance, PrimaryStatKind.Strength),
                Value(run, balance, PrimaryStatKind.Vitality), Value(run, balance, PrimaryStatKind.Agility),
                Value(run, balance, PrimaryStatKind.Dexterity));
        }

        public static GameNumber Cost(PrimaryStatKind stat, long nextPurchaseNumber, BalanceCatalog balance)
        {
            if (balance == null) throw new ArgumentNullException(nameof(balance));
            if (nextPurchaseNumber <= 0) throw new ArgumentOutOfRangeException(nameof(nextPurchaseNumber));
            var scale = balance.Require(new ContentId("primary.upgrade.cost.scale." + Suffix(stat)),
                BalanceValueKind.Integer).Number;
            if (scale.CompareTo(GameNumber.Zero) <= 0)
                throw new InvalidOperationException("Stat purchase cost scale must be positive.");
            var powerValue = balance.Require(new ContentId("primary.upgrade.cost.power"),
                BalanceValueKind.Integer).Number;
            double bounded = powerValue.ToBoundedDouble();
            if (bounded < 1 || bounded > 16 || Math.Truncate(bounded) != bounded)
                throw new InvalidOperationException("Stat purchase cost power is outside the supported integer range.");
            int power = (int)bounded;
            var factor = GameNumber.FromInt64(nextPurchaseNumber);
            var result = scale;
            for (int i = 0; i < power; i++) result = result.Multiply(factor);
            return result;
        }

        private static GameNumber Value(RunState run, BalanceCatalog balance, PrimaryStatKind stat)
        {
            var id = Id(stat);
            run.PurchasedStats.TryGetValue(id, out long bought);
            if (bought < 0) throw new InvalidOperationException("Negative purchased primary-stat count.");
            var starting = balance.Require(new ContentId("primary.start." + Suffix(stat)),
                BalanceValueKind.Integer).Number;
            if (starting.CompareTo(GameNumber.Zero) <= 0)
                throw new InvalidOperationException("Starting primary stat must be positive.");
            return starting.Add(GameNumber.FromInt64(bought));
        }

        private static string Suffix(PrimaryStatKind stat)
        {
            switch (stat)
            {
                case PrimaryStatKind.Strength: return "strength";
                case PrimaryStatKind.Vitality: return "vitality";
                case PrimaryStatKind.Agility: return "agility";
                case PrimaryStatKind.Dexterity: return "dexterity";
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
        }
    }

    public interface IPrimaryStatTransitionPolicy
    {
        void Apply(GameState before, GameState after, PrimaryStatKind changedStat);
    }
}
