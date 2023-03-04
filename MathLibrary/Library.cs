

// ReSharper disable once CheckNamespace
namespace Library;

using System.Diagnostics;
using VirtualMachine.VmRuntime;

public static class Library
{
    public static void Sqrt(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Sqrt(a));
    }

    private static decimal ReadDecimal(VmRuntime vmRuntime)
    {
        decimal a = (decimal)(vmRuntime.Memory.Pop() ?? throw new InvalidOperationException());
        return a;
    }

    public static void Pow(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        decimal b = (decimal)(vmRuntime.Memory.Pop() ?? throw new InvalidOperationException());
        vmRuntime.Memory.Push(NeilMath.Pow(a, b));
    }

    public static void Abs(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Abs(a));
    }

    public static void Cos(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Cos(a));
    }

    public static void Sin(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Sin(a));
    }

    public static void Tan(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Tan(a));
    }

    public static void Exp(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Exp(a));
    }

    public static void Truncate(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Truncate(a));
    }

    public static void Round(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(decimal.Round(a));
    }

    public static void Ceil(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(decimal.Ceiling(a));
    }

    public static void Sign(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push((decimal)NeilMath.Sign(a));
    }

    public static void Floor(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Floor(a));
    }

    public static void Acos(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Acos(a));
    }

    public static void Asin(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Asin(a));
    }

    public static void Atan(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Atan(a));
    }

    public static void Log10(VmRuntime vmRuntime)
    {
        decimal a = ReadDecimal(vmRuntime);
        vmRuntime.Memory.Push(NeilMath.Log10(a));
    }

    // <copyright file="MathM.cs" company="Neil McNeight">
    // Copyright © 2015-2019 Nathan P Jones.
    // Copyright © 2019 Neil McNeight. All rights reserved.
    // Licensed under the MIT license. See LICENSE file in the project root for
    // full license information.
    // </copyright>

    public static class NeilMath
    {
        public const decimal E = 2.7182818284590452353602874714m;

        public const decimal Pi = 3.1415926535897932384626433833m;

        public const decimal Tau = 6.2831853071795864769252867666m;

        private const decimal PiHalf = 1.5707963267948966192313216916m;

        private const decimal PiQuarter = 0.7853981633974483096156608458m;

        private const decimal Ln10 = 2.3025850929940456840179914547m;

        private const decimal SmallestNonZeroDec = 0.0000000000000000000000000001m;

        // This table is required for the Round function which can specify the number of digits to round to
        private static readonly decimal[] _roundPower10Decimal =
        {
            1E0m, 1E1m, 1E2m, 1E3m, 1E4m, 1E5m, 1E6m, 1E7m, 1E8m, 1E9m,
            1E10m, 1E11m, 1E12m, 1E13m, 1E14m, 1E15m, 1E16m, 1E17m, 1E18m, 1E19m,
            1E20m, 1E21m, 1E22m, 1E23m, 1E24m, 1E25m, 1E26m, 1E27m, 1E28m
        };


        public static decimal Abs(decimal m)
        {
            if (m < 0m) m = -m;
            return m;
        }


        public static decimal Acos(decimal m)
        {
            return m switch
            {
                < -1m or > 1m => throw new ArgumentOutOfRangeException(nameof(m)),
                -1m => Pi,
                0m => PiHalf,
                1m => 0m,
                _ => 2m * Atan(Sqrt(1m - m * m) / (1m + m))
            };
        }


        public static decimal Asin(decimal m)
        {
            return m switch
            {
                < -1m or > 1m => throw new ArgumentOutOfRangeException(nameof(m)),
                -1m => -PiHalf,
                0m => 0m,
                1m => PiHalf,
                _ => 2m * Atan(m / (1m + Sqrt(1m - m * m)))
            };
        }


        public static decimal Atan(decimal m)
        {
            switch (m)
            {
                // Special cases
                case -1:
                    return -PiQuarter;
                case 0:
                    return 0;
                case 1:
                    return PiQuarter;
                // Force down to -1 to 1 interval for faster convergence
                case < -1:
                    return -PiHalf - Atan(1 / m);
                // Force down to -1 to 1 interval for faster convergence
                case > 1:
                    return PiHalf - Atan(1 / m);
            }

            decimal result = 0m;
            int doubleIteration = 0; // current iteration * 2
            decimal y = m * m / (1 + m * m);
            decimal nextAdd = 0m;

            while (true)
            {
                if (doubleIteration == 0)
                    nextAdd = m / (1 + m * m); // is = y / x  but this is better for very small numbers where y = 9
                else
                    // We multiply by -1 each time so that the sign of the component
                    // changes each time. The first item is positive and it
                    // alternates back and forth after that.
                    // Following is equivalent to: nextAdd *= y * (iteration * 2) / (iteration * 2 + 1);
                    nextAdd *= y * doubleIteration / (doubleIteration + 1);

                if (nextAdd == 0) break;

                result += nextAdd;

                doubleIteration += 2;
            }

            return result;
        } // ReSharper disable UnusedMember.Local
        public static decimal Atan2(decimal y, decimal x)
        {
            switch (x)
            {
                case 0 when y == 0:
                    return 0; // X0, Y0
                case 0:
                    return y > 0
                        ? PiHalf // X0, Y+
                        : -PiHalf; // X0, Y-
            }

            if (y == 0)
                return x > 0
                    ? 0 // X+, Y0
                    : Pi; // X-, Y0

            decimal aTan = Atan(y / x);

            if (x > 0) return aTan; // Q1&4: X+, Y+-

            return y > 0
                ? aTan + Pi // Q2: X-, Y+
                : aTan - Pi; //   Q3: X-, Y-
        }


        public static decimal Ceiling(decimal m)
        {
            return decimal.Ceiling(m);
        }


        public static decimal Cos(decimal m)
        {
            // Normalize to between -2Pi <= m <= 2Pi
            m = Remainder(m, Tau);

            switch (m)
            {
                case 0 or Tau:
                    return 1m;
                case Pi:
                    return -1m;
                case PiHalf or Pi + PiHalf:
                    return 0m;
            }

            decimal result = 0m;
            int doubleIteration = 0; // current iteration * 2
            decimal xSquared = m * m;
            decimal nextAdd = 0m;

            while (true)
            {
                if (doubleIteration == 0)
                    nextAdd = 1m;
                else
                    // We multiply by -1 each time so that the sign of the component
                    // changes each time. The first item is positive and it
                    // alternates back and forth after that.
                    // Following is equivalent to: nextAdd *= -1 * x * x / ((2 * iteration - 1) * (2 * iteration));
                    nextAdd *= -1 * xSquared / (doubleIteration * doubleIteration - doubleIteration);

                if (nextAdd == 0m) break;

                result += nextAdd;

                doubleIteration += 2;
            }

            return result;
        }

        public static decimal Exp(decimal m)
        {
            decimal result;

            bool reciprocal = m < 0;
            m = Abs(m);

            decimal t = Truncate(m);

            switch (m)
            {
                case 0:
                    result = 1;
                    break;
                case 1:
                    result = E;
                    break;
                default:
                {
                    if (Abs(m) > 1 && t != m)
                    {
                        // Split up into integer and fractional
                        result = Exp(t) * Exp(m - t);
                    }
                    else if (m == t)
                    {
                        // Integer power
                        result = ExpBySquaring(E, m);
                    }
                    else
                    {
                        // Fractional power < 1
                        // See http://mathworld.wolfram.com/ExponentialFunction.html
                        int iteration = 0;
                        decimal nextAdd = 0;
                        result = 0;

                        while (true)
                        {
                            if (iteration == 0)
                                nextAdd = 1; // == Pow(d, 0) / Factorial(0) == 1 / 1 == 1
                            else
                                nextAdd *= m / iteration; // == Pow(d, iteration) / Factorial(iteration)

                            if (nextAdd == 0) break;

                            result += nextAdd;

                            iteration += 1;
                        }
                    }

                    break;
                }
            }

            // Take reciprocal if this was a negative power
            // Note that result will never be zero at this point.
            if (reciprocal) result = 1 / result;

            return result;
        }


        public static decimal Floor(decimal m)
        {
            return decimal.Floor(m);
        }


        public static decimal Log(decimal m)
        {
            switch (m)
            {
                case < 0:
                    throw new ArgumentException("Natural logarithm is a complex number for values less than zero!",
                        nameof(m));
                case 0:
                    throw new OverflowException(
                        "Natural logarithm is defined as negative infinity at zero which the Decimal data type can't represent!");
                case 1:
                    return 0;
                case >= 1:
                {
                    decimal power = 0m;

                    decimal x = m;
                    while (x > 1)
                    {
                        x /= 10;
                        power += 1;
                    }

                    return Log(x) + power * Ln10;
                }
            }

            // See http://en.wikipedia.org/wiki/Natural_logarithm#Numerical_value
            // for more information on this faster-converging series.

            decimal iteration = 0;
            decimal exponent = 0m;
            decimal result = 0m;

            decimal y = (m - 1) / (m + 1);
            decimal ySquared = y * y;

            while (true)
            {
                if (iteration == 0)
                    exponent = 2 * y;
                else
                    exponent *= ySquared;

                decimal nextAdd = exponent / (2 * iteration + 1);

                if (nextAdd == 0) break;

                result += nextAdd;

                iteration += 1;
            }

            return result;
        }


        public static decimal Log(decimal m, decimal newBase)
        {
            // Substituting null for double.NaN
            // if (!m.HasValue)
            // {
            //    return m;
            // }

            // Substituting null for double.NaN
            // if (!newBase.HasValue)
            // {
            //    return newBase;
            // }

            // Substituting null for double.NaN
            if (newBase == 1)
            {
                // throw new InvalidOperationException("Logarithm for base 1 is undefined.");
                // return null;
            }

            // if ((a != 1) && ((newBase == 0) || double.IsPositiveInfinity(newBase)))
            if (m != 1 && newBase == 0)
            {
                // return null;
            }

            switch (m)
            {
                case < 0:
                    throw new ArgumentException("Logarithm is a complex number for values less than zero!", nameof(m));
                case 0:
                    throw new OverflowException(
                        "Logarithm is defined as negative infinity at zero which the Decimal data type can't represent!");
            }

            switch (newBase)
            {
                case < 0:
                    throw new ArgumentException("Logarithm base would be a complex number for values less than zero!",
                        nameof(newBase));
                case 0:
                    throw new OverflowException(
                        "Logarithm base would be negative infinity at zero which the Decimal data type can't represent!");
            }

            // Short circuit the checks below if m is 1 because
            // that will yield 0 in the numerator below and give us
            // 0 for any base, even ones that would yield infinity.
            if (m == 1) return 0m;

            return Log(m) / Log(newBase);
        }


        public static decimal Log10(decimal m)
        {
            // Substituting null for double.NaN
            // if (!m.HasValue)
            // {
            //    return null;
            // }

            return m switch
            {
                < 0 => throw new ArgumentException("Logarithm is a complex number for values less than zero!",
                    nameof(m)),
                0 => throw new OverflowException(
                    "Logarithm is defined as negative infinity at zero which the Decimal data type can't represent!"),
                _ => Log(m) / Ln10
            };
        }


        public static decimal Max(decimal m1, decimal m2)
        {
            // return decimal.Max(ref val1, ref val2);
            //
            // Since decimal.Max() is inaccessable, perform the comparison
            // the old fashioned way.
            return m1 >= m2 ? m1 : m2;
        }


        public static decimal Min(decimal m1, decimal m2)
        {
            // return decimal.Min(ref val1, ref val2);
            //
            // Since decimal.Min() is inaccessable, perform the comparison
            // the old fashioned way.
            return m1 <= m2 ? m1 : m2;
        }


        public static decimal Pow(decimal x, decimal y)
        {
            decimal result;
            bool isNegativeExponent = false;

            // Handle negative exponents
            if (y < 0)
            {
                isNegativeExponent = true;
                y = Abs(y);
            }

            switch (y)
            {
                case 0:
                    result = 1;
                    break;
                case 1:
                    result = x;
                    break;
                default:
                {
                    decimal t = decimal.Truncate(y);

                    if (y == t)
                        // Integer powers
                        result = ExpBySquaring(x, y);
                    else
                        // Fractional power < 1
                        // See http://en.wikipedia.org/wiki/Exponent#Real_powers
                        // The next line is an optimization of Exp(y * Log(x)) for better precision
                        result = ExpBySquaring(x, t) * Exp((y - t) * Log(x));
                    break;
                }
            }

            if (isNegativeExponent)
            {
                // Note, for IEEE floats this would be Infinity and not an exception...
                if (result == 0) throw new OverflowException("Negative power of 0 is undefined!");

                result = 1 / result;
            }

            return result;
        }


        public static int Sign(decimal m)
        {
            // return decimal.Sign(ref value);
            //
            // Since decimal.Sign() is inaccessable, perform the comparison
            // the old fashioned way.
            return m switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => 0
            };
        }


        public static decimal Sin(decimal m)
        {
            // Normalize to between -2Pi <= m <= 2Pi
            m = Remainder(m, Tau);

            if (m is 0 or Pi or Tau) return 0;

            if (m == PiHalf) return 1;

            if (m == Pi + PiHalf) return -1;

            decimal result = 0m;
            int doubleIteration = 0; // current iteration * 2
            decimal mSquared = m * m;
            decimal nextAdd = 0m;

            while (true)
            {
                if (doubleIteration == 0)
                    nextAdd = m;
                else
                    // We multiply by -1 each time so that the sign of the component
                    // changes each time. The first item is positive and it
                    // alternates back and forth after that.
                    // Following is equivalent to: nextAdd *= -1 * m * m / ((2 * iteration) * (2 * iteration + 1));
                    nextAdd *= -1 * mSquared / (doubleIteration * doubleIteration + doubleIteration);

                // Debug.WriteLine("{0:000}:{1,33:+0.0000000000000000000000000000;-0.0000000000000000000000000000} ->{2,33:+0.0000000000000000000000000000;-0.0000000000000000000000000000}",
                //    doubleIteration / 2, nextAdd, result + nextAdd);
                if (nextAdd == 0) break;

                result += nextAdd;

                doubleIteration += 2;
            }

            return result;
        }


        public static decimal Sqrt(decimal m)
        {
            if (m < 0m)
                throw new ArgumentException("Square root not defined for Decimal data type when less than zero!",
                    nameof(m));

            // Prevent divide-by-zero errors below. Dividing either
            // of the numbers below will yield a recurring 0 value
            // for halfS eventually converging on zero.
            if (m is 0m or SmallestNonZeroDec) return 0m;

            decimal halfS = m / 2m;
            decimal lastX = -1m;
            decimal nextX;

            // Begin with an estimate for the square root.
            // Use hardware to get us there quickly.
            decimal x = (decimal)Math.Sqrt(decimal.ToDouble(m));

            while (true)
            {
                nextX = x / 2m + halfS / x;

                // The next check effectively sees if we've ran out of
                // precision for our data type.
                if (nextX == x || nextX == lastX) break;

                lastX = x;
                x = nextX;
            }

            return nextX;
        }


        public static decimal Tan(decimal m)
        {
            try
            {
                return Sin(m) / Cos(m);
            }
            catch (DivideByZeroException)
            {
                throw new Exception("Tangent is undefined at this angle!");
            }
        }


        public static decimal Truncate(decimal m)
        {
            return decimal.Truncate(m);
        }


        private static decimal ExpBySquaring(decimal x, decimal y)
        {
            Debug.Assert(y >= 0 && decimal.Truncate(y) == y, "Only non-negative, integer powers supported.");
            if (y < 0) throw new ArgumentOutOfRangeException(nameof(y), "Negative exponents not supported!");

            if (decimal.Truncate(y) != y) throw new ArgumentException("Exponent must be an integer!", nameof(y));

            decimal result = 1m;
            decimal multiplier = x;

            while (y > 0)
            {
                if (y % 2 == 1)
                {
                    result *= multiplier;
                    y -= 1;
                    if (y == 0) break;
                }

                multiplier *= multiplier;
                y /= 2;
            }

            return result;
        }


        private static int GetDecimalPlaces(decimal m, bool countTrailingZeros)
        {
            const int signMask = unchecked((int)0x80000000);
            const int scaleMask = 0x00FF0000;
            const int scaleShift = 16;

            int[] bits = decimal.GetBits(m);
            int result = (bits[3] & scaleMask) >> scaleShift; // extract exponent

            // Return immediately for values without a fractional portion or if we're counting trailing zeros
            if (countTrailingZeros || result == 0) return result;

            // Get a raw version of the decimal's integer
            bits[3] = bits[3] & ~(signMask | scaleMask); // clear out exponent and negative bit
            decimal rawValue = new(bits);

            // Account for trailing zeros
            while (result > 0 && rawValue % 10 == 0)
            {
                result--;
                rawValue /= 10;
            }

            return result;
        }


        private static decimal Remainder(decimal m1, decimal m2)
        {
            if (Abs(m1) < Abs(m2)) return m1;

            decimal timesInto = Truncate(m1 / m2);
            decimal shifting = m2;
            int sign = Sign(m1);

            for (int i = 0; i <= GetDecimalPlaces(m2, true); i++)
            {
                // Note that first "digit" will be the integer portion of d2
                decimal digit = Truncate(shifting);

                m1 -= timesInto * (digit / _roundPower10Decimal[i]);

                shifting = (shifting - digit) * 10m; // remove used digit and shift for next iteration
                if (shifting == 0m) break;
            }

            // If we've crossed zero because of the precision mismatch,
            // we need to add a whole d2 to get a correct result.
            if (m1 != 0 && Sign(m1) != sign)
                m1 = Sign(m2) == sign
                    ? m1 + m2
                    : m1 - m2;

            return m1;
        }
    }
}