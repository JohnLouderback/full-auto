using IdentifyMonitorsUtil.Contracts.Models;

namespace IdentifyMonitorsUtil.Contracts.Views;

public interface IMonitorListView : IWindowView {
  /// <summary>
  ///   Sets the monitors to be displayed in the view.
  /// </summary>
  /// <param name="monitors"> A collection of monitors to display. </param>
  void SetMonitors(IEnumerable<IMonitor> monitors);
}
