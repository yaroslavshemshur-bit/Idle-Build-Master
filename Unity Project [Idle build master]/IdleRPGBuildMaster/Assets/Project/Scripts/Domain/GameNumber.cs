using System;
using System.Globalization;
using System.Numerics;

namespace IBM.Domain
{
    /// <summary>Signed 34-digit decimal coefficient multiplied by 10^Exponent.</summary>
    public readonly struct GameNumber : IComparable<GameNumber>, IEquatable<GameNumber>
    {
        public const int Precision = 34;
        private const int AlignmentLimit = 70;
        public BigInteger Coefficient { get; }
        public long Exponent { get; }
        public bool IsZero => Coefficient.IsZero;
        public static GameNumber Zero => default;
        public static GameNumber One => FromInt64(1);

        private GameNumber(BigInteger coefficient, long exponent)
        {
            Coefficient = coefficient;
            Exponent = exponent;
        }

        public static GameNumber FromInt64(long value) => Create(new BigInteger(value), 0);
        public static GameNumber Parse(string coefficient, string exponent)
        {
            if (!BigInteger.TryParse(coefficient, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var c) ||
                !long.TryParse(exponent, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var e))
                throw new FormatException("GameNumber requires invariant integer coefficient and exponent strings.");
            return Create(c, e);
        }

        public static GameNumber Create(BigInteger coefficient, long exponent)
        {
            if (coefficient.IsZero) return Zero;
            int digits = Digits(coefficient);
            if (digits > Precision)
            {
                int remove = digits - Precision;
                BigInteger scale = Pow10(remove);
                BigInteger quotient = BigInteger.DivRem(coefficient, scale, out BigInteger remainder);
                BigInteger twice = BigInteger.Abs(remainder) * 2;
                if (twice > scale || (twice == scale && !quotient.IsEven))
                    quotient += coefficient.Sign;
                coefficient = quotient;
                exponent = checked(exponent + remove);
            }
            while (coefficient % 10 == 0)
            {
                coefficient /= 10;
                exponent = checked(exponent + 1);
            }
            _ = checked(exponent + Digits(coefficient) - 1);
            return new GameNumber(coefficient, exponent);
        }

        public GameNumber Add(GameNumber other)
        {
            if (IsZero) return other;
            if (other.IsZero) return this;
            long orderGap = Order.CompareTo(other.Order) >= 0 ? CheckedGap(Order, other.Order) : CheckedGap(other.Order, Order);
            if (orderGap > AlignmentLimit) return Order > other.Order ? this : other;
            long lower = Math.Min(Exponent, other.Exponent);
            int leftShift = checked((int)(Exponent - lower));
            int rightShift = checked((int)(other.Exponent - lower));
            return Create(Coefficient * Pow10(leftShift) + other.Coefficient * Pow10(rightShift), lower);
        }

        public GameNumber Subtract(GameNumber other) => Add(other.Negate());
        public GameNumber Negate() => new GameNumber(-Coefficient, Exponent);
        public GameNumber Multiply(GameNumber other) => Create(Coefficient * other.Coefficient, checked(Exponent + other.Exponent));

        public GameNumber Divide(GameNumber other)
        {
            if (other.IsZero) throw new DivideByZeroException();
            if (IsZero) return Zero;
            int scaleDigits = Precision + Digits(other.Coefficient) - Digits(Coefficient);
            BigInteger numerator = BigInteger.Abs(Coefficient) * Pow10(Math.Max(0, scaleDigits));
            BigInteger denominator = BigInteger.Abs(other.Coefficient) * Pow10(Math.Max(0, -scaleDigits));
            BigInteger quotient = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
            BigInteger twice = remainder * 2;
            if (twice > denominator || (twice == denominator && !quotient.IsEven)) quotient++;
            if (Coefficient.Sign != other.Coefficient.Sign) quotient = -quotient;
            return Create(quotient, checked(checked(Exponent - other.Exponent) - scaleDigits));
        }

        public int CompareTo(GameNumber other)
        {
            int sign = Coefficient.Sign.CompareTo(other.Coefficient.Sign);
            if (sign != 0 || IsZero) return sign;
            int order = Order.CompareTo(other.Order);
            if (order != 0) return Coefficient.Sign * order;
            int shift = checked((int)(Exponent - other.Exponent));
            return shift >= 0
                ? (Coefficient * Pow10(shift)).CompareTo(other.Coefficient)
                : Coefficient.CompareTo(other.Coefficient * Pow10(-shift));
        }

        public long Order => IsZero ? 0 : checked(Exponent + Digits(Coefficient) - 1);

        // Ratios are explicitly bounded; callers cannot accidentally overflow a full magnitude conversion.
        public double ToBoundedDouble()
        {
            if (IsZero) return 0;
            if (Order > 308 || Order < -324) throw new OverflowException("Value is outside bounded double range.");
            if (!double.TryParse(ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                throw new OverflowException("Value is outside bounded double range.");
            if (double.IsInfinity(result) || double.IsNaN(result) || (result == 0 && !IsZero))
                throw new OverflowException("Value is outside bounded double range.");
            return result;
        }

        public double Log2()
        {
            if (Coefficient.Sign <= 0) throw new ArgumentOutOfRangeException(nameof(Coefficient), "Logarithm requires a positive value.");
            int digits = Digits(Coefficient);
            double mantissa = (double)Coefficient / Math.Pow(10, digits - 1);
            return Math.Log(mantissa, 2) + Order * Math.Log(10, 2);
        }

        public static GameNumber Min(GameNumber a, GameNumber b) => a.CompareTo(b) <= 0 ? a : b;
        public static GameNumber Max(GameNumber a, GameNumber b) => a.CompareTo(b) >= 0 ? a : b;
        public bool Equals(GameNumber other) => Coefficient.Equals(other.Coefficient) && Exponent == other.Exponent;
        public override bool Equals(object obj) => obj is GameNumber other && Equals(other);
        public override int GetHashCode() => Coefficient.GetHashCode() ^ Exponent.GetHashCode();
        public override string ToString() => Coefficient.ToString(CultureInfo.InvariantCulture) + "e" + Exponent.ToString(CultureInfo.InvariantCulture);
        public static bool operator ==(GameNumber left, GameNumber right) => left.Equals(right);
        public static bool operator !=(GameNumber left, GameNumber right) => !left.Equals(right);

        private static int Digits(BigInteger value) => BigInteger.Abs(value).ToString(CultureInfo.InvariantCulture).Length;
        private static BigInteger Pow10(int power) => BigInteger.Pow(10, power);
        private static long CheckedGap(long high, long low)
        {
            if (high < low) throw new ArgumentException("Expected descending values.");
            if (low < 0 && high > long.MaxValue + low) return long.MaxValue;
            return high - low;
        }
    }
}
