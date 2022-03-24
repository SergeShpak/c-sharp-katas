using System;
using Xunit;

namespace SameHashCode.Tests;

public class SameHashCode_Tests
{
    [Fact]
    public void Seracher_FindStringsWithSameHashCode_ReturnsCorrectObject()
    {
        var result = SameHashCode.Searcher.FindStringsWithSameHash();

        Assert.True(
                (result.First.GetHashCode() == result.Second.GetHashCode())
                && (result.Second.GetHashCode() == result.Third.GetHashCode())
                && (!String.Equals(result.First, result.Second))
                && (!String.Equals(result.Second, result.Third))
                && (!String.Equals(result.Third, result.First)),
                $"same hash test failed on returned values: {result}"
        );
    }
}
