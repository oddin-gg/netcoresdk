using Oddin.OddinSdk.SDK.AMQP.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class AdditionalProbabilities : IAdditionalProbabilities
    {
        public double? Win { get; }

        public double? Lose { get; }

        public double? HalfWin { get; }

        public double? HalfLose { get; }

        public double? Refund { get; }

        public AdditionalProbabilities(double? win, double? lose, double? halfWin, double? halfLose, double? refund)
        {
            Win = win;
            Lose = lose;
            HalfWin = halfWin;
            HalfLose = halfLose;
            Refund = refund;
        }
    }
}
