using System;
using System.Collections.Generic;
using UnityEngine;

namespace IBM.Authoring
{
    public enum InterfaceMeasureKind
    {
        Pixels,
        Scale,
        Seconds,
        Percent,
        Count
    }

    [Serializable]
    public sealed class InterfaceMeasure
    {
        [Tooltip("Stable UI key, for example combat.hp_bar.width.")]
        public string id;
        public InterfaceMeasureKind kind;
        public float value;
    }

    [Serializable]
    public sealed class InterfaceColor
    {
        public string id;
        public Color value;
    }

    [CreateAssetMenu(fileName = "InterfaceTuning", menuName = "Idle Build Master/Tuning/Interface")]
    public sealed class InterfaceTuningAsset : ScriptableObject
    {
        [SerializeField] private List<InterfaceMeasure> measures = new List<InterfaceMeasure>();
        [SerializeField] private List<InterfaceColor> colors = new List<InterfaceColor>();

        public float RequireMeasure(string id, InterfaceMeasureKind expectedKind)
        {
            Validate();
            foreach (var entry in measures)
                if (StringComparer.Ordinal.Equals(entry.id, id))
                {
                    if (entry.kind != expectedKind) throw new InvalidOperationException("Wrong UI measure kind: " + id);
                    return entry.value;
                }
            throw new KeyNotFoundException("Missing UI measure: " + id);
        }

        public Color RequireColor(string id)
        {
            Validate();
            foreach (var entry in colors)
                if (StringComparer.Ordinal.Equals(entry.id, id)) return entry.value;
            throw new KeyNotFoundException("Missing UI color: " + id);
        }

        public void Validate()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in measures)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.id != entry.id.Trim() || !ids.Add(entry.id))
                    throw new InvalidOperationException(name + ": missing or duplicate UI measure ID.");
                if (!Enum.IsDefined(typeof(InterfaceMeasureKind), entry.kind) || float.IsNaN(entry.value) || float.IsInfinity(entry.value) || entry.value < 0f)
                    throw new InvalidOperationException(name + ": invalid UI measure: " + entry.id);
                if (entry.kind == InterfaceMeasureKind.Percent && entry.value > 1f)
                    throw new InvalidOperationException(name + ": UI percent outside [0,1]: " + entry.id);
                if (entry.kind == InterfaceMeasureKind.Count && entry.value != Mathf.Floor(entry.value))
                    throw new InvalidOperationException(name + ": UI count must be integral: " + entry.id);
            }
            foreach (var entry in colors)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.id != entry.id.Trim() || !ids.Add(entry.id))
                    throw new InvalidOperationException(name + ": missing or duplicate UI color ID.");
                if (!IsFinite(entry.value.r) || !IsFinite(entry.value.g) || !IsFinite(entry.value.b) || !IsFinite(entry.value.a))
                    throw new InvalidOperationException(name + ": invalid UI color: " + entry.id);
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
