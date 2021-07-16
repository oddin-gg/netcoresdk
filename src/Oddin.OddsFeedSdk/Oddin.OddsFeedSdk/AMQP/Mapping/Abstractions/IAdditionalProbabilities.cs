namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IAdditionalProbabilities
    {
        double? Win { get; }

        double? Lose { get; }

        double? HalfWin { get; }

        double? HalfLose { get; }

        double? Refund { get; }
    }
}
