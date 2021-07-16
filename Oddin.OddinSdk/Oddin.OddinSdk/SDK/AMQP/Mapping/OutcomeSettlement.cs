using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
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