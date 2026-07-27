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
/// \if KO
/// <para>Dreamine 로깅 서비스 등록 도우미를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides registration helpers for Dreamine logging services.</para>
/// \endif
/// </summary>
public static class DreamineLoggingRegistration
{
    /// <summary>
    /// \if KO
    /// <para>구성 작업을 적용해 Dreamine 핵심 로깅 서비스를 등록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Registers Dreamine core logging services after applying a configuration action.</para>
    /// \endif
    /// </summary>
    /// <param name="configure">
    /// \if KO
    /// <para>선택적 로깅 구성 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The optional logging configuration action.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>해제 시 등록된 출력을 비우는 비동기 해제 핸들입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An asynchronous disposal handle that drains the registered sink.</para>
    /// \endif
    /// </returns>
    public static IAsyncDisposable Register(Action<DreamineLoggingOptions>? configure = null)
    {
        var options = new DreamineLoggingOptions();
        configure?.Invoke(options);

        return Register(options);
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 옵션으로 Dreamine 핵심 로깅 서비스를 등록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Registers Dreamine core logging services using the specified options.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>로깅 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The logging options.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>해제 시 등록된 출력을 비우는 비동기 해제 핸들입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An asynchronous disposal handle that drains the registered sink.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>비동기 로그 출력을 제한 시간 내 종료하는 핸들입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Represents a handle that shuts down an asynchronous log sink within a timeout.</para>
    /// \endif
    /// </summary>
    private sealed class LoggingShutdownHandle : IAsyncDisposable
    {
        /// <summary>
        /// \if KO
        /// <para>async Sink 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the async sink value.</para>
        /// \endif
        /// </summary>
        private readonly AsyncQueueSink _asyncSink;
        /// <summary>
        /// \if KO
        /// <para>shutdown Timeout 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the shutdown timeout value.</para>
        /// \endif
        /// </summary>
        private readonly TimeSpan _shutdownTimeout;

        /// <summary>
        /// \if KO
        /// <para>종료 대상과 제한 시간으로 핸들을 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes the handle with its sink and timeout.</para>
        /// \endif
        /// </summary>
        /// <param name="asyncSink">
        /// \if KO
        /// <para>종료할 비동기 출력입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The asynchronous sink to shut down.</para>
        /// \endif
        /// </param>
        /// <param name="shutdownTimeout">
        /// \if KO
        /// <para>종료 제한 시간입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The shutdown timeout.</para>
        /// \endif
        /// </param>
        public LoggingShutdownHandle(AsyncQueueSink asyncSink, TimeSpan shutdownTimeout)
        {
            _asyncSink = asyncSink;
            _shutdownTimeout = shutdownTimeout;
        }

        /// <summary>
        /// \if KO
        /// <para>대기 중 로그를 처리하고 출력을 종료합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Drains pending logs and shuts down the sink.</para>
        /// \endif
        /// </summary>
        /// <returns>
        /// \if KO
        /// <para>비동기 종료 작업입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The asynchronous shutdown operation.</para>
        /// \endif
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            await _asyncSink.ShutdownAsync(_shutdownTimeout).ConfigureAwait(false);
        }
    }
}
