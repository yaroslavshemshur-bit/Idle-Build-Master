using System;

namespace IBM.Domain
{
    public interface IRandomStream
    {
        uint NextUInt32();
        uint NextBounded(uint exclusiveUpperBound);
        double NextUnitDouble();
    }

    // PCG-XSH-RR 32, 64-bit LCG. Unchecked wrap is part of RNG algorithm version 1.
    public sealed class Pcg32 : IRandomStream
    {
        public const int AlgorithmVersion = 1;
        public ulong State { get; private set; }
        public ulong Increment { get; }

        public Pcg32(ulong seed, ulong sequence)
        {
            Increment = unchecked((sequence << 1) | 1UL);
            NextUInt32();
            State = unchecked(State + seed);
            NextUInt32();
        }

        private Pcg32(ulong state, ulong increment, bool restored)
        {
            if ((increment & 1) == 0) throw new ArgumentException("PCG increment must be odd.", nameof(increment));
            State = state;
            Increment = increment;
        }

        public static Pcg32 Restore(ulong state, ulong increment) => new Pcg32(state, increment, true);

        public uint NextUInt32()
        {
            ulong old = State;
            State = unchecked(old * 6364136223846793005UL + Increment);
            uint shifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (shifted >> rotation) | (shifted << ((-rotation) & 31));
        }

        public uint NextBounded(uint exclusiveUpperBound)
        {
            if (exclusiveUpperBound == 0) throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
            uint threshold = unchecked((uint)(0u - exclusiveUpperBound)) % exclusiveUpperBound;
            while (true)
            {
                uint roll = NextUInt32();
                if (roll >= threshold) return roll % exclusiveUpperBound;
            }
        }

        public double NextUnitDouble() => NextUInt32() * (1d / 4294967296d);
    }

    public static class RandomSeeds
    {
        public const int DerivationVersion = 1;
        // FNV-1a 64 over UTF-8 identifier bytes and little-endian root seed.
        public static ulong Derive(ulong rootSeed, string streamId)
        {
            if (string.IsNullOrEmpty(streamId)) throw new ArgumentException("Stream ID is required.", nameof(streamId));
            ulong hash = 14695981039346656037UL;
            for (int i = 0; i < 8; i++) hash = unchecked((hash ^ (byte)(rootSeed >> (i * 8))) * 1099511628211UL);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(streamId);
            foreach (byte value in bytes) hash = unchecked((hash ^ value) * 1099511628211UL);
            return hash;
        }
    }
}
