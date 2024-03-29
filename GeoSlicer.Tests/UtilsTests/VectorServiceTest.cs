using System;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Tests.UtilsTests;

public class VectorServiceTest
{
    [Fact]
    public void ShiftPointAlongBisectorTest1()
    {
        //Arrange
        Coordinate coordAdjacentLine = new Coordinate(7, 1);
        Coordinate firstCoord = new Coordinate(6, 2);
        Coordinate secondCoord = new Coordinate(7, 3);
        double stepSize = 2.567;
        double tolerance = 1e-15;
        //Act
        Coordinate result1 = 
            VectorService.ShiftPointAlongBisector(
                coordAdjacentLine, 
                firstCoord,
                secondCoord, 
                stepSize, 
                tolerance);
        
        Coordinate result2 = 
            VectorService.ShiftPointAlongBisector(
                coordAdjacentLine, 
                firstCoord,
                secondCoord, 
                stepSize, 
                tolerance,
                false);
        //Assert
        Assert.True(
            result1.Equals2D(new Coordinate(8.567, 2), tolerance) ||
            result1.Equals2D(new Coordinate(3.433, 2), tolerance));
        Assert.True(
            result2.Equals2D(new Coordinate(8.567, 2), tolerance) ||
            result2.Equals2D(new Coordinate(3.433, 2), tolerance));
        Assert.False(result1.Equals2D(result2, tolerance));
    }
    
    [Fact]
    public void ShiftPointAlongBisectorTest2()
    {
        //Arrange
        Coordinate coordAdjacentLine = new Coordinate(5, 1);
        Coordinate firstCoord = new Coordinate(6, 2);
        Coordinate secondCoord = new Coordinate(7, 3);
        double stepSize = Math.Sqrt(2);
        double tolerance = 1e-15;
        //Act
        Coordinate result1 = 
            VectorService.ShiftPointAlongBisector(
                coordAdjacentLine, 
                firstCoord,
                secondCoord, 
                stepSize, 
                tolerance);
        
        Coordinate result2 = 
            VectorService.ShiftPointAlongBisector(
                coordAdjacentLine, 
                firstCoord,
                secondCoord, 
                stepSize, 
                tolerance,
                false);
        //Assert
        Assert.True(
            result1.Equals2D(new Coordinate(5, 3), tolerance) ||
            result1.Equals2D(new Coordinate(7, 1), tolerance));
        Assert.True(
            result2.Equals2D(new Coordinate(5, 3), tolerance) ||
            result2.Equals2D(new Coordinate(7, 1), tolerance));
        Assert.False(result1.Equals2D(result2, tolerance));
    }
    
    [Fact]
    public void ShiftPointAlongBisectorTest3()
    {
        //Arrange
        Coordinate coordAdjacentLine = new Coordinate(5.0626387275478,0.9369930576587);
        Coordinate firstCoord = new Coordinate(6, 2);
        Coordinate secondCoord = new Coordinate(7, 3);
        double stepSize = 0.4;
        double tolerance = 1e-15;
        //Act
        Coordinate result1 = 
            VectorService.ShiftPointAlongBisector(
                coordAdjacentLine, 
                firstCoord,
                secondCoord, 
                stepSize, 
                tolerance);
        
        Coordinate result2 = 
            VectorService.ShiftPointAlongBisector(
                coordAdjacentLine, 
                firstCoord,
                secondCoord, 
                stepSize, 
                tolerance,
                false);
        //Assert
        Assert.True(
            result1.Equals2D(new Coordinate(5.708426649628981, 2.273833857208014), tolerance) ||
            result1.Equals2D(new Coordinate(6.291573350371019, 1.7261661427919859), tolerance));
        Assert.True(
            result2.Equals2D(new Coordinate(5.708426649628981, 2.273833857208014), tolerance) ||
            result2.Equals2D(new Coordinate(6.291573350371019, 1.7261661427919859), tolerance));
        Assert.False(result1.Equals2D(result2, tolerance));
    }
}