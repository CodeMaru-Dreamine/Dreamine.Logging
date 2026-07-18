using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// \if KO
/// <para>Dreamine 구성 요소에서 사용하는 기본 로깅 API를 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines the main logging API used by Dreamine components.</para>
/// \endif
/// </summary>
public interface IDreamineLogger
{
    /// <summary>
    /// \if KO
    /// <para>추적 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a trace log message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    void Trace(string message);

    /// <summary>
    /// \if KO
    /// <para>디버그 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a debug log message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    void Debug(string message);

    /// <summary>
    /// \if KO
    /// <para>정보 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an information log message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    void Info(string message);

    /// <summary>
    /// \if KO
    /// <para>경고 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a warning log message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    void Warning(string message);

    /// <summary>
    /// \if KO
    /// <para>오류 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an error log message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    void Error(string message);

    /// <summary>
    /// \if KO
    /// <para>예외와 함께 오류 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an error log message with an exception.</para>
    /// \endif
    /// </summary>
    /// <param name="exception">
    /// \if KO
    /// <para>기록할 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The exception to log.</para>
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
    void Error(Exception exception, string message);

    /// <summary>
    /// \if KO
    /// <para>치명적 오류 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a fatal log message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>로그 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log message.</para>
    /// \endif
    /// </param>
    void Fatal(string message);

    /// <summary>
    /// \if KO
    /// <para>예외와 함께 치명적 오류 수준 로그 메시지를 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a fatal log message with an exception.</para>
    /// \endif
    /// </summary>
    /// <param name="exception">
    /// \if KO
    /// <para>기록할 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The exception to log.</para>
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
    void Fatal(Exception exception, string message);

    /// <summary>
    /// \if KO
    /// <para>구성된 로그 항목을 직접 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a log entry directly.</para>
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
    void Write(DreamineLogEntry entry);
}
