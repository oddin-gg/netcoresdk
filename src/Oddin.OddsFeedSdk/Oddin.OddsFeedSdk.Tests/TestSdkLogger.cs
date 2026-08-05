using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Oddin.OddsFeedSdk.Tests;

internal static class TestSdkLogger
{
    internal static readonly RecordingLogger Logger = new();

    [ModuleInitializer]
    internal static void Initialize() =>
        global::Oddin.OddsFeedSdk.SdkLoggerFactory.Initialize(new RecordingLoggerFactory(Logger));

    internal sealed class RecordingLogger : ILogger
    {
        private readonly List<(LogLevel Level, string Message)> _entries = new();

        public IReadOnlyCollection<(LogLevel Level, string Message)> Entries
        {
            get
            {
                lock (_entries)
                    return _entries.ToArray();
            }
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            lock (_entries)
                _entries.Add((logLevel, formatter(state, exception)));
        }

        public void Clear()
        {
            lock (_entries)
                _entries.Clear();
        }
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        public RecordingLoggerFactory(ILogger logger) => _logger = logger;

        public ILogger CreateLogger(string categoryName) => _logger;
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
