using Microsoft.VisualStudio.TestTools.UnitTesting;
using B.LargeNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Runtime.Intrinsics.Arm;

namespace B.LargeNumbers.Tests;

[TestClass()]
public class LargeNumAsByteListTests
{
    #region Constructor Tests

    [DataTestMethod()]
    [DataRow(1, (byte)10, "1")]
    [DataRow(10, (byte)10, "10")]
    [DataRow(100, (byte)10, "100")]
    [DataRow(0, (byte)10, "0")]
    [DataRow(00, (byte)10, "0")]
    [DataRow(000, (byte)10, "0")]
    [DataRow(123, (byte)10, "123")]
    [DataRow(-0, (byte)10, "0")]
    [DataRow(-1, (byte)10, "-1")]
    [DataRow(2, (byte)10, "2")]
    [DataRow(2, (byte)2, "10")]
    [DataRow(-2, (byte)2, "-10")]
    [DataRow(15, (byte)16, "F")]
    [DataRow(16, (byte)16, "10")]
    [DataRow(-15, (byte)16, "-F")]
    public void ConstructorFromInt_ShouldReturnExpectedResult(
        int input, byte @base, string expectedString)
    {
        var result = new LargeNumAsByteList(input, @base);
        result.ToString().Should().Be(expectedString);
    }

    [DataTestMethod()]
    [DataRow("1", (byte)10, "1")]
    [DataRow("10", (byte)10, "10")]
    [DataRow("100", (byte)10, "100")]
    [DataRow("0", (byte)10, "0")]
    [DataRow("00", (byte)10, "0")]
    [DataRow("000", (byte)10, "0")]
    [DataRow("123", (byte)10, "123")]
    [DataRow("-0", (byte)10, "0")]
    [DataRow("-1", (byte)10, "-1")]
    [DataRow("2", (byte)10, "2")]
    [DataRow("10", (byte)2, "10")]
    [DataRow("-10", (byte)2, "-10")]
    [DataRow("10", (byte)16, "10")]
    [DataRow("1A", (byte)16, "1A")]
    [DataRow("-1A", (byte)16, "-1A")]
    public void ConstructorFromString_ShouldReturnExpectedResult(
        string input, byte @base, string expectedString)
    {
        var result = new LargeNumAsByteList(input, @base);
        result.ToString().Should().Be(expectedString);
    }

    #endregion Constructor Tests


    [DataTestMethod()]
    [DataRow(0, 0, "0")]
    [DataRow(1, 0, "1")]
    [DataRow(0, 1, "1")]
    [DataRow(-1, 0, "-1")]
    [DataRow(0, -1, "-1")]
    [DataRow(-1, -1, "-2")]
    [DataRow(1, -1, "0")]
    public void Add_ShouldReturnExpectedResult(int x, int y, string expectedResult)
    {
        LargeNumAsByteList.Add(new LargeNumAsByteList(x), new LargeNumAsByteList(y)).ToString().Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(0, 0, 5)]
    [DataRow(8, 9, 5)]
    [DataRow(10000, 0, 5)]
    [DataRow(0, 10000, 5)]
    public void Add_ShouldRetainBase(int x, int y, int @base)
    {
        var result = LargeNumAsByteList.Add(
            new LargeNumAsByteList(x, (byte)@base),
            new LargeNumAsByteList(y, (byte)@base));
        
        result.Base.Should().Be((byte)@base);
    }


    [DataTestMethod()]
    ////[DataRow(0, 0, "0")]
    ////[DataRow(1, 0, "1")]
    ////[DataRow(0, 1, "-1")]
    ////[DataRow(-1, 0, "-1")]
    ////[DataRow(0, -1, "1")]
    ////[DataRow(-1, -1, "0")]
    ////[DataRow(1, -1, "2")]
    [DataRow(108, 81, "27")]
    public void Subtract_ShouldReturnExpectedResult(int x, int y, string expectedResult)
    {
        LargeNumAsByteList.Subtract(new LargeNumAsByteList(x), new LargeNumAsByteList(y)).ToString().Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(0, 0, 5)]
    [DataRow(8, 9, 5)]
    [DataRow(10000, 0, 5)]
    [DataRow(0, 10000, 5)]
    public void Subtract_ShouldRetainBase(int x, int y, int @base)
    {
        var result = LargeNumAsByteList.Subtract(
            new LargeNumAsByteList(x, (byte)@base),
            new LargeNumAsByteList(y, (byte)@base));

        result.Base.Should().Be((byte)@base);
    }

    [DataTestMethod()]
    [DataRow(1, true)]
    [DataRow(-1, false)]
    [DataRow(0, false)]
    [DataRow(-0, false)]
    public void FlipSign_ShouldReturnExpectedResult(int x, bool expectedIsNegative)
    {
        (-(new LargeNumAsByteList(x))).IsNegative.Should().Be(expectedIsNegative);
    }

    [DataTestMethod()]
    [DataRow("12", 1, "1", "2")]
    [DataRow("123", 1, "12", "3")]
    [DataRow("123", 2, "1", "23")]
    [DataRow("1234", 1, "123", "4")]
    [DataRow("1234", 2, "12", "34")]
    [DataRow("1234", 3, "1", "234")]
    public void Split_ShouldReturnExpectedResult(
        string digits, int splitIndex, string expectedLeft, string expectedRight)
    {
        var sut = new LargeNumAsByteList(digits);
        var (left, right) = sut.Split(splitIndex);

        left.ToString().Should().Be(expectedLeft);
        right.ToString().Should().Be(expectedRight);
    }

    [TestMethod()]
    public void Split_ShouldMaintainBase()
    {
        var sut = new LargeNumAsByteList("1234", @base: 5);
        var (left, right) = sut.Split(2);

        left.Base.Should().Be(5);
        right.Base.Should().Be(5);
    }

    #region Multiplication Tests

    [DataTestMethod()]
    [DataRow("0", "0", "0")]
    [DataRow("1", "0", "0")]
    [DataRow("0", "1", "0")]
    [DataRow("1", "1", "1")]
    [DataRow("1", "2", "2")]
    [DataRow("2", "1", "2")]
    [DataRow("10", "2", "20")]
    [DataRow("10", "10", "100")]
    [DataRow(
        "10000000000000000000000000000000000000000",
        "10000000000000000000000000000000000000000",
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow(
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow("11", "2", "22")]
    [DataRow("22", "2", "44")]
    [DataRow("44", "2", "88")]
    [DataRow("88", "2", "176")]
    [DataRow("32768", "10", "327680")]
    [DataRow("32768", "2", "65536")]
    [DataRow("32768", "20", "655360")]
    [DataRow("32768", "11", "360448")]
    [DataRow("32768", "999", "32735232")]
    [DataRow("10", "10", "100")]
    [DataRow("10", "-10", "-100")]
    [DataRow("-10", "10", "-100")]
    [DataRow("-10", "-10", "100")]
    [DataRow("073", "284", "20732")]
    [DataRow("525742114465711673162735154676120", "222474320742784515601516571520838", "116964119801634469627672476354216219307565287930466970475720988560")]
    [DataRow("0525742114465711673162735154676120", "0222474320742784515601516571520838", "116964119801634469627672476354216219307565287930466970475720988560")]
    public void Multiply_ShouldReturnExpectedResult(
        string xStr, string yStr, string expectedResult)
    {
        var x = new LargeNumAsByteList(xStr);
        var y = new LargeNumAsByteList(yStr);

        var result = LargeNumAsByteList.Multiply(x, y);

        result.ToString().Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow("0", "0", "0")]
    [DataRow("1", "0", "0")]
    [DataRow("0", "1", "0")]
    [DataRow("1", "1", "1")]
    [DataRow("1", "2", "2")]
    [DataRow("2", "1", "2")]
    [DataRow("10", "2", "20")]
    [DataRow("10", "10", "100")]
    [DataRow(
        "10000000000000000000000000000000000000000",
        "10000000000000000000000000000000000000000",
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow(
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow("11", "2", "22")]
    [DataRow("22", "2", "44")]
    [DataRow("44", "2", "88")]
    [DataRow("88", "2", "176")]
    [DataRow("32768", "10", "327680")]
    [DataRow("32768", "2", "65536")]
    [DataRow("32768", "20", "655360")]
    [DataRow("32768", "11", "360448")]
    [DataRow("32768", "999", "32735232")]
    [DataRow("10", "10", "100")]
    [DataRow("10", "-10", "-100")]
    [DataRow("-10", "10", "-100")]
    [DataRow("-10", "-10", "100")]
    [DataRow("073", "284", "20732")]
    [DataRow("525742114465711673162735154676120", "222474320742784515601516571520838", "116964119801634469627672476354216219307565287930466970475720988560")]
    [DataRow("0525742114465711673162735154676120", "0222474320742784515601516571520838", "116964119801634469627672476354216219307565287930466970475720988560")]
    [DataRow("078382428", "022635367", "1774215024131076")]
    [DataRow("07838", "02263", "17737394")]
    [DataRow("225215234453543222652331118134176867", "476251637453774644785257736353242834", "107259124188035723252813292474370623334196829315555059680098540756321078")]
    [DataRow("476251637453774644", "785257736353242834", "373980282761476359712957883243901096")]
    [DataRow("225215234453543222", "652331118134176867", "146914905711930643628229082577045474")]
    [DataRow("476251637453774644", "225215234453543222", "107259124188035722765287524101662968")]
    [DataRow("785257736353242834", "652331118134176867", "512248057178823563680098540756321078")]
    [DataRow("1261509373807017478", "877546352587720089", "1107032949739566941267334836994715542")]
    [DataRow("1465266441", "2068526851", "3030942977077707291")]
    [DataRow("20685", "14652", "303076620")]
    [DataRow("198", "291", "57618")]
    [DataRow("29", "19", "551")]
    public void SimpleMultiply_ShouldReturnExpectedResult(
        string xStr, string yStr, string expectedResult)
    {
        // Force multiplication to use SimpleMultiply for small and large numbers
        byte numBase = 10;
        LargeNumAsByteList.SimpleMultiplyThresholdsByBase[numBase] = 0;
        LargeNumAsByteList.KaratsubaMultiplyThreshold = int.MaxValue;
        var x = new LargeNumAsByteList(xStr, numBase);
        var y = new LargeNumAsByteList(yStr, numBase);

        var result = LargeNumAsByteList.SimpleMultiply(x, y);

        result.ToString().Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow("0", "0", "0")]
    [DataRow("1", "0", "0")]
    [DataRow("0", "1", "0")]
    [DataRow("1", "1", "1")]
    [DataRow("1", "2", "2")]
    [DataRow("2", "1", "2")]
    [DataRow("10", "2", "20")]
    [DataRow("10", "10", "100")]
    [DataRow(
        "10000000000000000000000000000000000000000",
        "10000000000000000000000000000000000000000",
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow(
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "100000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow("11", "2", "22")]
    [DataRow("22", "2", "44")]
    [DataRow("44", "2", "88")]
    [DataRow("88", "2", "176")]
    [DataRow("32768", "10", "327680")]
    [DataRow("32768", "2", "65536")]
    [DataRow("32768", "20", "655360")]
    [DataRow("32768", "11", "360448")]
    [DataRow("32768", "999", "32735232")]
    [DataRow("10", "10", "100")]
    [DataRow("10", "-10", "-100")]
    [DataRow("-10", "10", "-100")]
    [DataRow("-10", "-10", "100")]
    [DataRow("073", "284", "20732")]
    [DataRow("525742114465711673162735154676120", "222474320742784515601516571520838", "116964119801634469627672476354216219307565287930466970475720988560")]
    [DataRow("0525742114465711673162735154676120", "0222474320742784515601516571520838", "116964119801634469627672476354216219307565287930466970475720988560")]
    [DataRow("078382428232801365", "022635367272724200", "1774215050777405368724121028533000")]
    [DataRow("07838", "02263", "17737394")]
    [DataRow("225215234453543222652331118134176867", "476251637453774644785257736353242834", "107259124188035723252813292474370623334196829315555059680098540756321078")]
    [DataRow("476251637453774644", "785257736353242834", "373980282761476359712957883243901096")]
    [DataRow("225215234453543222", "652331118134176867", "146914905711930643628229082577045474")]
    [DataRow("476251637453774644", "225215234453543222", "107259124188035722765287524101662968")]
    [DataRow("785257736353242834", "652331118134176867", "512248057178823563680098540756321078")]
    [DataRow("1261509373807017478", "877546352587720089", "1107032949739566941267334836994715542")]
    [DataRow("1465266441", "2068526851", "3030942977077707291")]
    [DataRow("20685", "14652", "303076620")]
    [DataRow("198", "291", "57618")]
    [DataRow("29", "19", "551")]
    public void KaratsubaMultiply_ShouldReturnExpectedResult(
        string xStr, string yStr, string expectedResult)
    {
        // Force multiplication to use KaratsubaMultiply even for small numbers
        byte numBase = 10;
        LargeNumAsByteList.SimpleMultiplyThresholdsByBase[numBase] = 0;
        LargeNumAsByteList.KaratsubaMultiplyThreshold = 0;

        var x = new LargeNumAsByteList(xStr, numBase);
        var y = new LargeNumAsByteList(yStr, numBase);

        var result = LargeNumAsByteList.KaratsubaMultiply(x, y);

        result.ToString().Should().Be(expectedResult);
    }

    #endregion Multiplication Tests

    #region Comparison Tests

    [DataTestMethod()]
    [DataRow(10, 10, 0)]
    [DataRow(-10, 10, -1)]
    [DataRow(10, -10, 1)]
    [DataRow(100, 10, 1)]
    [DataRow(10, 100, -1)]
    public void CompareTo_ShouldReturnExpectedValue(int xInt, int yInt, int expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        x.CompareTo(y).Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(10, 10, false)]
    [DataRow(-10, 10, true)]
    [DataRow(10, -10, false)]
    [DataRow(100, 10, false)]
    [DataRow(10, 100, true)]
    public void LessThan_ShouldReturnExpectedValue(int xInt, int yInt, bool expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        (x < y).Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(10, 10, true)]
    [DataRow(-10, 10, true)]
    [DataRow(10, -10, false)]
    [DataRow(100, 10, false)]
    [DataRow(10, 100, true)]
    public void LessThanOrEqual_ShouldReturnExpectedValue(int xInt, int yInt, bool expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        (x <= y).Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(10, 10, false)]
    [DataRow(-10, 10, false)]
    [DataRow(10, -10, true)]
    [DataRow(100, 10, true)]
    [DataRow(10, 100, false)]
    public void GreaterThan_ShouldReturnExpectedValue(int xInt, int yInt, bool expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        (x > y).Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(10, 10, true)]
    [DataRow(-10, 10, false)]
    [DataRow(10, -10, true)]
    [DataRow(100, 10, true)]
    [DataRow(10, 100, false)]
    public void GreaterThanOrEqual_ShouldReturnExpectedValue(int xInt, int yInt, bool expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        (x >= y).Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(10, 10, true)]
    [DataRow(-10, 10, false)]
    [DataRow(10, -10, false)]
    [DataRow(100, 10, false)]
    [DataRow(10, 100, false)]
    public void Equal_ShouldReturnExpectedValue(int xInt, int yInt, bool expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        (x == y).Should().Be(expectedResult);
    }

    [DataTestMethod()]
    [DataRow(10, 10, false)]
    [DataRow(-10, 10, true)]
    [DataRow(10, -10, true)]
    [DataRow(100, 10, true)]
    [DataRow(10, 100, true)]
    public void NotEqual_ShouldReturnExpectedValue(int xInt, int yInt, bool expectedResult)
    {
        var x = new LargeNumAsByteList(xInt);
        var y = new LargeNumAsByteList(yInt);
        (x != y).Should().Be(expectedResult);
    }

    #endregion Comparison Tests

    [DataTestMethod]
    [DataRow("0", 3, "0")]
    [DataRow("1", 3, "1000")]
    [DataRow("-1", 3, "-1000")]
    public void ShiftRight_ShouldReturnExpectedResult(string xStr, int n, string expectedNum)
    {
        var sut = new LargeNumAsByteList(xStr, @base: 5);
        var result = sut.ShiftRight(n);
        result.ToString().Should().Be(expectedNum);
    }
}