using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IBM.Domain
{
    public enum BalanceValueKind
    {
        Number,
        Integer,
        Ratio,
        Probability,
        DurationSeconds
    }

    public readonly struct BalanceValue
    {
        public BalanceValueKind Kind { get; }
        public GameNumber Number { get; }

        public BalanceValue(BalanceValueKind kind, GameNumber number)
        {
            if (!Enum.IsDefined(typeof(BalanceValueKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            if (kind == BalanceValueKind.Probability && (number.CompareTo(GameNumber.Zero) < 0 || number.CompareTo(GameNumber.One) > 0))
                throw new ArgumentOutOfRangeException(nameof(number), "Probability must be between zero and one.");
            if (kind == BalanceValueKind.DurationSeconds && number.CompareTo(GameNumber.Zero) < 0)
                throw new ArgumentOutOfRangeException(nameof(number), "Duration must be nonnegative.");
            if (kind == BalanceValueKind.Integer && !IsInteger(number))
                throw new ArgumentException("Integer balance parameter must have an integral value.", nameof(number));
            Kind = kind;
            Number = number;
        }

        private static bool IsInteger(GameNumber number)
        {
            if (number.IsZero || number.Exponent >= 0) return true;
            if (number.Exponent < -GameNumber.Precision) return false;
            return number.Coefficient % System.Numerics.BigInteger.Pow(10, checked((int)-number.Exponent)) == 0;
        }
    }

    // Immutable catalog snapshot. Session code receives this, never the authoring ScriptableObject.
    public sealed class BalanceCatalog
    {
        private readonly Dictionary<ContentId, BalanceValue> _values;
        public string Revision { get; }
        public string MechanicalHash { get; }
        public int Count => _values.Count;

        public BalanceCatalog(string revision, IEnumerable<KeyValuePair<ContentId, BalanceValue>> values)
        {
            if (string.IsNullOrWhiteSpace(revision)) throw new ArgumentException("A balance revision is required.", nameof(revision));
            if (values == null) throw new ArgumentNullException(nameof(values));
            Revision = revision;
            _values = new Dictionary<ContentId, BalanceValue>();
            foreach (var pair in values)
            {
                if (string.IsNullOrEmpty(pair.Key.Value)) throw new ArgumentException("Balance key cannot be empty.", nameof(values));
                if (!_values.TryAdd(pair.Key, pair.Value)) throw new ArgumentException("Duplicate balance key: " + pair.Key, nameof(values));
            }
            var ordered = new List<ContentId>(_values.Keys);
            ordered.Sort();
            var canonical = new StringBuilder();
            foreach (var key in ordered)
            {
                var value = _values[key];
                canonical.Append(key.Value.Length).Append(':').Append(key.Value).Append('|')
                    .Append((int)value.Kind).Append('|')
                    .Append(value.Number.Coefficient).Append('|')
                    .Append(value.Number.Exponent).Append('\n');
            }
            using (var sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
                MechanicalHash = BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant();
            }
        }

        public BalanceValue Require(ContentId id, BalanceValueKind expectedKind)
        {
            if (!_values.TryGetValue(id, out var value)) throw new KeyNotFoundException("Missing balance key: " + id);
            if (value.Kind != expectedKind) throw new InvalidOperationException("Wrong balance kind for " + id + ": expected " + expectedKind + ", found " + value.Kind);
            return value;
        }
    }
}
