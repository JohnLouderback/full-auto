using Core.Contracts.Services;
using Core.Models;
using Core.Utils;

namespace Core.Services;

public class MonitorEnumerationService : IMonitorEnumerationService {
  /// <inheritdoc />
  public IEnumerable<Win32Monitor> EnumerateMonitors() {
    return NativeUtils.EnumerateMonitors();
  }
}
