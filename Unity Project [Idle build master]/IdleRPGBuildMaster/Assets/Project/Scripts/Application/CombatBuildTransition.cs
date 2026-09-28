using System;
using IBM.Domain;

namespace IBM.Application
{
    public interface IHeroStatSnapshotProvider
    {
        DerivedStats Resolve(GameState state);
    }

    public interface ICombatClockAdjustment
    {
        void Apply(GameState before, GameState after, DerivedStats previous, DerivedStats current);
    }

    // Shared transition for purchases and equipment. The caller commits the whole draft or none of it.
    public sealed class CombatBuildTransition : IPrimaryStatTransitionPolicy, IEquipmentTransitionPolicy
    {
        private readonly IHeroStatSnapshotProvider _stats;
        private readonly ICombatClockAdjustment _clocks;

        public CombatBuildTransition(IHeroStatSnapshotProvider stats, ICombatClockAdjustment clocks)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _clocks = clocks ?? throw new ArgumentNullException(nameof(clocks));
        }

        public void Apply(GameState before, GameState after, PrimaryStatKind changedStat) => Apply(before, after);
        public void Apply(GameState before, GameState after, string changedSlot) => Apply(before, after);

        private void Apply(GameState before, GameState after)
        {
            if (before == null || after == null || !before.Combat.Active || !after.Combat.Active ||
                before.Combat.Time.CompareTo(after.Combat.Time) != 0)
                throw new InvalidOperationException("Build transition requires one active combat safe point.");
            ActorState hero = null;
            foreach (var actor in after.Combat.Actors)
                if (actor.IsHero)
                {
                    if (hero != null) throw new InvalidOperationException("Combat has more than one hero.");
                    hero = actor;
                }
            if (hero == null) throw new InvalidOperationException("Combat has no hero.");
            var previous = _stats.Resolve(before);
            var current = _stats.Resolve(after);
            if (hero.MaxHp.CompareTo(previous.MaxHp) != 0 || current.MaxHp.CompareTo(GameNumber.Zero) <= 0)
                throw new InvalidOperationException("Build stat provider disagrees with active hero MaxHP.");
            hero.Hp = CombatMath.RebaseCurrentHp(hero.Hp, hero.MaxHp, current.MaxHp);
            hero.MaxHp = current.MaxHp;
            hero.RegenAnchorAt = after.Combat.Time;
            hero.RegenAnchorHp = hero.Hp;
            _clocks.Apply(before, after, previous, current);
        }
    }

    // Valid only before Powers, gear modifiers and temporary effects are active.
    public sealed class BaselineHeroStatSnapshotProvider : IHeroStatSnapshotProvider
    {
        private readonly BalanceCatalog _balance;
        public BaselineHeroStatSnapshotProvider(BalanceCatalog balance) =>
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));

        public DerivedStats Resolve(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Run.OwnedPowers.Count != 0 || state.Run.EquippedItems.Count != 0 ||
                state.Combat.Effects.Count != 0)
                throw new NotSupportedException("A full stat snapshot provider is required for Power, gear or timed modifiers.");
            return CombatMath.Baseline(PrimaryStatProgression.Resolve(state.Run, _balance), _balance);
        }
    }
}
