using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests;

public class ContainsCheckerTests
{
    private static readonly double Epsilon = 1E-9;

    private static readonly LineService LineService = new LineService(Epsilon);

    private static readonly ContainsChecker ContainsChecker =
        new(LineService, Epsilon);

    [Fact]
    public void TestIsPointInLinearRing()
    {
        Coordinate[] polygon1 = { new(2, 1), new(1, 4), new(4, 4), new(2, 1) };
        LinearRing polygonRing1 = new LinearRing(polygon1);
        
        Coordinate pointTrue1 = new Coordinate(2, 1);
        Coordinate pointTrue2 = new Coordinate(2, 2);
        Coordinate pointTrue3 = new Coordinate(1, 4);
        Coordinate pointTrue4 = new Coordinate(2, 4);
        Coordinate pointTrue5 = new Coordinate(4, 4);

        Coordinate pointFalse1 = new Coordinate(1, 1);
        Coordinate pointFalse2 = new Coordinate(1, 2);
        Coordinate pointFalse3 = new Coordinate(-1, 4);

        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue1, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue2, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue3, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue4, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue5, polygonRing1));

        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse1, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse2, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse3, polygonRing1));
    }

    [Fact]
    public void Test1IsPointInLinearRing()
    {
        Coordinate[] polygon1
              = { new(1, 4), new(3, 7), new(5, 4), new(3, 4), new(3, 2), new(1, 4) };
        LinearRing polygonRing1 = new LinearRing(polygon1);
        
        Coordinate pointTrue1 = new Coordinate(2, 4);
        Coordinate pointTrue2 = new Coordinate(2, 3);
        Coordinate pointTrue3 = new Coordinate(5, 4);
        Coordinate pointTrue4 = new Coordinate(3, 6);
        Coordinate pointTrue5 = new Coordinate(3, 3);
        Coordinate pointTrue6 = new Coordinate(2.5, 3.5);

        Coordinate pointFalse1 = new Coordinate(2, 7);
        Coordinate pointFalse2 = new Coordinate(1, 6);
        Coordinate pointFalse3 = new Coordinate(4, 3);
        Coordinate pointFalse4 = new Coordinate(2, 2);
        Coordinate pointFalse5 = new Coordinate(-1, 4);
        Coordinate pointFalse6 = new Coordinate(4, 7);
        Coordinate pointFalse7 = new Coordinate(6, 4);
        Coordinate pointFalse8 = new Coordinate(3, 1);

        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue1, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue2, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue3, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue4, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue5, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue6, polygonRing1));

        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse1, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse2, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse3, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse4, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse5, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse6, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse7, polygonRing1));
        Assert.False(ContainsChecker .IsPointInLinearRing(pointFalse8, polygonRing1));
    }

    [Fact]
    public void Test2IsPointInLinearRing()
    {
        Coordinate[] polygon1
              = { new(1,1), new(1,6), new(2,6), new(3,6), new(4,5), new(6,6),
                  new(7,0), new(5,3), new(4,1), new(3,3), new(2,1), new(1,1) };
        LinearRing polygonRing1 = new LinearRing(polygon1);

        Coordinate pointTrue1 = new Coordinate(1, 6);
        Coordinate pointTrue2 = new Coordinate(3, 6);
        Coordinate pointTrue3 = new Coordinate(6, 6);
        Coordinate pointTrue4 = new Coordinate(3, 5);
        Coordinate pointTrue5 = new Coordinate(2, 3);
        Coordinate pointTrue6 = new Coordinate(3, 3);
        Coordinate pointTrue7 = new Coordinate(5, 3);
        Coordinate pointTrue8 = new Coordinate(2, 2);
        Coordinate pointTrue9 = new Coordinate(4, 2);
        Coordinate pointTrue10 = new Coordinate(6, 2);
        Coordinate pointTrue11 = new Coordinate(4, 4);
        Coordinate pointTrue12 = new Coordinate(2, 6);

        Coordinate pointFalse1 = new Coordinate(2, 7);
        Coordinate pointFalse2 = new Coordinate(0, 6);
        Coordinate pointFalse3 = new Coordinate(4, 6);
        Coordinate pointFalse4 = new Coordinate(0, 5);
        Coordinate pointFalse5 = new Coordinate(0, 4);
        Coordinate pointFalse6 = new Coordinate(8, 4);
        Coordinate pointFalse7 = new Coordinate(0, 3);
        Coordinate pointFalse8 = new Coordinate(8, 3);
        Coordinate pointFalse9 = new Coordinate(0, 2);
        Coordinate pointFalse10 = new Coordinate(3, 2);
        Coordinate pointFalse11 = new Coordinate(5, 2);
        Coordinate pointFalse12 = new Coordinate(8, 2);
        Coordinate pointFalse13 = new Coordinate(0, 1);
        Coordinate pointFalse14 = new Coordinate(3, 1);
        Coordinate pointFalse15 = new Coordinate(5, 1);
        Coordinate pointFalse16 = new Coordinate(8, 1);
        Coordinate pointFalse17 = new Coordinate(1, -1);
        Coordinate pointFalse18 = new Coordinate(7, -1);

        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue1, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue2, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue3, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue4, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue5, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue6, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue7, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue8, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue9, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue10, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue11, polygonRing1));
        Assert.True(ContainsChecker.IsPointInLinearRing(pointTrue12, polygonRing1));

        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse1, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse2, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse3, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse4, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse5, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse6, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse7, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse8, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse9, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse10, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse11, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse12, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse13, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse14, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse15, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse16, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse17, polygonRing1));
        Assert.False(ContainsChecker.IsPointInLinearRing(pointFalse18, polygonRing1));
    }
}