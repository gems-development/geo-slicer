using System;

namespace GeoSlicer.Utils.AccuracyCalculation;

public class AccuracyCalculator
{
    public int SumAccuracy => _maxAccuracy;
    public int MulAccuracy(int multiplyCount) => _maxAccuracy * multiplyCount;

    public double SumEpsilon => _epsilon;
    public double MulEpsilon(int multiplyCount) => Math.Pow(_epsilon, multiplyCount);

    private readonly int _maxAccuracy;
    private readonly double _epsilon;


    public AccuracyCalculator(int maxAccuracy)
    {
        _maxAccuracy = maxAccuracy;
        _epsilon = Math.Pow(0.1, _maxAccuracy);
    }
}