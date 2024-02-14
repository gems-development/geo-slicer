using System;
using System.Collections.Generic;
using System.IO;

namespace GeoSlicer.Utils.AccuracyCalculation;

public class GeojsonAccuracyCalculator
{
    public int Calculate(IEnumerable<char> geojson)
    {
        return Calculate<char>(geojson, Char.IsDigit, c => c == '.');
    }

    public int Calculate(IEnumerable<string> geojson)
    {
        return Calculate<string>(geojson, s => Char.IsDigit(s[0]), s => s[0] == '.');
    }

    private int Calculate<T>(IEnumerable<T> geojson, Predicate<T> isDigit, Predicate<T> isDot)
    {
        bool isMantissa = false;
        int maxAccuracy = 0;
        int currentAccuracy = 0;

        foreach (T symbol in geojson)
        {
            if (isDigit(symbol))
            {
                if (isMantissa)
                {
                    currentAccuracy++;
                }
            }
            else if (isDot(symbol))
            {
                currentAccuracy = 0;
                isMantissa = true;
            }
            else
            {
                if (isMantissa)
                {
                    isMantissa = false;
                    maxAccuracy = Math.Max(currentAccuracy, maxAccuracy);
                }
            }
        }

        return Math.Max(maxAccuracy, currentAccuracy);
    }
}