using System.Collections.Generic;
using Xunit;

namespace NthRoot.Tests;

public class NthRoot_Tests
{
    [Fact]
    public void Calculator_GetNthRoot_CalculatesRootCorrectly()
    {
        const double diff = 0.000001;
        foreach (var tc in new List<(double inNumber, int inN)> {
        (100, 2),
        (10, 10),
        (1000000000, 15),
    })
        {
            var expected = System.Math.Pow(tc.inNumber, 1.0 / tc.inN);
            var actual = Calculator.GetNthRoot(tc.inNumber, tc.inN);
            Assert.True(System.Math.Abs(expected - actual) < diff);
        }
    }
}