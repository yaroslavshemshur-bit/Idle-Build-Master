using System;

namespace IBM.Domain
{
    public sealed class AffixDefinition
    {
        public ContentId Id { get; }
        public CombatStat Target { get; }
        public GameNumber MagnitudePerPower { get; }
        public bool Primary { get; }
        public AffixDefinition(ContentId id, CombatStat target, GameNumber magnitudePerPower, bool primary)
        {
            if (string.IsNullOrEmpty(id.Value) || !Enum.IsDefined(typeof(CombatStat), target) ||
                magnitudePerPower.CompareTo(GameNumber.Zero) <= 0)
                throw new ArgumentException("Invalid affix definition.");
            if (primary && target != CombatStat.Strength && target != CombatStat.Vitality &&
                target != CombatStat.Agility && target != CombatStat.Dexterity)
                throw new ArgumentException("First-affix primary pool contains a non-primary stat.");
            Id = id; Target = target; MagnitudePerPower = magnitudePerPower; Primary = primary;
        }
    }

    public enum AffixRepeatPolicy { Unspecified, WithoutReplacement, WithReplacement }
}
