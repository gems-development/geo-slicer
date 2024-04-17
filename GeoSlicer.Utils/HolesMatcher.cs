using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace GeoSlicer.Utils;

public class HolesMatcher
{
    private readonly ContainsChecker _containsChecker;

    public HolesMatcher(ContainsChecker containsChecker)
    {
        _containsChecker = containsChecker;
    }


    public IEnumerable<Polygon> PlaceHoles(IEnumerable<LinearRing> shells, LinkedList<LinearRing> holes)
    {
        LinkedList<Polygon> result = new LinkedList<Polygon>();

        foreach (LinearRing shell in shells)
        {
            if (holes.Count > 0)
            {
                LinkedList<LinearRing> currentHoles = new LinkedList<LinearRing>();
                LinkedListNode<LinearRing>? node = holes.First!;
                do
                {
                    if (_containsChecker.IsPointInLinearRing(node.Value.GetCoordinateN(0), shell))
                    {
                        currentHoles.AddLast(node.Value);
                        holes.Remove(node);
                    }

                    node = node.Next;
                } while (node is not null);

                result.AddLast(new Polygon(shell, currentHoles.ToArray()));
            }
            else
            {
                result.AddLast(new Polygon(shell));
            }
        }

        return result;
    }
}