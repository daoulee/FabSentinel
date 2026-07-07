namespace GreenVision.Helpers;

public static class SimulationHelper
{
    private static readonly Random _rng = Random.Shared;

    public static double NextGaussian(double mean = 0.0, double stdDev = 1.0)
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    public static double WalkValue(double current, double baseValue, double stdDev, double min, double max)
    {
        double delta = NextGaussian(0, stdDev);
        double pulled = current + delta + (baseValue - current) * 0.05;
        return Math.Clamp(pulled, min, max);
    }

    public static double NextBetween(double min, double max) =>
        min + _rng.NextDouble() * (max - min);

    public static bool Chance(double probability) =>
        _rng.NextDouble() < probability;
}
