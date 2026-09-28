using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    public sealed class ProceduralGearGenerator : IProceduralItemGenerator
    {
        private readonly ContentCatalog _catalog;
        private readonly BalanceCatalog _balance;
        private readonly List<ContentId> _primary = new List<ContentId>();
        private readonly List<ContentId> _all = new List<ContentId>();

        public ProceduralGearGenerator(ContentCatalog catalog, BalanceCatalog balance)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
            if (catalog.AffixRepeatPolicy == AffixRepeatPolicy.Unspecified)
                throw new InvalidOperationException("Affix repetition policy is not authored.");
            foreach (var affix in catalog.Affixes.Values)
            {
                _all.Add(affix.Id);
                if (affix.Primary) _primary.Add(affix.Id);
            }
            _all.Sort(); _primary.Sort();
            if (_primary.Count == 0 || _all.Count < 6 &&
                catalog.AffixRepeatPolicy == AffixRepeatPolicy.WithoutReplacement)
                throw new InvalidOperationException("Procedural affix pools cannot serve all rarity tiers.");
        }

        public ItemInstanceState Generate(ContentId itemDefinitionId, RewardRollDefinition roll,
            IRandomStream lootRandom)
        {
            if (roll == null || !roll.Procedural || lootRandom == null ||
                !_catalog.Items.ContainsKey(itemDefinitionId))
                throw new ArgumentException("Invalid procedural gear roll.");
            int rarity = SelectRarity(roll.RarityWeights, lootRandom);
            var affixes = new ContentId[rarity];
            affixes[0] = _primary[(int)lootRandom.NextBounded((uint)_primary.Count)];
            var pool = new List<ContentId>(_all);
            if (_catalog.AffixRepeatPolicy == AffixRepeatPolicy.WithoutReplacement) pool.Remove(affixes[0]);
            for (int i = 1; i < rarity; i++)
            {
                if (pool.Count == 0) throw new InvalidOperationException("Affix pool is exhausted.");
                int index = (int)lootRandom.NextBounded((uint)pool.Count);
                affixes[i] = pool[index];
                if (_catalog.AffixRepeatPolicy == AffixRepeatPolicy.WithoutReplacement) pool.RemoveAt(index);
            }
            return new ItemInstanceState { DefinitionId = itemDefinitionId, ItemLevel = roll.ItemLevel,
                Procedural = true, Rarity = (ItemRarity)rarity, AffixIds = affixes };
        }

        public GameNumber AffixPower(int itemLevel)
        {
            if (itemLevel <= 0) throw new ArgumentOutOfRangeException(nameof(itemLevel));
            var divisorValue = _balance.Require(new ContentId("gear.affix.level_divisor"),
                BalanceValueKind.Integer).Number;
            var minimum = _balance.Require(new ContentId("gear.affix.min_power"),
                BalanceValueKind.Integer).Number;
            double divisor = divisorValue.ToBoundedDouble();
            if (divisor < 1 || divisor > int.MaxValue || Math.Truncate(divisor) != divisor ||
                minimum.CompareTo(GameNumber.Zero) <= 0)
                throw new InvalidOperationException("Invalid authored affix-power curve.");
            long rounded = (long)Math.Round(itemLevel / divisor, MidpointRounding.AwayFromZero);
            return GameNumber.Max(minimum, GameNumber.FromInt64(rounded));
        }

        public StatModifier ResolveAffix(ContentId affixId, int itemLevel)
        {
            if (!_catalog.Affixes.TryGetValue(affixId, out var affix))
                throw new InvalidOperationException("Unknown affix: " + affixId);
            return new StatModifier(affix.Target, StatModifierKind.Flat,
                affix.MagnitudePerPower.Multiply(AffixPower(itemLevel)));
        }

        private static int SelectRarity(IReadOnlyList<double> weights, IRandomStream random)
        {
            double total = 0;
            foreach (var weight in weights) total += weight;
            double draw = random.NextUnitDouble() * total;
            for (int i = 0; i < weights.Count; i++)
            {
                draw -= weights[i];
                if (draw < 0) return i + 1;
            }
            return weights.Count;
        }
    }
}
