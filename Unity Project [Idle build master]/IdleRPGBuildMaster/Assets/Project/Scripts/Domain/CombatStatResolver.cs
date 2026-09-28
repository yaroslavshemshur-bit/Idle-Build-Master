using System;
using System.Collections.Generic;

namespace IBM.Domain
{
    public enum CombatStat
    {
        Strength, Vitality, Agility, Dexterity,
        MinDamage, MaxDamage, MaxHp, Block, Accuracy, Evasion,
        AttackSpeedRating, RegenPerSecond
    }

    public enum StatModifierKind { Flat, Multiplier, Conversion }

    public readonly struct StatModifier
    {
        public CombatStat Target { get; }
        public StatModifierKind Kind { get; }
        public GameNumber Amount { get; }
        public CombatStat Source { get; }

        public StatModifier(CombatStat target, StatModifierKind kind, GameNumber amount, CombatStat source = default)
        {
            if (!Enum.IsDefined(typeof(CombatStat), target) || !Enum.IsDefined(typeof(StatModifierKind), kind) ||
                !Enum.IsDefined(typeof(CombatStat), source)) throw new ArgumentOutOfRangeException();
            Target = target; Kind = kind; Amount = amount; Source = source;
        }
    }

    // Resolves source stats completely before adding conversions to a target's pre-multiplier value.
    // A cycle in either a baseline dependency or an authored conversion is rejected at the entry point.
    public sealed class CombatStatResolver
    {
        private readonly PrimaryAttributes _primary;
        private readonly BalanceCatalog _balance;
        private readonly List<StatModifier> _modifiers;
        private readonly Dictionary<CombatStat, GameNumber> _resolved = new Dictionary<CombatStat, GameNumber>();
        private readonly HashSet<CombatStat> _visiting = new HashSet<CombatStat>();

        public CombatStatResolver(PrimaryAttributes primary, BalanceCatalog balance, IEnumerable<StatModifier> modifiers)
        {
            _primary = primary;
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
            _modifiers = modifiers == null ? new List<StatModifier>() : new List<StatModifier>(modifiers);
        }

        public GameNumber Resolve(CombatStat stat)
        {
            if (!Enum.IsDefined(typeof(CombatStat), stat)) throw new ArgumentOutOfRangeException(nameof(stat));
            if (_resolved.TryGetValue(stat, out var value)) return value;
            if (!_visiting.Add(stat)) throw new InvalidOperationException("Combat stat dependency cycle at " + stat + ".");
            try
            {
                value = Baseline(stat);
                foreach (var modifier in _modifiers)
                    if (modifier.Target == stat && modifier.Kind == StatModifierKind.Flat)
                        value = value.Add(modifier.Amount);
                foreach (var modifier in _modifiers)
                    if (modifier.Target == stat && modifier.Kind == StatModifierKind.Conversion)
                        value = value.Add(Resolve(modifier.Source).Multiply(modifier.Amount));
                foreach (var modifier in _modifiers)
                    if (modifier.Target == stat && modifier.Kind == StatModifierKind.Multiplier)
                        value = value.Multiply(modifier.Amount);
                _resolved.Add(stat, value);
                return value;
            }
            finally { _visiting.Remove(stat); }
        }

        public DerivedStats ResolveDerived()
        {
            var rating = GameNumber.Max(GameNumber.Zero, Resolve(CombatStat.AttackSpeedRating));
            double rate = rating.IsZero ? 0d : N("combat.attack_speed.rating_scale").Multiply(rating).Log2();
            rate = Math.Max(0d, rate);
            if (double.IsNaN(rate) || double.IsInfinity(rate)) throw new InvalidOperationException("Attack speed cannot be represented.");
            return new DerivedStats(Resolve(CombatStat.MinDamage), Resolve(CombatStat.MaxDamage),
                Resolve(CombatStat.MaxHp), Resolve(CombatStat.Block), Resolve(CombatStat.Accuracy),
                Resolve(CombatStat.Evasion), Resolve(CombatStat.RegenPerSecond), rate);
        }

        private GameNumber Baseline(CombatStat stat)
        {
            switch (stat)
            {
                case CombatStat.Strength: return _primary.Strength;
                case CombatStat.Vitality: return _primary.Vitality;
                case CombatStat.Agility: return _primary.Agility;
                case CombatStat.Dexterity: return _primary.Dexterity;
                case CombatStat.MinDamage:
                    return N("combat.min_damage.base").Add(Resolve(CombatStat.Dexterity).Multiply(N("combat.min_damage.dex_factor")));
                case CombatStat.MaxDamage:
                    return Resolve(CombatStat.MinDamage).Add(Resolve(CombatStat.Strength).Multiply(N("combat.max_damage.str_factor")));
                case CombatStat.MaxHp:
                    return N("combat.max_hp.base").Add(Resolve(CombatStat.Vitality).Multiply(N("combat.max_hp.vit_factor")));
                case CombatStat.Block:
                    return Resolve(CombatStat.Strength).Multiply(N("combat.block.str_factor"))
                        .Add(Resolve(CombatStat.Vitality).Multiply(N("combat.block.vit_factor")));
                case CombatStat.Accuracy:
                    return N("combat.accuracy.base").Add(Resolve(CombatStat.Dexterity));
                case CombatStat.Evasion:
                    return N("combat.evasion.base").Add(Resolve(CombatStat.Agility));
                case CombatStat.AttackSpeedRating:
                    return N("combat.attack_speed.base_rating")
                        .Add(Resolve(CombatStat.Agility).Multiply(N("combat.attack_speed.agi_factor")));
                case CombatStat.RegenPerSecond:
                    return N("combat.regen.base").Add(Resolve(CombatStat.Vitality).Multiply(N("combat.regen.vit_factor")));
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
        }

        private GameNumber N(string key) => _balance.Require(new ContentId(key), BalanceValueKind.Number).Number;
    }
}
