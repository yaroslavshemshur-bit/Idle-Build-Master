using System;

namespace IBM.Domain
{
    public readonly struct PrimaryAttributes
    {
        public GameNumber Strength { get; }
        public GameNumber Vitality { get; }
        public GameNumber Agility { get; }
        public GameNumber Dexterity { get; }
        public PrimaryAttributes(GameNumber strength, GameNumber vitality, GameNumber agility, GameNumber dexterity)
        {
            Strength = strength; Vitality = vitality; Agility = agility; Dexterity = dexterity;
        }
    }

    public readonly struct DerivedStats
    {
        public GameNumber MinDamage { get; }
        public GameNumber MaxDamage { get; }
        public GameNumber MaxHp { get; }
        public GameNumber Block { get; }
        public GameNumber Accuracy { get; }
        public GameNumber Evasion { get; }
        public GameNumber RegenPerSecond { get; }
        public double AttacksPerSecond { get; }
        public DerivedStats(GameNumber minDamage, GameNumber maxDamage, GameNumber maxHp, GameNumber block,
            GameNumber accuracy, GameNumber evasion, GameNumber regenPerSecond, double attacksPerSecond)
        {
            MinDamage = minDamage; MaxDamage = maxDamage; MaxHp = maxHp; Block = block;
            Accuracy = accuracy; Evasion = evasion; RegenPerSecond = regenPerSecond; AttacksPerSecond = attacksPerSecond;
        }
    }

    // Only the approved baseline formulas. Power conversions and temporary layers require D02.
    public static class CombatMath
    {
        public static DerivedStats Baseline(PrimaryAttributes primary, BalanceCatalog balance)
        {
            if (balance == null) throw new ArgumentNullException(nameof(balance));
            var minDamage = N(balance, "combat.min_damage.base").Add(primary.Dexterity.Multiply(N(balance, "combat.min_damage.dex_factor")));
            var maxDamage = minDamage.Add(primary.Strength.Multiply(N(balance, "combat.max_damage.str_factor")));
            var maxHp = N(balance, "combat.max_hp.base").Add(primary.Vitality.Multiply(N(balance, "combat.max_hp.vit_factor")));
            var block = primary.Strength.Multiply(N(balance, "combat.block.str_factor"))
                .Add(primary.Vitality.Multiply(N(balance, "combat.block.vit_factor")));
            var accuracy = N(balance, "combat.accuracy.base").Add(primary.Dexterity);
            var evasion = N(balance, "combat.evasion.base").Add(primary.Agility);
            var rating = N(balance, "combat.attack_speed.base_rating")
                .Add(primary.Agility.Multiply(N(balance, "combat.attack_speed.agi_factor")));
            var scaledRating = rating.Multiply(N(balance, "combat.attack_speed.rating_scale"));
            if (scaledRating.CompareTo(GameNumber.Zero) <= 0) throw new InvalidOperationException("Attack speed rating must be positive.");
            double rate = scaledRating.Log2();
            if (double.IsNaN(rate) || double.IsInfinity(rate) || rate <= 0)
                throw new InvalidOperationException("Attack speed cannot be represented by the current rule.");
            var regen = N(balance, "combat.regen.base").Add(primary.Vitality.Multiply(N(balance, "combat.regen.vit_factor")));
            return new DerivedStats(minDamage, maxDamage, maxHp, block, accuracy, evasion, regen, rate);
        }

        public static SimDuration AttackInterval(DerivedStats stats)
        {
            double microseconds = 1000000d / stats.AttacksPerSecond;
            if (double.IsNaN(microseconds) || double.IsInfinity(microseconds) || microseconds < 1d)
                throw new NotSupportedException("Attack interval below one simulation microsecond requires an exact aggregation resolver.");
            return SimDuration.FromSeconds(1d / stats.AttacksPerSecond);
        }

        public static double HitChance(DerivedStats attacker, DerivedStats target, BalanceCatalog balance)
        {
            if (target.Evasion.CompareTo(GameNumber.Zero) <= 0) throw new InvalidOperationException("Target Evasion must be positive.");
            double raw = attacker.Accuracy.Divide(target.Evasion).ToBoundedDouble();
            double low = N(balance, "combat.hit.min").ToBoundedDouble();
            double high = N(balance, "combat.hit.max").ToBoundedDouble();
            if (low < 0 || high > 1 || low > high) throw new InvalidOperationException("Invalid authored hit bounds.");
            return Math.Max(low, Math.Min(high, raw));
        }

        public static GameNumber RollDamage(DerivedStats attacker, IRandomStream random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            uint roll = random.NextUInt32();
            var fraction = GameNumber.FromInt64(roll).Divide(GameNumber.FromInt64(4294967296L));
            return attacker.MinDamage.Add(attacker.MaxDamage.Subtract(attacker.MinDamage).Multiply(fraction));
        }

        public static GameNumber ApplyBlock(GameNumber damage, GameNumber effectiveBlock, BalanceCatalog balance)
        {
            if (damage.CompareTo(GameNumber.Zero) < 0 || effectiveBlock.CompareTo(GameNumber.Zero) < 0)
                throw new ArgumentOutOfRangeException(nameof(damage));
            var reference = N(balance, "combat.block.reference");
            if (reference.CompareTo(GameNumber.Zero) <= 0) throw new InvalidOperationException("Block reference must be positive.");
            return damage.Multiply(reference.Divide(effectiveBlock.Add(reference)));
        }

        private static GameNumber N(BalanceCatalog balance, string id) =>
            balance.Require(new ContentId(id), BalanceValueKind.Number).Number;
    }
}
