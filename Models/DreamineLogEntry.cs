using System;

namespace Dreamine.Logging.Models;

/// <summary>
/// \if KO
/// <para>하나의 Dreamine 로그 항목을 나타냅니다.</para>
/// \endif
/// \if EN
/// <para>Represents a single Dreamine log entry.</para>
/// \endif
/// </summary>
public sealed class DreamineLogEntry
{
    /// <summary>
    /// \if KO
    /// <para>로그 항목이 생성된 시각을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the time when the log entry was created.</para>
    /// \endif
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// \if KO
    /// <para>로그 항목의 심각도 수준을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the severity level of the log entry.</para>
    /// \endif
    /// </summary>
    public DreamineLogLevel Level { get; }

    /// <summary>
    /// \if KO
    /// <para>로그 범주를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the log category.</para>
    /// \endif
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// \if KO
    /// <para>로그 메시지를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the log message.</para>
    /// \endif
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// \if KO
    /// <para>로그 항목과 연결된 예외를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the exception associated with the log entry.</para>
    /// \endif
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// \if KO
    /// <para>로그 항목이 생성된 관리 스레드 ID를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the managed thread ID where the log entry was created.</para>
    /// \endif
    /// </summary>
    public int ThreadId { get; }

    /// <summary>
    /// \if KO
    /// <para>서식이 적용된 타임스탬프 텍스트를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the formatted timestamp text.</para>
    /// \endif
    /// </summary>
    public string TimestampText => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

    /// <summary>
    /// \if KO
    /// <para>간단한 UI 보기에서 사용할 표시 텍스트를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the display text used by simple UI views.</para>
    /// \endif
    /// </summary>
    public string DisplayText
    {
        get
        {
            var text = $"[{TimestampText}] [{Level}] [{Category}] [T{ThreadId}] {Message}";

            if (Exception is null)
            {
                return text;
            }

            return text + Environment.NewLine + Exception;
        }
    }

    /// <summary>
    /// \if KO
    /// <para><see cref="T:Dreamine.Logging.Models.DreamineLogEntry" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of the <see cref="T:Dreamine.Logging.Models.DreamineLogEntry" /> class.</para>
    /// \endif
    /// </summary>
    /// <param name="timestamp">
    /// \if KO
    /// <para>생성 시각입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The creation time.</para>
    /// \endif
    /// </param>
    /// <param name="level">
    /// \if KO
    /// <para>심각도 수준입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The severity level.</para>
    /// \endif
    /// </param>
    /// <param name="category">
    /// \if KO
    /// <para>로그 범주이며 비어 있으면 Dreamine이 사용됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log category; Dreamine is used when empty.</para>
    /// \endif
    /// </param>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지이며 <see langword="null"/>이면 빈 문자열이 사용됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message; an empty string is used when <see langword="null"/>.</para>
    /// \endif
    /// </param>
    /// <param name="exception">
    /// \if KO
    /// <para>연결된 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The associated exception.</para>
    /// \endif
    /// </param>
    /// <param name="threadId">
    /// \if KO
    /// <para>관리 스레드 ID입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The managed thread ID.</para>
    /// \endif
    /// </param>
    public DreamineLogEntry(
        DateTimeOffset timestamp,
        DreamineLogLevel level,
        string category,
        string message,
        Exception? exception,
        int threadId)
    {
        Timestamp = timestamp;
        Level = level;
        Category = string.IsNullOrWhiteSpace(category) ? "Dreamine" : category;
        Message = message ?? string.Empty;
        Exception = exception;
        ThreadId = threadId;
    }
}
