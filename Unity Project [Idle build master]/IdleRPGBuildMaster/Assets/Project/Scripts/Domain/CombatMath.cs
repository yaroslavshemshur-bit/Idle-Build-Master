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

    public readonly struct DamageResolution
    {
        public GameNumber ResolvedDamage { get; }
        public GameNumber ProposedHp { get; }
        public GameNumber FinalHp { get; }
        public GameNumber ActualHpLost { get; }
        public bool FatalPreventionApplied { get; }
        public DamageResolution(GameNumber resolvedDamage, GameNumber proposedHp, GameNumber finalHp,
            GameNumber actualHpLost, bool fatalPreventionApplied)
        {
            ResolvedDamage = resolvedDamage; ProposedHp = proposedHp; FinalHp = finalHp;
            ActualHpLost = actualHpLost; FatalPreventionApplied = fatalPreventionApplied;
        }
    }

    // Only the approved baseline formulas. Power conversions and temporary layers require D02.
    public static class CombatMath
    {
        public static DerivedStats Baseline(PrimaryAttributes primary, BalanceCatalog balance)
        {
            return new CombatStatResolver(primary, balance, null).ResolveDerived();
        }

        public static SimDuration AttackInterval(DerivedStats stats)
        {
            if (stats.AttacksPerSecond <= 0d) throw new InvalidOperationException("Actor cannot attack at zero attack speed.");
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

        public static GameNumber RebaseCurrentHp(GameNumber currentHp, GameNumber oldMaxHp, GameNumber newMaxHp)
        {
            if (oldMaxHp.CompareTo(GameNumber.Zero) <= 0 || newMaxHp.CompareTo(GameNumber.Zero) < 0 ||
                currentHp.CompareTo(GameNumber.Zero) < 0 || currentHp.CompareTo(oldMaxHp) > 0)
                throw new ArgumentOutOfRangeException(nameof(currentHp));
            return GameNumber.Min(newMaxHp, GameNumber.Max(GameNumber.Zero,
                currentHp.Divide(oldMaxHp).Multiply(newMaxHp)));
        }

        // Resolve mitigation before HP clamping. The prevention callback sees the post-damage proposal
        // and may replace the final HP; no state is committed until this transaction returns.
        public static DamageResolution ResolveDamage(GameNumber outgoingDamage, GameNumber effectiveBlock,
            GameNumber currentHp, GameNumber maxHp, BalanceCatalog balance,
            Func<DamageResolution, GameNumber?> fatalPrevention = null)
        {
            if (currentHp.CompareTo(GameNumber.Zero) < 0 || currentHp.CompareTo(maxHp) > 0 ||
                maxHp.CompareTo(GameNumber.Zero) <= 0) throw new ArgumentOutOfRangeException(nameof(currentHp));
            var resolved = ApplyBlock(outgoingDamage, effectiveBlock, balance);
            var proposedHp = GameNumber.Max(GameNumber.Zero, currentHp.Subtract(resolved));
            var proposal = new DamageResolution(resolved, proposedHp, proposedHp,
                currentHp.Subtract(proposedHp), false);
            if (!proposedHp.IsZero || fatalPrevention == null) return proposal;
            var replacement = fatalPrevention(proposal);
            if (!replacement.HasValue) return proposal;
            if (replacement.Value.CompareTo(GameNumber.Zero) <= 0 || replacement.Value.CompareTo(maxHp) > 0)
                throw new InvalidOperationException("Fatal prevention must restore HP within (0, MaxHP].");
            return new DamageResolution(resolved, proposedHp, replacement.Value,
                GameNumber.Max(GameNumber.Zero, currentHp.Subtract(replacement.Value)), true);
        }

        public static GameNumber DeathRegenerationPerSecond(DerivedStats stats, BalanceCatalog balance) =>
            stats.RegenPerSecond.Multiply(N(balance, "combat.death_regen.multiplier"));

        private static GameNumber N(BalanceCatalog balance, string id) =>
            balance.Require(new ContentId(id), BalanceValueKind.Number).Number;
    }
}
