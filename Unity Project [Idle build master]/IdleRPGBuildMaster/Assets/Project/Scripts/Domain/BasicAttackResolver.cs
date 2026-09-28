using System;

namespace IBM.Domain
{
    public readonly struct BasicAttackResult
    {
        public bool Hit { get; }
        public double HitChance { get; }
        public DamageResolution Damage { get; }
        public BasicAttackResult(bool hit, double hitChance, DamageResolution damage)
        { Hit = hit; HitChance = hitChance; Damage = damage; }
    }

    // One ordinary direct attack. Target selection, stat snapshots, Crit and triggered effects
    // are explicit caller responsibilities so they cannot silently alter this RNG order.
    public static class BasicAttackResolver
    {
        public static BasicAttackResult Resolve(DerivedStats attacker, DerivedStats target, GameNumber targetHp,
            GameNumber effectiveBlock, IRandomStream random, BalanceCatalog balance,
            Func<DamageResolution, GameNumber?> fatalPrevention = null)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            double chance = CombatMath.HitChance(attacker, target, balance);
            if (random.NextUnitDouble() >= chance)
                return new BasicAttackResult(false, chance, default);
            var damage = CombatMath.ResolveDamage(CombatMath.RollDamage(attacker, random), effectiveBlock,
                targetHp, target.MaxHp, balance, fatalPrevention);
            return new BasicAttackResult(true, chance, damage);
        }
    }
}
