namespace Dreamine.Logging.Models;

/// <summary>
/// Defines the severity level of a Dreamine log entry.
/// </summary>
public enum DreamineLogLevel
{
    /// <summary>
    /// Detailed diagnostic information.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Debugging information.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// General information.
    /// </summary>
    Info = 2,

    /// <summary>
    /// Warning information.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error information.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Fatal error information.
    /// </summary>
    Fatal = 5
}