using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IMarketWithSettlement : IMarketCancel
    {
        MarketStatus MarketStatus { get; }

        IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }
    }

    public interface IMarketCancel : IMarket
    {
        int? VoidReason { get; }
    }

    public interface INamedValue
    {
        int Id { get; }

        string Description { get; }
    }

    public interface IOutcomeSettlement : IOutcome
    {
        double? DeadHeatFactor { get; }

        VoidFactor? VoidFactor { get; }

        OutcomeResult OutcomeResult { get; }
    }

    public enum VoidFactor
    {
        Half,
        One
    }

    public enum OutcomeResult
    {
        Lost,
        Won,
        UndecidedYet
    }

    internal class MarketWithSettlement : MarketCancel, IMarketWithSettlement
    {
        public IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }

        public string ExtentedSpecifiers { get; }

        public MarketStatus MarketStatus { get; }

        public MarketWithSettlement(
            MarketStatus marketStatus,
            int marketId,
            IDictionary<string, string> specifiers,
            string extentedSpecifiers,
            IEnumerable<IOutcomeSettlement> outcomes,
            IApiClient apiClient,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            int? voidReason)
            : base(marketId, specifiers, apiClient, exceptionHandlingStrategy, voidReason)
        {
            ExtentedSpecifiers = extentedSpecifiers;
            MarketStatus = marketStatus;
            var readonlyOutcomes = outcomes as IReadOnlyCollection<IOutcomeSettlement>;
            OutcomeSettlements = readonlyOutcomes ?? new ReadOnlyCollection<IOutcomeSettlement>(outcomes.ToList());
        }
    }

    /// <summary>
    /// Represents a result of the betting market with void reason
    /// </summary>
    internal class MarketCancel : Market, IMarketCancel
    {
        public int? VoidReason { get; set; }

        internal MarketCancel(int id,
                            IDictionary<string, string> specifiers,
                            IApiClient client,
                            ExceptionHandlingStrategy exceptionHandlingStrategy,
                            int? voidReason)
            : base(id, specifiers, client, exceptionHandlingStrategy)
        {
            VoidReason = voidReason;
        }
    }

    /// <summary>
    /// Represents the result of a market outcome (selection)
    /// </summary>
    internal class OutcomeSettlement : Outcome, IOutcomeSettlement
    {
        internal OutcomeSettlement(double? deadHeatFactor,
                                   string id,
                                   IApiClient client,
                                   int result,
                                   VoidFactor? voidFactor)
            : base(id, client)
        {
            DeadHeatFactor = deadHeatFactor;
            VoidFactor = voidFactor;
            OutcomeResult = result switch
            {
                0 => OutcomeResult.Lost,
                1 => OutcomeResult.Won,
                _ => OutcomeResult.UndecidedYet
            };
        }

        public double? DeadHeatFactor { get; }

        public VoidFactor? VoidFactor { get; }

        public OutcomeResult OutcomeResult { get; }
    }
}