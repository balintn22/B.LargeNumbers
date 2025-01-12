using System;

namespace B.LargeNumbers;

public static class StringExtensions
{
    public static string Reverse(this string s)
    {
        if (s == null)
            return null;

        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static string Left(this string self, int charCount)
    {
        if (string.IsNullOrEmpty(self))
            return self;

        return (self.Length <= charCount
            ? self
            : self.Substring(0, charCount));
    }

    /// <summary>
    /// Takes the rightmost N characters of the string.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="charCount">0-based.</param>
    /// <returns></returns>
    public static string Right(this string self, int charCount)
    {
        if (self == null)
            return null;

        charCount = Math.Min(charCount, self.Length);
        int StartIndex = self.Length - charCount;
        string ret = self.Substring(StartIndex);
        return ret;
    }
}
