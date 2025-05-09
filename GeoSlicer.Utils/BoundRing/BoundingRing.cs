﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils.BoundRing;

public class BoundingRing
{
    private readonly LineService _lineService;

    // Координаты левой снизу и правой сверху точек у рамки, которая содержит кольцо Ring
    public Coordinate PointMin { get; private set; }
    public Coordinate PointMax { get; private set; }


    // Координаты точек кольца, которые касаются сторон рамки
    public LinkedNode<Coordinate> PointLeftNode { get; private set; }
    public LinkedNode<Coordinate> PointRightNode { get; private set; }
    public LinkedNode<Coordinate> PointUpNode { get; private set; }
    public LinkedNode<Coordinate> PointDownNode { get; private set; }
    public LinkedNode<Coordinate> Ring { get; private set; }
    public int PointsCount { get; private set; }

    // true - обход кольца по часовой стрелке
    private readonly bool _counterClockwiseBypass;

    public BoundingRing(Coordinate pointMin,
        Coordinate pointMax,
        LinkedNode<Coordinate> pointLeftNode,
        LinkedNode<Coordinate> pointRightNode,
        LinkedNode<Coordinate> pointUpNode,
        LinkedNode<Coordinate> pointDownNode,
        LinkedNode<Coordinate> ring, int pointsCount, bool counterClockwiseBypass,
        LineService lineService)
    {
        PointMin = pointMin;
        PointMax = pointMax;
        PointLeftNode = pointLeftNode;
        PointRightNode = pointRightNode;
        PointUpNode = pointUpNode;
        PointDownNode = pointDownNode;
        Ring = ring;
        PointsCount = pointsCount;
        _counterClockwiseBypass = counterClockwiseBypass;
        _lineService = lineService;
    }

    private bool Equals(BoundingRing other)
    {
        return PointMin.Equals(other.PointMin)
               && PointMax.Equals(other.PointMax)
               && PointLeftNode.Equals(other.PointLeftNode)
               && PointRightNode.Equals(other.PointRightNode)
               && PointUpNode.Equals(other.PointUpNode)
               && PointDownNode.Equals(other.PointDownNode)
               && Ring.Equals(other.Ring) && PointsCount == other.PointsCount;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BoundingRing)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return HashCode.Combine(PointMin, PointMax, PointLeftNode, PointRightNode,
            PointUpNode, PointDownNode, Ring, PointsCount);
    }


    /// <summary>
    /// Преобразует Polygon в список из BoundingRing, в котором первый элемент - оболочка полигона,
    /// остальные элементы - дыры полигона
    /// </summary>
    public static LinkedList<BoundingRing> PolygonToBoundRings(
        Polygon polygon, LineService lineService)
    {
        LinkedList<BoundingRing> boundRings = new LinkedList<BoundingRing>();
        boundRings.AddFirst(
            BoundRService.LinearRingToBoundingRing(polygon.Shell, true, lineService));
        foreach (LinearRing ring in polygon.Holes)
        {
            boundRings.AddLast(
                BoundRService.LinearRingToBoundingRing(ring, false, lineService));
        }

        return boundRings;
    }

    /// <summary>
    /// Преобразует список из BoundingRing в Polygon. В списке первый элемент должен быть оболочкой полигона.
    /// Остальные элементы - дырами полигона
    /// </summary>
    public static Polygon BoundRingsToPolygon(LinkedList<BoundingRing> boundRings, GeometryFactory? factory = null)
    {
        factory ??= NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

        LinearRing shell = BoundRService.BoundRingToLinearRing(boundRings.First!.Value);
        LinearRing[] holes = new LinearRing[boundRings.Count - 1];

        using (var enumerator = boundRings.GetEnumerator())
        {
            enumerator.MoveNext();
            int i = 0;
            while (enumerator.MoveNext())
            {
                holes[i] = BoundRService.BoundRingToLinearRing(enumerator.Current);
                i++;
            }
        }

        return new Polygon(shell, holes, factory);
    }

    /// <summary>
    /// Соединяет два BoundingRing нулевым туннелем.
    /// <paramref name="boundRing2"/> переходит в некорректное состояние после работы метода.
    /// </summary>
    public void ConnectBoundRings(
        BoundingRing boundRing2,
        LinkedNode<Coordinate> point1Node, LinkedNode<Coordinate> point2Node)
    {
        point1Node = BoundRService.FindCorrectLinkedCoord(
            point1Node, point2Node.Elem, this._counterClockwiseBypass, _lineService);
        point2Node = BoundRService.FindCorrectLinkedCoord(
            point2Node, point1Node.Elem, boundRing2._counterClockwiseBypass, _lineService);
        Ring = BoundRService.ConnectRingsNodes(point1Node, point2Node);

        PointRightNode = CoordinateNodeService.GetMaxByX(
            PointRightNode,
            boundRing2.PointRightNode);

        PointLeftNode = CoordinateNodeService.GetMinByX(
            PointLeftNode,
            boundRing2.PointLeftNode);

        PointUpNode = CoordinateNodeService.GetMaxByY(
            PointUpNode,
            boundRing2.PointUpNode);

        PointDownNode = CoordinateNodeService.GetMinByY(
            PointDownNode,
            boundRing2.PointDownNode);

        PointMax = new Coordinate(
            PointRightNode.Elem.X,
            PointUpNode.Elem.Y);

        PointMin = new Coordinate(
            PointLeftNode.Elem.X,
            PointDownNode.Elem.Y);

        PointsCount = PointsCount + boundRing2.PointsCount + 2;
    }
}