using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using System;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class MatchSummary : IMatchSummary
    {
        private readonly string _name;
        private readonly DateTime? _scheduledTime;
        private readonly DateTime? _scheduledEndTime;
        private readonly URN _sportId;

        public MatchSummary(MatchSummaryModel matchSummaryModel)
        {
            _name = matchSummaryModel.sport_event.name;
            _scheduledTime = matchSummaryModel.sport_event.scheduled;
            _scheduledEndTime = matchSummaryModel.sport_event.scheduled_end;
            _sportId = new URN(matchSummaryModel.sport_event.tournament.sport.id);
        }

        public string Name => _name;

        public DateTime? ScheduledTime => _scheduledTime;

        public DateTime? ScheduledEndTime => _scheduledEndTime;

        public URN SportId => _sportId;
    }
}
