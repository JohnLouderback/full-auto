using BrightIdeasSoftware;
using DarkModeForms;
using IdentifyMonitorsUtil.Contracts.Models;
using IdentifyMonitorsUtil.Contracts.Presenters;
using IdentifyMonitorsUtil.Contracts.Views;
using Monitor = IdentifyMonitorsUtil.Models.Monitor;

namespace IdentifyMonitorsUtil.Views;

internal partial class MonitorList : Form, IMonitorListView {
  private readonly Lazy<IMonitorListPresenter> presenter;
  private readonly DarkModeCS                  darkMode;
  private          IEnumerable<IMonitor>       monitors = Enumerable.Empty<IMonitor>();


  public MonitorList(Lazy<IMonitorListPresenter> presenter) {
    this.presenter = presenter;

    darkMode = new DarkModeCS(this) {
      //[Optional] Choose your preferred color mode here:
      // ColorMode = DarkModeCS.DisplayMode.ClearMode
      ColorMode = DarkModeCS.DisplayMode.SystemDefault
    };

    InitializeComponent();

    // Generate columns for the list view based on the model properties.
    Generator.GenerateColumns(list, typeof(Monitor), true);

    // var monitors = new MonitorEnumerationService().EnumerateMonitors();
    //
    // // Set the list view's objects to the model data.
    // list.SetObjects(monitors);

    // Prevent sorting by clicking on the column headers.
    list.BeforeSorting += (sender, e) => { e.Canceled = true; };

    list.Dropped += (sender, e) => {
      // The list must re-render when rows are dragged and dropped so that the row numbers are
      // kept in sync with the user's order of preference.
      RerenderList();
    };

    list.FullRowSelect = true;

    // Resize the columns to fit the content and header text.
    Shown += (sender, args) => {
      AddUserOrderColumn();
      AdjustIsPrimaryColumn();
      UpdateHeadersStyle();
      this.presenter.Value.Initialize();
      list.AutoSizeColumns();
      list.AutoResizeColumns();
    };
  }


  /// <inheritdoc />
  public void SetMonitors(IEnumerable<IMonitor> monitors) {
    this.monitors = monitors;
    list.SetObjects(this.monitors);
    list.RebuildColumns(); // Refresh the columns to apply the changes
    RerenderList();
  }


  /// <summary>
  ///   Adds a programmatic column to the list view that displays the row number, which represents
  ///   the user's order of preference for the monitors.
  /// </summary>
  private void AddUserOrderColumn() {
    // Create the sequential number column
    var seqColumn = new OLVColumn("User Order", "Order");
    seqColumn.AspectGetter = rowObject => {
      // Calculate the row index dynamically
      var rowIndex = list.IndexOf(rowObject);
      return rowIndex + 1; // Index is zero-based, so add 1 for 1-based numbering
    };

    // Add the column to your OLV
    list.AllColumns.Insert(0, seqColumn);
    list.RebuildColumns(); // Refresh the columns to apply the changes
    RerenderList();
  }


  /// <summary>
  ///   Adjusts how the "IsPrimary" column is rendered.
  /// </summary>
  private void AdjustIsPrimaryColumn() {
    // Find the column for "IsPrimary"
    var primaryColumn = list.GetColumn("Primary?");

    // Enable checkbox rendering
    primaryColumn.CheckBoxes = true;

    // Suppress the text rendering
    primaryColumn.AspectToStringConverter = value => "";

    RerenderList();
  }


  /// <summary>
  ///   Re-renders the list view. Necessary when the list view's objects are modified.
  /// </summary>
  private void RerenderList() {
    list.RefreshObjects(monitors.ToList());
    for (var i = 0; i < list.GetItemCount(); i++) {
      var item = list.GetItem(i);
      list.RefreshItem(item);
    }
  }


  private void UpdateHeadersStyle() {
    list.HeaderUsesThemes  = false;
    list.HeaderStyle       = ColumnHeaderStyle.Nonclickable;
    list.HeaderFormatStyle = new HeaderFormatStyle();
    list.HeaderFormatStyle.SetBackColor(darkMode.OScolors.Surface);
    list.HeaderFormatStyle.SetForeColor(darkMode.OScolors.TextActive);
    list.ContextMenuStrip            = null;
    list.ShowFilterMenuOnRightClick  = false;
    list.ShowCommandMenuOnRightClick = false;

    foreach (OLVColumn item in list.Columns) {
      var headerStyle = new HeaderFormatStyle();
      headerStyle.SetBackColor(darkMode.OScolors.Surface);
      headerStyle.SetForeColor(darkMode.OScolors.TextActive);
      item.HeaderFormatStyle = headerStyle;
    }

    // Make the last column fill the remaining space.
    // Get the last column
    var lastColumn = list.AllColumns.Last();

    // Set it to fill the remaining space
    lastColumn.FillsFreeSpace = true;
  }
}
