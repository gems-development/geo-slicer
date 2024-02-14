namespace GeoSlicer.Utils.AccuracyCalculation;

public class AccuracyCalculator
{
    public int SumAccuracy => _maxAccuracy;
    public int MulAccuracy(int multiplyCount) => _maxAccuracy * multiplyCount;

    private readonly int _maxAccuracy;


    public AccuracyCalculator(int maxAccuracy)
    {
        _maxAccuracy = maxAccuracy;
    }
}