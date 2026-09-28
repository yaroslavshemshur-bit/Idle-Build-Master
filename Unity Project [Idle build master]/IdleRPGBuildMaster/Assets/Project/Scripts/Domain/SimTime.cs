using System;

namespace IBM.Domain
{
    public readonly struct SimDuration : IEquatable<SimDuration>
    {
        public long Microseconds { get; }
        public SimDuration(long microseconds)
        {
            if (microseconds < 0) throw new ArgumentOutOfRangeException(nameof(microseconds));
            Microseconds = microseconds;
        }
        public static SimDuration FromSeconds(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0 || seconds > long.MaxValue / 1000000d)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            return new SimDuration(checked((long)Math.Round(seconds * 1000000d, MidpointRounding.AwayFromZero)));
        }
        public bool Equals(SimDuration other) => Microseconds == other.Microseconds;
        public override bool Equals(object obj) => obj is SimDuration other && Equals(other);
        public override int GetHashCode() => Microseconds.GetHashCode();
    }

    // Absolute simulation time; UTC belongs to a separate application port.
    public readonly struct SimTime : IEquatable<SimTime>, IComparable<SimTime>
    {
        public long Microseconds { get; }
        public SimTime(long microseconds) => Microseconds = microseconds;
        public SimTime Add(SimDuration duration) => new SimTime(checked(Microseconds + duration.Microseconds));
        public int CompareTo(SimTime other) => Microseconds.CompareTo(other.Microseconds);
        public bool Equals(SimTime other) => Microseconds == other.Microseconds;
        public override bool Equals(object obj) => obj is SimTime other && Equals(other);
        public override int GetHashCode() => Microseconds.GetHashCode();
    }

    // Preserve fractional microseconds when integrating frame deltas.
    public sealed class RealTimeResidue
    {
        private double _microsecondRemainder;
        public long ConsumeSeconds(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            double total = seconds * 1000000d + _microsecondRemainder;
            if (total >= long.MaxValue) throw new OverflowException("Elapsed simulation time overflow.");
            long whole = (long)Math.Floor(total);
            _microsecondRemainder = total - whole;
            return whole;
        }
    }
}
