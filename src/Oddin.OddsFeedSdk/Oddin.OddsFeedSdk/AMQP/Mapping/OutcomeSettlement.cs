using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class OutcomeSettlement : Outcome, IOutcomeSettlement
    {
        public double? DeadHeatFactor { get; }

        public VoidFactor? VoidFactor { get; }

        public OutcomeResult OutcomeResult { get; }

        internal OutcomeSettlement(
            double? deadHeatFactor,
            string id,
            IApiClient client,
            int result,
            double? voidFactor)
            : base(id, client)
        {
            DeadHeatFactor = deadHeatFactor;

            VoidFactor = voidFactor switch
            {
                1 => Enums.VoidFactor.One,
                0.5 => Enums.VoidFactor.Half,
                _ => null
            };

            OutcomeResult = result switch
            {
                0 => OutcomeResult.Lost,
                1 => OutcomeResult.Won,
                _ => OutcomeResult.UndecidedYet
            };
        }
    }
}