using Core.Contracts.Services;
using Core.Models;
using Core.Utils;

namespace Core.Services;

public class MonitorManagerService : IMonitorManagerService {
  /// <inheritdoc />
  public IEnumerable<Win32Monitor> EnumerateMonitors() {
    return NativeUtils.EnumerateMonitors();
  }
}
