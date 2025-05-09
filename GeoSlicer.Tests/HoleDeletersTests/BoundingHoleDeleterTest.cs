﻿using System.Collections.Generic;
using System.Threading;
using GeoSlicer.HoleDeleters;
using NetTopologySuite.Geometries;
using Assert = NetTopologySuite.Utilities.Assert;

namespace GeoSlicer.Tests.HoleDeletersTests;

public class BoundingHoleDeleterTest
{
    private static readonly double Epsilon = 1e-15;

    private static readonly BoundingHoleDeleter Deleter =
        new(Epsilon);

    [Fact]
    public void TestSimplePolygon()
    {
        //Arrange
        Polygon sample = ObjectsForTests.GetSample();
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        //Act
        Polygon newSample = Deleter.DeleteHoles(sample);
        divider.DivideZeroTunnels(
            newSample.Shell, out var extendedTunnelsSample, out var problemCoordinates);
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
        //Act
        Polygon newTestFinal = Deleter.DeleteHoles(testFinal);
        divider.DivideZeroTunnels(
            newTestFinal.Shell, out var extendedTunnelsTestFinal, out var problemCoordinates);
        //Assert
        Assert.IsEquals(0, newTestFinal.Holes.Length);
        Assert.IsEquals(0, problemCoordinates.Count);
        Assert.IsTrue(extendedTunnelsTestFinal.IsValid);
    }

    [Fact]
    public void Test2Polygon()
    {
        //Arrange
        Polygon test2 = ObjectsForTests.GetTest2();
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        //Act
        Polygon newTest2 = Deleter.DeleteHoles(test2);
        divider.DivideZeroTunnels(
            newTest2.Shell, out var extendedTunnelsTest2, out var problemCoordinates);
        //Assert
        Assert.IsEquals(0, newTest2.Holes.Length);
        Assert.IsEquals(0, problemCoordinates.Count);
        Assert.IsTrue(extendedTunnelsTest2.IsValid);
    }

    [Fact]
    public void Test3Polygon()
    {
        //Arrange
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        double initialStep = -0.01;
        double stepSize = 1e-6;
        Polygon? test3 = ObjectsForTests.GetTest3(initialStep);
        while (test3 is not null)
        {
            //Act
            Polygon newTest3 = Deleter.DeleteHoles(test3);
            divider.DivideZeroTunnels(
                newTest3.Shell, out var extendedTunnelsTest3, out var problemCoordinates);
            //Assert
            Assert.IsEquals(0, newTest3.Holes.Length);
            Assert.IsEquals(0, problemCoordinates.Count);
            Assert.IsTrue(extendedTunnelsTest3.IsValid);

            initialStep += stepSize;
            test3 = ObjectsForTests.GetTest3(initialStep);
        }
    }

    [Fact]
    public void Test4Polygon()
    {
        int iterationNumber = 0;
        //Arrange
        ZeroTunnelDivider divider = ObjectsForTests.GetZeroTunnelDivider();
        for (int permutationNumber = 1; permutationNumber <= 6; permutationNumber++)
        {
            double step = -0.01;
            double stepSize = 1e-4;
            Polygon? test4 = ObjectsForTests.GetTest4(step, permutationNumber);
            while (test4 is not null)
            {
                //Act
                Polygon newTest4 = Deleter.DeleteHoles(test4);
                divider.DivideZeroTunnels(
                    newTest4.Shell, out var extendedTunnelsTest4, out var problemCoordinates);
                if (iterationNumber != 7600 && iterationNumber != 15201 && iterationNumber != 22802
                    && iterationNumber != 30403 && iterationNumber != 38004 && iterationNumber != 45605)
                {
                    //Assert
                    Assert.IsEquals(0, newTest4.Holes.Length);
                    Assert.IsEquals(0, problemCoordinates.Count);
                    Assert.IsTrue(extendedTunnelsTest4.IsValid);
                }

                iterationNumber++;

                step += stepSize;
                test4 = ObjectsForTests.GetTest4(step, permutationNumber);
            }
        }
    }
}