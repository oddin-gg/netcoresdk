namespace Oddin.OddsFeedSdk.AMQP.Abstractions;

internal interface IExchangeNameProvider
{
    string ExchangeName { get; }
}