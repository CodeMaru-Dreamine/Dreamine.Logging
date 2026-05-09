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
    /// <returns>The registered async queue sink.</returns>
    public static AsyncQueueSink Register(Action<DreamineLoggingOptions>? configure = null)
    {
        var options = new DreamineLoggingOptions();
        configure?.Invoke(options);

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
        DMContainer.RegisterSingleton<IDreamineLogSink>(asyncSink);
        DMContainer.RegisterSingleton<IDreamineLogger>(logger);

        return asyncSink;
    }
}