using System;
using Dreamine.Logging.Interfaces;
using Dreamine.Logging.Models;

namespace Dreamine.Logging.Services;

/// <summary>
/// \if KO
/// <para>기본 Dreamine 로거 구현을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides the default Dreamine logger implementation.</para>
/// \endif
/// </summary>
public sealed class DreamineLogger : IDreamineLogger
{
    /// <summary>
    /// \if KO
    /// <para>sinks 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sinks value.</para>
    /// \endif
    /// </summary>
    private readonly IReadOnlyList<IDreamineLogSink> _sinks;
    /// <summary>
    /// \if KO
    /// <para>minimum Level 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the minimum level value.</para>
    /// \endif
    /// </summary>
    private readonly DreamineLogLevel _minimumLevel;
    /// <summary>
    /// \if KO
    /// <para>category 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the category value.</para>
    /// \endif
    /// </summary>
    private readonly string _category;

    /// <summary>
    /// \if KO
    /// <para>단일 출력으로 <see cref="T:Dreamine.Logging.Services.DreamineLogger" />를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes <see cref="T:Dreamine.Logging.Services.DreamineLogger" /> with a single sink.</para>
    /// \endif
    /// </summary>
    /// <param name="sink">
    /// \if KO
    /// <para>로그 출력입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log sink.</para>
    /// \endif
    /// </param>
    public DreamineLogger(IDreamineLogSink sink)
        : this(new[] { sink }, DreamineLogLevel.Trace, "Dreamine")
    {
    }

    /// <summary>
    /// \if KO
    /// <para>단일 출력과 범주로 <see cref="T:Dreamine.Logging.Services.DreamineLogger" />를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes <see cref="T:Dreamine.Logging.Services.DreamineLogger" /> with a single sink and category.</para>
    /// \endif
    /// </summary>
    /// <param name="sink">
    /// \if KO
    /// <para>로그 출력입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log sink.</para>
    /// \endif
    /// </param>
    /// <param name="category">
    /// \if KO
    /// <para>로그 범주입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log category.</para>
    /// \endif
    /// </param>
    public DreamineLogger(IDreamineLogSink sink, string category)
        : this(new[] { sink }, DreamineLogLevel.Trace, category)
    {
    }

    /// <summary>
    /// \if KO
    /// <para>단일 출력, 최소 수준 및 범주로 <see cref="DreamineLogger"/>를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes <see cref="DreamineLogger"/> with a single sink, minimum level, and category.</para>
    /// \endif
    /// </summary>
    /// <param name="sink">
    /// \if KO
    /// <para>로그 출력입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log sink.</para>
    /// \endif
    /// </param>
    /// <param name="minimumLevel">
    /// \if KO
    /// <para>최소 기록 수준입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The minimum log level.</para>
    /// \endif
    /// </param>
    /// <param name="category">
    /// \if KO
    /// <para>로그 범주입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log category.</para>
    /// \endif
    /// </param>
    /// <remarks>
    /// \if KO
    /// <para>단일 출력을 내부에서 시퀀스로 감싸 1개짜리 배열을 만들 필요가 없게 합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Wraps a single sink as a sequence so callers need not create a one-element array.</para>
    /// \endif
    /// </remarks>
    public DreamineLogger(
        IDreamineLogSink sink,
        DreamineLogLevel minimumLevel,
        string category)
        : this(new[] { sink }, minimumLevel, category)
    {
    }

    /// <summary>
    /// \if KO
    /// <para>여러 출력, 최소 수준 및 범주로 <see cref="T:Dreamine.Logging.Services.DreamineLogger" />를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes <see cref="T:Dreamine.Logging.Services.DreamineLogger" /> with multiple sinks, a minimum level, and a category.</para>
    /// \endif
    /// </summary>
    /// <param name="sinks">
    /// \if KO
    /// <para>로그 출력 시퀀스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log sinks.</para>
    /// \endif
    /// </param>
    /// <param name="minimumLevel">
    /// \if KO
    /// <para>최소 기록 수준입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The minimum log level.</para>
    /// \endif
    /// </param>
    /// <param name="category">
    /// \if KO
    /// <para>로그 범주입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log category.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="sinks"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="sinks"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para>출력 시퀀스가 비어 있는 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the sink sequence is empty.</para>
    /// \endif
    /// </exception>
    public DreamineLogger(
        IEnumerable<IDreamineLogSink> sinks,
        DreamineLogLevel minimumLevel = DreamineLogLevel.Trace,
        string category = "Dreamine")
    {
        if (sinks is null)
        {
            throw new ArgumentNullException(nameof(sinks));
        }

        _sinks = sinks.ToArray();

        if (_sinks.Count == 0)
        {
            throw new ArgumentException("At least one log sink is required.", nameof(sinks));
        }

        _minimumLevel = minimumLevel;
        _category = string.IsNullOrWhiteSpace(category) ? "Dreamine" : category;
    }

    /// <summary>
    /// \if KO
    /// <para>추적 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a trace-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Trace(string message) => Write(DreamineLogLevel.Trace, message, null);

    /// <summary>
    /// \if KO
    /// <para>디버그 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a debug-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Debug(string message) => Write(DreamineLogLevel.Debug, message, null);

    /// <summary>
    /// \if KO
    /// <para>정보 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an information-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Info(string message) => Write(DreamineLogLevel.Info, message, null);

    /// <summary>
    /// \if KO
    /// <para>경고 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a warning-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Warning(string message) => Write(DreamineLogLevel.Warning, message, null);

    /// <summary>
    /// \if KO
    /// <para>오류 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an error-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Error(string message) => Write(DreamineLogLevel.Error, message, null);

    /// <summary>
    /// \if KO
    /// <para>예외와 오류 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an exception and error-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="exception">
    /// \if KO
    /// <para>예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The exception.</para>
    /// \endif
    /// </param>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Error(Exception exception, string message) => Write(DreamineLogLevel.Error, message, exception);

    /// <summary>
    /// \if KO
    /// <para>치명적 오류 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a fatal-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Fatal(string message) => Write(DreamineLogLevel.Fatal, message, null);

    /// <summary>
    /// \if KO
    /// <para>예외와 치명적 오류 수준 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an exception and fatal-level message.</para>
    /// \endif
    /// </summary>
    /// <param name="exception">
    /// \if KO
    /// <para>예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The exception.</para>
    /// \endif
    /// </param>
    /// <param name="message">
    /// \if KO
    /// <para>메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message.</para>
    /// \endif
    /// </param>
    public void Fatal(Exception exception, string message) => Write(DreamineLogLevel.Fatal, message, exception);

    /// <summary>
    /// \if KO
    /// <para>최소 수준 이상인 항목을 모든 출력에 기록하며 출력별 실패를 격리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an entry at or above the minimum level to every sink while isolating sink failures.</para>
    /// \endif
    /// </summary>
    /// <param name="entry">
    /// \if KO
    /// <para>기록할 로그 항목입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log entry to write.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="entry"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="entry"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public void Write(DreamineLogEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (entry.Level < _minimumLevel)
        {
            return;
        }

        foreach (var sink in _sinks)
        {
            try
            {
                sink.Write(entry);
            }
            catch
            {
                // Logging failure must not terminate the application.
            }
        }
    }

    /// <summary>
    /// \if KO
    /// <para>현재 시각과 스레드 정보로 로그 항목을 만들어 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates and writes a log entry with the current time and thread information.</para>
    /// \endif
    /// </summary>
    /// <param name="level">
    /// \if KO
    /// <para>로그 수준입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log level.</para>
    /// \endif
    /// </param>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    /// <param name="exception">
    /// \if KO
    /// <para>선택적 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The optional exception.</para>
    /// \endif
    /// </param>
    private void Write(DreamineLogLevel level, string message, Exception? exception)
    {
        var entry = new DreamineLogEntry(
            DateTimeOffset.Now,
            level,
            _category,
            message,
            exception,
            Environment.CurrentManagedThreadId);

        Write(entry);
    }
}
