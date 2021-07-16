using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using System;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class MatchSummary : IMatchSummary
    {
        private readonly string _name;
        private readonly DateTime? _scheduledTime;
        private readonly DateTime? _scheduledEndTime;
        private readonly URN _sportId;

        public MatchSummary(string name, DateTime? scheduledTime, DateTime? scheduledEndTime, URN sportId)
        {
            _name = name;
            _scheduledTime = scheduledTime;
            _scheduledEndTime = scheduledEndTime;
            _sportId = sportId;
        }

        public string Name => _name;

        public DateTime? ScheduledTime => _scheduledTime;

        public DateTime? ScheduledEndTime => _scheduledEndTime;

        public URN SportId => _sportId;
    }
}
