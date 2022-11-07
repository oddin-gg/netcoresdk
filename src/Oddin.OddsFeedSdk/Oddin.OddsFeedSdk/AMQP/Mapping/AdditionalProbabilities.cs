using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class AdditionalProbabilities : IAdditionalProbabilities
{
    public AdditionalProbabilities(double? win, double? lose, double? halfWin, double? halfLose, double? refund)
    {
        Win = win;
        Lose = lose;
        HalfWin = halfWin;
        HalfLose = halfLose;
        Refund = refund;
    }

    public double? Win { get; }

    public double? Lose { get; }

    public double? HalfWin { get; }

    public double? HalfLose { get; }

    public double? Refund { get; }
}