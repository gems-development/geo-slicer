using GeoSlicer.Utils;
using NetTopologySuite.Geometries;

namespace GeoSlicer.DivideAndRuleSlicers.OppositesIndexesGivers;

public abstract class OppositesIndexesGiver : IDivisionIndexesGiver
{
    private readonly LineService _lineService;

    protected OppositesIndexesGiver(LineService lineService)
    {
        _lineService = lineService;
    }
    
    public abstract void GetIndexes(LinearRing ring, out int first, out int second);
    
    protected void FindAnyInnerIndex(Coordinate[] coordinates, out int first, out int second)
    {
        int halfOfLen = coordinates.Length / 2;
        int shift = 1;

        while (true)
        {
            for (int i = 0; i < coordinates.Length; i++)
            {
                if (IsInnerLine(
                        coordinates, i, (i + halfOfLen + shift) % coordinates.Length))
                {
                    first = i;
                    second = (i + halfOfLen + shift) % coordinates.Length;
                    return;
                }
            }

            shift++;
        }
    }

    protected bool IsInnerLine(Coordinate[] coordinates, int first, int second)
    {
        return _lineService.InsideTheAngleWithoutBorders(
                   coordinates[first],
                   coordinates[second],
                   coordinates[(first + 1) % coordinates.Length],
                   coordinates[first],
                   coordinates[(first - 1 + coordinates.Length) % coordinates.Length])
               && _lineService.InsideTheAngleWithoutBorders(
                   coordinates[second],
                   coordinates[first],
                   coordinates[(second + 1) % coordinates.Length],
                   coordinates[second],
                   coordinates[(second - 1 + coordinates.Length) % coordinates.Length]);
    }
}