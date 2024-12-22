using Core.Contracts.Services;
using Core.Models;
using IdentifyMonitorsUtil.Contracts.Models;
using IdentifyMonitorsUtil.Contracts.Presenters;
using IdentifyMonitorsUtil.Contracts.Views;
using Monitor = IdentifyMonitorsUtil.Models.Monitor;

namespace IdentifyMonitorsUtil.Presenters;

public class MonitorListPresenter : IMonitorListPresenter {
  private readonly IMonitorListView           view;
  private readonly IMonitorEnumerationService monitorEnumerationService;


  public MonitorListPresenter(
    IMonitorListView view,
    IMonitorEnumerationService monitorEnumerationService
  ) {
    this.view                      = view;
    this.monitorEnumerationService = monitorEnumerationService;
  }


  /// <inheritdoc />
  public void Initialize() {
    LoadMonitors();
  }


  /// <inheritdoc />
  public void LoadMonitors() {
    var monitors = monitorEnumerationService.EnumerateMonitors()
      .Select<Win32Monitor, IMonitor>(win32Monitor => new Monitor(win32Monitor));
    view.SetMonitors(monitors);
  }
}
