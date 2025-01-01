using Core.Contracts.Services;
using Core.Models;
using IdentifyMonitorsUtil.Contracts.Models;
using IdentifyMonitorsUtil.Contracts.Presenters;
using IdentifyMonitorsUtil.Contracts.Views;
using Monitor = IdentifyMonitorsUtil.Models.Monitor;

namespace IdentifyMonitorsUtil.Presenters;

public class MonitorListPresenter : IMonitorListPresenter {
  private readonly IMonitorListView       view;
  private readonly IMonitorManagerService monitorManagerService;


  public MonitorListPresenter(
    IMonitorListView view,
    IMonitorManagerService monitorManagerService
  ) {
    this.view                  = view;
    this.monitorManagerService = monitorManagerService;
  }


  /// <inheritdoc />
  public void Initialize() {
    LoadMonitors();
  }


  /// <inheritdoc />
  public void LoadMonitors() {
    var monitors = monitorManagerService.EnumerateMonitors()
      .Select<Win32Monitor, IMonitor>(win32Monitor => new Monitor(win32Monitor));
    view.SetMonitors(monitors);
  }
}
