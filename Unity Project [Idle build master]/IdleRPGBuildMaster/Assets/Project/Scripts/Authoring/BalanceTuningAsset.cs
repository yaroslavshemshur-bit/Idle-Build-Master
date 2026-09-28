using System;
using System.Collections.Generic;
using IBM.Domain;
using UnityEngine;

namespace IBM.Authoring
{
    [Serializable]
    public sealed class BalanceParameter
    {
        [Tooltip("Stable mechanical key. Never use a display label as a key.")]
        public string id;
        public BalanceValueKind kind;
        [Tooltip("Invariant signed decimal integer, up to 34 significant digits after normalization.")]
        public string coefficient = "0";
        [Tooltip("Signed base-10 exponent, stored as text to preserve the full 64-bit range.")]
        public string exponent = "0";
    }

    [CreateAssetMenu(fileName = "BalanceTuning", menuName = "Idle Build Master/Tuning/Balance")]
    public sealed class BalanceTuningAsset : ScriptableObject
    {
        [SerializeField] private string revision = "balance-v1";
        [SerializeField] private List<BalanceParameter> parameters = new List<BalanceParameter>();

        public string Revision => revision;
        public IReadOnlyList<BalanceParameter> Parameters => parameters;

        // Conversion happens once at catalog activation, outside the simulation hot path.
        public BalanceCatalog Compile()
        {
            var pairs = new List<KeyValuePair<ContentId, BalanceValue>>(parameters.Count);
            for (int i = 0; i < parameters.Count; i++)
            {
                var entry = parameters[i];
                if (entry == null) throw new InvalidOperationException(name + ": parameter [" + i + "] is null.");
                try
                {
                    var id = new ContentId(entry.id);
                    var number = GameNumber.Parse(entry.coefficient, entry.exponent);
                    pairs.Add(new KeyValuePair<ContentId, BalanceValue>(id, new BalanceValue(entry.kind, number)));
                }
                catch (Exception error) when (error is ArgumentException || error is FormatException || error is OverflowException)
                {
                    throw new InvalidOperationException(name + ": invalid balance parameter [" + i + "] (" + entry.id + ").", error);
                }
            }
            return new BalanceCatalog(revision, pairs);
        }
    }
}
