﻿using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class Producer : IProducer
    {
        private int _id;
        private string _name;
        private string _description;
        private bool _active;
        // TODO: change to smth cleaner than string (are multiple |-separated values possible?) and find usage
        private string _scope;
        private int _statefulRecoveryWindowInMinutes;

        public int Id => _id;

        public string Name => _name;

        public string Description => _description;

        public bool IsAvailable => _active;

        public bool IsDisabled => throw new NotImplementedException();

        public bool IsProducerDown => throw new NotImplementedException();

        public DateTime LastTimestampBeforeDisconnect => throw new NotImplementedException();

        public int MaxRecoveryTime => throw new NotImplementedException();

        public int MaxInactivitySeconds => throw new NotImplementedException();

        public IRecoveryInfo RecoveryInfo => throw new NotImplementedException();


        public Producer(
            int id,
            string name,
            string description,
            bool active,
            string scope,
            int statefulRecoveryWindowInMinutes
            )
        {
            _id = id;
            _name = name;
            _description = description;
            _active = active;
            _scope = scope;
            _statefulRecoveryWindowInMinutes = statefulRecoveryWindowInMinutes;
        }
    }
}
