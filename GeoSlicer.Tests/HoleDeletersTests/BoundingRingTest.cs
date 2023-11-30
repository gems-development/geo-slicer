using NetTopologySuite.Geometries;
using GeoSlicer.Utils.BoundRing;


namespace GeoSlicer.Tests.HoleDeletersTests;

public class BoundingRingTest
{
    private LinkedList<BoundingRing>? _boundRings;
    private Polygon? _polygon;

    private void Initialize()
    {
        LinearRing shell = new LinearRing(new[]
        {
            new Coordinate(-2, -2), new Coordinate(-3, 1.5), new Coordinate(-2, 2), new Coordinate(2, 2),
            new Coordinate(2, -2), new Coordinate(-2, -2)
        });
        LinearRing firstHole = new LinearRing(new[]
        {
            new Coordinate(0, 0), new Coordinate(0, 0.1), new Coordinate(0.9, 0.1), new Coordinate(0.9, 0.9),
            new Coordinate(0, 0.9), new Coordinate(0, 1), new Coordinate(1, 1), new Coordinate(1, 0),
            new Coordinate(0, 0)
        });
        LinearRing secondHole = new LinearRing(new[]
            { new Coordinate(-1, 0.3),
                new Coordinate(0, 0.6),
                new Coordinate(0.7, 0.3),
                new Coordinate(-1, 0.3) });
        
        _polygon = new Polygon(shell, new[] { firstHole, secondHole });

        LinkedNode<Coordinate> shellNode = LinearRingToRingNode(shell);
        LinkedNode<Coordinate> firstHoleNode = LinearRingToRingNode(firstHole);
        LinkedNode<Coordinate> secondHoleNode = LinearRingToRingNode(secondHole);

        _boundRings = new LinkedList<BoundingRing>();

        _boundRings.AddFirst(new BoundingRing(
            new Coordinate(-3, -2),
            new Coordinate(2, 2),
            FindRingNode(shellNode, new Coordinate(-3, 1.5)),
            FindRingNode(shellNode, new Coordinate(2, 2)),
            FindRingNode(shellNode, new Coordinate(-2, 2)),
            FindRingNode(shellNode, new Coordinate(2, -2)),
            shellNode, shell.Count - 1));

        _boundRings.AddLast(new BoundingRing(
            new Coordinate(0, 0),
            new Coordinate(1, 1),
            FindRingNode(firstHoleNode, new Coordinate(0, 1)),
            FindRingNode(firstHoleNode, new Coordinate(1, 1)),
            FindRingNode(firstHoleNode, new Coordinate(0, 1)),
            FindRingNode(firstHoleNode, new Coordinate(1, 0)),
            firstHoleNode, firstHole.Count - 1));
        
        _boundRings.AddLast(new BoundingRing(
            new Coordinate(-1, 0.3), new Coordinate(0.7, 0.6),
            FindRingNode(secondHoleNode, new Coordinate(-1, 0.3)),
            FindRingNode(secondHoleNode, new Coordinate(0.7, 0.3)),
            FindRingNode(secondHoleNode, new Coordinate(0, 0.6)),
            FindRingNode(secondHoleNode, new Coordinate(0.7, 0.3)), 
            secondHoleNode, secondHole.Count - 1));
    }
    private LinkedNode<Coordinate> LinearRingToRingNode(LinearRing ring)
    {
        LinkedNode<Coordinate> ringNode = new LinkedNode<Coordinate>(ring.Coordinates[0]);
        for(int i = 1; i < ring.Coordinates.Length - 1; i++)
        {
            ringNode = new LinkedNode<Coordinate>(ring.Coordinates[i], ringNode);
        }
        return ringNode.Next;
    }
    private LinkedNode<Coordinate> FindRingNode(LinkedNode<Coordinate> ringNode, Coordinate point)
    {
        LinkedNode<Coordinate> bufferRingNode = ringNode;
        do
        {
            if (bufferRingNode.Elem.Equals(point))
                return bufferRingNode;
            bufferRingNode = bufferRingNode.Next;
        } while (!ReferenceEquals(bufferRingNode, ringNode));

        throw new InvalidDataException("point not found in ringNode");
    }
    [Fact]
    public void PolygonToBoundRingsTest()
    {
        //Arrange
        Initialize();
        //Act
        LinkedList<BoundingRing> actualBoundRings = BoundRingService.PolygonToBoundRings(_polygon!);
        //Assert
        Assert.Equal(_boundRings, actualBoundRings);
    }

    [Fact]
    public void ConnectBoundRingsNodesTest()
    {
        //Arrange
        Initialize();
        LinkedNode<Coordinate> ringNode = new LinkedNode<Coordinate>(new Coordinate(-1, 0.3));
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 0.6), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 0.9), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 1), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(1, 1), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(1, 0), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 0), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 0.1), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0.9, 0.1), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0.9, 0.9), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 0.9), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0, 0.6), ringNode);
        ringNode = new LinkedNode<Coordinate>(new Coordinate(0.7, 0.3), ringNode);
        ringNode = ringNode.Next.Next;
        
        BoundingRing boundRing = new BoundingRing(
            new Coordinate(-1, 0),
            new Coordinate(1, 1),
            FindRingNode(ringNode, new Coordinate(-1, 0.3)),
            FindRingNode(ringNode, new Coordinate(1, 1)),
            FindRingNode(ringNode, new Coordinate(0, 1)),
            FindRingNode(ringNode, new Coordinate(1, 0)),
            ringNode, 13);

        var boundRingsIter = _boundRings!.GetEnumerator();
        boundRingsIter.MoveNext();
        boundRingsIter.MoveNext();
        BoundingRing boundFirstHole = boundRingsIter.Current;
        boundRingsIter.MoveNext();
        BoundingRing boundSecondHole = boundRingsIter.Current;
        boundRingsIter.Dispose();
        //Act
        BoundingRing actualBoundRing = BoundRingService.ConnectBoundRings(
            boundSecondHole, 
            boundFirstHole,
            FindRingNode(boundSecondHole.Ring, new Coordinate(0, 0.6)),
            FindRingNode(boundFirstHole.Ring, new Coordinate(0, 0.9)));
        //Assert
        Assert.Equal(boundRing, actualBoundRing);
    }
}