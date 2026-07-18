namespace Dreamine.Logging.Models;

/// <summary>
/// \if KO
/// <para>Dreamine 로그 항목의 심각도 수준을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines the severity level of a Dreamine log entry.</para>
/// \endif
/// </summary>
public enum DreamineLogLevel
{
    /// <summary>
    /// \if KO
    /// <para>상세 진단 정보입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Detailed diagnostic information.</para>
    /// \endif
    /// </summary>
    Trace = 0,

    /// <summary>
    /// \if KO
    /// <para>디버깅 정보입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Debugging information.</para>
    /// \endif
    /// </summary>
    Debug = 1,

    /// <summary>
    /// \if KO
    /// <para>일반 정보입니다.</para>
    /// \endif
    /// \if EN
    /// <para>General information.</para>
    /// \endif
    /// </summary>
    Info = 2,

    /// <summary>
    /// \if KO
    /// <para>경고 정보입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Warning information.</para>
    /// \endif
    /// </summary>
    Warning = 3,

    /// <summary>
    /// \if KO
    /// <para>오류 정보입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Error information.</para>
    /// \endif
    /// </summary>
    Error = 4,

    /// <summary>
    /// \if KO
    /// <para>치명적 오류 정보입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Fatal error information.</para>
    /// \endif
    /// </summary>
    Fatal = 5
}
