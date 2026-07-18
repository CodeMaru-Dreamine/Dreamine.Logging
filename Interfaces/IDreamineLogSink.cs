using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// \if KO
/// <para>로그 출력 대상을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines a log output target.</para>
/// \endif
/// </summary>
public interface IDreamineLogSink
{
    /// <summary>
    /// \if KO
    /// <para>로그 항목을 출력 대상에 기록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a log entry to the sink.</para>
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
