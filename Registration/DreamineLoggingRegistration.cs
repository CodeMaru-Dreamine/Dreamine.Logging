using System;
using Dreamine.Logging.Formatters;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;
using Dreamine.Logging.Options;
using Dreamine.Logging.Services;
using Dreamine.Logging.Sinks;
using Dreamine.MVVM.Core;

namespace Dreamine.Logging.Registration;

/// <summary>
/// Provides registration helpers for Dreamine logging services.
/// </summary>
public static class DreamineLoggingRegistration
{
    /// <summary>
    /// Registers Dreamine core logging services.
    /// </summary>
    /// <param name="configure">The optional logging configuration action.</param>
    /// <returns>An async-disposable handle that drains the registered sink on disposal.</returns>
    public static IAsyncDisposable Register(Action<DreamineLoggingOptions>? configure = null)
    {
        var options = new DreamineLoggingOptions();
        configure?.Invoke(options);

        return Register(options);
    }

    /// <summary>
    /// Registers Dreamine core logging services.
    /// </summary>
    /// <param name="options">The logging options.</param>
    /// <returns>An async-disposable handle that drains the registered sink on disposal.</returns>
    public static IAsyncDisposable Register(DreamineLoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var logStore = new InMemoryLogStore(options.StoreCapacity);
        var formatter = new DreamineTextLogFormatter();

        var textFileSink = new TextFileLogSink(
            options.LogDirectory,
            formatter,
            options.FlushEveryWriteCount);

        var compositeSink = new CompositeLogSink(new IDreamineLogSink[]
        {
            logStore,
            textFileSink
        });

        var asyncSink = new AsyncQueueSink(
            compositeSink,
            options.QueueCapacity,
            options.DrainBatchSize);

        var logger = new DreamineLogger(
            new IDreamineLogSink[] { asyncSink },
            DreamineLogLevel.Trace,
            options.Category);

        DMContainer.RegisterSingleton(logStore);
        DMContainer.RegisterSingleton<IDreamineLogStore>(logStore);
        DMContainer.RegisterSingleton<IDreamineLogFormatter>(formatter);
        DMContainer.RegisterSingleton(asyncSink);
        DMContainer.RegisterSingleton<IDreamineLogSink>(asyncSink);
        DMContainer.RegisterSingleton<IDreamineLogger>(logger);

        return new LoggingShutdownHandle(asyncSink, options.ShutdownTimeout);
    }

    private sealed class LoggingShutdownHandle : IAsyncDisposable
    {
        private readonly AsyncQueueSink _asyncSink;
        private readonly TimeSpan _shutdownTimeout;

        public LoggingShutdownHandle(AsyncQueueSink asyncSink, TimeSpan shutdownTimeout)
        {
            _asyncSink = asyncSink;
            _shutdownTimeout = shutdownTimeout;
        }

        public async ValueTask DisposeAsync()
        {
            await _asyncSink.ShutdownAsync(_shutdownTimeout).ConfigureAwait(false);
        }
    }
}
