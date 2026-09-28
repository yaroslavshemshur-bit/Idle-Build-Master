using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    public interface ICombatAttackModifierPolicy
    {
        ulong ReplaceTarget(ActorState attacker, ulong selectedTargetId, IRandomStream random);
        GameNumber ModifyBlock(ActorState attacker, ActorState target,
            GameNumber effectiveBlock, IRandomStream random);
    }

    // Executes the authored single-source attack replacement and block-bypass operations.
    // Multiple effects of one kind need an explicit composition rule before activation.
    public sealed class CatalogAttackModifierPolicy : ICombatAttackModifierPolicy
    {
        private readonly EffectDefinition _redirect;
        private readonly EffectDefinition _ignoreBlock;

        public CatalogAttackModifierPolicy(ContentCatalog catalog, RunState run)
        {
            if (catalog == null || run == null) throw new ArgumentNullException("Catalog and run are required.");
            var ids = new List<ContentId>(run.OwnedPowers);
            ids.Sort();
            foreach (var id in ids)
            {
                if (!catalog.Powers.TryGetValue(id, out var power))
                    throw new InvalidOperationException("Unknown owned Power: " + id);
                foreach (var effectId in power.EffectIds)
                {
                    var effect = catalog.Effects[effectId];
                    if (effect.Operation == EffectOperation.RedirectAttack)
                    {
                        if (effect.Trigger != EffectTrigger.OnIncomingAttack || _redirect != null)
                            throw new NotSupportedException("Incoming-attack replacement needs one authored effect.");
                        _redirect = effect;
                    }
                    if (effect.Operation == EffectOperation.IgnoreBlock)
                    {
                        if (effect.Trigger != EffectTrigger.OnAttack || _ignoreBlock != null ||
                            effect.Magnitude.CompareTo(GameNumber.Zero) < 0 ||
                            effect.Magnitude.CompareTo(GameNumber.One) > 0)
                            throw new NotSupportedException("Block bypass needs one authored OnAttack ratio effect.");
                        _ignoreBlock = effect;
                    }
                }
            }
        }

        public ulong ReplaceTarget(ActorState attacker, ulong selectedTargetId, IRandomStream random)
        {
            if (attacker == null || random == null) throw new ArgumentNullException();
            return !attacker.IsHero && _redirect != null && Proc(_redirect, random)
                ? attacker.InstanceId : selectedTargetId;
        }

        public GameNumber ModifyBlock(ActorState attacker, ActorState target,
            GameNumber effectiveBlock, IRandomStream random)
        {
            if (attacker == null || target == null || random == null) throw new ArgumentNullException();
            if (!attacker.IsHero || target.IsHero || _ignoreBlock == null || !Proc(_ignoreBlock, random))
                return effectiveBlock;
            return effectiveBlock.Multiply(GameNumber.One.Subtract(_ignoreBlock.Magnitude));
        }

        private static bool Proc(EffectDefinition effect, IRandomStream random) =>
            effect.Probability >= 1d || effect.Probability > 0d &&
            random.NextUnitDouble() < effect.Probability;
    }
}
