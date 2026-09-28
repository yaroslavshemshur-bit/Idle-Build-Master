using System;

namespace IBM.Domain
{
    // A mechanical reference, never a display name or Unity asset GUID.
    public readonly struct ContentId : IEquatable<ContentId>, IComparable<ContentId>
    {
        public string Value { get; }

        public ContentId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value != value.Trim())
                throw new ArgumentException("Content ID must be nonempty and have no surrounding whitespace.", nameof(value));
            Value = value;
        }

        public bool Equals(ContentId other) => StringComparer.Ordinal.Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is ContentId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(ContentId other) => StringComparer.Ordinal.Compare(Value, other.Value);
        public override string ToString() => Value ?? "";
        public static bool operator ==(ContentId left, ContentId right) => left.Equals(right);
        public static bool operator !=(ContentId left, ContentId right) => !left.Equals(right);
    }

    // Marker types make a Power reference incompatible with an Enemy reference at API boundaries.
    public readonly struct PowerKind { }
    public readonly struct ItemKind { }
    public readonly struct EnemyKind { }
    public readonly struct LocationKind { }
    public readonly struct StageKind { }
    public readonly struct EffectKind { }
    public readonly struct AffixKind { }

    public readonly struct DefinitionId<TKind> : IEquatable<DefinitionId<TKind>>, IComparable<DefinitionId<TKind>>
    {
        public ContentId Content { get; }
        public DefinitionId(string value) => Content = new ContentId(value);
        public int CompareTo(DefinitionId<TKind> other) => Content.CompareTo(other.Content);
        public bool Equals(DefinitionId<TKind> other) => Content.Equals(other.Content);
        public override bool Equals(object obj) => obj is DefinitionId<TKind> other && Equals(other);
        public override int GetHashCode() => Content.GetHashCode();
        public override string ToString() => Content.ToString();
    }

    // Instance keys are allocated from a durable counter; definitions and instances cannot be confused.
    public readonly struct InstanceId : IEquatable<InstanceId>, IComparable<InstanceId>
    {
        public ulong Value { get; }
        public InstanceId(ulong value)
        {
            if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }
        public bool Equals(InstanceId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is InstanceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(InstanceId other) => Value.CompareTo(other.Value);
        public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public sealed class InstanceIdSequence
    {
        public ulong NextValue { get; private set; }
        public InstanceIdSequence(ulong nextValue = 1)
        {
            if (nextValue == 0) throw new ArgumentOutOfRangeException(nameof(nextValue));
            NextValue = nextValue;
        }
        public InstanceId Next()
        {
            if (NextValue == ulong.MaxValue) throw new OverflowException("Instance ID sequence exhausted.");
            return new InstanceId(NextValue++);
        }
    }
}
