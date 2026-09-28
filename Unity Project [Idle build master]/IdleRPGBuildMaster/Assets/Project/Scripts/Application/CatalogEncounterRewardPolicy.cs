using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    public interface IProceduralItemGenerator
    {
        ItemInstanceState Generate(ContentId itemDefinitionId, RewardRollDefinition roll,
            IRandomStream lootRandom);
    }

    // Chance groups are independent and ordered as authored. Zero/one chance consumes no chance roll;
    // a one-entry table consumes no selection roll. Every other roll uses only the Loot stream.
    public sealed class CatalogEncounterRewardPolicy : IEncounterRewardPolicy
    {
        private readonly ContentCatalog _catalog;
        private readonly IProceduralItemGenerator _procedural;

        public CatalogEncounterRewardPolicy(ContentCatalog catalog, IProceduralItemGenerator procedural = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _procedural = procedural;
        }

        public EncounterRewardPlan Resolve(EncounterDefinition encounter, GameState snapshot, IRandomStream lootRandom)
        {
            if (encounter == null || snapshot == null || lootRandom == null)
                throw new ArgumentNullException("Reward encounter, state and RNG are required.");
            if (!_catalog.Encounters.ContainsKey(encounter.Id))
                throw new InvalidOperationException("Reward encounter is absent from the active catalog.");
            var exp = encounter.ExpBudget;
            var owned = new List<ContentId>(snapshot.Run.OwnedPowers);
            owned.Sort();
            foreach (var power in owned)
                if (_catalog.PowerExpModifiers.TryGetValue(power, out var modifier))
                    exp = exp.Multiply(modifier.Multiplier);
            var items = new List<ItemInstanceState>();
            if (!_catalog.RewardProfiles.TryGetValue(encounter.Id, out var profile))
            {
                if (!string.IsNullOrEmpty(encounter.LootTableId.Value))
                    throw new InvalidOperationException("Encounter loot has no authored reward profile.");
                return new EncounterRewardPlan(exp, items);
            }
            foreach (var group in profile.Rolls)
            {
                var table = _catalog.LootTables[group.LootTableId];
                for (int i = 0; i < group.RollCount; i++)
                {
                    if (group.Chance <= 0) continue;
                    if (group.Chance < 1 && lootRandom.NextUnitDouble() >= group.Chance) continue;
                    var itemId = Select(table, lootRandom);
                    ItemInstanceState item;
                    if (group.Procedural)
                    {
                        if (_procedural == null)
                            throw new NotSupportedException("Procedural reward group has no item generator.");
                        item = _procedural.Generate(itemId, group, lootRandom);
                        if (item == null || item.DefinitionId != itemId || item.ItemLevel != group.ItemLevel)
                            throw new InvalidOperationException("Procedural item generator returned a foreign item.");
                    }
                    else item = new ItemInstanceState { DefinitionId = itemId, ItemLevel = group.ItemLevel };
                    items.Add(item);
                }
            }
            return new EncounterRewardPlan(exp, items);
        }

        private static ContentId Select(LootTableDefinition table, IRandomStream random)
        {
            if (table.Entries.Count == 1) return table.Entries[0].ItemId;
            double sum = 0;
            foreach (var entry in table.Entries) sum += entry.Weight;
            if (double.IsInfinity(sum) || sum <= 0)
                throw new InvalidOperationException("Loot-table weight sum is invalid.");
            double draw = random.NextUnitDouble() * sum;
            foreach (var entry in table.Entries)
            {
                draw -= entry.Weight;
                if (draw < 0) return entry.ItemId;
            }
            return table.Entries[table.Entries.Count - 1].ItemId;
        }
    }
}
