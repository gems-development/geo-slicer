using System.Collections.Generic;
using System.Threading;
using GeoSlicer.HoleDeleters;
using GeoSlicer.Utils;
using NetTopologySuite.Geometries;
using Assert = NetTopologySuite.Utilities.Assert;

namespace GeoSlicer.Tests.HoleDeletersTests;

public class BoundingHoleDeleterTest
{
    private static readonly double Epsilon = 1e-15;
    private static readonly TraverseDirection Traverse = new (new LineService(Epsilon));

    private static readonly BoundingHoleDeleter Deleter =
        new (Traverse);
    
    [Fact]
    public void TestSimplePolygon()
    {
        //Arrange
        Polygon sample = ObjectsForTests.GetSample();
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        LinkedList<Coordinate> problemCoordinates;
        LinearRing extendedTunnelsSample;
        //Act
        Polygon newSample = Deleter.DeleteHoles(sample);
        divider.DivideZeroTunnels(newSample.Shell, out extendedTunnelsSample, out problemCoordinates);
        //Assert
        Assert.IsEquals(0, newSample.Holes.Length);
        Assert.IsEquals(0, problemCoordinates.Count);
        Assert.IsTrue(extendedTunnelsSample.IsValid);
    }
    
    [Fact]
    public void TestBaikalAndKazanPolygons()
    {
        //Arrange
        Polygon baikal = ObjectsForTests.GetBaikal();
        Polygon kazan = ObjectsForTests.GetKazan();
        ZeroTunnelDivider dividerThread1 = ObjectsForTests.GetZeroTunnelDivider();
        ZeroTunnelDivider dividerThread2 = ObjectsForTests.GetZeroTunnelDivider();
        LinearRing? extendedTunnelsBaikal = null;
        LinearRing? extendedTunnelsKazan = null;
        LinkedList<Coordinate>? problemCoordinatesBaikal = null;
        LinkedList<Coordinate>? problemCoordinatesKazan = null;
        //Act
        Polygon newBaikal = Deleter.DeleteHoles(baikal);
        Polygon newKazan = Deleter.DeleteHoles(kazan);
        Thread thread1 = new Thread(() =>
        {
            dividerThread1.DivideZeroTunnels
                (newBaikal.Shell, out extendedTunnelsBaikal, out problemCoordinatesBaikal);
        });
        Thread thread2 = new Thread(() =>
        {
            dividerThread2.DivideZeroTunnels
                (newKazan.Shell, out extendedTunnelsKazan, out problemCoordinatesKazan);
        });
        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();
        //Assert
        Assert.IsEquals(0, newBaikal.Holes.Length);
        Assert.IsEquals(0, problemCoordinatesBaikal!.Count);
        Assert.IsTrue(extendedTunnelsBaikal!.IsValid);
        
        Assert.IsEquals(0, newKazan.Holes.Length);
        Assert.IsEquals(0, problemCoordinatesKazan!.Count);
        Assert.IsTrue(extendedTunnelsKazan!.IsValid); 
    }
    [Fact]
    public void TestFinalPolygon()
    {
        //Arrange
        Polygon testFinal = ObjectsForTests.GetTestFinal();
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        LinkedList<Coordinate> problemCoordinates;
        LinearRing extendedTunnelsSample;
        //Act
        Polygon newTestFinal = Deleter.DeleteHoles(testFinal);
        divider.DivideZeroTunnels(newTestFinal.Shell, out extendedTunnelsSample, out problemCoordinates);
        //Assert
        Assert.IsEquals(0, newTestFinal.Holes.Length);
        Assert.IsEquals(0, problemCoordinates.Count);
        Assert.IsTrue(extendedTunnelsSample.IsValid);
    }
    [Fact]
    public void Test2Polygon()
    {
        //Arrange
        Polygon test2 = ObjectsForTests.GetTest2();
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        LinkedList<Coordinate> problemCoordinates;
        LinearRing extendedTunnelsSample;
        //Act
        Polygon newTest2 = Deleter.DeleteHoles(test2);
        divider.DivideZeroTunnels(newTest2.Shell, out extendedTunnelsSample, out problemCoordinates);
        //Assert
        Assert.IsEquals(0, newTest2.Holes.Length);
        Assert.IsEquals(0, problemCoordinates.Count);
        Assert.IsTrue(extendedTunnelsSample.IsValid);
    }
}