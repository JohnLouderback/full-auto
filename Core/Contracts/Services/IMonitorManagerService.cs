using Core.Models;

namespace Core.Contracts.Services;

/// <summary>
///   Provides methods for listing and identifying monitors on the current system as well as
///   storing user preferences regarding monitor configuration.
/// </summary>
public interface IMonitorManagerService {
  /// <summary>
  ///   Enumerates all monitors on the system.
  /// </summary>
  /// <returns>
  ///   An <see cref="IEnumerable{T}" /> of <see cref="Win32Monitor" /> that contains all monitors on the
  ///   system.
  /// </returns>
  IEnumerable<Win32Monitor> EnumerateMonitors();
}
