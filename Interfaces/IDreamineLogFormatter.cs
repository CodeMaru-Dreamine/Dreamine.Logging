using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// \if KO
/// <para>Dreamine 로그 항목을 텍스트로 변환하는 포맷터를 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines a formatter that converts a Dreamine log entry to text.</para>
/// \endif
/// </summary>
public interface IDreamineLogFormatter
{
    /// <summary>
    /// \if KO
    /// <para>지정한 로그 항목을 텍스트로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Formats the specified log entry.</para>
    /// \endif
    /// </summary>
    /// <param name="entry">
    /// \if KO
    /// <para>변환할 로그 항목입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log entry to format.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>변환된 로그 텍스트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The formatted log text.</para>
    /// \endif
    /// </returns>
    string Format(DreamineLogEntry entry);
}
