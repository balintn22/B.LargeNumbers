using System;
using System.Diagnostics;

namespace B.LargeNumbers.PerformanceTest;

internal class Program
{
    private static Random _random = new Random();

    static void Main(string[] args)
    {
        byte numBase = 10;

        Write($"#Digits\tDotnetMultiply [msec]\tSimpleMultiply [msec]\tKaratsubaMultiplyTimetaken\tMultiply [msec]");

        var sw = new Stopwatch();
        LargeNumAsByteList n = new LargeNumAsByteList(RandomDigit().ToString(), @base: numBase);
        LargeNumAsByteList m = new LargeNumAsByteList(RandomDigit().ToString(), @base: numBase);
        for (int numberOfDigits = 1; numberOfDigits < 5000; numberOfDigits++)
        {
            n.ReverseDigits.Add(RandomDigit());
            m.ReverseDigits.Add(RandomDigit());

            if (numberOfDigits % 100 != 0)
                continue;

            Int64? dotnetMultiplyTimetaken = null;
            if (numberOfDigits <= 18)
            {
                sw.Restart();
                var dotnetMultiplyResult = LargeNumAsByteList.DotnetMultiply(n, m);
                sw.Stop();
                dotnetMultiplyTimetaken = sw.ElapsedMilliseconds;
            }

            // Optimize algorithm to use
            Int64? multiplyTimetaken = null;
            LargeNumAsByteList multiplyResult = null;
            LargeNumAsByteList.SimpleMultiplyThresholdsByBase[numBase] = 0;
            LargeNumAsByteList.KaratsubaMultiplyThreshold = int.MaxValue;
            sw.Restart();
            multiplyResult = LargeNumAsByteList.Multiply(n, m);
            sw.Stop();
            multiplyTimetaken = sw.ElapsedMilliseconds;

            Int64? simpleMultiplyTimetaken = null;
            LargeNumAsByteList simpleMultiplyResult = null;
            // Force multiplication to use SimpleMultiply for small and large numbers
            LargeNumAsByteList.SimpleMultiplyThresholdsByBase[numBase] = 0;
            LargeNumAsByteList.KaratsubaMultiplyThreshold = int.MaxValue;
            sw.Restart();
            simpleMultiplyResult = LargeNumAsByteList.SimpleMultiply(n, m);
            sw.Stop();
            simpleMultiplyTimetaken = sw.ElapsedMilliseconds;

            // Force multiplication to use KaratsubaMultiply even for small numbers
            LargeNumAsByteList.SimpleMultiplyThresholdsByBase[numBase] = 0;
            LargeNumAsByteList.KaratsubaMultiplyThreshold = 0;
            sw.Restart();
            var karatsubaMultiplyResult = LargeNumAsByteList.KaratsubaMultiply(n, m);
            sw.Stop();
            Int64 karatsubaMultiplyTimetaken = sw.ElapsedMilliseconds;

            if (simpleMultiplyResult != karatsubaMultiplyResult)
                Write($"Result mismatch between SimpleMultiply and KaratsubaMultiply for (n,m) = ({n},{m})");

            if (simpleMultiplyResult != multiplyResult)
                Write($"Result mismatch between SimpleMultiply and Multiply for (n,m) = ({n},{m})");

            Write($"{numberOfDigits}\t{dotnetMultiplyTimetaken}\t{simpleMultiplyTimetaken}\t{karatsubaMultiplyTimetaken}\t{multiplyTimetaken}");
        }
    }

    private static byte RandomDigit()
        => (byte)_random.Next(9);

    private static void Write(string txt) => Console.WriteLine(txt);
}
