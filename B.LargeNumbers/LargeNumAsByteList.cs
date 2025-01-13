using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace B.LargeNumbers;

public class LargeNumAsByteList : IComparable<LargeNumAsByteList>
{
    private static Dictionary<char, byte> _digitsByCharacter = new Dictionary<char, byte>
    {
        { '0', 0 },
        { '1', 1 },
        { '2', 2 },
        { '3', 3 },
        { '4', 4 },
        { '5', 5 },
        { '6', 6 },
        { '7', 7 },
        { '8', 8 },
        { '9', 9 },

        { 'a', 10 },
        { 'b', 11 },
        { 'c', 12 },
        { 'd', 13 },
        { 'e', 14 },
        { 'f', 15 },
        { 'g', 16 },
        { 'h', 17 },
        { 'i', 18 },
        { 'j', 19 },
        { 'k', 20 },
        { 'l', 21 },
        { 'm', 22 },
        { 'n', 23 },
        { 'o', 24 },
        { 'p', 25 },
        { 'q', 26 },
        { 'r', 27 },
        { 's', 28 },
        { 't', 29 },
        { 'u', 30 },
        { 'v', 31 },
        { 'w', 32 },
        { 'x', 33 },
        { 'y', 34 },
        { 'z', 35 },

        { 'A', 10 },
        { 'B', 11 },
        { 'C', 12 },
        { 'D', 13 },
        { 'E', 14 },
        { 'F', 15 },
        { 'G', 16 },
        { 'H', 17 },
        { 'I', 18 },
        { 'J', 19 },
        { 'K', 20 },
        { 'L', 21 },
        { 'M', 22 },
        { 'N', 23 },
        { 'O', 24 },
        { 'P', 25 },
        { 'Q', 26 },
        { 'R', 27 },
        { 'S', 28 },
        { 'T', 29 },
        { 'U', 30 },
        { 'V', 31 },
        { 'W', 32 },
        { 'X', 33 },
        { 'Y', 34 },
        { 'Z', 35 },
    };

    private static Dictionary<byte, char> _charactersByDigit = new Dictionary<byte, char>
    {
        { 0, '0' },
        { 1, '1' },
        { 2, '2' },
        { 3, '3' },
        { 4, '4' },
        { 5, '5' },
        { 6, '6' },
        { 7, '7' },
        { 8, '8' },
        { 9, '9' },
        { 10, 'A' },
        { 11, 'B' },
        { 12, 'C' },
        { 13, 'D' },
        { 14, 'E' },
        { 15, 'F' },
        { 16, 'G' },
        { 17, 'H' },
        { 18, 'I' },
        { 19, 'J' },
        { 20, 'K' },
        { 21, 'L' },
        { 22, 'M' },
        { 23, 'N' },
        { 24, 'O' },
        { 25, 'P' },
        { 26, 'Q' },
        { 27, 'R' },
        { 28, 'S' },
        { 29, 'T' },
        { 30, 'U' },
        { 31, 'V' },
        { 32, 'W' },
        { 33, 'X' },
        { 34, 'Y' },
        { 35, 'Z' },
    };

    /// <summary>
    /// Specifies the switch-over thresholds from using dotnet int64 to SimpleMultiply.
    /// Item i specifies the threshold value for numeric base i.
    /// </summary>
    public static List<int> SimpleMultiplyThresholdsByBase { get; set; } = new List<int>
    {
        0,  // Base 0 - N/A
        0,  // Base 1 - N/A
        63, // Base 2 (binary) - switch from using int64 to SimpleMultiply
        31, // Base 3
        31, // Base 4
        15,
        15,
        15,
        15, // Base 8
        18,
        18, // Base 10
        7,
        7,
        7,
        7,
        7,
        7, // Base 16
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5,
        5, // Base 32
        4,
        4,
        4,
    };

    /// <summary>
    /// Specifies the switch-over from SimpleMultiply to KaratsubaMultiply
    /// </summary>
    public static int KaratsubaMultiplyThreshold { get; set; } = 100;

    public byte Base { get; set; }

    /// <summary>
    /// Contains the digits of the number in a reverse order:
    /// ReverseDigits[0] is the least significant digit,
    /// ReverseDigits[Count-1] is the most significant digit
    /// </summary>
    public List<byte> ReverseDigits { get; set; }

    public bool IsNegative { get; set; }

    public int Length => ReverseDigits.Count;

    public bool IsZero { get => ReverseDigits.Count == 1 && ReverseDigits[0] == 0; }

    #region Ctors

    public LargeNumAsByteList(byte @base = 10, bool isNegative = false, int? expectedLength = null)
    {
        if (@base < 2)
            throw new ArgumentException("@base must be at least 2");

        if (@base > 36)
            throw new ArgumentException($"Bases over 36 are not (yet) supported)");

        Base = @base;
        ReverseDigits = expectedLength == null
            ? new List<byte>()
            : new List<byte>((int)expectedLength);
            
        IsNegative = false;
    }

    public LargeNumAsByteList(Int64 n, byte @base = 10, bool isNegative = false, int? expectedLength = null)
        : this(@base, isNegative, expectedLength)
    {
        if (n < 0)
        {
            IsNegative = true;
            n = -n;
        }

        while (n > 0)
        {
            ReverseDigits.Add((byte)(n % Base));
            n = n / @Base;
        }

        if (ReverseDigits.Count == 0)
            ReverseDigits.Append((byte)0);
    }

    public LargeNumAsByteList(string digits, byte @base = 10, bool isNegative = false, int? expectedLength = null)
        : this(@base, isNegative, expectedLength)
    {
        if (string.IsNullOrWhiteSpace(digits))
            throw new ArgumentException($"Source digits must have at least one digit");

        if (digits[0] == '-')
        {
            IsNegative = true;
            digits = digits.Substring(1);
        }

        digits = digits.TrimStart('0');

        if (digits.Length == 0 || digits == "0")
        {
            ReverseDigits.Append((byte)0);
            IsNegative = false;
            return;
        }

        for(int i = digits.Length - 1; i >= 0 ; i--)
        {
            char character = digits[i];
            if (!_digitsByCharacter.ContainsKey(character))
                throw new ArgumentException($"Character at position {i} ({character}) is not known");

            byte digit = _digitsByCharacter[character];
            if(digit >= Base)
                throw new ArgumentException($"Invalid digit at position {i} (character). Must be less then the base ({Base})");

            ReverseDigits.Add(digit);
        }
    }

    #endregion Ctors


    public override string ToString()
    {
        var sb = new StringBuilder();

        if (IsNegative)
            sb.Append('-');
        for(int i = Length - 1; i >= 0; i--)
            sb.Append(_charactersByDigit[ReverseDigits[i]]);

        return sb.ToString();
    }


    #region Conversions

    public static implicit operator LargeNumAsByteList(int i)
    {
        return new LargeNumAsByteList(i);
    }

    public static explicit operator int(LargeNumAsByteList largeNum)
    {
        int ret = 0;
        for (int i = largeNum.Length - 1; i >= 0 ; i--)
            ret = ret * largeNum.Base + largeNum.ReverseDigits[i];

        return ret;
    }

    public static explicit operator Int64(LargeNumAsByteList largeNum)
    {
        Int64 ret = 0;
        for (int i = largeNum.Length - 1; i >= 0; i--)
            ret = ret * largeNum.Base + largeNum.ReverseDigits[i];

        return ret;
    }

    #endregion Conversions


    #region Addition

    internal static (int Carry, byte Result) AddDigits(byte digit1, byte digit2, int previousCarry, byte @base)
    {
        int intResult = digit1 + digit2 + previousCarry;
        byte digit = (byte)(intResult % @base);
        int carry = intResult >= @base ? 1 : 0;
        return (carry, digit);
    }

    public static LargeNumAsByteList Add(LargeNumAsByteList x, LargeNumAsByteList y, int xRightShift = 0, int yRightShift = 0)
    {
        if (!x.IsNegative && !y.IsNegative)
            return AddPositives(x, y, xRightShift, yRightShift);
        else if (!x.IsNegative && y.IsNegative)
            return SubtractPositives(x.ShiftRight(xRightShift), -y.ShiftRight(yRightShift));
        else if (x.IsNegative && !y.IsNegative)
            return SubtractPositives(y.ShiftRight(yRightShift), -x.ShiftRight(xRightShift));
        else // both are negative
            return -AddPositives(-x, -y, xRightShift, yRightShift);
    }

    private static LargeNumAsByteList AddPositives(LargeNumAsByteList x, LargeNumAsByteList y, int xRightShift, int yRightShift)
    {
        if (x.IsNegative || y.IsNegative)
            throw new ArgumentException("Arguments must be non-negative");

        if (x.Base != y.Base)
            throw new ArgumentException("I can only work with numbers of the same base");

        x.RemoveLeadingZeros();
        y.RemoveLeadingZeros();

        int inputNumberLength = Math.Max(x.Length + xRightShift, y.Length + yRightShift);
        int xLengthWithShift = xRightShift + x.Length;
        int yLengthWithShift = yRightShift + y.Length;
        var retDigits = new List<byte>(Math.Max(xLengthWithShift, yLengthWithShift));
        int carry = 0;
        for (int i = 0; i < inputNumberLength; i++)
        {
            if (i < xRightShift)
            {
                if (i < yRightShift)
                {
                    retDigits.Append((byte)0);
                }
                else
                {
                    byte yDigit =
                        i < yLengthWithShift ? y.ReverseDigits[i - yRightShift]
                        : (byte)0;
                    retDigits.Append(yDigit);
                }
            }
            else
            {
                if (i < yRightShift)
                {
                    byte xDigit =
                        i < xLengthWithShift ? x.ReverseDigits[i - xRightShift]
                        : (byte)0;
                    retDigits.Append(xDigit);
                }
                else
                {
                    byte xDigit =
                        i < xRightShift ? (byte)0
                        : i < xLengthWithShift ? x.ReverseDigits[i - xRightShift]
                        : (byte)0;
                    byte yDigit =
                        i < yRightShift ? (byte)0
                        : i < yLengthWithShift ? y.ReverseDigits[i - yRightShift]
                        : (byte)0;
                    var digitAdditionResult = AddDigits(xDigit, yDigit, carry, x.Base);
                    retDigits.Append(digitAdditionResult.Result);
                    carry = digitAdditionResult.Carry;
                }
            }
        }
        if (carry != 0)
            retDigits.Append((byte)carry);

        return new LargeNumAsByteList { ReverseDigits = retDigits, Base = x.Base, IsNegative = false };
    }

    public static LargeNumAsByteList operator +(LargeNumAsByteList x, LargeNumAsByteList y) =>
        Add(x, y, 0, 0);


    #endregion Addition


    #region Subtraction

    internal static (int Carry, byte Result) SubtractDigits(byte digit1, byte digit2, int previousCarry, byte @base)
    {
        byte digit2WithCarry = (byte)(digit2 + (byte)previousCarry);

        return digit1 >= digit2WithCarry
            ? (0, (byte)(digit1 - digit2WithCarry))
            : (1, (byte)(digit1 + @base - digit2WithCarry));
    }

    private static LargeNumAsByteList SubtractPositives(LargeNumAsByteList x, LargeNumAsByteList y)
    {
        if (x.IsNegative || y.IsNegative)
            throw new ArgumentException("Arguments must be non-negative");

        if (x.Base != y.Base)
            throw new ArgumentException("I can only work with numbers of the same base");

        if (x < y)
            return -(SubtractPositives(y, x));

        x.RemoveLeadingZeros();
        y.RemoveLeadingZeros();

        var ret = new LargeNumAsByteList(x.Base, expectedLength: x.Length);

        int length = Math.Max(x.Length, y.Length);
        int carry = 0;
        for (int i = 0; i < length; i++)
        {
            byte xi = i < x.Length ? x.ReverseDigits[i] : (byte)0;
            byte yi = i < y.Length ? y.ReverseDigits[i] : (byte)0;
            var digitSubstractionResult = SubtractDigits(xi, yi, carry, ret.Base);
            ret.ReverseDigits.Append(digitSubstractionResult.Result);
            carry = digitSubstractionResult.Carry;
        }
        if (carry != 0)
            ret.ReverseDigits.Append((byte)carry);

        if (ret.Length == 1 && ret.ReverseDigits[0] == 0)
            ret.IsNegative = false;

        ret.RemoveLeadingZeros();

        return ret;
    }

    public static LargeNumAsByteList Subtract(LargeNumAsByteList x, LargeNumAsByteList y)
    {
        if (!x.IsNegative && !y.IsNegative)
            return SubtractPositives(x, y);
        else if (!x.IsNegative && y.IsNegative)
            return AddPositives(x, -y, 0, 0);
        else if (x.IsNegative && !y.IsNegative)
            return -AddPositives(-x, y, 0, 0);
        else // both are negative
            return -SubtractPositives(-y, -x);
    }

    public static LargeNumAsByteList operator -(LargeNumAsByteList x, LargeNumAsByteList y) =>
        Subtract(x, y);

    // Unary -' operator to flip sign
    public static LargeNumAsByteList operator -(LargeNumAsByteList x) =>
        new LargeNumAsByteList
        {
            ReverseDigits = x.ReverseDigits, 
            IsNegative = !x.IsNegative && !x.IsZero, 
            Base = x.Base
        };

    #endregion Subtraction


    #region Multipication

    public static LargeNumAsByteList MultiplyBySingleDigit(
        LargeNumAsByteList largeNum, byte digit, bool isDigitNegative)
    {
        if (digit == (byte)0)
            return new LargeNumAsByteList(0, largeNum.Base);

        LargeNumAsByteList ret;

        if (digit == (byte)1)
        {
            ret = largeNum;
        }
        else
        {
            ret = new LargeNumAsByteList(@base: largeNum.Base, expectedLength: largeNum.Length + 1);
            int carry = 0;
            for (int i = 0; i < largeNum.ReverseDigits.Count; i++)
            {
                int digitMultiplicationResult = (int)(largeNum.ReverseDigits[i]) * (int)digit + carry;
                ret.ReverseDigits.Append((byte)(digitMultiplicationResult % largeNum.Base));
                carry = digitMultiplicationResult / largeNum.Base;
            }
            if (carry != 0)
                ret.ReverseDigits.Append((byte)carry);
        }

        if(ret.IsZero)
            ret.IsNegative = false;
        else if (isDigitNegative)
            ret.IsNegative ^= true;
        
        return ret;
    }

    public static LargeNumAsByteList Multiply(LargeNumAsByteList x, LargeNumAsByteList y)
    {
        int simpleMultiplyThreshold = LargeNumAsByteList.SimpleMultiplyThresholdsByBase[x.Base];

        int combinedLength = x.Length + y.Length;
        int maxLength = Math.Max(x.Length, y.Length);

        return
            combinedLength <= simpleMultiplyThreshold ? (LargeNumAsByteList)((Int64)x * (Int64)y)
            : maxLength <= LargeNumAsByteList.KaratsubaMultiplyThreshold ? SimpleMultiply(x, y)
            : KaratsubaMultiply(x, y);
    }

    public static LargeNumAsByteList operator *(LargeNumAsByteList x, LargeNumAsByteList y) =>
        Multiply(x, y);

    public static LargeNumAsByteList DotnetMultiply(LargeNumAsByteList x, LargeNumAsByteList y) =>
        new LargeNumAsByteList((Int64)x * (Int64)y, @base: x.Base);

    public static LargeNumAsByteList SimpleMultiply(LargeNumAsByteList x, LargeNumAsByteList y)
    {
        if (x.Base != y.Base)
            throw new ArgumentException("I can only work with numbers of the same base");

        (var shorter, var longer) = LargeNumAsByteList.GetShorterAndLonger(x, y);

        var ret = new LargeNumAsByteList(@base: shorter.Base, expectedLength: shorter.Length + longer.Length);

        for (int i = shorter.Length - 1; i >= 0; i--)
        {
            var longerMultipliedByIthDigitFromShorter = MultiplyBySingleDigit(longer, shorter.ReverseDigits[i], shorter.IsNegative);
            ret = Add(ret, longerMultipliedByIthDigitFromShorter, 0, i);
        }

        ret.RemoveLeadingZeros();
        return ret;
    }

    /// <summary>
    /// Implements multiplication using the Karatsuba algorithm.
    /// Applies simpler algorithms as the numbers get smaller, as driven by the static variables
    ///  LargeNumAsByteList.SimpleMultiplyThresholdsByBase[base] and
    ///  LargeNumAsByteList.KaratsubaMultiplyThreshold
    /// To force using the karasuba algorithm for smaller numbers, set those two variable to smaller values, i.e 0 or 1.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static LargeNumAsByteList KaratsubaMultiply(LargeNumAsByteList x, LargeNumAsByteList y)
    {
        // See great description here: https://www.quora.com/What-is-the-Karatsuba-algorithm
        // xy = a* B^2m + b * B^m + c
        // where
        //  a = x1 * y1
        //  b = (x1+x2)(y1+y2) - a - c
        //  c = x2 * y2

        if (x.Base != y.Base)
            throw new ArgumentException("I can only work with numbers of the same base");

        x.RemoveLeadingZeros();
        y.RemoveLeadingZeros();
        (var shorter, var longer) = LargeNumAsByteList.GetShorterAndLonger(Abs(x), Abs(y));

        // Edge case: one of the numbers is single digit. Multiply and we're done.
        if (shorter.Length == 1)
            return MultiplyBySingleDigit(longer, shorter.ReverseDigits[0], shorter.IsNegative);

        int splitIndex = longer.Length >> 1;  // longer.Length / 2;
        LargeNumAsByteList x1, x2, y1, y2;

        if (shorter.Length <= splitIndex)
        {   // Edge case: the shorter number is not split, just use 0 for the more significant half
            x1 = 0;
            x2 = shorter;
            (y1, y2) = longer.Split(splitIndex);
        }
        else
        {   // General case: both numbers are split
            (x1, x2) = shorter.Split(splitIndex);
            (y1, y2) = longer.Split(splitIndex);
        }

        // Split
        LargeNumAsByteList a = Multiply(x1, y1);
        LargeNumAsByteList c = Multiply(x2, y2);
        LargeNumAsByteList b = Multiply(x1 + x2, y1 + y2) - a - c;
        //LargeNumAsByteList a = KaratsubaMultiply(x1, y1);
        //LargeNumAsByteList c = KaratsubaMultiply(x2, y2);
        //LargeNumAsByteList b = KaratsubaMultiply(x1 + x2, y1 + y2) - a - c;
        var ret = Add(a, b, splitIndex, 0);
        ret = Add(ret, c, splitIndex, 0);

        ret.IsNegative = (x.IsNegative ^ y.IsNegative) && !ret.IsZero;
        return ret;
    }

    internal LargeNumAsByteList ShiftRight(int n)
    {
        if (IsZero)
            return this;

        var reverseDigits = new List<byte>(Length + n);
        reverseDigits.AddRange(Enumerable.Repeat((byte)0, n));
        reverseDigits.AddRange(ReverseDigits);

        var ret = new LargeNumAsByteList
        {
            ReverseDigits = reverseDigits,
            IsNegative = this.IsNegative,
            Base = this.Base,
        };

        return ret;
    }

    #endregion Multipication

    public static LargeNumAsByteList Abs(LargeNumAsByteList x) =>
        new LargeNumAsByteList { ReverseDigits = x.ReverseDigits, IsNegative = false, Base = x.Base };

    #region Comparisons

    public int CompareTo(LargeNumAsByteList? other)
    {
        if (other is null)
            return -1;

        if (IsNegative && !other.IsNegative)
            return -1;

        if (!IsNegative && other.IsNegative)
            return 1;

        if (IsNegative && other.IsNegative)
            return -1 * (Abs(this).CompareTo(Abs(other)));

        // Assume no leading zeros
        if (Length < other.Length)
            return -1;

        if (Length > other.Length)
            return 1;

        for (int i = Length - 1; i >= 0; i--)
        {
            if (ReverseDigits[i] < other.ReverseDigits[i])
                return -1;

            if (ReverseDigits[i] > other.ReverseDigits[i])
                return 1;
        }

        return 0;
    }

    public static bool operator <(LargeNumAsByteList x, LargeNumAsByteList y) =>
        -1 == x.CompareTo(y);

    public static bool operator >(LargeNumAsByteList x, LargeNumAsByteList y) =>
        1 == x.CompareTo(y);

    public static bool operator <=(LargeNumAsByteList x, LargeNumAsByteList y) =>
        1 != x.CompareTo(y);

    public static bool operator >=(LargeNumAsByteList x, LargeNumAsByteList y) =>
        -1 != x.CompareTo(y);

    public static bool operator ==(LargeNumAsByteList x, LargeNumAsByteList y) =>
        0 == x.CompareTo(y);

    public static bool operator !=(LargeNumAsByteList x, LargeNumAsByteList y) =>
        0 != x.CompareTo(y);

    #endregion Comparisons

    /// <summary>
    /// Splits the number in two.
    /// Both numbers retain the original sign.
    /// </summary>
    /// <param name="n">Specifies the number of digits to keep in the less significant half.</param>
    /// <returns>
    /// A tuple of Left and Right, where Left is the more significant half and
    /// Right is the less significant one.
    /// </returns>
    public (LargeNumAsByteList Left, LargeNumAsByteList Right) Split(int n)
    {
        if (Length == 1)
            throw new ArgumentException($"Can't split a number with 1 digit.");

        if (n == 0 || n > Length - 1)
            throw new ArgumentException($"Split index ({n}) must be between 1 and length-1 ({Length - 1})");

        if (IsNegative)
            throw new ArgumentException($"I can only split non-negative numbers");

        var rightDigits = ReverseDigits.GetRange(0, n);
        var leftDigits = ReverseDigits.GetRange(n, Length - n);

        var left = new LargeNumAsByteList { ReverseDigits = leftDigits, IsNegative = false, Base = this.Base };
        var right = new LargeNumAsByteList { ReverseDigits = rightDigits, IsNegative = false, Base = this.Base };

        return (left, right);
    }

    private static (LargeNumAsByteList Shorter, LargeNumAsByteList Longer) GetShorterAndLonger(
        LargeNumAsByteList x, LargeNumAsByteList y)
    {
        return x.Length >= y.Length
            ? (y, x)
            : (x, y);
    }

    /// <summary>
    /// Mutates this by removing leading zeros (the ones at most significant digits)
    /// </summary>
    private void RemoveLeadingZeros()
    {
        for (int i = ReverseDigits.Count - 1; i > 0; i--)
        {
            if (ReverseDigits[i] == 0)
                ReverseDigits.RemoveAt(i);
            else
                break;
        }
    }
}
