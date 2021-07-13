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
        IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }
    }

    public interface IMarketCancel : IMarket
    {
        INamedValue VoidReason { get; }
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
        Zero,
        Half,
        One
    }

    public enum OutcomeResult
    {
        Lost,
        Won,
        UndecidedYet
    }

    internal class MarketWithSettlement : MarketCancel, IMarketWithOdds
    {
        public MarketStatus Status { get; }

        public bool IsFavorite { get; }

        public IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

        public IMarketMetadata MarketMetadata { get; }

        public IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }

        public MarketWithSettlement(
            int marketId,
            IDictionary<string, string> specifiers,
            IApiClient apiClient,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            MarketStatus marketStatus,
            bool isFavorite,
            int? voidReason,
            IEnumerable<IOutcomeSettlement> outcomes,
            IEnumerable<IOutcomeOdds> outcomeOdds,
            IMarketMetadata marketMetadata)
            : base(marketId, specifiers, apiClient, exceptionHandlingStrategy, voidReason)
        {
            if (outcomes is null)
                throw new ArgumentNullException(nameof(outcomes));

            Status = marketStatus;
            IsFavorite = isFavorite;
            OutcomeOdds = outcomeOdds;
            MarketMetadata = marketMetadata;

            var readonlyOutcomes = outcomes as IReadOnlyCollection<IOutcomeSettlement>;
            OutcomeSettlements = readonlyOutcomes ?? new ReadOnlyCollection<IOutcomeSettlement>(outcomes.ToList());
        }
    }

    /// <summary>
    /// Represents a result of the betting market with void reason
    /// </summary>
    internal class MarketCancel : Market, IMarketCancel
    {
        private readonly int? _voidReason;

        public INamedValue VoidReason => throw new NotImplementedException();//_voidReason == null ? null : _voidReasonsCache.GetNamedValue(_voidReason.Value);

        internal MarketCancel(int id,
                            IDictionary<string, string> specifiers,
                            IApiClient client,
                            ExceptionHandlingStrategy exceptionHandlingStrategy,
                            int? voidReason)
            : base(id, specifiers, client, exceptionHandlingStrategy, null)
        {
            _voidReason = voidReason;
        }
    }
}