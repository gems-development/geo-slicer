using System.Collections.Generic;
using System.Linq;
using GeoSlicer.Utils;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters;

public class BruteForceHoleDeleter
{
    private readonly GeometryFactory _gf;
    private readonly LineIntersector _lineIntersector;

    public BruteForceHoleDeleter(GeometryFactory? gf = null, LineIntersector? lineIntersector = null)
    {
        _lineIntersector = lineIntersector ?? new RobustLineIntersector();
        _gf = gf ?? NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    }

    public LinearRing DeleteHoles(Polygon polygon)
    {
        LinearRing shell = polygon.Shell;
        LinearRing[] holes = polygon.Holes;
        LinkedList<int> unconnectedHolesNumbers = new LinkedList<int>(Enumerable.Range(0, holes.Length));

        int totalDotCount = shell.Count;
        totalDotCount += holes.Select(ring => ring.Count).Sum();
        totalDotCount += holes.Length;
        List<Coordinate> resultCoordinates = new List<Coordinate>(totalDotCount);
        LinkedList<LineSegment> allLines = new LinkedList<LineSegment>();
        foreach (LineSegment segment in Converters.LineStringToLineSegments(shell))
        {
            allLines.AddLast(segment);
        }

        foreach (LinearRing hole in holes)
        {
            foreach (LineSegment segment in Converters.LineStringToLineSegments(hole))
            {
                allLines.AddLast(segment);
            }
        }

        LinkedList<(int from, int ring, int to)>[] table =
            new LinkedList<(int from, int ring, int to)>[holes.Length + 1];
        FillLines(shell, table, 0, 0, unconnectedHolesNumbers, allLines, holes);
        FillResultCoordinates(resultCoordinates, shell, table, 0, 0, holes);

        LinearRing result = _gf.CreateLinearRing(resultCoordinates.ToArray());
        return result;

        // Проверить, что totalDotCount = resCoords.Count т.е. не пересоздается внутренний массив
        // Проверить, что unconnectedHoles.Count + 1 = connectedUnchecked.Count т.е. не пересоздается внутренний массив
    }

    private void FillLines(
        LinearRing ring,
        LinkedList<(int from, int ring, int to)>[] table,
        int tableStringNum,
        int startCoordNum,
        LinkedList<int> unconnectedHolesNumbers,
        LinkedList<LineSegment> allLines,
        LinearRing[] holes)
    {
        LinkedList<(int from, int ring, int to)> list = new LinkedList<(int from, int ring, int to)>();
        table[tableStringNum] = list;
        Coordinate[] coordinates = ring.Coordinates;
        for (int i = 0; i < coordinates.Length - 1 && unconnectedHolesNumbers.Count != 0; i++)
        {
            Coordinate outerCoordinate = coordinates[i + startCoordNum % (coordinates.Length - 1)];
            LinkedListNode<int>? innerRingNumberNode = unconnectedHolesNumbers.First!;
            do
            {
                Coordinate[] innerCoordinates = holes[innerRingNumberNode.Value].Coordinates;
                for (int j = 0; j < innerCoordinates.Length - 1; j++)
                {
                    bool isIntersect = false;
                    foreach (LineSegment line in allLines)
                    {
                        _lineIntersector.ComputeIntersection(line.P0, line.P1, outerCoordinate, innerCoordinates[j]);
                        if (_lineIntersector.IsProper)
                        {
                            isIntersect = true;
                            break;
                        }

                    }

                    if (isIntersect)
                        continue;
                    unconnectedHolesNumbers.Remove(innerRingNumberNode);
                    allLines.AddLast(new LineSegment(outerCoordinate, innerCoordinates[j]));
                    list.AddLast((i + startCoordNum % (coordinates.Length - 1), innerRingNumberNode.Value, j));
                    break;
                }

                innerRingNumberNode = innerRingNumberNode!.Next;
                // Если к этой внешней точке был присоединен объект, то ссылка будет null и мы перейдем к след точке
            } while (innerRingNumberNode is not null);
        }

        foreach ((int from, int ring, int to) valueTuple in list)
        {
            if (unconnectedHolesNumbers.Count == 0)
            {
                break;
            }

            FillLines(
                holes[valueTuple.ring], table, valueTuple.ring + 1,
                (valueTuple.to + 1) % (holes[valueTuple.ring].Coordinates.Length - 1), unconnectedHolesNumbers,
                allLines, holes);
        }
    }

    private void FillResultCoordinates(
        List<Coordinate> result,
        LinearRing ring,
        LinkedList<(int from, int ring, int to)>[] table,
        int tableStringNum,
        int startCoordNum,
        LinearRing[] holes)
    {
        LinkedList<(int from, int ring, int to)> list = table[tableStringNum];
        Coordinate[] coordinates = ring.Coordinates;

        LinkedListNode<(int from, int ring, int to)>? node = list?.First;
        int from = (node is null) ? -1 : node.Value.from;
        for (int i = 0; i < coordinates.Length; i++)
        {
            int n = (i + startCoordNum) % (coordinates.Length - 1);
            result.Add(coordinates[n]);
            if (n == from || n == 0 && from == coordinates.Length - 1)
            {
                FillResultCoordinates(result, holes[node!.Value.ring], table,
                    node!.Value.ring + 1, node!.Value.to, holes);
                result.Add(coordinates[n]);
                node = node.Next;
                from = (node is null) ? -1 : node.Value.from;
            }
        }
    }
}