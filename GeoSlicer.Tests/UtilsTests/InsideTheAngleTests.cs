﻿using GeoSlicer.Utils;
using GeoSlicer.Utils.Intersectors.CoordinateComparators;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests;

public class InsideTheAngleTests
{
    private readonly LineService _lineService = new(1e-9, new EpsilonCoordinateComparator(1e-9));
    [Fact]
    public void TestInsideTheAngleLeft()
    {
        Coordinate vecBegin = new Coordinate(1, 1);
        Coordinate vecEnd = new Coordinate(0, 1);

        Coordinate angleA = new Coordinate(0,1);
        Coordinate angleB = new Coordinate(1,1);
        Coordinate angleC = new Coordinate(2,1);

        Assert.True(_lineService.InsideTheAngle(vecBegin, vecEnd,
            angleA, angleB, angleC));
        Assert.False(_lineService.InsideTheAngleWithoutBorders(vecBegin, vecEnd,
            angleA, angleB, angleC));
    } 
    
    [Fact]
    public void TestInsideTheAngleRight()
    {
        Coordinate vecBegin = new Coordinate(1, 1);
        Coordinate vecEnd = new Coordinate(2, 1);

        Coordinate angleA = new Coordinate(0,1);
        Coordinate angleB = new Coordinate(1,1);
        Coordinate angleC = new Coordinate(2,1);

        Assert.True(_lineService.InsideTheAngle(vecBegin, vecEnd,
            angleA, angleB, angleC));
        Assert.False(_lineService.InsideTheAngleWithoutBorders(vecBegin, vecEnd,
            angleA, angleB, angleC));
    } 
    
    [Fact]
    public void TestInsideTheAngleReversed()
    {
        Coordinate vecBegin = new Coordinate(0, 0);
        Coordinate vecEnd = new Coordinate(4, 0);

        Coordinate angleA = new Coordinate(4,0);
        Coordinate angleB = new Coordinate(0,0);
        Coordinate angleC = new Coordinate(-2,0);

        Assert.False(_lineService.InsideTheAngleWithoutBorders(vecBegin, vecEnd,
            angleC, angleB, angleA));
        Assert.False(_lineService.InsideTheAngleWithoutBorders(vecBegin, vecEnd,
            angleA, angleB, angleC));
    }
}