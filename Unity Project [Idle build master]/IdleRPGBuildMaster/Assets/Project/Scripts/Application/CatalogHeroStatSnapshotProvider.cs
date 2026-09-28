using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    // Compiles passive Power, equipped-item and procedural-affix modifiers into the shared stat graph.
    // Triggered and timed effects require the action/effect handlers and are never silently treated as passive.
    public sealed class CatalogHeroStatSnapshotProvider : IHeroStatSnapshotProvider
    {
        private readonly ContentCatalog _catalog;
        private readonly BalanceCatalog _balance;

        public CatalogHeroStatSnapshotProvider(ContentCatalog catalog, BalanceCatalog balance)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
        }

        public DerivedStats Resolve(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var modifiers = new List<StatModifier>();
            var powers = new List<ContentId>(state.Run.OwnedPowers);
            powers.Sort();
            foreach (var powerId in powers)
            {
                if (!_catalog.Powers.TryGetValue(powerId, out var power))
                    throw new InvalidOperationException("Unknown owned Power: " + powerId);
                AddPassive(power.EffectIds, modifiers);
            }
            var itemIds = new List<ulong>(state.Run.EquippedItems.Values);
            itemIds.Sort();
            ProceduralGearGenerator gear = null;
            foreach (var id in itemIds)
            {
                ItemInstanceState item = null;
                foreach (var candidate in state.Run.Inventory)
                    if (candidate.InstanceId == id) { item = candidate; break; }
                if (item == null || !_catalog.Items.TryGetValue(item.DefinitionId, out var definition))
                    throw new InvalidOperationException("Equipped item is absent from inventory or catalog.");
                AddPassive(definition.EffectIds, modifiers);
                if (item.Procedural)
                {
                    gear = gear ?? new ProceduralGearGenerator(_catalog, _balance);
                    foreach (var affixId in item.AffixIds)
                        modifiers.Add(gear.ResolveAffix(affixId, item.ItemLevel));
                }
            }
            foreach (var active in state.Combat.Effects)
                if (IsHeroOwned(state.Combat, active.OwnerActorId))
                {
                    var effect = _catalog.Effects[active.DefinitionId];
                    if (effect.Operation != EffectOperation.ModifyStat || active.Stacks <= 0)
                        throw new NotSupportedException("Timed hero effect has no stat evaluator: " + effect.Id);
                    for (int i = 0; i < active.Stacks; i++)
                        modifiers.Add(new StatModifier(effect.StatTarget, effect.StatKind,
                            effect.Magnitude, effect.SourceStat));
                }
            return new CombatStatResolver(PrimaryStatProgression.Resolve(state.Run, _balance),
                _balance, modifiers).ResolveDerived();
        }

        private void AddPassive(IReadOnlyList<ContentId> effectIds, List<StatModifier> modifiers)
        {
            foreach (var id in effectIds)
            {
                var effect = _catalog.Effects[id];
                if (effect.Trigger != EffectTrigger.Passive) continue;
                if (effect.Operation != EffectOperation.ModifyStat || effect.Probability != 1d)
                    throw new NotSupportedException("Unsupported passive effect operation: " + id);
                modifiers.Add(new StatModifier(effect.StatTarget, effect.StatKind,
                    effect.Magnitude, effect.SourceStat));
            }
        }

        private static bool IsHeroOwned(CombatState combat, ulong actorId)
        {
            foreach (var actor in combat.Actors)
                if (actor.InstanceId == actorId) return actor.IsHero;
            throw new InvalidOperationException("Effect owner is absent from combat.");
        }
    }
}
