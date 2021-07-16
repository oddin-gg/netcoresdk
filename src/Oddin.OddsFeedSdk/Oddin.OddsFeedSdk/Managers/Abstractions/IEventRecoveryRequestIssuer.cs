using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.Managers.Abstractions
{
    public interface IEventRecoveryRequestIssuer
    {
        Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId);

        Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId);
    }
}
