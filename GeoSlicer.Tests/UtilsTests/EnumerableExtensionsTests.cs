using System.Collections.Generic;
using GeoSlicer.Utils;

namespace GeoSlicer.Tests.UtilsTests;

public class EnumerableExtensionsTests
{
    [Theory]
    [MemberData(nameof(Data.DataContainsIEnumerableTrue), MemberType = typeof(Data))]
    private void TestContainsIEnumerableTrue(int?[] first, int?[] second)
    {
        Assert.True(first.Contains(second));
    }
    
    [Theory]
    [MemberData(nameof(Data.DataContainsIEnumerableFalse), MemberType = typeof(Data))]
    private void TestContainsIEnumerableFalse(int?[] first, int?[] second)
    {
        Assert.False(first.Contains(second));
    }
    
    private static class Data
    {
        public static IEnumerable<IEnumerable<int?[]>> DataContainsIEnumerableTrue =>
            new[]
            {
                new[] { new int?[] { 1, null, 3, 4, 5 }, new int?[] { 1, null } },
                new[] { new int?[] { 1, null, 3, 4, 5 }, new int?[] { 3, 4 } },
                new[] { new int?[] { 1, null, 3, 4, 5 }, new int?[] { 4, 5 } },
                new[] { new int?[] { null, null, 3, 4, null }, new int?[] { 3, 4 } },
                new[] { new int?[] { 1, 2, 1, 2, 4, 2, 1, 2, 1, 2, 1, 3, 2, 1, 2, 1, 4}, new int?[] { 1, 2, 1, 3 } }
            };
        
        public static IEnumerable<IEnumerable<int?[]>> DataContainsIEnumerableFalse =>
            new[]
            {
                new[] { new int?[] { 1, 2, null, 3, 4, 5 }, new int?[] { 1, null } },
                new[] { new int?[] { 1, null, 3, null, 4, 5 }, new int?[] { 3, 4 } },
                new[] { new int?[] { 1, null, 3, 4 }, new int?[] { 4, 5 } },
                new[] { new int?[] { 1, 2, 1, 2, 4, 2, 1, 2, 1, 2, 1, 1, 3, 2, 1, 2, 1, 4}, new int?[] { 1, 2, 1, 3 } }
            };

    }
}