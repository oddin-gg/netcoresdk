using Oddin.OddsFeedSdk.AMQP.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP;

internal class ExchangeNameProvider : IExchangeNameProvider
{
    public string ExchangeName => "oddinfeed";
}

internal class ReplayExchangeNameProvider : IExchangeNameProvider
{
    public string ExchangeName => "oddinreplay";
}