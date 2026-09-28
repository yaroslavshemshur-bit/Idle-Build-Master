using System;

namespace IBM.Application
{
    // Separate versions prevent a balance edit from masquerading as a save-schema change.
    public readonly struct VersionStamp : IEquatable<VersionStamp>
    {
        public int Schema { get; }
        public int Rules { get; }
        public int Numeric { get; }
        public int Rng { get; }
        public string Content { get; }

        public VersionStamp(int schema, int rules, int numeric, int rng, string content)
        {
            if (schema <= 0 || rules <= 0 || numeric <= 0 || rng <= 0 || string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Every version component is required.");
            Schema = schema;
            Rules = rules;
            Numeric = numeric;
            Rng = rng;
            Content = content;
        }
        public bool Equals(VersionStamp other) => Schema == other.Schema && Rules == other.Rules && Numeric == other.Numeric && Rng == other.Rng && StringComparer.Ordinal.Equals(Content, other.Content);
        public override bool Equals(object obj) => obj is VersionStamp other && Equals(other);
        public override int GetHashCode() => Schema ^ (Rules << 4) ^ (Numeric << 8) ^ (Rng << 12) ^ StringComparer.Ordinal.GetHashCode(Content ?? "");
    }
}
