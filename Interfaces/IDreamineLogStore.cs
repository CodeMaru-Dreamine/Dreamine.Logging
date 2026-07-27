using Dreamine.Logging.Models;

namespace Dreamine.Logging.Interfaces;

/// <summary>
/// \if KO
/// <para>UI 또는 진단에서 읽을 수 있는 로그 저장소를 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines a readable log store used by UI or diagnostics.</para>
/// \endif
/// </summary>
public interface IDreamineLogStore
{
    /// <summary>
    /// \if KO
    /// <para>로그 항목이 추가될 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when a log entry is added.</para>
    /// \endif
    /// </summary>
    event EventHandler<DreamineLogEntry>? LogAdded;

    /// <summary>
    /// \if KO
    /// <para>저장된 모든 로그 항목을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets all stored log entries.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>저장된 로그 항목의 스냅샷입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A snapshot of stored log entries.</para>
    /// \endif
    /// </returns>
    IReadOnlyList<DreamineLogEntry> GetEntries();

    /// <summary>
    /// \if KO
    /// <para>로그 항목을 저장소에 추가합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Adds a log entry to the store.</para>
    /// \endif
    /// </summary>
    /// <param name="entry">
    /// \if KO
    /// <para>추가할 로그 항목입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The log entry to add.</para>
    /// \endif
    /// </param>
    void Add(DreamineLogEntry entry);

    /// <summary>
    /// \if KO
    /// <para>저장된 모든 로그 항목을 지웁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Clears all stored log entries.</para>
    /// \endif
    /// </summary>
    void Clear();
}
