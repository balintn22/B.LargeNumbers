using Microsoft.VisualStudio.TestTools.UnitTesting;
using B.LargeNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace B.LargeNumbers.Tests;

[TestClass()]
public class StringExtensionsTests
{
    [DataTestMethod()]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow("a", "a")]
    [DataRow("abc", "cba")]
    public void ReverseTest(string input, string expectedOutput)
    {
        string output = input.Reverse();

        output.Should().Be(expectedOutput);
    }

    [TestMethod]
    [TestCategory("Unit test any build config")]
    public void StringExtensions_Left_Test()
    {
        Assert.AreEqual(null, ((string)null).Left(0));
        Assert.AreEqual("", "abc".Left(0));
        Assert.AreEqual("a", "abc".Left(1));
        Assert.AreEqual("ab", "abc".Left(2));
        Assert.AreEqual("abc", "abc".Left(3));
        Assert.AreEqual("abc", "abc".Left(4));
    }

    [TestMethod]
    [TestCategory("Unit test any build config")]
    public void StringExtensions_Right_Test()
    {
        Assert.AreEqual(null, ((string)null).Right(0));
        Assert.AreEqual("", "abc".Right(0));
        Assert.AreEqual("c", "abc".Right(1));
        Assert.AreEqual("bc", "abc".Right(2));
        Assert.AreEqual("abc", "abc".Right(3));
        Assert.AreEqual("abc", "abc".Right(4));
    }
}